﻿using CRM.Common;
using CRM.Features.BankStatementDetails;
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

namespace CRM.Features.BankStatement
{
    public class BankStatementAppService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankStatementAppService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
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
                Account = bankStatementDto.Account,
                Description = bankStatementDto.Description,
                Status = BankStatatementState.Pending,
                CreateDateTime = bankStatementDto.CreateDateTime
            };

            _unitOfWork.Repository<BankStatement>().Add(bankStatement);
            await _unitOfWork.SaveChangesAsync();
            bankStatementDto.BankStatementId = bankStatement.BankStatementId;
            return EntityResponse.CreateOk();
        }

        public async Task<EntityResponse> ImportStatementFromFileByAccount(string accountId)
        {
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
                            string year = DateTime.Now.Year.ToString();
                            string month = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
                            string day = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
                            string date = year + "-" + month + "-" + day;
                            string fileName = (bankConfiguraion.FileName + bankConfiguraion.AccountNumber + '-' + date).Trim();
                            foreach (var file in files)
                            {
                                if (!file.IsDirectory)
                                {
                                    string fileFromSftp = file.Name.Substring(0, 47).Trim();
                                    string ruta = @""+ bankConfiguraion.LocalFileRoute + file.Name + ".txt";
                                    if (fileFromSftp.Equals(fileName))
                                    {
                                        using (var fileStream = System.IO.File.Create(ruta))
                                        {
                                            client.DownloadFile(file.FullName, fileStream);
                                            fileStream.Close();

                                            List<MT940Transaction> transactions = ReadMT940File(ruta);
                                            if (transactions.Count > 0)
                                            {
                                                bankStatementDto.Account = transactions.FirstOrDefault().Account;
                                                bankStatementDto.CreateDateTime = DateTime.Now;
                                                bankStatementDto.Description = $"Archivo {transactions.FirstOrDefault().Account} Fecha {transactions.FirstOrDefault().Date}";
                                                bankStatementDto.Status = BankStatatementState.Pending;

                                                if (bankStatementDto == null)
                                                {
                                                    return EntityResponse.CreateError("Los datos para crear estado de cuenta son obligatorios.");
                                                }

                                                BankStatement bankStatement = new BankStatement
                                                {
                                                    BankStatementId = bankStatementDto.BankStatementId,
                                                    Account = bankStatementDto.Account,
                                                    Description = bankStatementDto.Description,
                                                    Status = BankStatatementState.Pending,
                                                    CreateDateTime = bankStatementDto.CreateDateTime
                                                };
                                                List<CRM.Features.BankStatementDetails.BankStatementDetails> bankStatementDetails = new List<CRM.Features.BankStatementDetails.BankStatementDetails>();
                                                foreach (MT940Transaction transaction in transactions)
                                                {
                                                    bankStatementDetails.Add(new CRM.Features.BankStatementDetails.BankStatementDetails
                                                    {
                                                        BankStatementId = bankStatement.BankStatementId,
                                                        TransactionDate = transaction.Date,
                                                        TransactionCode = transaction.Description.Substring(0, 2),
                                                        Description = transaction.Description,
                                                        Reference = transaction.Reference,
                                                        Amount = transaction.Amount,
                                                        Type = transaction.Type.Equals("C") ? TransactionType.Credit : TransactionType.Debit,
                                                        BankStatement = bankStatement
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
                                    }
                                }
                            }
                        }
                    }
                    
                    client.Dispose();
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

        public async Task<List<BankStatementDto>> GetAll()
        {
            List<BankStatementDto> bankStatements = await (from u in _unitOfWork.Repository<BankStatement>().Query()
                                                            select new BankStatementDto
                                                            {
                                                                BankStatementId = u.BankStatementId,
                                                                Account = u.Account,
                                                                Description = u.Description,
                                                                CreateDateTime = u.CreateDateTime,
                                                                Status = u.Status
                                                            }).ToListAsync();
            return bankStatements;
        }

        public BankStatementDto FindById(int id)
        {
            return (from u in _unitOfWork.Repository<BankStatement>().Query()
                    where u.BankStatementId == id
                    select new BankStatementDto
                    {
                        BankStatementId = u.BankStatementId,
                        Account = u.Account,
                        Description = u.Description,
                        CreateDateTime = u.CreateDateTime,
                        Status = u.Status,
                    }).FirstOrDefault();
        }
        private List<MT940Transaction> ReadMT940File(string filePath) 
        {
            List<MT940Transaction> transactions = new List<MT940Transaction>();
            string account = "";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                MT940Transaction currentTransaction = null;

                while ((line = reader.ReadLine()) != null)
                {
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
                        }
                    }
                }
                reader.Close();
            }
            return transactions;
        }
    }
}
