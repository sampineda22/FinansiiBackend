using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CRM.Features.Gira.Historical
{
    [Route("[Controller]")]
    [ApiController]

    public class HistoricalController : ControllerBase
    {
        private readonly HistoricalService _historicalService;

        public HistoricalController(HistoricalService historicalService)
        {
            _historicalService = historicalService;
        }

        [HttpGet("HistoricalDetails/{companyCode}/{expenseType}/{startDate}/{endDate}")]
        public async Task<IActionResult> HistoricalDetails(string companyCode, int expenseType, DateTime startDate, DateTime endDate, [FromQuery] string? personalCode)
        {
            try
            {
                EntityResponse response = await _historicalService.GetHistoricalDetails(companyCode, personalCode, expenseType, startDate, endDate.Date/*, true*/);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get HistoricalDetails: " + ex.Message);
            }
        }

        [HttpGet("HistoricalDetailById/{id}")]
        public async Task<IActionResult> HistoricalDetailById(int id)
        {
            try
            {
                EntityResponse response = await _historicalService.GetHistoricalDetailById(id);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get HistoricalDetailById: " + ex.Message);
            }
        }

        [Authorize]
        [HttpGet("DownloadExcel/{companyCode}/{salesAgent}/{expenseType}/{startDate}/{endDate}")]
        public IActionResult DownloadExcel(string companyCode, string salesAgent, int expenseType, DateTime startDate, DateTime endDate)
        {
            try
            {
                EntityResponse response = _historicalService.DownloadExcel(companyCode, salesAgent, expenseType, startDate, endDate);
                if (!response.Ok)
                {
                    return BadRequest(response);
                }

                if (response is EntityResponse<(byte[], string, string)> genericResponse)
                {
                    return File(genericResponse.Data.Item1, genericResponse.Data.Item2, genericResponse.Data.Item3);
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get HistoricalDetails: " + ex.Message);
            }

            return BadRequest("No se pudo generar el archivo para su descarga.");
        }

        [HttpGet("Image/{id}/{companyCode}")]
        public IActionResult Image(int id, string companyCode)
        {
            try
            {
                var response = _historicalService.GetImage(id, companyCode).Result;

                if (!response.Ok)
                {
                    if (response.Mensaje.Contains("imagen"))
                    {
                        return NotFound(new { message = response.Mensaje });
                    }
                    else
                    {
                        return BadRequest(new { message = response.Mensaje });
                    }
                }

                if (response is EntityResponse<string> genericResponse)
                {
                    var imageUrl = genericResponse.Data.ToString();

                    using var httpClient = new HttpClient();
                    var imageBytes = httpClient.GetByteArrayAsync(imageUrl).Result;

                    var extension = Path.GetExtension(imageUrl)?.ToLower();
                    var contentType = extension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        _ => "application/octet-stream"
                    };

                    return File(imageBytes, contentType, "imagen.jpg");
                }

                return BadRequest(new { message = "No se pudo obtener la URL de la imagen." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error en método get HistoricalDetails: " + ex.Message });
            }
        }
    }
}
