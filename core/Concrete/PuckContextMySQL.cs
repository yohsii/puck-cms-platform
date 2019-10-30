using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using puck.core.Abstract;
using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Concrete
{
    public class PuckContextMySQL:PuckContext, I_Puck_Context
    {
        public PuckContextMySQL(IConfiguration config)
            :base(new DbContextOptionsBuilder().UseMySql(config.GetConnectionString("MySQL")).Options) { }
    }
}
