using CRM.Common;
using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CRM.Infrastructure.Enum.BankStatementStatus;
using static CRM.Infrastructure.Enum.TransactionsType;
using Renci.SshNet;
using static CRM.Infrastructure.Enum.Banks;
using Renci.SshNet.Sftp;
using CRM.Features.Accounting.BankStatementDetails;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Text;
using System.Text.RegularExpressions;
using OfficeOpenXml;

namespace CRM.Features.Accounting.BankStatement
{
    public class BankStatementAppService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BankStatementDetailsAppService _bankStatementDetailsAppService;
        private static string IMPrivateKeyPath = @"//10.100.2.30//Host to Host//llavesecIMProd.asc"; 
        private static string Password = "$,0mx&J5U/%4"; 

        public BankStatementAppService(IUnitOfWork unitOfWork, BankStatementDetailsAppService bankStatementDetailsAppService)
        {
            _unitOfWork = unitOfWork;
            _bankStatementDetailsAppService = bankStatementDetailsAppService;
        }

        public async Task<EntityResponse> AddBankStatementAsync(BankStatementDto bankStatementDto)
        {
            if (bankStatementDto == null)
            {
                return EntityResponse.CreateError("Los datos para crear estado de cuenta son obligatorios.");
            }

            BankStatement bankStatement = new BankStatement
            {
                BankStatementId = bankStatementDto.BankStatementId,
                CompanyId = bankStatementDto.CompanyId,
                AccountId = bankStatementDto.AccountId,
                Account = bankStatementDto.Account,
                TransactionDate = bankStatementDto.TransactionDate,
                Status = BankStatatementState.Pending,
                CreateDateTime = bankStatementDto.CreateDateTime
            };

            _unitOfWork.Repository<BankStatement>().Add(bankStatement);
            await _unitOfWork.SaveChangesAsync();
            bankStatementDto.BankStatementId = bankStatement.BankStatementId;
            return EntityResponse.CreateOk();
        }

