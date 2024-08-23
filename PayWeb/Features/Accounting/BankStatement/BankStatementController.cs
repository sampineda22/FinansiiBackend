using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using PayWeb.Features.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace CRM.Features.Accounting.BankStatement
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class BankStatementController : ControllerBase
    {
        private readonly BankStatementAppService _bankStatementAppService;
        private User loggedUser = new User { };

        public BankStatementController(IHttpContextAccessor httpContextAccessor, BankStatementAppService bankStatementAppService)
        {
            _bankStatementAppService = bankStatementAppService;

            if (httpContextAccessor.HttpContext.User.Identity.Name != null)
            {
                var empresa = "";
                try
                {
                    empresa = httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "Cod_Empresa")?.Value ?? string.Empty;
                }
                catch (Exception)
                {
                    empresa = "";
                }

                loggedUser = new User
                {
                    UserId = httpContextAccessor.HttpContext.User.Identity.Name ?? string.Empty,
                    Cod_Empresa = empresa
                };
            }
        }

        [HttpPost("AddBankStatement")]
        public async Task<IActionResult> AddBankStatementAsync([FromBody] BankStatementDto bankStatement)
        {
            EntityResponse response = await _bankStatementAppService.AddBankStatementAsync(bankStatement);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(bankStatement);
        }


        [HttpGet("for-BankStatementId")]
        public IActionResult FindByBankStatemenId([FromQuery] int bankStatemenId)
        {
            BankStatementDto bankStatement = _bankStatementAppService.FindById(bankStatemenId);
            return Ok(bankStatement);
        }

        [HttpGet("GetAllBankStatement")]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _bankStatementAppService.GetAll();
            return Ok(response);
        }

        [HttpGet("GetByAccountId/{accountId}/{date}/{companyCode}")]
        public async Task<IActionResult> GetByAccountId(string accountId, string date, string companyCode)
        {
            var response = await _bankStatementAppService.GetByAccountId(accountId, date, companyCode);
            return Ok(response);
        }

        [HttpGet("ImportStatementFromFileByAccountId/{companyCode}/{date}/{account}")]
        public async Task<IActionResult> ImportStatementFromFileByAccountId(string companyCode, string date, string account)
        {
            EntityResponse response = await _bankStatementAppService.ImportStatementFromFileByAccount(date, companyCode, account);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
