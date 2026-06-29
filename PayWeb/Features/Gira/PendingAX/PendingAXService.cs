using CRM.Features.Gira.Historical;
using CRM.Infrastructure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Gira.PendingAX
{
    public class PendingAXService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EVAConnectionSettings _evaConnectionSettings;

        public PendingAXService(IUnitOfWork unitOfWork, IOptions<EVAConnectionSettings> evaConnectionSettings)
        {
            _unitOfWork = unitOfWork;
            _evaConnectionSettings = evaConnectionSettings.Value;
        }

        public async Task<EntityResponse> GetPendingAX(string companyCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@companyCode", companyCode),
                    new SqlParameter("@code", "PAX")
                };

                List<ExpenseDetailDto> details = _unitOfWork.Repository<ExpenseDetailDto>().GetSP<ExpenseDetailDto>("[Gira].[GetExpensesDetails]", parameters).ToList();

                return EntityResponse.CreateOk(details);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetPendingApprovals: " + ex.Message);
            }
        }

        public async Task<EntityResponse> GetPendingAXByUser(string companyCode, string personalCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@companyCode", companyCode),
                    new SqlParameter("@code", "PAX")
                };

                List<ExpenseDetailDto> details = _unitOfWork.Repository<ExpenseDetailDto>().GetSP<ExpenseDetailDto>("[Gira].[GetExpensesDetails]", parameters).ToList();
                details = details.FindAll(x => x.PersonalCode == personalCode);

                return EntityResponse.CreateOk(details);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetPendingApprovals: " + ex.Message);
            }
        }
    }
}
