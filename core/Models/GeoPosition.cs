using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using puck.core.Abstract;
using puck.core.Attributes;
namespace puck.core.Models
{
    [GeoTransform()]
    public class GeoPosition
    {
        [IndexSettings(Ignore=true)]
        public double? Longitude { get; set; }

        [IndexSettings(Ignore = true)]
        public double? Latitude { get; set; }
        
        [HiddenInput(DisplayValue=false)]
        [IndexSettings(Spatial=true)]
        public string LatLong { get; set; }
    }
}
