using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace CRM.Features.BankStatement
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class BankStatementController: ControllerBase
    {
        private readonly BankStatementAppService _bankStatementAppService;

        public BankStatementController(BankStatementAppService bankStatementAppService)
        {
            this._bankStatementAppService = bankStatementAppService;
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
    }
}
