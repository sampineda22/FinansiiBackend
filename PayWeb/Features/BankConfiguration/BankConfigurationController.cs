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

namespace CRM.Features.BankConfiguration
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class BankConfigurationController : ControllerBase
    {
        private readonly BankConfigurationAppService _bankConfigurationAppService;
        private User loggedUser = new User { };

        public BankConfigurationController(IHttpContextAccessor httpContextAccessor, BankConfigurationAppService bankConfigurationAppService)
        {
            this._bankConfigurationAppService = bankConfigurationAppService;

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

        [HttpPost("AddBankConfiguration")]
        public async Task<IActionResult> AddBankConfigurationAsync([FromBody] BankConfigurationDto bankConfigurationDto)
        {
            EntityResponse response = await _bankConfigurationAppService.AddBankConfigurationAsync(bankConfigurationDto);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(bankConfigurationDto);
        }


        [HttpGet("for-BankConfigurationId")]
        public IActionResult FindByBankConfigurationId([FromQuery] int bankConfigurationId)
        {
            BankConfigurationDto bankConfiguraion = _bankConfigurationAppService.FindById(bankConfigurationId);
            return Ok(bankConfiguraion);
        }

        [HttpGet("for-AccountId")]
        public IActionResult FindByAccountId([FromQuery] string accountId)
        {
            BankConfigurationDto bankConfiguraion = _bankConfigurationAppService.FindByAccountId(accountId);
            return Ok(bankConfiguraion);
        }

        [HttpGet("GetAllBankConfiguration/{companyCode}")]
        public async Task<IActionResult> GetAllAsync(string companyCode)
        {
            var response = await _bankConfigurationAppService.GetAll(companyCode);
            return Ok(response);
        }
    }
}
