using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Features.Users;
using System.Linq;
using System;
using PayWeb.Common;
using System.Threading.Tasks;
using System.IO;

namespace CRM.Features.Accounting.CD
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class CertificateDepositController : ControllerBase
    {
        private readonly CertificateDepositService _certificateDepositService;
        private User loggedUser = new User { };

        public CertificateDepositController(IHttpContextAccessor httpContextAccessor, CertificateDepositService certificateDepositService)
        {
            _certificateDepositService = certificateDepositService;

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

        [HttpGet("GetActiveCDBanks")]
        public async Task<IActionResult> GetActiveCDBanks()
        {
            EntityResponse response = await _certificateDepositService.GetActiveCDBanks(loggedUser.Cod_Empresa);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("GetCertificatesDeposit/{companyCode}")]
        public async Task<IActionResult> GetCertificatesDeposit(string companyCode)
        {
            EntityResponse response = await _certificateDepositService.GetCertificatesDeposit(companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("GetCertificateWeeklyDetails/{id}")]
        public async Task<IActionResult> GetCertificateWeeklyDetails(int id)
        {
            EntityResponse response = await _certificateDepositService.GetCertificateWeeklyDetails(id);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("GetAllBanks/{companyCode}")]
        public async Task<IActionResult> GetAllBanks(string companyCode)
        {
            EntityResponse response = await _certificateDepositService.GetAllBanks(companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("PostCertificateDeposit")]
        public async Task<IActionResult> PostCertificateDeposit([FromBody] CertificateDeposit certificate)
        {
            EntityResponse response = await _certificateDepositService.PostCertificateDeposit(certificate);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("PostWeeklyJournal/{companyCode}/{fiscalYearId}/{week}")]
        public async Task<IActionResult> PostWeeklyJournal(string companyCode, string fiscalYearId, string week)
        {
            EntityResponse response = await _certificateDepositService.PostWeeklyJournal(companyCode, fiscalYearId, week);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("PostFinalJournal/{companyCode}/{certificateId}")]
        public async Task<IActionResult> PostFinalJournal(string companyCode, int certificateId)
        {
            EntityResponse response = await _certificateDepositService.PostFinalJournal(companyCode, certificateId);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("DeleteCertificate/{id}")]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            EntityResponse response = await _certificateDepositService.DeleteCertificate(id);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("DownloadExcel/{companyCode}")]
        public IActionResult DownloadExcele(string companyCode)
        {
            EntityResponse response = _certificateDepositService.downloadExcel(companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }

            if (response is EntityResponse<MemoryStream> genericResponse)
            {
                return File(genericResponse.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "");
            }

            return BadRequest("No se pudo generar el archivo para su descarga.");
        }

    }
}
