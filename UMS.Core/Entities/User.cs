using System.ComponentModel.DataAnnotations.Schema;

namespace UMS.Core.Entities
{
    [Table("USERS", Schema = "DBO")]
    public class User : EntityBase
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public bool? IsAccountLocked { get; set; }
        public int? NumOfAttempt { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public User Clone()
        {
            return new User
            {
                Username = this.Username,
                Email = this.Email,
                PasswordHash = this.PasswordHash,
                PasswordSalt = this.PasswordSalt,
                FirstName = this.FirstName,
                LastName = this.LastName,
                IsActive = this.IsActive,
                IsAccountLocked = this.IsAccountLocked,
                NumOfAttempt = this.NumOfAttempt,
                UserRoles = this.UserRoles != null ? new List<UserRole>(this.UserRoles) : null
            };
        }


    }
}
