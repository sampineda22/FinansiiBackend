using CRM.Features.Gira.Historical;
using CRM.Infrastructure.Core;
using CRM.Infrastructure.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Gira.AXExpenses
{
    public class AXExpensesService
    {
        private readonly IUnitOfWorkGira _unitOfWorkGira;
        private readonly IUnitOfWork _unitOfWork;

        public AXExpensesService(IUnitOfWorkGira unitOfWorkGira, IUnitOfWork unitOfWork)
        {
            _unitOfWorkGira = unitOfWorkGira;
            _unitOfWork = unitOfWork;
        }

        public async Task<EntityResponse> GetAXExpenses(string companyCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@companyCode", companyCode),
                    new SqlParameter("@code", "A")
                };

                List<ExpenseDetailDto> details = _unitOfWork.Repository<ExpenseDetailDto>().GetSP<ExpenseDetailDto>("[Gira].[GetExpensesDetails]", parameters).ToList();
                details = details.Where(x => x.JournalNum != "" || x.JournalNum != null).ToList();

                return EntityResponse.CreateOk(details);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetAXExpenses: " + ex.Message);
            }
        }

        public async Task<EntityResponse> UpdateStatus(string companyCode, int id, string rejectionMotive, string personalCode)
        {
            EntityResponse entityResponse = new();

            try
            {
                ExpenseDetail detail = _unitOfWorkGira.Repository<ExpenseDetail>().Query().Where(x => x.CompanyCode == companyCode && x.Id == id).FirstOrDefault();

                if (detail.InUse)
                {
                    return EntityResponse.CreateError("El detalle del gasto esta en uso. Favor esperar a que se deje de utilizar para poder aprobar/rechazar.");
                }

                Status status = _unitOfWorkGira.Repository<Status>().Query().Where(x => x.Code == ExpensesStatus.Status.RECHAZADO.ToString()).FirstOrDefault();

                detail.StatusId = status.Id;
                detail.PersonalCodeAdmin = personalCode;
                detail.RejectionMotive = rejectionMotive;

                _unitOfWorkGira.Repository<ExpenseDetail>().Update(detail);
                await _unitOfWorkGira.SaveChangesAsync();

                return EntityResponse.CreateOk(detail);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en UpdateStatus: " + ex.Message);
            }
        }
    }
}