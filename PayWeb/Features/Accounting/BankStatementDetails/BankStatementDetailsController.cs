using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace CRM.Features.Accounting.BankStatementDetails
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class BankStatementDetailsController : ControllerBase
    {
        private readonly BankStatementDetailsAppService _bankStatementDetailsAppService;

        public BankStatementDetailsController(BankStatementDetailsAppService bankStatementDetailsAppService)
        {
            _bankStatementDetailsAppService = bankStatementDetailsAppService;
        }

        [HttpPost("AddBankStatementDetails")]
        public async Task<IActionResult> AddBankStatementDetailsAsync([FromBody] BankStatementDetailsDto bankStatementDetails)
        {
            EntityResponse response = await _bankStatementDetailsAppService.AddBankStatementDetailsAsync(bankStatementDetails);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(bankStatementDetails);
        }


        [HttpGet("for-BankStatementDetailsId")]
        public IActionResult FindByBankStatemenDetailsId([FromQuery] int bankStatemenDetailsId)
        {
            BankStatementDetailsDto bankStatementDetails = _bankStatementDetailsAppService.FindById(bankStatemenDetailsId);
            return Ok(bankStatementDetails);
        }

        [HttpGet("GetAllBankStatementDetails")]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _bankStatementDetailsAppService.GetAll();
            return Ok(response);
        }

        [HttpGet("Details-BankStatementId")]
        public async Task<IActionResult> GetAllBankStatementDetailsAsync([FromQuery] int bankStatementId)
        {
            var response = await _bankStatementDetailsAppService.GetAllByBankStatement(bankStatementId);
            return Ok(response);
        }
    }
}
