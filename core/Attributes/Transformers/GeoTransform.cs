using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using puck.core.Abstract;
using puck.core.Base;
using puck.core.Models;

namespace puck.core.Attributes.Transformers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GeoTransform : Attribute, I_Property_Transformer<GeoPosition, GeoPosition>
    {
        public async Task<GeoPosition> Transform(BaseModel m, string propertyName, string ukey, GeoPosition pos,Dictionary<string,object> dict)
        {
            if(pos.Longitude.HasValue && pos.Latitude.HasValue)
                pos.LatLong = string.Concat(pos.Latitude, ",", pos.Longitude);
            return pos;
        }
    }
}
