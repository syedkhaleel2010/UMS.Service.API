using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Entities;
using UMS.Service.Interfaces;

namespace UMS.Service
{
    public class UserSession : IUserSession
    {
        private readonly HttpContext _context;

        public UserSession(IHttpContextAccessor context)
        {
            _context = context.HttpContext;
          
        }

        public User GetUser()
        {
            if (_context == null)
            {
                return null;
            }

            ClaimsIdentity claimsIdentity = _context.User?.Identity as ClaimsIdentity;
            
            User result = null;
            if (claimsIdentity?.Claims != null && claimsIdentity.Claims.Any() && !string.IsNullOrEmpty(claimsIdentity.Name))
            {
                User user = new User();
                user.Username = claimsIdentity.Name;
                result = user;
            }

            return result;
        }


    }
}
