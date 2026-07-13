using CRM.Features.Admin.Users;
using CRM.Features.Gira.Approve;
using CRM.Features.Gira.Historical;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Threading.Tasks;

namespace CRM.Features.Gira.ExpensesDetails
{
    [Route("[Controller]")]
    [ApiController]
    
    public class ExpenseDetailController : ControllerBase
    {
        private readonly ExpenseDetailService _expenseDetailService;
        private readonly ApproveService _approveService;

        public ExpenseDetailController(ExpenseDetailService expenseDetailService, ApproveService approveService)
        {
            _expenseDetailService = expenseDetailService;
            _approveService = approveService;
        }

        [HttpPost("ExpenseDetail/{user}")]
        public async Task<IActionResult> ExpenseDetail(string user, [FromBody] ExpenseDetail detail)
        {
            try
            {
                EntityResponse detailResponse = await _expenseDetailService.PostExpenseDetail(detail);

                if (!detailResponse.Ok)
                {
                    if (detailResponse.Mensaje.Contains("misma factura"))
                    {
                        return Conflict(detailResponse);
                    }

                    return BadRequest(detailResponse);
                }

                bool isAutoApprove = _expenseDetailService.IsAutoApprovePosition(detail.CompanyCode, detail.PersonalCode);

                if (isAutoApprove)
                {
                    if (detailResponse is EntityResponse<ExpenseDetail> genericResponse)
                    {
                        EntityResponse response = await _approveService.UpdateStatus(detail.CompanyCode, genericResponse.Data.Id, null, detail.PersonalCode, user);

                        if (!response.Ok)
                        {
                            return BadRequest(response);
                        }
                    }
                }
                return Ok(detailResponse);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post ExpenseDetail: " + ex.Message);
            }
        }

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
