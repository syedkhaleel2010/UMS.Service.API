using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Dto;
using UMS.Core.Entities;

namespace UMS.Service.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(RegisterUserDto request);
        bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    }
}
