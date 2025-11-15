using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Entities;
using UMS.Core.Interfaces;
using UMS.Infrastructure.Data;

namespace UMS.Infrastructure.Repositories
{
    public class UserRepository(UMSDbContext dbContext) : IUserRepository
    {
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await Task.FromResult(dbContext.Users.FirstOrDefault(u => u.Username == username));
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await Task.FromResult(dbContext.Users.FirstOrDefault(u => u.Email == email));
        }
        public async Task<User?> RegisterUser(User user)
        {
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
            return await Task.FromResult<User?>(user);
        }
    }
}
