using CRM.Features.Admin.Users;
using CRM.Features.Gira.Historical;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Gira.Approve
{
    [Route("[Controller]")]
    [ApiController]
    public class ApproveController : ControllerBase
    {
        private readonly ApproveService _approveService;
        private User loggedUser = new User { };

        public ApproveController(IHttpContextAccessor httpContextAccessor, ApproveService approveService)
        {
            _approveService = approveService;

            if (httpContextAccessor.HttpContext.User.Identity.Name != null)
            {
                var companyCode = "";
                try
                {
                    companyCode = httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "Cod_Empresa")?.Value ?? string.Empty;
                }
                catch (Exception)
                {
                    companyCode = "";
                }

                loggedUser = new User
                {
                    UserId = httpContextAccessor.HttpContext.User.Identity.Name ?? string.Empty,
                    Cod_Empresa = companyCode
                };
            }
        }

        [HttpGet("PendingApprovals/{companyCode}")]
        public async Task<IActionResult> PendingApprovals(string companyCode)
        {
            try
            {
                EntityResponse response = await _approveService.GetPendingApprovals(companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get PendingApprovals: " + ex.Message);
            }
        }

        [HttpPut("Status/{companyCode}/{id}/{personalCode}/{user}/{rejectionMotive?}")]
        public async Task<IActionResult> Status(string companyCode, int id, string personalCode, string user,string? rejectionMotive)
        {
            try
            {
                string message = String.IsNullOrEmpty(rejectionMotive) ? "aprobado" : "rechazado";
                EntityResponse response = await _approveService.UpdateStatus(companyCode, id, rejectionMotive, personalCode, user);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }

                if (response is EntityResponse<ExpenseDetail> genericResponse)
                    response.Mensaje = String.IsNullOrEmpty(rejectionMotive) ? $"El detalle del gasto ha sido aprobado exitosamente. El detalle se asignó al diario {genericResponse.Data.JournalNum}" : "El detalle del gasto ha sido rechazado";

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método put Status: " + ex.Message);
            }
        }
    }
}