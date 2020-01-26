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
    /*don't add your entities to this db context, add them to ApplicationDbContext. if targeting PostgreSQL, use this db context for your migrations*/
    public class DbContextPostgreSQL : ApplicationDbContext
    {
        public DbContextPostgreSQL(IConfiguration config)
            : base(new DbContextOptionsBuilder().UseNpgsql(config.GetConnectionString("PostgreSQL")).Options) 
        { 
        
        }
        
    }
}
