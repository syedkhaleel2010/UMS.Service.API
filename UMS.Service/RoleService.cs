using System.Security.Cryptography;
using System.Text;
using UMS.Core.Dto;
using UMS.Core.Entities;
using UMS.Service.Interfaces;
using Microsoft.Extensions.Logging;
using UMS.Core.Interfaces;
using AutoMapper;
namespace UMS.Service
{
    public class RoleService : BaseService, IRoleService
    {
        private readonly ILogger<RoleService> _logger;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly IUserSession _userSession;
        public RoleService(ILogger<RoleService> logger, IRoleRepository roleRepository, IMapper mapper, IUserSession userSession) : base(userSession)
        {
            _logger = logger;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _userSession = userSession;
        }

        public async Task<RoleDto> CreateRole(RoleDto dto)
        {
            dto.ID = Guid.NewGuid().ToString();

            var role = _mapper.Map<Role>(dto);
            role.CreatedBy = "SYSTEM";
            role.CreatedOn = DateTime.UtcNow;

            await _roleRepository.CreateRole(role);

            return dto;
        }
        public async Task<UserRoleDto> AssignRole(UserRoleDto userRole)
        {
            userRole.ID = Guid.NewGuid().ToString();
            var userRoleEntity = _mapper.Map<UserRole>(userRole);

            userRoleEntity.IsActive = true;
            userRoleEntity.CreatedOn = DateTime.UtcNow;
            userRoleEntity.CreatedBy = "SYSTEM";
            await _roleRepository.AssignRole(userRoleEntity);

            return userRole;
        }
        public async Task<IEnumerable<RoleDto>> GetAllRoles()
        {
            var roles = await _roleRepository.GetAllRoles();
            var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(roles);
            return roleDtos;
        }

    }
}