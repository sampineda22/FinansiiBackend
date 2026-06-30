using CRM.Features.Gira.ExpensesDetails;
using CRM.Features.Gira.Historical;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Threading.Tasks;

namespace CRM.Features.Gira.Vendors
{
    public class VendorController : ControllerBase
    {
        private readonly VendorService _vendorService;

        public VendorController(VendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpPost("Vendor")]
        public async Task<IActionResult> Vendor()
        {
            try
            {
                EntityResponse response = await _vendorService.SendEmailNewVendor("IMHN");

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
