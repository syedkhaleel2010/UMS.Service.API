using UMS.Service.API.Models;

namespace UMS.Service.API.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}

