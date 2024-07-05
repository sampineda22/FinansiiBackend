using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace CRM.Features.BankStatementServiceAX
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class BankStatementServiceAXController: ControllerBase
    {
        private readonly BanskStatementServiceAXService _banskStatementServiceAXService;

        public BankStatementServiceAXController(BanskStatementServiceAXService banskStatementServiceAXService)
        {
            _banskStatementServiceAXService = banskStatementServiceAXService;
        }

        [HttpGet("SendBankStatementServiceAX/{bankStatements}")]
        public async Task<IActionResult> SendBankStatementServiceAX(string bankStatements)
        {
            EntityResponse response = await _banskStatementServiceAXService.SendBankStatement(bankStatements);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);

        }
    }
}
