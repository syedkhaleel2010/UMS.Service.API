using System.Security.Cryptography;
using System.Text;
using UMS.Service.API.Models;

namespace UMS.Service.API.Services;

public class UserService : IUserService
{
    // In-memory user store for demo purposes
    // In production, replace this with a database context
    private static readonly List<User> _users = new();
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public async Task<User> CreateUserAsync(RegisterRequest request)
    {
        var user = new User
        {
            Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(user);
        _logger.LogInformation("User created: {Username}", user.Username);
        return await Task.FromResult(user);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        // Simple hash verification for demo purposes
        // In production, use BCrypt or similar
        var hashedPassword = HashPassword(password);
        return hashedPassword == passwordHash;
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}

