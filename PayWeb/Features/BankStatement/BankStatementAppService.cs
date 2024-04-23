using CRM.Common;
using CRM.Features.BankStatementDetails;
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

        public async Task<EntityResponse> ImportStatementFromFile()
        {
            BankStatementDto bankStatementDto = new BankStatementDto();

            // Validar configuracion por Banco
            string filePath = "C:\\Users\\LHERCULES\\Desktop\\Carga de estados de cuenta\\MT940-HN-0001.txt";
            List<MT940Transaction> transactions = ReadMT940File(filePath);
            if (transactions.Count > 0) 
            {
                bankStatementDto.Account = transactions.FirstOrDefault().Account;
                bankStatementDto.CreateDateTime = DateTime.Now;
                bankStatementDto.Description = $"Archivo {transactions.FirstOrDefault().Account} Fecha {transactions.FirstOrDefault().Date}";
                bankStatementDto.Status = BankStatatementState.Pending;
            }

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
                    TransactionCode = transaction.Description.Substring(1,2),
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
                            currentTransaction.Date = Convert.ToDateTime(parts[0].Substring(4, 6));
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
            }
            return transactions;
        }
    }
}
