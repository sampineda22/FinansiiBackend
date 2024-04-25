using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
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

        public BankConfigurationController(BankConfigurationAppService bankConfigurationAppService)
        {
            this._bankConfigurationAppService = bankConfigurationAppService;
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

        [HttpGet("GetAllBankConfiguration")]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _bankConfigurationAppService.GetAll();
            return Ok(response);
        }
    }
}
