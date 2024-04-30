using CRM.Features.BankStatement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayWeb.Common;
using PayWeb.Features.Users;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace CRM.Features.Admin.Roles
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class RolesController: ControllerBase
    {
        private readonly RolesService _rolesService;
        private User loggedUser = new User { };

        public RolesController(IHttpContextAccessor httpContextAccessor, RolesService rolesService)
        {
            this._rolesService = rolesService;

            if (httpContextAccessor.HttpContext.User.Identity.Name != null)
            {
                var empresa = "";
                try
                {
                    empresa = httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == /*"Cod_Empresa"*/ "CompanyCode")?.Value ?? string.Empty;
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

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var response = await _rolesService.GetAllRoles();
            return Ok(response);
        }

        [HttpPost("PostRole")]
        public async Task<IActionResult> PostRole([FromBody] RoleDto roleDto)
        {
            EntityResponse response = await _rolesService.PostRole(roleDto, "0001", loggedUser.UserId);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("DeleteRole/{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            EntityResponse response = await _rolesService.DeleteRole(roleId);
            if (!response.Ok)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
