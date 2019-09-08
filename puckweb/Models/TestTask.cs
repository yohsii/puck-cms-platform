using puck.core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace puck.Models
{
    public class TestTask:BaseTask
    {
        public string Alias { get; set; }
        public override void Run(CancellationToken t)
        {
            
        }
    }
}