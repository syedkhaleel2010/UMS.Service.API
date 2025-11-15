using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        ILogger<AuthController> logger) : base(logger)
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
            return BadRequestResponse("Please check the input and try again");
        }

        // Check if username already exists
        var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            return BadRequestResponse("Username already exists");
        }

        // Check if email already exists
        var existingEmail = await _userService.GetUserByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            return BadRequestResponse("Email already exists");
        }

        // Create user
        var user = await _userService.CreateUserAsync(request);

        
        return OkServiceResponse(user,"User created successfully");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("update-userdetails")]
    public async Task<IActionResult> UpdateUserDetails([FromBody] RegisterUserDto request)
    {
        var user = await _userService.UpdateUserAsync(request);
        return OkServiceResponse(user, "User updated successfully");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("deleteUser")]
    public async Task<IActionResult> deleteUser([FromBody] DeleteUserDto request)
    {
        var user = await _userService.DeleteUser(request);
        return OkServiceResponse(user, "User deleted successfully");
    }

    [HttpPost]
    [Route("UserPasswordResetRequest")]
   
    public async Task<IActionResult> UserPasswordResetRequest(UserPasswordResetRequest request)
    {
        string user = User.Identity.Name;
            await _userService.ResetUserPassword(request, user);
            return Ok(ServiceResponse.SuccessResponse("Password reset successfully!"));
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
            Roles = roles
        };

        _logger.LogInformation("User logged in successfully: {Username}", user.Username);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        // Validate the token
        if (!_jwtService.ValidateToken(request.Token))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

      
        return BadRequest(new { message = "Please login again to get a new token" });
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
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

