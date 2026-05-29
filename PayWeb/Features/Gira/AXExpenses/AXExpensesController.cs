using CRM.Features.Admin.Users;
using CRM.Features.Gira.Approve;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Gira.AXExpenses
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class AXExpensesController : ControllerBase
    {
        private readonly AXExpensesService _axExpensesService;
        private User loggedUser = new User { };

        public AXExpensesController(IHttpContextAccessor httpContextAccessor, AXExpensesService axExpensesService)
        {
            _axExpensesService = axExpensesService;

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

        [HttpGet("AXExpenses/{companyCode}")]
        public async Task<IActionResult> AXExpenses(string companyCode)
        {
            try
            {
                EntityResponse response = await _axExpensesService.GetAXExpenses(companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get AXExpenses: " + ex.Message);
            }
        }

        [HttpPut("Status/{companyCode}/{id}/{rejectionMotive}")]
        public async Task<IActionResult> Status(string companyCode, int id, string rejectionMotive)
        {
            try
            {
                string message = "rechazado";
                EntityResponse response = await _axExpensesService.UpdateStatus(companyCode, id, rejectionMotive, loggedUser.PersonalCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }

                response.Mensaje = message;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método put Status: " + ex.Message);
            }
        }
    }
}
