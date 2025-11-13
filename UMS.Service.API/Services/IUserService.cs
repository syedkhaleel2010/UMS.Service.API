using UMS.Service.API.Models;

namespace UMS.Service.API.Services;

public interface IUserService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(RegisterRequest request);
    bool VerifyPassword(string password, string passwordHash);
    string HashPassword(string password);
}

