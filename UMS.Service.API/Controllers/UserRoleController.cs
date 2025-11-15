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
    public class UserRoleController : ServiceControllerBase
    {
        private  readonly ILogger<UserRoleController> _logger;
        private  readonly IRoleService roleService;
        public UserRoleController(ILogger<UserRoleController> logger,IRoleService roleService) : base(logger)
        {
            _logger = logger;
            this.roleService = roleService;

        }

        [Authorize(Roles = "Admin")]
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
                return OkServiceResponse(createdRole,$"{roleDto.RoleName} Role created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating role.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleDto userRoleDto)
        {
            try
            {
                if (userRoleDto == null)
                {
                    return BadRequest("UserRole data is null.");
                }
                var assignedRole = await this.roleService.AssignRole(userRoleDto);
                return OkServiceResponse(assignedRole,"Role Assigned to User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning role.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
