using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using puck.core.Abstract;
using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Concrete
{
    public class PuckContextSQLServer:PuckContext, I_Puck_Context
    {
        public PuckContextSQLServer(IConfiguration config)
            :base(new DbContextOptionsBuilder().UseSqlServer(config.GetConnectionString("SQLServer")).Options) { }
    }
}
