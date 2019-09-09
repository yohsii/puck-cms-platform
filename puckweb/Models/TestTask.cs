using puck.core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace puck.Models
{
    public class TestTask:BaseTask
    {
        public string Alias { get; set; }
        public override async Task Run(CancellationToken t)
        {
            
        }
    }
}