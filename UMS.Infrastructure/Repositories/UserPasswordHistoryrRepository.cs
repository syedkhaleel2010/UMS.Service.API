using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Entities;
using UMS.Core.Interfaces;
using UMS.Infrastructure.Data;

namespace UMS.Infrastructure.Repositories
{
    public class UserPasswordHistoryrRepository :Repository<USER_PASSWORD_HISTORY>, IUserPasswordHistoryrRepository
    {
        public UserPasswordHistoryrRepository(UMSDbContext context) : base(context)
        {
        }
    }
}
