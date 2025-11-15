
using UMS.Core.Entities;

namespace UMS.Service.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
    }
}
