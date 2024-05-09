using CRM.Common;
using CRM.Features.BankConfiguration;
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
using CRM.Features.BankStatementDetails;
using Microsoft.AspNetCore.Http;

namespace CRM.Features.BankStatement
{
    public class BankStatementAppService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BankStatementDetailsAppService _bankStatementDetailsAppService;

        public BankStatementAppService(IUnitOfWork unitOfWork, BankStatementDetailsAppService bankStatementDetailsAppService)
        {
            this._unitOfWork = unitOfWork;
            this._bankStatementDetailsAppService = bankStatementDetailsAppService;
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

        public async Task<EntityResponse> ImportStatementFromFileByAccount(string accountId, string dateString, string companyCode)
        {
            DateTime transactionDate = DateTime.Now;
            bool wasFound = false;
            string fileName, ruta;

            if (dateString != null)
            {
                if(dateString.Replace(" ", "") != "")
                {
                    transactionDate= DateTime.Parse(dateString);
                    List<BankStatement> bankStatements = await GetByAccountId(accountId, dateString, companyCode);

                    foreach (BankStatement bankStatement in bankStatements.FindAll(x => x.Status != BankStatatementState.Processed))
                    {
                        await _bankStatementDetailsAppService.DeleteDetails(bankStatement.BankStatementId);
                        await DeleteBankStatement(bankStatement);
                    }
                }
            }

            var bankConfiguraion = _unitOfWork.Repository<CRM.Features.BankConfiguration.BankConfiguration>().Query().FirstOrDefault(b => b.AccountId.Equals(accountId));
            if (bankConfiguraion == null)
            {
                return EntityResponse.CreateError($"No se encontro una configuracion para la cuenta {accountId}.");
            }
            BankStatementDto bankStatementDto = new BankStatementDto();
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
                            foreach (var file in files)
                            {
                                if (!file.IsDirectory)
                                {
                                    string fileFromSftp = file.Name.Substring(0, 47).Trim();
                                    ruta = @""+ bankConfiguraion.LocalFileRoute + file.Name + ".txt";

                                    if (fileFromSftp.Equals(fileName))
                                    {
                                        wasFound = true;
                                        using (var fileStream = System.IO.File.Create(ruta))
                                        {
                                            client.DownloadFile(file.FullName, fileStream);
                                            fileStream.Close();
                                            return await SaveTransactions(ruta, bankConfiguraion);
                                        }
                                    }
                                }
                            }

                            if (!wasFound)
                            {
                                ruta = getFilePath(fileName, bankConfiguraion.LocalFileRoute);
                                return await SaveTransactions(ruta, bankConfiguraion);
                            }
                        }
                    }
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    client.Dispose();
                    client.Disconnect();
                    return EntityResponse.CreateError($"Error al acceder al sftp:  {ex.Message}.");
                }
            }
            return EntityResponse.CreateOk();
        }

        public string getFilePath(string baseFileName, string serverPath)
        {
            string[] files = Directory.GetFiles(serverPath);

            foreach (string file in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                string fileNameTrimmed = fileNameWithoutExtension.Substring(0, 47).Trim();

                if (fileNameTrimmed.Equals(baseFileName))
                {
                    return file;
                }
            }

            return "";
        }

        public async Task<EntityResponse> SaveTransactions(string path, CRM.Features.BankConfiguration.BankConfiguration bankConfiguraion)
        {
            BankStatementDto bankStatementDto = new BankStatementDto();
            List<MT940Transaction> transactions = ReadMT940File(path);
            if (transactions.Count > 0)
            {
                bankStatementDto.CompanyId = bankConfiguraion.CompanyId;
                bankStatementDto.AccountId = bankConfiguraion.AccountId;
                bankStatementDto.Account = transactions.FirstOrDefault().Account;
                bankStatementDto.CreateDateTime = DateTime.Now;
                bankStatementDto.TransactionDate = transactions[0].Date;
                bankStatementDto.Status = BankStatatementState.Pending;

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
                List<CRM.Features.BankStatementDetails.BankStatementDetails> bankStatementDetails = new List<CRM.Features.BankStatementDetails.BankStatementDetails>();
                foreach (MT940Transaction transaction in transactions)
                {
                    bool esInteres = false;
                    if(transaction.Description.Substring(0, 2).Equals("3Y"))
                    {
                        if (transaction.Description.Contains("INTERESES"))
                        {
                            esInteres = true;
                        }
                    }

                    bankStatementDetails.Add(new CRM.Features.BankStatementDetails.BankStatementDetails
                    {
                        BankStatementId = bankStatement.BankStatementId,
                        TransactionDate = transaction.Date,
                        TransactionCode = esInteres ? "4Y" : transaction.Description.Substring(0, 2),
                        Description = transaction.Description,
                        Reference = transaction.Reference,
                        Amount = transaction.Amount,
                        Type = transaction.Type.Equals("C") ? TransactionType.Credit : TransactionType.Debit,
                        BankStatement = bankStatement,
                        CurrencyCode = transaction.CurrencyCode
                    });
                }
                _unitOfWork.Repository<BankStatement>().Add(bankStatement);
                _unitOfWork.Repository<CRM.Features.BankStatementDetails.BankStatementDetails>().Add(bankStatementDetails);
                await _unitOfWork.SaveChangesAsync();
                bankStatementDto.BankStatementId = bankStatement.BankStatementId;
                return EntityResponse.CreateOk(bankStatementDto);
            }
            return EntityResponse.CreateOk("Sin transacciones");
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
        private List<MT940Transaction> ReadMT940File(string filePath) 
        {
            List<MT940Transaction> transactions = new List<MT940Transaction>();
            string account = "";
            string currencyCode = "";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                MT940Transaction currentTransaction = null;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(":60F:"))
                    {
                        currencyCode = line.Substring(12,3);
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
                            currentTransaction.Account = account;
                            currentTransaction.CurrencyCode = currencyCode;
                            var date = "20" + parts[0].Substring(4, 6);
                            var year = int.Parse(date.Substring(0, 4));
                            var month = int.Parse(date.Substring(4, 2));
                            var day = int.Parse(date.Substring(6, 2));
                            currentTransaction.Date = new DateTime(year, month, day);
                            currentTransaction.Amount = decimal.Parse(parts[0].Substring(15));
                            currentTransaction.Type = parts[0].Substring(14, 1);
                            currentTransaction.Reference = parts[1].ToString();
                        }
                    }
                    else if (line.StartsWith(":86:"))
                    {
                        if (currentTransaction != null)
                        {
                            currentTransaction.Description = line.Substring(4);
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
