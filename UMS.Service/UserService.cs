using System.Security.Cryptography;
using System.Text;
using UMS.Core.Dto;
using UMS.Core.Entities;
using UMS.Service.Interfaces;
using Microsoft.Extensions.Logging;
using UMS.Core.Interfaces;
using AutoMapper;
namespace UMS.Service
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository userRepository;
        private readonly IMapper _mapper;

        public UserService(ILogger<UserService> logger,IUserRepository userRepository, IMapper mapper)
        {
            _logger = logger;
            this.userRepository = userRepository;
            _mapper = mapper;
        }
       public async Task<RegisterUserDto> CreateUserAsync(RegisterUserDto request)
        {

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            request.ID = Guid.NewGuid().ToString();
            var User = _mapper.Map<User>(request);
           
            User.PasswordHash = passwordHash;
            User.PasswordSalt = passwordSalt;
            User.CreatedBy = "SYSTEM";
            User.CreatedOn = DateTime.UtcNow;
           
            await userRepository.RegisterUser(User);

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

    }
}
