using CRM.Features.Accounting.BankStatementDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PayWeb.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
        /*Commented on 2026-feb.-18 by spineda - Begin*/
        [Consumes("multipart/form-data")]
        /*Commented on 2026-feb.-18 by spineda - End*/
        public async Task<IActionResult> GetProvidersReport(IFormFile file)
        {
            try
            {
                EntityResponse response = await _providerReportService.getProvidersReport(file);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                /*Commented on 2026-feb.-18 by spineda - Begin*/
                return BadRequest("Error al procesar el archivo");
                /*Commented on 2026-feb.-18 by spineda - End*/
            }
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