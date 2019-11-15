using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using puck.core.Abstract;
using puck.core.Base;

namespace puck.core.Attributes.Transformers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTransformer :Attribute, I_Property_Transformer<DateTime,String>
    {
        public async Task<string> Transform(BaseModel m,string propertyName,string ukey,DateTime dt,Dictionary<string,object> dict)
        {
            return dt.ToString("yyyyMMddHHmmss");
        }
                
    }
}
