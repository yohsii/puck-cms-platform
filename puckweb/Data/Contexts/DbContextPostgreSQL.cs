using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using puckweb.Data.Entities;

namespace puckweb.Data.Contexts
{
    public class DbContextPostgreSQL : ApplicationDbContext
    {
        public DbContextPostgreSQL(IConfiguration config)
            : base(new DbContextOptionsBuilder().UseNpgsql(config.GetConnectionString("PostgreSQL")).Options) 
        { 
        
        }
        
    }
}
