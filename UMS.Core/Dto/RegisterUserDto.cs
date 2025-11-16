using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UMS.Core.Entities;

namespace UMS.Core.Dto
{
    public class RegisterUserDto
    {
        public string? ID { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public virtual string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DeleteUserDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string ID { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class RegisterUserWithRolesDto : RegisterUserDto
    {
        public List<string> RoleIds { get; set; } = new List<string>();
    }

    public class UpdateUserDto
    {
        public string? ID { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<UserRoleDto> UserRoleDto { get; set; }
    }

    public class GetUserByIdResponseDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; } // Optional, will be null/undefined
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public bool? IsAccountLocked { get; set; } = false;
        public List<string> RoleIds { get; set; } = new List<string>();
    }

    public class UpdateUserWithRoleDto : RegisterUserDto
    {
        public bool? IsAccountLocked { get; set; } = false;
        public List<string> RoleIds { get; set; } = new List<string>();
        public override string Password { get; set; } = "Welcome@1234";
    }
}
