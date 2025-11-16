namespace UMS.Core.Dto
{
    public class UserListResponseDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public bool? IsAccountLocked { get; set; }
        public int? NumOfAttempt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class PagedUserListResponseDto
    {
        public List<UserListResponseDto> Users { get; set; } = new List<UserListResponseDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}

