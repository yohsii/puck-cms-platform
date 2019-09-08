using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace puck.core.Abstract
{
    interface I_Puck_Cache
    {
        void Add(string key,object value,int minutes);
        void Add(string key,object value);
        void Remove(string key);
    }
}
