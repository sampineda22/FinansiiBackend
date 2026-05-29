using CRM.Features.Accounting.BankStatementServiceAX;
using CRM.Features.Accounting.CD;
using CRM.Features.Accounting.VendPaymentReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System.Threading.Tasks;

namespace CRM.Features.Accounting.AccountingConfiguration
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class AccountingConfigurationController : ControllerBase
    {
        private readonly AccountingConfigurationService _accountingConfigService;

        public AccountingConfigurationController(AccountingConfigurationService accountingConfigService)
        {
            _accountingConfigService = accountingConfigService;
        }

        #region ExceptionCodes
        [HttpGet("ExceptionCodes/{companyCode}")]
        public IActionResult ExceptionCodes(string companyCode)
        {
            EntityResponse response = _accountingConfigService.GetExceptionCodes(companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("MT940Banks/{companyCode}")]
        public IActionResult MT940Banks(string companyCode)
        {
            EntityResponse response = _accountingConfigService.GetMT940Banks(companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("ExceptionCode")]
        public IActionResult PaymentDate([FromBody] ExceptionCode code)
        {
            EntityResponse response = _accountingConfigService.PostExceptionCode(code).Result;

            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("ExceptionCode/{companyCode}/{code}/{accountId}")]
        public async Task<IActionResult> ExceptionCode(string companyCode, string code, string accountId)
        {
            EntityResponse response = await _accountingConfigService.DeleteExceptionCode(companyCode, code, accountId);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        #endregion

        #region PaymentDates
        [HttpGet("PaymentDates/{companyCode}")]
        public IActionResult PaymentDates(string companyCode)
        {
            EntityResponse response = _accountingConfigService.GetPaymentDates(companyCode);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("PaymentDate")]
        public IActionResult PaymentDate([FromBody] PaymentDate date)
        {
            EntityResponse response = _accountingConfigService.PostPaymentDate(date).Result;

            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("PaymentDate/{companyCode}/{year}/{month}")]
        public async Task<IActionResult> DeleteCertificate(string companyCode, int year, int month)
        {
            EntityResponse response = await _accountingConfigService.DeleteDate(companyCode, year, month);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        #endregion
    }
}