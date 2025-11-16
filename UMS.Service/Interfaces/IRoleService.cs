using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Dto;
using UMS.Core.Entities;

namespace UMS.Service.Interfaces
{
    public interface IRoleService
    {
        Task<RoleDto> CreateRole(RoleDto Role);

        Task<UserRoleDto> AssignRole(UserRoleDto userRole);
        Task<IEnumerable<RoleDto>> GetAllRoles();
    }
}
