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
    /*don't add your entities to this db context, add them to ApplicationDbContext. if targeting SQLServer, use this db context for your migrations*/
    public class DbContextSQLServer : ApplicationDbContext
    {
        public DbContextSQLServer(IConfiguration config)
            : base(new DbContextOptionsBuilder().UseSqlServer(config.GetConnectionString("SQLServer")).Options) 
        { 
        
        }
        
    }
}
