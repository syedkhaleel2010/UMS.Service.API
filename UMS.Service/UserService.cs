using System.Security.Cryptography;
using System.Text;
using System.Linq;
using UMS.Core.Dto;
using UMS.Core.Entities;
using UMS.Service.Interfaces;
using Microsoft.Extensions.Logging;
using UMS.Core.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using UMS.Infrastructure.Data;
using UMS.Core.Helper;
using Microsoft.EntityFrameworkCore;
namespace UMS.Service
{
    public class UserService :BaseService, IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUserSession _userSession;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserPasswordHistoryrRepository _userPasswordHistoryrRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IRoleService _roleService;
        public UserService(ILogger<UserService> logger,IUserRepository userRepository, 
            IMapper mapper, IUserSession userSession,IConfiguration config,
            IUserPasswordHistoryrRepository userPasswordHistoryrRepository, IUnitOfWork unitOfWork,IAuditLogRepository auditLogRepository, IRoleService roleService) : base(userSession)
        {
            _logger = logger;
            _userRepository = userRepository;
            _mapper = mapper;
            _userSession = userSession;
            _config = config;
            _userPasswordHistoryrRepository = userPasswordHistoryrRepository;
            _unitOfWork = unitOfWork;
            _auditLogRepository = auditLogRepository;
            _roleService = roleService;

        }

        public async Task<User?> UserLogin(LoginRequest request)
        {
           
            var user = await GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with invalid username: {Username}", request.Username);
                throw new ArgumentException("Either username or password you have entered is incorrect!!");
            }

            int NoOfLoginAttemptsAllowed;
            if (!int.TryParse(_config["NoOfLoginAttemptsAllowed"], out NoOfLoginAttemptsAllowed))
            {
                NoOfLoginAttemptsAllowed = -1;
            }

