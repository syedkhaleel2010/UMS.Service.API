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
    public class RoleRepository(UMSDbContext dbContext) :IRoleRepository
    {
        public async Task<Role> CreateRole(Role role)
        {
            dbContext.Roles.Add(role);
            dbContext.SaveChanges();
            return await Task.FromResult<Role>(role);
        }
    }
    
}
