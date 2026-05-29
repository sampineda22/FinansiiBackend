using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using PayWeb.Features.Users;
using System;
using System.Linq;

namespace CRM.Features.Accounting.VendPaymentReport
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class VendPaymentReportController : ControllerBase
    {
        private readonly VendPaymentReportService _vendPaymentReportService;
        private User loggedUser = new User { };

        public VendPaymentReportController(IHttpContextAccessor httpContextAccessor, VendPaymentReportService vendPaymentReportService)
        {
            _vendPaymentReportService = vendPaymentReportService;

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

        [HttpGet("UnpostedJournals/{companyCode}")]
        public IActionResult UnpostedJournals(string companyCode)
        {
            EntityResponse response = _vendPaymentReportService.GetJournalsUnposted(companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);

        }

        [HttpGet("VendPaymentLines/{journalNum}/{companyCode}")]
        public IActionResult VendPaymentJournal(string journalNum, string companyCode)
        {
            EntityResponse response = _vendPaymentReportService.GetVendPaymentLines(journalNum, companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("Report/{journalNum}/{companyCode}/{offSetLedgerDimension}")]
        public IActionResult Report(string journalNum, string companyCode, string offSetLedgerDimension)
        {
            EntityResponse response = _vendPaymentReportService.CreateReport(journalNum, companyCode, loggedUser.UserId);
            string fileName = _vendPaymentReportService.GetFileName(journalNum, companyCode, offSetLedgerDimension);

            if (response is EntityResponse<ReportResult> genericResponse)
            {
                Response.Headers.Append("X-Report-Warnings", genericResponse.Data.Warnings ?? "");
                return File(genericResponse.Data.PdfBytes, "application/pdf", fileName + ".pdf");
            }
            else if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
