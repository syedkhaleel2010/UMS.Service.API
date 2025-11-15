namespace UMS.Core.Entities
{
    public class Role
    {
        public string ID { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }

    }
}
