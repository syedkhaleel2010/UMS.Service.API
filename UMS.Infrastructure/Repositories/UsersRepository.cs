using Microsoft.EntityFrameworkCore;
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
    public class UserRepository  :Repository<User>, IUserRepository


    {

        private readonly UMSDbContext dbContext;
        public UserRepository(UMSDbContext _dbContext) : base(_dbContext)
        {
            dbContext = _dbContext;
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await Task.FromResult(dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Username == username && u.IsActive == true));

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

        public async Task<User?> UpdateUser(User user)
        {
            dbContext.Users.Update(user);
            dbContext.SaveChanges();
            return await Task.FromResult<User?>(user);
        }
        public async Task<User?> DeleteUser(User user)
        {
            dbContext.Users.Update(user);
            dbContext.SaveChanges();
            return await Task.FromResult<User?>(user);
        }
    }
}
