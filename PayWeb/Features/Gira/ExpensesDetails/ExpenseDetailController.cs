using CRM.Features.Gira.Historical;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Threading.Tasks;

namespace CRM.Features.Gira.ExpensesDetails
{
    [Route("[Controller]")]
    [ApiController]
    
    public class ExpenseDetailController : ControllerBase
    {
        private readonly ExpenseDetailService _expenseDetailService;

        public ExpenseDetailController(ExpenseDetailService expenseDetailService)
        {
            _expenseDetailService = expenseDetailService;
        }

        [HttpPost("ExpenseDetail")]
        public async Task<IActionResult> ExpenseDetail([FromBody] ExpenseDetail detail)
        {
            try
            {
                EntityResponse response = await _expenseDetailService.PostExpenseDetail(detail);

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

        [Authorize]
        [HttpGet("Path/{projectName}")]
        public async Task<IActionResult> Path(string projectName)
        {
            try
            {
                string response = _expenseDetailService.GetPath(projectName);

                if (string.IsNullOrEmpty(response))
                {
                    return BadRequest("No se pudo obtener la ruta en donde se guardara la información");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get Path: " + ex.Message);
            }
        }
    }
}
