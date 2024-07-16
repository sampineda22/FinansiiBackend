using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace CRM.Features.Accounting.HostToHostBanPais
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class HostToHostbanPaisController : ControllerBase
    {
        private readonly HostToHostBanPaisServices _hostToHostBanPaisServices;

        public HostToHostbanPaisController(HostToHostBanPaisServices hostToHostBanPaisServices)
        {
            _hostToHostBanPaisServices = hostToHostBanPaisServices;
        }

        [HttpPost("GenerarEncryptarArchivo")]
        public async Task<IActionResult> GenerarEncryptarArchivo([FromBody] string json)
        {
            EntityResponse response = await _hostToHostBanPaisServices.GenerarEncryptarArchivo(json);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
