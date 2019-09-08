using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Abstract;
using puck.core.Base;

namespace puck.core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultGUIDTransformer:Attribute,I_Property_Transformer<Guid,string>
    {
        public string Transform(BaseModel m,string propertyName,string ukey,Guid p) {
            return p.ToString();
            //if (p == default(Guid))
            //    return Guid.NewGuid().ToString();
            //else
            //    return p.ToString();
        }
    }
}
