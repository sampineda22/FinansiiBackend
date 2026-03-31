using CRM.Migrations.Gira;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Threading.Tasks;

namespace CRM.Features.Gira.ExpensesSettings
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class ExpensesSettingsController : ControllerBase
    {
        private readonly ExpensesSettingsService _expensesSettingsService;

        public ExpensesSettingsController(ExpensesSettingsService expensesSettingsService)
        {
            _expensesSettingsService = expensesSettingsService;
        }

        #region ExpensesTypes
        [HttpGet("GetExpensesTypes/{companyCode}")]
        public async Task<IActionResult> GetExpensesTypes(string companyCode)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.GetExpensesType(companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método GetExpensesTypes: " + ex.Message);
            }
        }

        [HttpPost("ExpenseType/{companyCode}")]
        public async Task<IActionResult> ExpenseType([FromBody] ExpenseType expenseType, string companyCode)
        {
            try
            {
                string message = expenseType.Id == 0 ? "Creación" : "Actualización";
                EntityResponse response = await _expensesSettingsService.PostPutExpenseType(expenseType, companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                response.Mensaje = message;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post ExpenseType: " + ex.Message);
            }
        }

        [HttpPost("Status")]
        public async Task<IActionResult> Status([FromBody] ExpenseType expenseType)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.PostStatus(expenseType);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post Status: " + ex.Message);
            }
        }
        #endregion

        #region ExpensesCategories
        [HttpGet("ExpensesCategories/{companyCode}")]
        public async Task<IActionResult> ExpensesCategories(string companyCode)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.GetExpensesCategories(companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get ExpensesCategories: " + ex.Message);
            }
        }

        [HttpPost("StatusCategory")]
        public async Task<IActionResult> StatusCategory([FromBody] ExpenseCategory expenseCategory)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.PostStatusCategory(expenseCategory);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post StatusCategory: " + ex.Message);
            }
        }

        [HttpPost("ExpenseCategory/{companyCode}")]
        public async Task<IActionResult> ExpenseCategory([FromBody] ExpenseCategory expenseCategory, string companyCode)
        {
            try
            {
                string message = expenseCategory.Id == 0 ? "Creación" : "Actualización";
                EntityResponse response = await _expensesSettingsService.PostPutExpenseCategory(expenseCategory, companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                response.Mensaje = message;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post ExpenseCategory: " + ex.Message);
            }
        }
        #endregion

        #region ExpensesAccounts
        [HttpGet("ExpensesAccounts/{companyCode}")]
        public async Task<IActionResult> ExpensesAccounts(string companyCode)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.GetExpensesAccounts(companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get ExpensesAccounts: " + ex.Message);
            }
        }

        [HttpGet("MainAccounts")]
        public async Task<IActionResult> MainAccounts()
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.GetMainAccounts();

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get MainAccounts: " + ex.Message);
            }
        }

        [HttpPost("ExpenseAccount/{companyCode}")]
        public async Task<IActionResult> ExpenseAccount([FromBody] ExpenseAccount expense, string companyCode)
        {
            try
            {
                string message = expense.Id == 0 ? "Creación" : "Actualización";

                EntityResponse response = await _expensesSettingsService.PostPutExpenseAccount(expense, companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                response.Mensaje = message;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post ExpensesCostCenter: " + ex.Message);
            }
        }

        [HttpDelete("ExpenseAccount/{companyCode}/{id}")]
        public async Task<IActionResult> ExpenseAccount(string companyCode, int id)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.DeleteExpenseAccount(id, companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método delete ExpenseAccount: " + ex.Message);
            }
        }
        #endregion

        #region TaxGroup
        [HttpGet("TaxGroups/{companyCode}")]
        public async Task<IActionResult> TaxGroups(string companyCode)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.GetTaxGroups(companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get TaxGroups: " + ex.Message);
            }
        }

        [HttpPost("TaxGroup/{companyCode}")]
        public async Task<IActionResult> TaxGroup([FromBody] TaxGroup taxGroup, string companyCode)
        {
            try
            {
                string mensaje = taxGroup.Id > 0 ? "Actualización" : "Creación";
                EntityResponse response = await _expensesSettingsService.PostPutTaxGroup(taxGroup, companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                response.Mensaje = mensaje;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post TaxGroup: " + ex.Message);
            }
        }
        #endregion

        #region Users
        [HttpGet("Users/{companyCode}")]
        public async Task<IActionResult> Users(string companyCode)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.GetUsers(companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get Users: " + ex.Message);
            }
        }

        [HttpGet("Employees/{companyCode}")]
        public async Task<IActionResult> Employees(string companyCode)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.GetEmployees(companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método get Employees: " + ex.Message);
            }
        }

        [HttpPost("User/{companyCode}")]
        public async Task<IActionResult> User([FromBody] User user, string companyCode)
        {
            try
            {
                string mensaje = user.Id > 0 ? "Actualización" : "Creación";
                EntityResponse response = await _expensesSettingsService.PostPutUser(user, companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }

                response.Mensaje = mensaje;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post User: " + ex.Message);
            }
        }

        [HttpPost("UserState")]
        public async Task<IActionResult> UserState([FromBody] User user)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.PostUserState(user);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método post User: " + ex.Message);
            }
        }

        [HttpPut("ResetPassword/{id}/{newPassword}/{companyCode}")]
        public async Task<IActionResult> ResetPassword(int id, string newPassword, string companyCode)
        {
            try
            {
                EntityResponse response = await _expensesSettingsService.ResetPassword(id, newPassword, companyCode);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error en método put ResetPassword: " + ex.Message);
            }
        }
        #endregion
    }
}
