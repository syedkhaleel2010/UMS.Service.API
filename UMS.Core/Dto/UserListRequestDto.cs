namespace UMS.Core.Dto
{
    public class UserListRequestDto
    {
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedOn";
        public string? SortDirection { get; set; } = "desc"; // "asc" or "desc"
    }
}

