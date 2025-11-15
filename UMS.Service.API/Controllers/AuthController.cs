using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UMS.Core.Dto;
using UMS.Service.API.Models;
using UMS.Service.Interfaces;

namespace UMS.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        IJwtService jwtService,
        Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if username already exists
        var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username already exists" });
        }

        // Check if email already exists
        var existingEmail = await _userService.GetUserByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            return BadRequest(new { message = "Email already exists" });
        }

        // Create user
        var user = await _userService.CreateUserAsync(request);

        // Generate token
        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var response = new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Username = user.Username,
            Email = user.Email
        };

        _logger.LogInformation("User registered successfully: {Username}", user.Username);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find user by username
        var user = await _userService.GetUserByUsernameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Login attempt with invalid username: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // Verify password
        if (!_userService.VerifyPassword(request.Password, user.PasswordHash,user.PasswordSalt))
        {
            _logger.LogWarning("Login attempt with invalid password for user: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // Generate token
        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var response = new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Username = user.Username,
            Email = user.Email
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

        // In a real implementation, you would:
        // 1. Validate the refresh token against a database
        // 2. Get the user from the token claims
        // 3. Generate a new token

        // For demo purposes, we'll return an error suggesting to login again
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

