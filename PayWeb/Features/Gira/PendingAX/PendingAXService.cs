using CRM.Features.Gira.Approve;
using CRM.Features.Gira.Historical;
using CRM.Infrastructure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Reporting.Map.WebForms.BingMaps;
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
        private readonly ApproveService _approveService;
        private readonly IUnitOfWorkGira _unitOfWorkGira;

        public PendingAXService(IUnitOfWork unitOfWork, ApproveService approveService, IUnitOfWorkGira unitOfWorkGira)
        {
            _unitOfWork = unitOfWork;
            _approveService = approveService;
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

        public async Task<EntityResponse> PostPendingAX(string companyCode, string personalCode)
        {
            try
            {
                EntityResponse response = new();
                List<ExpenseDetailDto> pendingDetails = new();
                List<string> errors = new();

                response = await GetPendingAXByUser(companyCode, personalCode);
                if (!response.Ok)
                {
                    return EntityResponse.CreateError("No se pudieron obtener los gastos pendientes de sincronizar.");
                }

                if (response is EntityResponse<List<ExpenseDetailDto>> genericResponse)
                {
                    pendingDetails = genericResponse.Data;
                }

                foreach(ExpenseDetailDto detail in pendingDetails)
                {
                    response = await _approveService.UpdateStatus(companyCode, detail.Id, detail.RejectionMotive, detail.PersonalCode);

                    if (!response.Ok)
                    {
                        errors.Add($"Error en sincronización de la factura {detail.InvoiceId} del proveedor {detail.VendAccount}: {response.Mensaje}");
                    }
                }

                if (errors.Count > 0)
                {
                    return EntityResponse.CreateError(String.Join(',', errors));
                }

                int syncDetails = pendingDetails.Count() - errors.Count();
                return EntityResponse.CreateOk($"Se sincronizaron {syncDetails} gastos exitosamente.");
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostPendingAX: " + ex.Message);
            }
        }
    }
}
