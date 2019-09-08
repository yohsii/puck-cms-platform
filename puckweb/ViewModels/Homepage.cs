using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using puck.core.Models;
using puck.ViewModels;

namespace puck.ViewModels
{
    public class Homepage:Page
    {
        [Display(Name="Carousel Items")]
        [UIHint("PuckPicker")]
        public List<PuckPicker> CarouselItems { get; set; }
    }
}