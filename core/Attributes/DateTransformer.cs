using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using puck.core.Abstract;
using puck.core.Base;

namespace puck.core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTransformer :Attribute, I_Property_Transformer<DateTime,String>
    {
        public async Task<string> Transform(BaseModel m,string propertyName,string ukey,DateTime dt)
        {
            return dt.ToString("yyyyMMddHHmmss");
        }
                
    }
}
