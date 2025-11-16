using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Entities;
using UMS.Core.Interfaces;
using UMS.Core.Dto;
using UMS.Infrastructure.Data;

namespace UMS.Infrastructure.Repositories
{
    public class UserRepository  :Repository<User>, IUserRepository


    {

        private readonly UMSDbContext dbContext;
        public UserRepository(UMSDbContext _dbContext) : base(_dbContext)
        {
            dbContext = _dbContext;
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await Task.FromResult(dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Username == username && u.IsActive == true));

        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await Task.FromResult(dbContext.Users.FirstOrDefault(u => u.Email == email));
        }
        public async Task<User?> RegisterUser(User user)
        {
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
            return await Task.FromResult<User?>(user);
        }

        public async Task<User?> UpdateUser(User user)
        {
            dbContext.Users.Update(user);
            dbContext.SaveChanges();
            return await Task.FromResult<User?>(user);
        }
        public async Task<User?> DeleteUser(User user)
        {
            dbContext.Users.Update(user);
            dbContext.SaveChanges();
            return await Task.FromResult<User?>(user);
        }

        public async Task<(List<User> users, int totalCount)> GetUsersWithPaginationAsync(UserListRequestDto request)
        {
            var query = dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

          
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(u =>
                    (u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.Contains(searchTerm)) ||
                    u.Email.Contains(searchTerm) ||
                    u.Username.Contains(searchTerm) ||
                    (u.CreatedBy != null && u.CreatedBy.Contains(searchTerm)) ||
                    (u.ModifiedBy != null && u.ModifiedBy.Contains(searchTerm))
                );
            }

            // Apply role filter
            if (!string.IsNullOrWhiteSpace(request.RoleFilter))
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == request.RoleFilter));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            var sortBy = request.SortBy?.ToLower() ?? "createdon";
            var sortDirection = request.SortDirection?.ToLower() ?? "desc";

            query = sortBy switch
            {
                "firstname" => sortDirection == "asc"  ? query.OrderBy(u => u.FirstName)  : query.OrderByDescending(u => u.FirstName),
                "lastname" => sortDirection == "asc"   ? query.OrderBy(u => u.LastName)   : query.OrderByDescending(u => u.LastName),
                "email" => sortDirection == "asc"      ? query.OrderBy(u => u.Email)      : query.OrderByDescending(u => u.Email),
                "createdon" => sortDirection == "asc"  ? query.OrderBy(u => u.CreatedOn)  : query.OrderByDescending(u => u.CreatedOn),
                "createdby" => sortDirection == "asc"  ? query.OrderBy(u => u.CreatedBy)  : query.OrderByDescending(u => u.CreatedBy),
                "modifiedon" => sortDirection == "asc" ? query.OrderBy(u => u.ModifiedOn) : query.OrderByDescending(u => u.ModifiedOn),
                "modifiedby" => sortDirection == "asc" ? query.OrderBy(u => u.ModifiedBy) : query.OrderByDescending(u => u.ModifiedBy),

                _ => query.OrderByDescending(u => u.CreatedOn)
            };

           
            var skip = (request.PageNumber - 1) * request.PageSize;
            var users = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            return (users, totalCount);
        }
    }
}
