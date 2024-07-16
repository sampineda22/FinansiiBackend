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
using CRM.Features.Admin.Roles;
using Microsoft.Identity.Client;
using CRM.Infrastructure.Enum;
using Microsoft.AspNetCore.Http;
using Renci.SshNet.Sftp;
using Newtonsoft.Json;
using CRM.Features.Accounting.BankStatementDetails;

namespace CRM.Features.Accounting.BankStatement
{
    public class BankStatementAppService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BankStatementDetailsAppService _bankStatementDetailsAppService;

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

        public async Task<EntityResponse> ImportStatementFromFileByAccount(string accountsIds, string dateString, string companyCode)
        {
            DateTime transactionDate = DateTime.Parse(dateString).AddDays(1);
            bool wasFound = false;
            string fileName;
            EntityResponse<BankStatementDto> response = new();
            List<BankStatementDto> bankStatementDtos = new();
            List<string> accounts = accountsIds.Split(',').ToList();
            List<string> errors = new();


            foreach (string accountId in accounts)
            {
                if (dateString != null)
                {
                    if (dateString.Replace(" ", "") != "")
                    {
                        List<BankStatement> bankStatements = await GetByAccountId(accountId, dateString, companyCode);

                        if (bankStatements.Exists(x => x.Status == BankStatatementState.Processed))
                        {
                            errors.Add($"No se puede importar las transacciones de la fecha {bankStatements.Find(x => x.Status == BankStatatementState.Processed).TransactionDate} para la cuenta {accountId} ya que se encuentran exportadas en AX.");
                            break;
                        }

                        foreach (BankStatement bankStatement in bankStatements.FindAll(x => x.Status != BankStatatementState.Processed))
                        {
                            await _bankStatementDetailsAppService.DeleteDetails(bankStatement.BankStatementId);
                            await DeleteBankStatement(bankStatement);
                        }
                    }
                }

                var bankConfiguraion = _unitOfWork.Repository<BankConfiguration.BankConfiguration>().Query().FirstOrDefault(b => b.AccountId.Equals(accountId));
                if (bankConfiguraion == null)
                {
                    errors.Add($"No se encontro una configuracion para la cuenta {accountId}.");
                    break;
                }

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
                                response = IterateFilesAndSaveTransactions(files, transactionDate, bankConfiguraion, client, bankConfiguraion.FileName.Length, bankConfiguraion.FileName, $" - {transactionDate.Year}-{transactionDate.Month}-{transactionDate.Day}.txt").Result;
                            }

                            if (!response.Ok)
                            {
                                errors.Add(response.Mensaje);
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
                        return EntityResponse.CreateError($"Error al acceder al sftp:  {ex.Message}.");
                    }
                }
            }

            if (errors.Count > 0)
            {
                string error = $"Se generaron los siguientes errores: {string.Join(", ", errors)}. ";
                error += bankStatementDtos.Count > 0 ? $"Se generaron los BankStatements con los Ids:{string.Join(", ", bankStatementDtos.Select(p => p.BankStatementId))}" : "";
                return EntityResponse.CreateError(error);
            }

            return EntityResponse.CreateOk(bankStatementDtos);
        }

        public async Task<EntityResponse<BankStatementDto>> IterateFilesAndSaveTransactions(IEnumerable<ISftpFile> files, DateTime transactionDate, BankConfiguration.BankConfiguration bankConfiguration, SftpClient client, int trimEnd, string fileName, string nameExtension)
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

        public string getFilePath(string baseFileName, string serverPath)
        {
            string[] files = Directory.GetFiles(serverPath);
            string baseFileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);

            foreach (string file in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                string fileNameTrimmed = fileNameWithoutExtension.Substring(0, 47).Trim();

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
                                                                    bankConfiguraion.Bank == Bank.BAC ? 15 :
                                                                    bankConfiguraion.Bank == Bank.BANPAIS ? 11 : 0,
                                                                    bankConfiguraion);
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
                        if (transaction.Description.Substring(0, 2).Equals("3Y"))
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
        private List<MT940Transaction> ReadMT940File(string filePath, int amountSubstring, BankConfiguration.BankConfiguration bankConfiguration)
        {
            List<MT940Transaction> transactions = new List<MT940Transaction>();
            string account = "";
            string currencyCode = "";
            int typeSubstring = amountSubstring - 1;
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                MT940Transaction currentTransaction = null;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(":60F:"))
                    {
                        currencyCode = line.Substring(12, 3);
                    }
                    if (line.StartsWith(":25:"))
                    {
                        account = line.Substring(4);
                    }
                    if (line.StartsWith(":61:"))
                    {
                        currentTransaction = new MT940Transaction();
                        transactions.Add(currentTransaction);

                        string[] parts = line.Split(',');
                        if (parts.Length >= 2)
                        {
                            decimal amount = decimal.Parse(parts[0].Substring(amountSubstring)) + decimal.Parse(parts[1].Substring(0, 2)) / 100;

                            currentTransaction.Account = account;
                            currentTransaction.CurrencyCode = currencyCode;
                            var date = "20" + parts[0].Substring(4, 6);
                            var year = int.Parse(date.Substring(0, 4));
                            var month = int.Parse(date.Substring(4, 2));
                            var day = int.Parse(date.Substring(6, 2));
                            currentTransaction.Date = new DateTime(year, month, day);
                            currentTransaction.Amount = amount;
                            currentTransaction.Type = parts[0].Substring(typeSubstring, 1);
                            currentTransaction.Reference = parts[1].Substring(2).Replace(" ", "");
                        }

                        if (bankConfiguration.Bank == Bank.BANPAIS)
                        {
                            currentTransaction.TrasactionCode = parts[1].Substring(3, 3);
                        }
                    }
                    else if (line.StartsWith(":86:"))
                    {
                        if (currentTransaction != null)
                        {
                            currentTransaction.Description = line.Substring(4);
                            currentTransaction.TrasactionCode ??= currentTransaction.Description.Substring(0, 2);
                            currentTransaction = null;
                        }
                    }
                }
                reader.Close();
            }
            return transactions;
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
