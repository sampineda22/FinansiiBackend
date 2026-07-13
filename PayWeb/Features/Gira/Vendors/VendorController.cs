using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Threading.Tasks;

namespace CRM.Features.Gira.Vendors
{
    [Route("[Controller]")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly VendorService _vendorService;

        public VendorController(VendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpPost("EmailNewVendor/{companyCode}")]
        public async Task<IActionResult> EmailNewVendor(string companyCode, [FromBody] VendorRequest vendorRequest)
        {
            try
            {
                EntityResponse response = await _vendorService.SendEmailNewVendor(companyCode, vendorRequest);

                if (!response.Ok)
                {
                    if (response.Mensaje.Contains("misma factura"))
                    {
                        return Conflict(response);
                    }

                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post ExpenseDetail: " + ex.Message);
            }
        }
    }
}
