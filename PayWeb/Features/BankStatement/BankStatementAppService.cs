using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CRM.Infrastructure.Enum.BankStatementStatus;

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
    }
}
