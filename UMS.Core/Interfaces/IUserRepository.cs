using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Entities;

namespace UMS.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> RegisterUser(User user);
        Task<User?> UpdateUser(User user); 
        Task<User?> DeleteUser(User user);
        Task<User?> GetUserByEmailAsync(string username);
    }
}
