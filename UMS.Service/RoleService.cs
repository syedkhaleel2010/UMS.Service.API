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
    public class RoleService : IRoleService
    {
        private readonly ILogger<RoleService> _logger;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RoleService(ILogger<RoleService> logger,IRoleRepository roleRepository, IMapper mapper)
        {
            _logger = logger;
            _roleRepository = roleRepository;
            _mapper = mapper;
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
    }
}