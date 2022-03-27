using puck.core.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.tests.ViewModels
{
    public class Folder:BaseModel
    {
        public int Age { get; set; }
        public bool ShouldShow { get; set; }
        public bool Banned { get; set; }
        public bool Admin { get; set; }
        public DateTime AdminUntil { get; set; }
        public bool SuperAdmin { get; set; }
    }
}
