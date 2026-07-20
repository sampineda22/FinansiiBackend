using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Threading.Tasks;

namespace CRM.Features.Admin.Screen
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class ScreenController : ControllerBase
    {
        private readonly ScreenService _screenService;

        public ScreenController(ScreenService screenService)
        {
            _screenService = screenService;
        }

        [HttpGet("GetScreensByUser/{userId}")]
        public async Task<IActionResult> GetScreensByUser(string userId)
        {
            try
            {
                EntityResponse response = await _screenService.getScreensByUser(userId);

                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error en método GetScreensByUser: {ex.Message}");
            }
        }
    }
}