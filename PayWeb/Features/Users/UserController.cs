using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace PayWeb.Features.Users
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class UserController:ControllerBase
    {
        private readonly UserAppService _userAppService;

        public UserController(UserAppService userAppService)
        {
            this._userAppService = userAppService;
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUserAsync([FromBody] UserDto user)
        {
            EntityResponse response = await _userAppService.AddUserAsync(user);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(user);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _userAppService.GetAll();
            return Ok(response);
        }

        [HttpGet("for-id")]
        public IActionResult FindById([FromQuery] int id)
        {
            UserDto user = _userAppService.FindById(id);
            return Ok(user);
        }

        [HttpGet("for-UserId")]
        [Authorize]
        public IActionResult FindByUserId([FromQuery] string userId)
        {
            UserDto user = _userAppService.FindByUserId(userId);
            return Ok(user);
        }
    }
}
