using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PuckHint : Attribute
    {
        public string Name { get; set; }
    }
}
