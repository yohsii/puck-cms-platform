using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Base;

namespace puck.core.Abstract
{
    public interface I_Property_Transformer<TIn,TOut>
    {
        TOut Transform(BaseModel m,string propertyName,string uniquePropertyName,TIn p);
    }
}
