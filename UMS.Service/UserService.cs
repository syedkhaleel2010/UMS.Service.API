using System.Security.Cryptography;
using System.Text;
using UMS.Core.Dto;
using UMS.Core.Entities;
using UMS.Service.Interfaces;
using Microsoft.Extensions.Logging;
using UMS.Core.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using UMS.Infrastructure.Data;
namespace UMS.Service
{
    public class UserService :BaseService, IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository userRepository;
        private readonly IMapper _mapper;
        private readonly IUserSession _userSession;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserPasswordHistoryrRepository _userPasswordHistoryrRepository;

        public UserService(ILogger<UserService> logger,IUserRepository userRepository, 
            IMapper mapper, IUserSession userSession,IConfiguration config,
            IUserPasswordHistoryrRepository userPasswordHistoryrRepository, IUnitOfWork unitOfWork) : base(userSession)
        {
            _logger = logger;
            this.userRepository = userRepository;
            _mapper = mapper;
            _userSession = userSession;
            _config = config;
            _userPasswordHistoryrRepository = userPasswordHistoryrRepository;
            _unitOfWork = unitOfWork;
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
           
            await userRepository.RegisterUser(User);

            return request;

        }



        public async Task<RegisterUserDto> UpdateUserAsync(RegisterUserDto request)
        {

            var user = await GetUserByUsernameAsync(request.Username);

            if (user == null) {
                throw new Exception("User not found");
            }
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.IsActive = request.IsActive;

            SetUpdatedBy(user);

            await userRepository.RegisterUser(user);

            return request;

        }
        public async  Task<User?> GetUserByEmailAsync(string email)
        {
            return await this.userRepository.GetUserByEmailAsync(email);
        }

       public async  Task<User?> GetUserByUsernameAsync(string username)
        {
         return await this.userRepository.GetUserByUsernameAsync(username);
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
            await userRepository.DeleteUser(user);
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
                this.userRepository.AddOrUpdate(externalUserData);

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
                 await this.userRepository.UpdateUser(User);
               

                if (User.IsAccountLocked.Value)
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
                await this.userRepository.UpdateUser(User);
                
            }
        }

    }
}
