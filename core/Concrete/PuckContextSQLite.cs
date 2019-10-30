using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using puck.core.Abstract;
using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Concrete
{
    public class PuckContextSQLite:PuckContext, I_Puck_Context
    {
        public PuckContextSQLite(IConfiguration config)
            :base(new DbContextOptionsBuilder().UseSqlite(config.GetConnectionString("SQLite")).Options) { }
    }
}
