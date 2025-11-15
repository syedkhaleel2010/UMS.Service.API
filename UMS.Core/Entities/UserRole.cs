namespace UMS.Core.Entities
{
    public class UserRole : EntityBase
    {
        public string Username { get; set; }
        public string Email { get; set; }

        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
    }
}