        public async Task<EntityResponse> ImportStatementFromFileByAccount(string dateString, string companyCode, string account)
        {
            DateTime transactionDate = DateTime.Parse(dateString).AddDays(1);
            bool wasFound = false;
            string fileName;
            EntityResponse<BankStatementDto> response = new();
            List<BankStatementDto> bankStatementDtos = new();
            List<BankConfiguration.BankConfiguration> accounts = new();
            List<string> errors = new();

            if(account == "" || account == null || account == "x")
            {
                accounts = _unitOfWork.Repository<BankConfiguration.BankConfiguration>().Query().Where(x => x.ActiveState == true && x.CompanyId == companyCode).ToList();
            }
            else
            {
                accounts = _unitOfWork.Repository<BankConfiguration.BankConfiguration>().Query().Where(x => x.AccountId == account && x.CompanyId == companyCode).ToList();
            }

            foreach (BankConfiguration.BankConfiguration bankAccount in accounts)
            {
                if (dateString != null)
                {
                    if (dateString.Replace(" ", "") != "")
                    {
                        List<BankStatement> bankStatements = await GetByAccountId(bankAccount.AccountId, dateString, companyCode);

                        if (bankStatements.Exists(x => x.Status == BankStatatementState.Processed))
                        {
                            errors.Add($"No se puede importar las transacciones de la fecha {bankStatements.Find(x => x.Status == BankStatatementState.Processed).TransactionDate} para la cuenta {bankAccount.AccountId} ya que se encuentran exportadas en AX.");
                            continue;
                        }

                        foreach (BankStatement bankStatement in bankStatements.FindAll(x => x.Status != BankStatatementState.Processed))
                        {
                            await _bankStatementDetailsAppService.DeleteDetails(bankStatement.BankStatementId);
                            await DeleteBankStatement(bankStatement);
                        }
                    }
                }

                var bankConfiguraion = _unitOfWork.Repository<BankConfiguration.BankConfiguration>().Query().FirstOrDefault(b => b.AccountId.Equals(bankAccount.AccountId));
                if (bankConfiguraion == null)
                {
                    errors.Add($"No se encontro una configuracion para la cuenta {bankAccount.AccountId}.");
                    continue;
                }

                //response = await SaveTransactions(@"\\gim-ser-finanzas\MT940\Ficohsa\FICOHSA\Intermoda_021102000000101963_MSG.2089383033.txt", bankConfiguraion);

                List<string> filesMT940 = new List<string>();
                using (var client = new SftpClient(bankConfiguraion.Host, bankConfiguraion.Port, bankConfiguraion.UserName, bankConfiguraion.Password))
                {
                    try
                    {
                        client.Connect();
                        if (client.IsConnected)
                        {
                            var files = client.ListDirectory(bankConfiguraion.FileRoute);
                            if (bankConfiguraion.Bank == Bank.BAC)
                            {
                                string year = transactionDate.Year.ToString();
                                string month = transactionDate.Month < 10 ? "0" + transactionDate.Month.ToString() : transactionDate.Month.ToString();
                                string day = transactionDate.Day < 10 ? "0" + transactionDate.Day.ToString() : transactionDate.Day.ToString();
                                string date = year + "-" + month + "-" + day;
                                fileName = (bankConfiguraion.FileName + bankConfiguraion.AccountNumber + '-' + date).Trim();

                                response = IterateFilesAndSaveTransactions(files, transactionDate, bankConfiguraion, client, fileName.Length, fileName, $".txt").Result;
                            }
                            else if (bankConfiguraion.Bank == Bank.BANPAIS)
                            {
                                response = /*await SaveTransactions(@"C:\Users\spineda\OneDrive - INTERMODA SA DE CV\Escritorio\MT940V1-010010026603 143 - 2024-12-7.txt", bankConfiguraion);*/ IterateFilesAndSaveTransactions(files, transactionDate, bankConfiguraion, client, bankConfiguraion.FileName.Length, bankConfiguraion.FileName, $" - {transactionDate.Year}-{transactionDate.Month}-{transactionDate.Day}.txt").Result;
                            }
                            else if (bankConfiguraion.Bank == Bank.ATLANTIDAD)
                            {
                                fileName = bankConfiguraion.FileName + $"{transactionDate.Year}{transactionDate.Month.ToString("D2")}{transactionDate.Day.ToString("D2")}_{transactionDate.Year}{transactionDate.Month.ToString("D2")}{transactionDate.Day.ToString("D2")}";
                                response = IterateFilesAndSaveTransactions(files, transactionDate, bankConfiguraion, client, fileName.Length, fileName, $".txt").Result;
                            }
                            else if (bankConfiguraion.Bank == Bank.FICOHSA)
                            {
                                response = IterateFilesAndSaveTransactions(files, transactionDate, bankConfiguraion, client, bankConfiguraion.FileName.Length, bankConfiguraion.FileName, $".txt").Result;
                            }
                            else if (bankConfiguraion.Bank == Bank.BANRURAL)
                            {
                                fileName = $"{transactionDate.Year}{transactionDate.Month.ToString("D2")}{transactionDate.Day.ToString("D2")}{bankConfiguraion.FileName}";
                                response = IterateFilesAndSaveTransactions(files, transactionDate, bankConfiguraion, client, fileName.Length, fileName, $".txt").Result;
                            }
                            else if (bankConfiguraion.Bank == Bank.BANCO_INDUSTRIAL)
                            {
                                fileName = $"F{transactionDate.Day.ToString("D2")}{transactionDate.Month.ToString("D2")}{transactionDate.Year}{bankConfiguraion.FileName}";
                                response = IterateFilesAndSaveTransactions(files, transactionDate, bankConfiguraion, client, fileName.Length, fileName, $".txt").Result;
                            }

                            if (!response.Ok)
                            {
                                errors.Add($"{bankConfiguraion.AccountId}: {response.Mensaje} ");
                            }
                            else
                            {
                                bankStatementDtos.Add(response.Data);
                            }
                        }
                        client.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{bankConfiguraion.Bank}: Error al acceder al SFTP. {ex.Message}");
                        continue;
                    }
                }
            }

            if (errors.Count > 0)
            {
                string error = $"Se generaron los siguientes errores: {string.Join(", ", errors)}. ";
                error += bankStatementDtos.Count > 0 ? $"Se generaron los BankStatements con los Ids:{string.Join(", ", bankStatementDtos.Select(p => p.BankStatementId))}" : "";
                return EntityResponse.CreateError(error);
            }

            return EntityResponse.CreateOk($"Se generaron los BankStatements con los Ids:{string.Join(", ", bankStatementDtos.Select(p => p.BankStatementId))}");
        }

