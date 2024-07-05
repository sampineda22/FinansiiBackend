using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Features.Users;
using System.Linq;
using System;
using PayWeb.Common;
using System.Threading.Tasks;

namespace CRM.Features.Credits.ReceiptBreakdown
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class ReceiptDetailBreakdownController : ControllerBase
    {
        private readonly ReceiptDetailBreakdownService _receiptBreakdownService;
        private User loggedUser = new User { };

        public ReceiptDetailBreakdownController(IHttpContextAccessor httpContextAccessor, ReceiptDetailBreakdownService receiptBreakdownService)
        {
            _receiptBreakdownService = receiptBreakdownService;

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

        [HttpGet("CreateReceiptBreakdownReports/{start}/{end}/{weekNumber}/{companyCode}/{salesAgentSelected}")]
        public async Task<IActionResult> CreateReceiptBreakdownReports(string start, string end, string weekNumber, string companyCode, string salesAgentSelected)
        {
            EntityResponse response = await _receiptBreakdownService.CreateReceiptDetailBreakdownReports(start, end, weekNumber, companyCode, salesAgentSelected);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("GetReceiptDetailBreakdown/{startDate}/{endDate}/{salesmanCode}/{companyCode}")]
        public async Task<IActionResult> GetReceiptDetailBreakdown(string startDate, string endDate, string salesmanCode, string companyCode)
        {
            EntityResponse response = await _receiptBreakdownService.GetReceiptDetailBreakdown(startDate, endDate, salesmanCode, companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("GetSalesAgents/{companyCode}")]
        public async Task<IActionResult> GetSalesmen(string companyCode)
        {
            EntityResponse response = await _receiptBreakdownService.GetSalesAgents(companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("GetFiscalYears")]
        public async Task<IActionResult> GetFiscalYears()
        {
            EntityResponse response = await _receiptBreakdownService.GetFiscalYears();
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("GetFiscalWeeks/{recId}")]
        public async Task<IActionResult> GetFiscalWeeks(string recId)
        {
            EntityResponse response = await _receiptBreakdownService.GetFiscalWeeks(recId);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
