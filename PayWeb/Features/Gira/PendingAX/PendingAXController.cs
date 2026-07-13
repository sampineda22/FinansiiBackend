using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Threading.Tasks;

namespace CRM.Features.Gira.PendingAX
{
    [Route("[Controller]")]
    [ApiController]
    
    public class PendingAXController : ControllerBase
    {
        private readonly PendingAXService _pendingAXService;

        public PendingAXController(PendingAXService pendingAXService)
        {
            _pendingAXService = pendingAXService;
        }

        [Authorize]
        [HttpGet("PendingAX/{companyCode}")]
        public async Task<IActionResult> PendingAX(string companyCode)
        {
            try
            {
                EntityResponse response = await _pendingAXService.GetPendingAX(companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get PendingAX: " + ex.Message);
            }
        }

        [HttpGet("PendingAXByUser/{companyCode}/{personalCode}")]
        public async Task<IActionResult> PendingAXByUser(string companyCode, string personalCode)
        {
            try
            {
                EntityResponse response = await _pendingAXService.GetPendingAXByUser(companyCode, personalCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get PendingAXByUser: " + ex.Message);
            }
        }

        [HttpPost("PendingAX/{companyCode}/{personalCode}")]
        public async Task<IActionResult> PendingAX(string companyCode, string personalCode)
        {
            try
            {
                EntityResponse response = await _pendingAXService.PostPendingAX(companyCode, personalCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get PendingAX: " + ex.Message);
            }
        }
    }
}
