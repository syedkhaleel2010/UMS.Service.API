namespace UMS.Core.Entities
{
    public class UserRole : EntityBase
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } =string.Empty;
        public string RoleId { get; set; } = string.Empty;

        public User User { get; set; }

        public Role Role { get; set; }
    }
}
