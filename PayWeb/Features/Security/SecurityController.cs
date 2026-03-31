using CRM.Features.Admin.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PayWeb.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PayWeb.Features.Security
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly ILogger<SecurityController> _logger;
        private readonly UserAppService _userService;
        private readonly IJwtAuthManager _jwtAuthManager;
        private readonly IConfiguration _configuration;

        public SecurityController(ILogger<SecurityController> logger, UserAppService userAppService, IJwtAuthManager jwtAuthManager, IConfiguration configuration)
        {
            this._userService = userAppService;
            _logger = logger;
            _jwtAuthManager = jwtAuthManager;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            string connectionString = _configuration.GetConnectionString("IMFinanzas");

            if (!connectionString.ToLower().Contains("imfinanzasdev"))
            {
                if (!_userService.IsValidUserCredentials(request.User.ToLower(), request.Password))
                {
                    return Unauthorized();
                }
            }

            request.CompanyCode = _userService.FindByUserId(request.User).CompanyCode;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.User),
                new Claim("Cod_Empresa",request.CompanyCode),
                new Claim("Contra",request.Password)

            };

            var jwtResult = _jwtAuthManager.GenerateTokens(request.User, claims, DateTime.Now);
            _logger.LogInformation($"User [{request.User}] logged in the system.");
            return Ok(new LoginResult
            {
                UserId = request.User,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString,
                CompanyCode = request.CompanyCode
            });
        }

        [AllowAnonymous]
        [HttpPost("checkcredentials")]
        public static int CheckCredentials([FromBody] string userId, string password)
        {
            if (!UserAppService.IsValidUserCredentialsNew(userId, password))
            {
                return 0;
            }
            return 1;
        }

        [HttpGet("GetCurrentUser")]
        public ActionResult GetCurrentUser()
        {
            return Ok(new LoginWithCompanyResult
            {
                UserId = User.Identity?.Name,
                Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty,
                Cod_Empresa = User.FindFirst("Cod_Empresa")?.Value ?? string.Empty
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

        [Authorize]
        [HttpPost("refresh-token")]
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
        [JsonPropertyName("user")]
        public string User { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }
        public string CompanyCode { get; set; }
    }
    public class LoginUserPasswordRequest
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
        public string CompanyCode { get; set; }
        public int CodEmpresaInt { get; set; }
    }
    public class LoginWithCompanyResult
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
        public string Cod_Empresa { get; set; }
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
