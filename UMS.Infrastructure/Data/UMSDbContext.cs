using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Entities;

namespace UMS.Infrastructure.Data
{
    public class UMSDbContext :DbContext
    {
        public UMSDbContext(DbContextOptions<UMSDbContext> options) : base(options)
        {

            
        }

        public DbSet<User> users { get; set; }
    }
}
