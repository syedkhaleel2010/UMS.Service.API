using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
        public UserRoleController(ILogger<UserRoleController> logger, IRoleService roleService, IStringLocalizer<SharedResource> localizer) : base(logger, localizer)
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
                    return BadRequest(Localizer["RoleDataIsNull"].ToString());
                }
                var createdRole = await this.roleService.CreateRole(roleDto);
                return OkServiceResponse(createdRole, string.Format(Localizer["RoleCreated"].ToString(), roleDto.RoleName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating role.");
                return StatusCode(StatusCodes.Status500InternalServerError, Localizer["InternalServerError"].ToString());
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
                    return BadRequest(Localizer["UserRoleDataIsNull"].ToString());
                }
                var assignedRole = await this.roleService.AssignRole(userRoleDto);
                return OkServiceResponse(assignedRole, Localizer["RoleAssignedToUser"].ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning role.");
                return StatusCode(StatusCodes.Status500InternalServerError, Localizer["InternalServerError"].ToString());
            }
        }
    }
}
