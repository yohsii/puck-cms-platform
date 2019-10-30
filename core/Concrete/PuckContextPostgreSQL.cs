using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using puck.core.Abstract;
using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Concrete
{
    public class PuckContextPostgreSQL:PuckContext, I_Puck_Context
    {
        public PuckContextPostgreSQL(IConfiguration config)
            :base(new DbContextOptionsBuilder().UseNpgsql(config.GetConnectionString("PostgreSQL")).Options) { }
    }
}
