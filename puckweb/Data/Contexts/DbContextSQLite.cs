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
    /*don't add your entities to this db context, add them to ApplicationDbContext. if targeting SQLite, use this db context for your migrations*/
    public class DbContextSQLite : ApplicationDbContext
    {
        public DbContextSQLite(IConfiguration config)
            : base(new DbContextOptionsBuilder().UseSqlite(config.GetConnectionString("SQLite")).Options) 
        { 
        
        }
        
    }
}
