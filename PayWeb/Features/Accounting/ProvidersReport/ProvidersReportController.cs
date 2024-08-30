using CRM.Features.Accounting.BankStatementDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using PayWeb.Common;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CRM.Features.Accounting.ProvidersReport
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class ProvidersReportController : ControllerBase
    {
        private readonly ProvidersReportService _providerReportService;

        public ProvidersReportController(ProvidersReportService providerReportService)
        {
            _providerReportService = providerReportService;
        }

        [HttpPost("GetProvidersReport")]
        public async Task<IActionResult> GetProvidersReport(IFormFile file)
        {
            EntityResponse response = await _providerReportService.getProvidersReport(file);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("DownloadProvidersReport")]
        public IActionResult DonwloadProvidersReport([FromBody] List<ProviderReportDto> providers)
        {
            EntityResponse response = _providerReportService.downloadProvidersReport(providers);
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