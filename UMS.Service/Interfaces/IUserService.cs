using UMS.Core.Dto;
using UMS.Core.Entities;

namespace UMS.Service.Interfaces
{
    public interface IUserService
    {
        Task<User?> UserLogin(LoginRequest request);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<RegisterUserDto> CreateUserAsync(RegisterUserDto request);
        Task<RegisterUserDto> UpdateUserAsync(RegisterUserDto request);
        Task<bool> DeleteUser(DeleteUserDto request);
        Task ResetUserPassword(UserPasswordResetRequest request, string modifyingUser);
        bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    }
}
