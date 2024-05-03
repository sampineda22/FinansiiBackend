using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CRM.Infrastructure.Enum.TransactionsType;

namespace CRM.Features.BankStatementDetails
{
    public class BankStatementDetailsAppService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankStatementDetailsAppService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public async Task<EntityResponse> AddBankStatementDetailsAsync(BankStatementDetailsDto bankStatementDetailsDto)
        {
            if (bankStatementDetailsDto == null)
            {
                return EntityResponse.CreateError("Los datos para crear el detalle del estado de cuenta son obligatorios.");
            }

            BankStatementDetails bankStatementDetails = new BankStatementDetails
            {
                BankStatementDetailId = bankStatementDetailsDto.BankStatementDetailId,
                BankStatementId = bankStatementDetailsDto.BankStatementId,
                CurrencyCode = bankStatementDetailsDto.CurrencyCode,
                TransactionDate = bankStatementDetailsDto.TransactionDate,
                TransactionCode = bankStatementDetailsDto.TransactionCode,
                Description = bankStatementDetailsDto.Description,
                Reference = bankStatementDetailsDto.Reference,
                Amount = bankStatementDetailsDto.Amount,
                Type = bankStatementDetailsDto.Type
            };

            _unitOfWork.Repository<BankStatementDetails>().Add(bankStatementDetails);
            await _unitOfWork.SaveChangesAsync();
            bankStatementDetailsDto.BankStatementId = bankStatementDetails.BankStatementDetailId;
            return EntityResponse.CreateOk();
        }

        public async Task<List<BankStatementDetailsDto>> GetAll()
        {
            List<BankStatementDetailsDto> bankStatementsDetails = await (from u in _unitOfWork.Repository<BankStatementDetails>().Query()
                                                           select new BankStatementDetailsDto
                                                           {
                                                               BankStatementDetailId = u.BankStatementDetailId,
                                                               BankStatementId = u.BankStatementId,
                                                               CurrencyCode = u.CurrencyCode,
                                                               TransactionDate = u.TransactionDate,
                                                               TransactionCode = u.TransactionCode,
                                                               Description = u.Description,
                                                               Reference = u.Reference,
                                                               Amount = u.Amount,
                                                               Type = u.Type
                                                           }).ToListAsync();
            return bankStatementsDetails;
        }

        public async Task<List<BankStatementDetailsDto>> GetAllByBankStatement(int id)
        {
            List<BankStatementDetailsDto> bankStatementsDetails = await (from u in _unitOfWork.Repository<BankStatementDetails>().Query()
                                                                         where u.BankStatementId == id
                                                                         select new BankStatementDetailsDto
                                                                         {
                                                                             BankStatementDetailId = u.BankStatementDetailId,
                                                                             BankStatementId = u.BankStatementId,
                                                                             CurrencyCode = u.CurrencyCode,
                                                                             TransactionDate = u.TransactionDate,
                                                                             TransactionCode = u.TransactionCode,
                                                                             Description = u.Description,
                                                                             Reference = u.Reference,
                                                                             Amount = u.Amount,
                                                                             Type = u.Type
                                                                         }).ToListAsync();
            return bankStatementsDetails;
        }
        public BankStatementDetailsDto FindById(int id)
        {
            return (from u in _unitOfWork.Repository<BankStatementDetails>().Query()
                    where u.BankStatementDetailId == id
                    select new BankStatementDetailsDto
                    {
                        BankStatementDetailId = u.BankStatementDetailId,
                        BankStatementId = u.BankStatementId,
                        CurrencyCode = u.CurrencyCode,
                        TransactionDate = u.TransactionDate,
                        TransactionCode = u.TransactionCode,
                        Description = u.Description,
                        Reference = u.Reference,
                        Amount = u.Amount,
                        Type = u.Type
                    }).FirstOrDefault();
        }

        public async Task<EntityResponse> DeleteDetails(int bankStatementId)
        {
            List<BankStatementDetailsDto> details = await GetAllByBankStatement(bankStatementId);

            var detailsToDelete = details.Select(detailsDto => new BankStatementDetails
            {
                BankStatementDetailId = detailsDto.BankStatementDetailId,
                BankStatementId = detailsDto.BankStatementId,
                CurrencyCode = detailsDto.CurrencyCode,
                TransactionDate = detailsDto.TransactionDate,
                TransactionCode = detailsDto.TransactionCode,
                Description = detailsDto.Description,
                Reference = detailsDto.Reference,
                Amount = detailsDto.Amount,
                Type = detailsDto.Type
            }).ToList();

            _unitOfWork.Repository<BankStatementDetails>().DeleteRange(detailsToDelete);
            await _unitOfWork.SaveChangesAsync();

            return EntityResponse.CreateOk();
        }
    }
}
