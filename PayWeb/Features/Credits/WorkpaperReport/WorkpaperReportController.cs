using CRM.Features.BankStatement;
using CRM.Features.BankStatementServiceAX;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using PayWeb.Features.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Credits.ReceiptBreakdownReport
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class WorkpaperReportController : ControllerBase
    {
        private readonly WorkpaperReportService _receiptBreakdownReportService;
        private User loggedUser = new User { };

        public WorkpaperReportController(IHttpContextAccessor httpContextAccessor, WorkpaperReportService receiptBreakdownReportService)
        {
            _receiptBreakdownReportService = receiptBreakdownReportService;

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

        [HttpGet("CreateWorkpaperReports/{start}/{end}/{weekNumber}/{companyCode}/{salesAgentSelected}")]
        public async Task<IActionResult> CreateReports(string start, string end, string weekNumber, string companyCode, string salesAgentSelected)
        {
            EntityResponse response = await _receiptBreakdownReportService.CreateWorkpaperReports(start, end, weekNumber, companyCode, salesAgentSelected);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok();
        }
    }
}
