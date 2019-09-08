using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Attributes
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface)]
    public class FriendlyClassNameAttribute:Attribute
    {
        public string Name { get; set; }
    }
}
