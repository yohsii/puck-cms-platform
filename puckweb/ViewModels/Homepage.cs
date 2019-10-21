using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using puck.core.Constants;
using puck.core.Models;
//using puck.ViewModels;

namespace puckweb.ViewModels
{
    public class Homepage:Page
    {
        [Display(Name="Carousel Items",GroupName ="Content")]
        [UIHint(EditorTemplates.PuckPicker)]
        public List<PuckPicker> CarouselItems { get; set; }
    }
}