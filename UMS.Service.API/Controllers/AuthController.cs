using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using UMS.Core.Dto;
using UMS.Service.API.Common;
using UMS.Service.API.Models;
using UMS.Service.Interfaces;

namespace UMS.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ServiceControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        IJwtService jwtService,
        Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings,
        ILogger<AuthController> logger,
        IStringLocalizer<SharedResource> localizer) : base(logger, localizer)
    {
        _userService = userService;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequestResponse(Localizer["PleaseCheckInput"].ToString());
        }

        // Check if username already exists
        var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            return BadRequestResponse(Localizer["UsernameAlreadyExists"].ToString());
        }

        // Check if email already exists
        var existingEmail = await _userService.GetUserByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            return BadRequestResponse(Localizer["EmailAlreadyExists"].ToString());
        }

        // Create user
        var user = await _userService.CreateUserAsync(request);

        
        return OkServiceResponse(user, Localizer["UserCreatedSuccessfully"].ToString());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register-with-roles")]
    public async Task<IActionResult> RegisterWithRoles([FromBody] RegisterUserWithRolesDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequestResponse(Localizer["PleaseCheckInput"].ToString());
        }

        // Check if username already exists
        var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            return BadRequestResponse(Localizer["UsernameAlreadyExists"].ToString());
        }

        // Check if email already exists
        var existingEmail = await _userService.GetUserByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            return BadRequestResponse(Localizer["EmailAlreadyExists"].ToString());
        }

        // Create user with roles
        var user = await _userService.CreateUserWithRolesAsync(request);

        return OkServiceResponse(user, Localizer["UserCreatedSuccessfully"].ToString());
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPut("update-userdetails-with-role")]
    public async Task<IActionResult> UpdateUserDetailsWithRole([FromBody] UpdateUserWithRoleDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequestResponse(Localizer["PleaseCheckInput"].ToString());
        }
        string userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized(new { message = Localizer["AuthenticationRequired"].ToString() });
        }

        // Get user roles from JWT token claims
        var userRoles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Check if user has Admin role
        bool isAdmin = userRoles.Contains("Admin");

        if (isAdmin && request.RoleIds is not null && request.RoleIds.Count() > 0)
        {
            var user = await _userService.UpdateUserDetailsWithRoleAsync(request);
            return OkServiceResponse(user, Localizer["UserUpdatedSuccessfully"].ToString());
        }
        
        return BadRequestResponse(Localizer["InsufficientPermissions"].ToString());

    }


    [Authorize(Roles = "Admin,User")]
    [HttpPut("update-userdetails")]
    public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequestResponse(Localizer["PleaseCheckInput"].ToString());
        }
        string userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized(new { message = Localizer["AuthenticationRequired"].ToString() });
        }
        
            var user = await _userService.UpdateUserDetailsAsync(request);
            return OkServiceResponse(user, Localizer["UserUpdatedSuccessfully"].ToString());

    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("deleteUser")]
    public async Task<IActionResult> deleteUser([FromBody] DeleteUserDto request)
    {
        var user = await _userService.DeleteUser(request);
        return OkServiceResponse(user, Localizer["UserDeletedSuccessfully"].ToString());
    }

    [HttpPost]
    [Route("UserPasswordResetRequest")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> UserPasswordResetRequest([FromBody] UserPasswordResetRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequestResponse(Localizer["PleaseCheckInput"].ToString());
        }

        string user = User.Identity?.Name;
        if (string.IsNullOrEmpty(user))
        {
            return Unauthorized(new { message = Localizer["AuthenticationRequired"].ToString() });
        }

        // Get user roles from JWT token claims
        var userRoles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Check if user has Admin role
        bool isAdmin = userRoles.Contains("Admin");

        // If user has only "User" role (not Admin), OldPassword is mandatory
        if (!isAdmin && string.IsNullOrWhiteSpace(request.OldPassword))
        {
            ModelState.AddModelError(nameof(request.OldPassword), "OldPassword is required for users with User role only.");
            return BadRequestResponse(Localizer["PleaseCheckInput"].ToString());
        }

        await _userService.ResetUserPassword(request, user);
        return Ok(ServiceResponse.SuccessResponse(Localizer["PasswordResetSuccessfully"].ToString()));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] Core.Dto.LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find user by username
        var user = await _userService.UserLogin(request);
       
        var roles = user?.UserRoles.Select(r => r.Role.RoleName).ToList();

        // Generate token
        var token = _jwtService.GenerateToken(user,roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var response = new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Username = user.Username,
            Email = user.Email,
            Roles = roles,
            Id= user.ID
        };

        _logger.LogInformation("User logged in successfully: {Username}", user.Username);
        return OkServiceResponse(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { message = Localizer["TokenRequired"].ToString() });
        }

        // Validate the token
        if (!_jwtService.ValidateToken(request.Token))
        {
            return Unauthorized(new { message = Localizer["InvalidToken"].ToString() });
        }

      
        return BadRequest(new { message = Localizer["PleaseLoginAgain"].ToString() });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var username = User.Identity?.Name;
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new
        {
            UserId = userId,
            Username = username,
            Email = email
        });
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsersList([FromQuery] UserListRequestDto request)
    {
        try
        {
            // Validate pagination parameters
            if (request.PageNumber < 1)
            {
                request.PageNumber = 1;
            }
            if (request.PageSize < 1 || request.PageSize > 100)
            {
                request.PageSize = 10; // Default page size
            }

            var result = await _userService.GetUsersListAsync(request);
            return OkServiceResponse(result, Localizer["UsersRetrievedSuccessfully"]?.ToString() ?? "Users retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users list");
            return HandleOtherException(ex);
        }
    }

    [HttpGet("user/{id}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetUserById([FromRoute] string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequestResponse(Localizer["PleaseCheckInput"]?.ToString() ?? "User ID is required");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(Localizer["UserNotFound"]?.ToString() ?? "User not found");
            }

            return OkServiceResponse(user, Localizer["UserRetrievedSuccessfully"]?.ToString() ?? "User retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user by ID");
            return HandleOtherException(ex);
        }
    }
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