        public async Task<EntityResponse<BankStatementDto>> IterateFilesAndSaveTransactions(IEnumerable<ISftpFile> files, DateTime transactionDate, BankConfiguration.BankConfiguration bankConfiguration, 
                                                                                            SftpClient client, int trimEnd, string fileName, string nameExtension)
        {
            try
            {
                bool wasFound = false;
                string ruta = "";
                EntityResponse<BankStatementDto> response = new();

                foreach (var file in files)
                {
                    ruta = @"" + bankConfiguration.LocalFileRoute + Path.GetFileNameWithoutExtension(file.Name) + nameExtension;

                    if (!file.IsDirectory)
                    {
                        string fileFromSftp = file.Name.Substring(0, trimEnd).Trim();

                        if (fileFromSftp.Equals(fileName))
                        {
                            if (file.LastWriteTime.Year == transactionDate.Year && file.LastWriteTime.Month == transactionDate.Month && file.LastWriteTime.Day == transactionDate.Day)
                            {
                                wasFound = true;

                                if (bankConfiguration.Bank == Bank.ATLANTIDAD || bankConfiguration.Bank == Bank.BANCO_INDUSTRIAL)
                                {
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        string filePath = file.FullName;

                                        client.DownloadFile(filePath, memoryStream); 
                                        byte[] fileBytes = memoryStream.ToArray();
                                        string base64String = Convert.ToBase64String(fileBytes);
                                        byte[] decodedBytes = Convert.FromBase64String(base64String);

                                        byte[] decryptedData = DecryptFile(decodedBytes, IMPrivateKeyPath, Password);

                                        ruta = @"" + bankConfiguration.LocalFileRoute + fileName + nameExtension;
                                        File.WriteAllBytes(ruta, decryptedData);
                                        response = await SaveTransactions(ruta, bankConfiguration);
                                        break;
                                    }
                                }

                                using (var fileStream = File.Create(ruta))
                                {
                                    client.DownloadFile(file.FullName, fileStream);
                                    fileStream.Close();
                                    response = await SaveTransactions(ruta, bankConfiguration);
                                    break;

                                }
                            }
                        }
                    }
                }

                if (!wasFound)
                {
                    ruta = getFilePath(fileName + nameExtension, bankConfiguration.LocalFileRoute);
                    if(ruta == null || ruta == "")
                    {
                        return EntityResponse.CreateError<BankStatementDto>("No se encontró el archivo.");
                    }
                    response = await SaveTransactions(ruta, bankConfiguration);
                }

                if (!response.Ok)
                {
                    return EntityResponse.CreateError<BankStatementDto>(response.Mensaje);
                }

                return EntityResponse.CreateOk(response.Data);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError<BankStatementDto>($"Error en método IterateFilesAndSaveTransactions:  {ex.Message}.");
            }
        }

        public static byte[] DecryptFile(byte[] encryptedData, string secretKeyPath, string password)
        {
            using (var inputStream = new MemoryStream(encryptedData))
            using (var keyIn = File.OpenRead(secretKeyPath))
            using (var outputStream = new MemoryStream())
            {
                var pgpFactory = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
                PgpEncryptedDataList encryptedDataList = null;
                PgpObject pgpObject = pgpFactory.NextPgpObject();

                if (pgpObject is PgpEncryptedDataList)
                    encryptedDataList = (PgpEncryptedDataList)pgpObject;
                else
                    encryptedDataList = (PgpEncryptedDataList)pgpFactory.NextPgpObject();

                PgpPrivateKey privateKey = null;
                PgpPublicKeyEncryptedData pbe = null;

                foreach (PgpPublicKeyEncryptedData pked in encryptedDataList.GetEncryptedDataObjects())
                {
                    privateKey = FindSecretKey(keyIn, pked.KeyId, password.ToCharArray());
                    if (privateKey != null)
                    {
                        pbe = pked;
                        break;
                    }
                }

                if (privateKey == null)
                    throw new ArgumentException("Secret key for message not found.");

                using (Stream clearStream = pbe.GetDataStream(privateKey))
                {
                    var plainFactory = new PgpObjectFactory(clearStream);
                    PgpObject message = plainFactory.NextPgpObject();

                    if (message is PgpCompressedData compressedData)
                    {
                        var compressedFactory = new PgpObjectFactory(compressedData.GetDataStream());
                        message = compressedFactory.NextPgpObject();
                    }

                    if (message is PgpLiteralData literalData)
                    {
                        Stream unc = literalData.GetInputStream();
                        unc.CopyTo(outputStream);
                    }
                    else
                    {
                        throw new PgpException("Message is not a simple encrypted file.");
                    }
                }

                return outputStream.ToArray();
            }
        }

        private static PgpPrivateKey FindSecretKey(Stream keyIn, long keyID, char[] pass)
        {
            PgpSecretKeyRingBundle secretKeyRingBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
            PgpSecretKey secretKey = secretKeyRingBundle.GetSecretKey(keyID);

            return secretKey?.ExtractPrivateKey(pass);
        }

        public string getFilePath(string baseFileName, string serverPath)
        {
            string[] files = Directory.GetFiles(serverPath);
            string baseFileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);

            foreach (string file in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                int amountCharacters = fileNameWithoutExtension.Length;
                string fileNameTrimmed = amountCharacters >= 47 ? fileNameWithoutExtension.Substring(0, 47).Trim() : fileNameWithoutExtension;

                if (fileNameTrimmed.Equals(baseFileNameWithoutExtension))
                {
                    return file;
                }
            }

            return "";
        }

        public async Task<EntityResponse<BankStatementDto>> SaveTransactions(string path, BankConfiguration.BankConfiguration bankConfiguraion)
        {
            try
            {
                BankStatementDto bankStatementDto = new BankStatementDto();
                List<MT940Transaction> transactions = ReadMT940File(path,
                                                                    (bankConfiguraion.Bank == Bank.BAC || bankConfiguraion.Bank == Bank.FICOHSA) ? 15 :
                                                                    (bankConfiguraion.Bank == Bank.BANPAIS || bankConfiguraion.Bank == Bank.ATLANTIDAD || bankConfiguraion.Bank == Bank.BANCO_INDUSTRIAL || bankConfiguraion.Bank == Bank.BANRURAL) ? 11 : 0,
                                                                     bankConfiguraion).Result;

                if(transactions.Where(x => string.IsNullOrEmpty(x.TrasactionCode)).Count() > 0)
                {
                    return EntityResponse.CreateError<BankStatementDto>($"{bankConfiguraion.Bank}: No se pudieron obtener todos los codigos de transacción.");
                }

                if (bankConfiguraion.Bank == Bank.ATLANTIDAD && transactions.Where(x => !long.TryParse(x.TrasactionCode, out _)).Count() > 0)
                {
                    return EntityResponse.CreateError<BankStatementDto>($"{bankConfiguraion.Bank}: Se encontrarón codigos de transacción inválidos.");
                }

                if (transactions.Count > 0)
                {
                    DateTime transactionDate = transactions.OrderByDescending(x => x.Date).FirstOrDefault().Date;

                    bankStatementDto.CompanyId = bankConfiguraion.CompanyId;
                    bankStatementDto.AccountId = bankConfiguraion.AccountId;
                    bankStatementDto.Account = transactions.FirstOrDefault().Account;
                    bankStatementDto.CreateDateTime = DateTime.Now;
                    bankStatementDto.TransactionDate = transactionDate;
                    bankStatementDto.Status = BankStatatementState.Pending;

                    if (bankStatementDto == null)
                    {
                        return EntityResponse.CreateError<BankStatementDto>($"{bankConfiguraion.Bank}: Los datos para crear estado de cuenta son obligatorios.");
                    }else if(bankStatementDto.Account == "")
                    {
                        return EntityResponse.CreateError<BankStatementDto>($"{bankConfiguraion.Bank}: El número de cuenta del archivo no concuerda con el número de cuenta configurado.");
                    }

                    BankStatement bankStatement = new BankStatement
                    {
                        BankStatementId = bankStatementDto.BankStatementId,
                        CompanyId = bankStatementDto.CompanyId,
                        AccountId = bankStatementDto.AccountId,
                        Account = bankStatementDto.Account,
                        TransactionDate = bankStatementDto.TransactionDate,
                        Status = BankStatatementState.Pending,
                        CreateDateTime = bankStatementDto.CreateDateTime
                    };
                    List<BankStatementDetails.BankStatementDetails> bankStatementDetails = new();
                    foreach (MT940Transaction transaction in transactions)
                    {
                        bool esInteres = false;

                        if (transaction.TrasactionCode.Equals("3Y"))
                        {
                            if (transaction.Description.Contains("INTERESES"))
                            {
                                esInteres = true;
                            }
                        }

                        bankStatementDetails.Add(new Features.Accounting.BankStatementDetails.BankStatementDetails
                        {
                            BankStatementId = bankStatement.BankStatementId,
                            TransactionDate = transaction.Date,
                            TransactionCode = esInteres ? "4Y" : transaction.TrasactionCode,
                            Description = transaction.Description,
                            Reference = transaction.Reference,
                            Amount = transaction.Amount,
                            Type = transaction.Type.Equals("C") ? TransactionType.Credit : TransactionType.Debit,
                            BankStatement = bankStatement,
                            CurrencyCode = transaction.CurrencyCode
                        });
                    }
                    _unitOfWork.Repository<BankStatement>().Add(bankStatement);
                    _unitOfWork.Repository<BankStatementDetails.BankStatementDetails>().Add(bankStatementDetails);
                    await _unitOfWork.SaveChangesAsync();
                    bankStatementDto.BankStatementId = bankStatement.BankStatementId;
                    return EntityResponse.CreateOk(bankStatementDto);
                }

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError<BankStatementDto>($"Error en método SaveTransactions:{ex.Message}");
            }

            return EntityResponse.CreateError<BankStatementDto>("Sin transacciones");
        }

        public async Task<List<BankStatementDto>> GetAll()
        {
            List<BankStatementDto> bankStatements = await (from u in _unitOfWork.Repository<BankStatement>().Query()
                                                           select new BankStatementDto
                                                           {
                                                               BankStatementId = u.BankStatementId,
                                                               CompanyId = u.CompanyId,
                                                               AccountId = u.AccountId,
                                                               Account = u.Account,
                                                               TransactionDate = u.TransactionDate,
                                                               CreateDateTime = u.CreateDateTime,
                                                               Status = u.Status
                                                           }).ToListAsync();
            return bankStatements;
        }

        public async Task<List<BankStatement>> GetByAccountId(string accountId, string date, string companyCode)
        {
            DateTime transferDate = DateTime.Parse(date);

            List<BankStatement> bankStatements = _unitOfWork.Repository<BankStatement>().Query().Where(x => x.AccountId == accountId &&
                                                                                                            x.TransactionDate.Year == transferDate.Year &&
                                                                                                            x.TransactionDate.Month == transferDate.Month &&
                                                                                                            x.TransactionDate.Date == transferDate.Date &&
                                                                                                            x.CompanyId == companyCode).ToList();
            return bankStatements;
        }

        public BankStatementDto FindById(int id)
        {
            return (from u in _unitOfWork.Repository<BankStatement>().Query()
                    where u.BankStatementId == id
                    select new BankStatementDto
                    {
                        BankStatementId = u.BankStatementId,
                        CompanyId = u.CompanyId,
                        AccountId = u.AccountId,
                        Account = u.Account,
                        TransactionDate = u.TransactionDate,
                        CreateDateTime = u.CreateDateTime,
                        Status = u.Status,
                    }).FirstOrDefault();
        }

        public async Task<List<MT940Transaction>> ReadMT940File(string filePath, int amountSubstring, BankConfiguration.BankConfiguration bankConfiguration)
        {
            try
            {
                List<MT940Transaction> transactions = new List<MT940Transaction>();
                string account = "", currencyCode = "";
                int typeSubstring = amountSubstring - 1;
                Encoding encoding = Encoding.UTF8;

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(fs, encoding))
                {
                    long currentPosition = 0;
                    MT940Transaction currentTransaction = null;

                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();

                        long lineLength = encoding.GetByteCount(line /*+ Environment.NewLine*/);
                        currentPosition += lineLength;

                        if (char.IsLetter(line[0]))
                        {
                            continue;
                        }

                        if (line.StartsWith(":60F:"))
                        {
                            currencyCode = line.Substring(12, 3);
                        }
                        if (line.StartsWith(":25:"))
                        {
                            string lineValue = line.Substring(4);
                            int startIndex = lineValue.IndexOf(bankConfiguration.AccountNumber);
                            account = startIndex != -1 ? lineValue.Substring(startIndex, bankConfiguration.AccountNumber.Length) : "" ;
                        }
                        if (line.StartsWith(":61:"))
                        {
                            currentTransaction = new MT940Transaction();
                            transactions.Add(currentTransaction);

                            string[] parts = line.Split(',');
                            if (parts.Length >= 2)
                            {
                                string amountPart = parts[0].Substring(amountSubstring);
                                string amountStr = Regex.Replace(amountPart, @"[^\d]", "");
                                decimal amount = decimal.Parse(amountStr) + decimal.Parse(parts[1].Substring(0, 2)) / 100;

                                currentTransaction.Account = account;
                                currentTransaction.CurrencyCode = currencyCode;
                                var date = "20" + parts[0].Substring(4, 6);
                                var year = int.Parse(date.Substring(0, 4));
                                var month = int.Parse(date.Substring(4, 2));
                                var day = int.Parse(date.Substring(6, 2));
                                currentTransaction.Date = new DateTime(year, month, day);
                                currentTransaction.Amount = amount;
                                currentTransaction.Type = parts[0].Substring(typeSubstring, 1);

                                int index = parts[1].IndexOf("//");

                                if (bankConfiguration.Bank == Bank.FICOHSA) 
                                {
                                    index += 2; //Se coloco 2 para no incluir los //
                                    currentTransaction.Reference = parts[1].Substring(index).Replace(" ", "");

                                }else if (bankConfiguration.Bank == Bank.BANRURAL)
                                {
                                    string reference = parts[1].Substring(0, index);
                                    currentTransaction.Reference = reference.Substring(6);
                                }
                                else
                                {
                                    currentTransaction.Reference = parts[1].Substring(/*2*/6).Replace(" ", "");
                                }
                            }

                            currentTransaction.TrasactionCode = bankConfiguration.Bank == Bank.FICOHSA ? ReadSection(reader).Trim() : null;

                            /*if (bankConfiguration.Bank == Bank.BANPAIS) //quitar if cuando Banpais haya hecho la modificación
                            {
                                currentTransaction.TrasactionCode = parts[1].Substring(3, 3);
                            }*/

                            fs.Seek(currentPosition, SeekOrigin.Begin);
                            reader.DiscardBufferedData();
                        }
                        else if (line.StartsWith(":86:"))
                        {
                            if (currentTransaction != null)
                            {
                                StringBuilder section = new();
                                section.Append(line.Substring(4));
                                section.Append(ReadSection(reader));

                                string description = section.ToString().Replace("BEN: ORD: ", "");
                                
                                currentTransaction.TrasactionCode = bankConfiguration.Bank == Bank.ATLANTIDAD ? description.Substring(0, 6) :
                                                                    bankConfiguration.Bank == Bank.BANPAIS ? GetTransactionCodeBP(description) :
                                                                    bankConfiguration.Bank == Bank.BANCO_INDUSTRIAL ? description.Substring(20, 4) :
                                                                    description.Contains("(") ? Regex.Replace(description, @"\s*\(.*?\)", "") :
                                                                    bankConfiguration.Bank == Bank.BANRURAL ? description :
                                                                    currentTransaction.TrasactionCode == null ? description.Substring(0, 2) : currentTransaction.TrasactionCode;

                                description = description.Replace("CHQ ", "");
                                description = description.Replace("NO:", "").Trim();
                                description = description.Replace("  ", " ");
                                description = description.Replace("          ", " ");

                                string newDescription = String.IsNullOrEmpty(currentTransaction.TrasactionCode) ? description : description.Replace(currentTransaction.TrasactionCode, "");
                                currentTransaction.Description = string.IsNullOrEmpty(newDescription) ? currentTransaction.TrasactionCode : newDescription;

                                /*if (bankConfiguration.Bank == Bank.BANPAIS) //usar cuando Banpais haya hecho la modificación
                                {
                                    string descriptionLine = currentTransaction.Description;

                                    int i = 0;
                                    while (i < descriptionLine.Length && char.IsDigit(descriptionLine[i]))
                                    {
                                        i++;
                                    }
                                    if (i > 0)
                                    {
                                        currentTransaction.TrasactionCode = descriptionLine.Substring(0, i);
                                    }

                                    currentTransaction.Description = (line.Substring(4).Replace("-", "")).Substring(i);
                                }*/

                                currentTransaction = null;

                                fs.Seek(currentPosition, SeekOrigin.Begin);
                                reader.DiscardBufferedData();
                            }
                        }
                    }
                }
                return transactions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return null;
            }
        }

        public string GetTransactionCodeBP(string line)
        {
            string code = "";
            try
            {
                List<TransactionCode> codes = new ();

                line = line.Substring(0, 30); 
                line = Regex.Replace(line, @"\d", "");

                string filePath = "\\\\gim-ser-finanzas\\MT940\\Codigos de Transacciones.xlsx";

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    int row = 2;
                    while (true)
                    {
                        var transactionCode = worksheet.Cells[row, 1].Text;
                        var description = worksheet.Cells[row, 2].Text;
                        var type = worksheet.Cells[row, 3].Text;

                        if (string.IsNullOrEmpty(transactionCode) && string.IsNullOrEmpty(description) && string.IsNullOrEmpty(type))
                        {
                            break;
                        }

                        codes.Add(new TransactionCode{
                            BankAccountId= Bank.BANPAIS.ToString(),
                            Code= transactionCode,
                            Description = description.Replace(" ", "").ToUpper(),
                            TransactionType = type
                        });
                        row++;
                    }
                }

                TransactionCode transactionItem = new();
                transactionItem = codes.Find(x => x.Description == line.Replace(" ", "").ToUpper());

                code = transactionItem == null ? "" : transactionItem.Code;
            }
            catch(Exception ex){
                Console.WriteLine($"Error reading file: {ex.Message}");
            }

            return code;
        }

        public static string ReadSection(StreamReader reader)
        {
            StringBuilder section = new StringBuilder();

            while (reader.Peek() >= 0)
            {
                long position = reader.BaseStream.Position; 
                string nextLine = reader.ReadLine();

                if (nextLine.StartsWith(":"))
                {
                    reader.BaseStream.Seek(position, SeekOrigin.Current);
                    break;
                }

                section.Append(" " + nextLine.Trim());
            }
            return section.ToString();
        }

        public async Task<EntityResponse> UpdateStatus(int bankStatementId, BankStatatementState status)
        {
            BankStatement bankStatement = _unitOfWork.Repository<BankStatement>().Query().Where(x => x.BankStatementId == bankStatementId).FirstOrDefault();
            bankStatement.Status = status;

            _unitOfWork.Repository<BankStatement>().Update(bankStatement);
            await _unitOfWork.SaveChangesAsync();
            return EntityResponse.CreateOk();
        }

        public async Task<EntityResponse> DeleteBankStatement(BankStatement bankStatement)
        {
            _unitOfWork.Repository<BankStatement>().Delete(bankStatement);
            await _unitOfWork.SaveChangesAsync();

            return EntityResponse.CreateOk();
        }
    }
}
