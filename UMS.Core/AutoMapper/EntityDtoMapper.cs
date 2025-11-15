using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Dto;
using UMS.Core.Entities;

namespace UMS.Core.AutoMapper
{
    public class EntityDtoMapper : Profile
    {
        public EntityDtoMapper()
        {
            CreateMap<RegisterUserDto, User>()
                .ForMember(d => d.Username, opt => opt.MapFrom(s => s.Username.Trim()))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Trim().ToLower()))
                .ForMember(d=> d.FirstName, opt => opt.MapFrom(s => s.FirstName != null ? s.FirstName.Trim() : null))
                .ForMember(d=>d.LastName,opt=>opt.MapFrom(s=>s.LastName != null ? s.LastName.Trim() : null))
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => true))
                .ReverseMap();

            CreateMap<RoleDto, Role>()
                .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.RoleName.Trim()))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description != null ? s.Description.Trim() : null))
                .ReverseMap();

            CreateMap<UserRoleDto,UserRole>()
                .ForMember(d=>d.UserId, opt => opt.MapFrom(s => s.UserId.Trim()))
                .ForMember(d => d.RoleId, opt => opt.MapFrom(s => s.RoleId.Trim()))
                .ForMember(d=>d.UserName, opt => opt.MapFrom(s => s.UserName.Trim()))
                .ReverseMap();
        }
    }
}
