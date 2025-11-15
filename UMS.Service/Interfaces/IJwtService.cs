
using UMS.Core.Entities;

namespace UMS.Service.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user,List<string> Roles);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
    }
}
