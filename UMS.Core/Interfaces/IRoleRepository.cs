using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Entities;

namespace UMS.Core.Interfaces
{
    public interface IRoleRepository
    {
       Task<Role> CreateRole(Role role);
    }
}
