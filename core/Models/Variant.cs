using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace puck.core.Models
{
    public class Variant
    {
        public string FriendlyName{get;set;}
        public string Key { get; set; }
        public bool IsDefault { get; set; }
    }
}