            if (NoOfLoginAttemptsAllowed > 0)
            {
                await CheckLoginAttempts(user, request.Password);
            }
            else
            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new ArgumentException("Either username or password you have entered is incorrect!!");
            }


            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("Login attempt with invalid password for user: {Username}", request.Username);
                throw new Exception("Invalid username or password");
            }

            return user;
        }
        public async Task<RegisterUserDto> CreateUserAsync(RegisterUserDto request)
        {

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            request.ID = Guid.NewGuid().ToString();
            var User = _mapper.Map<User>(request);
           
            User.PasswordHash = passwordHash;
            User.PasswordSalt = passwordSalt;
            SetCreatedBy(User);
           
            await _userRepository.RegisterUser(User);

            return request;

        }

        public async Task<RegisterUserWithRolesDto> CreateUserWithRolesAsync(RegisterUserWithRolesDto request)
        {
            // First create the user
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            request.ID = Guid.NewGuid().ToString();
            var user = _mapper.Map<User>(request);
           
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            SetCreatedBy(user);
           
            await _userRepository.RegisterUser(user);

            // Then assign roles if provided
            if (request.RoleIds != null && request.RoleIds.Any())
            {
                foreach (var roleId in request.RoleIds)
                {
                    var userRoleDto = new UserRoleDto
                    {
                        UserId = request.ID,
                        UserName = request.Username,
                        RoleId = roleId
                    };
                    await _roleService.AssignRole(userRoleDto);
                }
            }

            return request;
        }



        public async Task<UpdateUserDto> UpdateUserDetailsAsync(UpdateUserDto request)
        {

            var user = await GetUserByUsernameAsync(request.Username);
            var cloneUser = user.Clone();
            if (user == null) {
                throw new Exception("User not found");
            }
           
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            SetUpdatedBy(user);

            await _userRepository.UpdateUser(user);

            var changes = Utility.EntityChangeHelper.GetEntityChanges(cloneUser, user);

            if (changes.Count > 0)
            {
                var changeHistory = new AuditLog()
                {
                    ID = Guid.NewGuid().ToString(),
                    RowId = request.ID,
                    PerformedFromIp = Utility.GetIP(),
                    UserAction = "Update",
                    DbTable = typeof(User).Name,
                    FieldChanges = Newtonsoft.Json.JsonConvert.SerializeObject(changes),
                    IsActive = true
                };
                SetCreatedBy(changeHistory);

                _auditLogRepository.Add(changeHistory);
            }
            await _unitOfWork.CommitAsync();


            return request;

        }

        public async Task<UpdateUserWithRoleDto> UpdateUserDetailsWithRoleAsync(UpdateUserWithRoleDto request)
        {

            var user = await GetUserByUsernameAsync(request.Username);
            var cloneUser = user.Clone();
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.IsActive = request.IsActive;
            user.IsAccountLocked = request.IsAccountLocked;

            SetUpdatedBy(user);

            _userRepository.AddOrUpdate(user);

            var changes = Utility.EntityChangeHelper.GetEntityChanges(cloneUser, user);

            if (changes.Count > 0)
            {
                var changeHistory = new AuditLog()
                {
                    ID = Guid.NewGuid().ToString(),
                    RowId = request.ID,
                    PerformedFromIp = Utility.GetIP(),
                    UserAction = "Update",
                    DbTable = typeof(User).Name,
                    FieldChanges = Newtonsoft.Json.JsonConvert.SerializeObject(changes),
                    IsActive = true
                };
                SetCreatedBy(changeHistory);

                _auditLogRepository.Add(changeHistory);
            }

             changes = Utility.EntityChangeHelper.GetEntityChanges(cloneUser.UserRoles, user.UserRoles);

            if (changes.Count > 0)
            {
                var changeHistory = new AuditLog()
                {
                    ID = Guid.NewGuid().ToString(),
                    RowId = request.ID,
                    PerformedFromIp = Utility.GetIP(),
                    UserAction = "Update",
                    DbTable = typeof(UserRole).Name,
                    FieldChanges = Newtonsoft.Json.JsonConvert.SerializeObject(changes),
                    IsActive = true
                };
                SetCreatedBy(changeHistory);

                _auditLogRepository.Add(changeHistory);
            }

            await _unitOfWork.CommitAsync();


            return request;

        }
        public async  Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

       public async  Task<User?> GetUserByUsernameAsync(string username)
        {
         return await _userRepository.GetUserByUsernameAsync(username);
        }

        public async Task<GetUserByIdResponseDto?> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            return new GetUserByIdResponseDto
            {
                Username = user.Username,
                Email = user.Email,
                Password = null, 
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                IsAccountLocked = user.IsAccountLocked.HasValue ? user.IsAccountLocked.Value : false,
                RoleIds = user.UserRoles?.Select(ur => ur.RoleId).ToList() ?? new List<string>()
            };
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                // compare byte by byte
                return computedHash.SequenceEqual(storedHash);
            }
        }


        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key; 
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> DeleteUser(DeleteUserDto request)
        {
            var user = await GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            user.IsActive = false;
            SetUpdatedBy(user);
            await _userRepository.DeleteUser(user);
            return true;
        }

        public async Task ResetUserPassword(UserPasswordResetRequest request, string modifyingUser)
        {
            try
            {
                var externalUserData = await GetUserByUsernameAsync(request.Username);

                if (!request.IsAdmin && !string.IsNullOrEmpty(request.OldPassword) && externalUserData != null)
                {
                    CreatePasswordHash(request.OldPassword, out byte[] passwordHash, out byte[] passwordSalt);

                    if (externalUserData.PasswordHash != passwordHash)
                        throw new ArgumentException("Invalid Old password");
                }
                if (string.IsNullOrEmpty(request.Password) ||
                    string.IsNullOrEmpty(request.ConfirmPassword) ||
                    !request.Password.Equals(request.ConfirmPassword))
                {
                    throw new ArgumentException("Invalid New Password");
                }


                CreatePasswordHash(request.Password, out byte[] _passwordHash, out byte[] _passwordSalt);
                await CheckPasswordMatch(externalUserData.ID, _passwordHash.ToString());

                

              
                externalUserData.PasswordHash = _passwordHash;
                externalUserData.PasswordSalt = _passwordSalt;
                SetUpdatedBy(externalUserData);
                _userRepository.AddOrUpdate(externalUserData);

                _userPasswordHistoryrRepository.Add(new USER_PASSWORD_HISTORY()
                {
                    ID = Guid.NewGuid().ToString(),
                    UserId = externalUserData.ID,
                    PasswordHash = externalUserData.PasswordHash.ToString(),
                    UserEmail = externalUserData.Email,
                    CreatedBy = modifyingUser ?? "ADMTSYATEM",
                    CreatedOn = DateTime.UtcNow
                });
                //Commit changes
                await _unitOfWork.CommitAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task CheckPasswordMatch(string ID, string password)
        {
            var getAllOldPasswords = _userPasswordHistoryrRepository.GetAll(x => x.UserId == ID).OrderByDescending(x => x.CreatedOn);

            int PasswordReuseLimit;
            if (!int.TryParse(_config["PasswordReuseLimit"], out PasswordReuseLimit))
            {
                PasswordReuseLimit = 0;
            }

            if (getAllOldPasswords.Count() > 0)
            {
                if (getAllOldPasswords.Take(PasswordReuseLimit).Any(x => x.PasswordHash == password))
                {
                    throw new ArgumentException("Your new password must be different from your previous ##PasswordReuseLimit## passwords".Replace("##PasswordReuseLimit##", Convert.ToString(PasswordReuseLimit)));
                }
                if (getAllOldPasswords.Count() >= PasswordReuseLimit)
                {
                    var oldPasswordsToRemove = getAllOldPasswords.Skip(PasswordReuseLimit - 1).ToList();
                    foreach (var oldPwd in oldPasswordsToRemove)
                    {
                        _userPasswordHistoryrRepository.Delete(oldPwd);
                    }
                }
            }
        }
        private async Task CheckLoginAttempts(User User, string Password)
        {
            if (User.IsAccountLocked == true)
            {
                throw new ArgumentException("Account Locked, Please contact administrator!!");
            }
            SetUpdatedBy(User);
            if (!VerifyPassword(Password, User.PasswordHash, User.PasswordSalt))
            {
                int NoOfLoginAttemptsAllowed;
                if (!int.TryParse(_config["NoOfLoginAttemptsAllowed"], out NoOfLoginAttemptsAllowed))
                {
                    NoOfLoginAttemptsAllowed = -1;
                }

                User.NumOfAttempt = User.NumOfAttempt.HasValue ? (User.NumOfAttempt.Value + 1) : 1;

                if (User.NumOfAttempt == NoOfLoginAttemptsAllowed)
                {
                    User.IsAccountLocked = true;
                }
                
                
                await _userRepository.UpdateUser(User);
               

                if (User.IsAccountLocked is not null && User.IsAccountLocked.Value)
                {
                    throw new ArgumentException("Account Locked, Please contact administrator!!");
                }
                else
                {
                    throw new ArgumentException("Incorrect password.You have ##remaingAttempts## more attempts before your account is locked".
                        Replace("##remaingAttempts##", Convert.ToString(NoOfLoginAttemptsAllowed - User.NumOfAttempt)));
                }
            }
            else
            {
                User.NumOfAttempt = 0;
                await _userRepository.UpdateUser(User);
            }
        }

        public async Task<PagedUserListResponseDto> GetUsersListAsync(UserListRequestDto request)
        {
            var (users, totalCount) = await _userRepository.GetUsersWithPaginationAsync(request);

            var userList = users.Select(u => new UserListResponseDto
            {
                ID=u.ID,
                Username=u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                CreatedOn = u.CreatedOn,
                CreatedBy = u.CreatedBy,
                ModifiedOn = u.ModifiedOn,
                ModifiedBy = u.ModifiedBy,
                IsAccountLocked = u.IsAccountLocked,
                NumOfAttempt = u.NumOfAttempt,
                IsActive= u.IsActive,
                Roles = u.UserRoles?.Select(ur => ur.Role?.RoleName).Where(r => r != null).ToList() ?? new List<string>()
            }).ToList();

            return new PagedUserListResponseDto
            {
                Users = userList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

    }
}
