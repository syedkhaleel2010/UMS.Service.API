using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UMS.Core.Dto;
using UMS.Service.Interfaces;

namespace UMS.Service.API.Controllers
{
    [Route("api/UserRole")]
    [ApiController]
    [Authorize]
    public class UserRoleController : ControllerBase
    {
        private  readonly ILogger<UserRoleController> _logger;
        private  readonly IRoleService roleService;
        public UserRoleController(ILogger<UserRoleController> logger,IRoleService roleService)
        {
            _logger = logger;
            this.roleService = roleService;

        }

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto roleDto)
        {
            try
            {
                if (roleDto == null)
                {
                    return BadRequest("Role data is null.");
                }
                var createdRole = await this.roleService.CreateRole(roleDto);
                return Ok(createdRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating role.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
