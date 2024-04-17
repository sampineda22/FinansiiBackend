using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PayWeb.Common;
using PayWeb.Features.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PayWeb.Features.Security
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly ILogger<SecurityController> _logger;
        private readonly UserAppService _userService;
        private readonly IJwtAuthManager _jwtAuthManager;

        public SecurityController(ILogger<SecurityController> logger, UserAppService userAppService, IJwtAuthManager jwtAuthManager)
        {
            this._userService = userAppService;
            _logger = logger;
            _jwtAuthManager = jwtAuthManager;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!_userService.IsValidUserCredentials(request.UserId, request.Password))
            {
                return Unauthorized();
            }

            if (!_userService.IsAnExistingUser(request.UserId)) 
            {
                var user = new UserDto
                {
                    UserId = request.UserId,
                    Password = Common.Common.GetMD5(request.Password),
                    State = true,
                    CreateDateTime = DateTime.Now
                };
                EntityResponse response = await _userService.AddUserAsync(user);
                if (!response.Ok)
                {
                    return BadRequest(response);
                }
                
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.UserId)

            };

            var jwtResult = _jwtAuthManager.GenerateTokens(request.UserId, claims, DateTime.Now);
            _logger.LogInformation($"User [{request.UserId}] logged in the system.");
            return Ok(new LoginResult
            {
                UserId = request.UserId,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString
            });
        }

        [HttpGet("user")]
        [Authorize]
        public ActionResult GetCurrentUser()
        {
            return Ok(new LoginResult
            {
                UserId = User.Identity?.Name,
                Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty,
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            // optionally "revoke" JWT token on the server side --> add the current token to a block-list
            // https://github.com/auth0/node-jsonwebtoken/issues/375

            var userId = User.Identity?.Name;
            _jwtAuthManager.RemoveRefreshTokenByUserName(userId);
            _logger.LogInformation($"User [{userId}] logged out the system.");
            return Ok();
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var userId = User.Identity?.Name;
                _logger.LogInformation($"User [{userId}] is trying to refresh JWT token.");

                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return Unauthorized();
                }

                var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
                var jwtResult = _jwtAuthManager.Refresh(request.RefreshToken, accessToken, DateTime.Now);
                _logger.LogInformation($"User [{userId}] has refreshed JWT token.");
                return Ok(new LoginResult
                {
                    UserId = userId,
                    Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString
                });
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(e.Message); // return 401 so that the client side can redirect the user to login page
            }
        }
    }

    public class LoginRequest
    {
        [Required]
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    public class LoginResult
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenRequest
    {
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }

    public class ImpersonationRequest
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }
    }
}
