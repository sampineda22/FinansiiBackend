using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public RolesController(RolesService rolesService)
        {
            this._rolesService = rolesService;
        }

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var response = await _rolesService.GetAllRoles();
            return Ok(response);
        }
    }
}
