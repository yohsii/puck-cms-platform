using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using puck.core.Abstract;
using puck.core.Base;

namespace puck.core.Attributes.Transformers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultIFormFileTransformer : Attribute,I_Property_Transformer<IFormFile,IFormFile>
    {
        public async Task<IFormFile> Transform(BaseModel m,string propertyName,string ukey,IFormFile p,Dictionary<string,object> dict) {
            p = null;
            return p;
        }
    }
}
