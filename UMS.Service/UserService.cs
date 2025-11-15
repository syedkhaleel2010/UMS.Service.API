using System.Security.Cryptography;
using System.Text;
using UMS.Core.Dto;
using UMS.Core.Entities;
using UMS.Service.Interfaces;
using Microsoft.Extensions.Logging;
using UMS.Core.Interfaces;
namespace UMS.Service
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository userRepository;

        public UserService(ILogger<UserService> logger,IUserRepository userRepository)
        {
            _logger = logger;
            this.userRepository = userRepository;
        }
       public async Task<User> CreateUserAsync(RegisterUserDto request)
        {

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            User user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FirstName = request.FirstName,
                LastName=request.LastName,
                CreatedBy= "3929FF4B-E546-4D07-A992-AD7A84DBDD30",
                CreatedOn = DateTime.UtcNow
            };
            return await userRepository.RegisterUser(user);

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

    }
}
