using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using puck.core.Constants;
using puck.core.Models;
using puck.core.Models.EditorSettings.Attributes;
//using puck.ViewModels;

namespace puckweb.ViewModels
{
    public class Homepage:Page
    {
        [Display(Name="Carousel Items",GroupName ="Content")]
        [PuckPickerEditorSettings(MaxPick =5)]
        [UIHint(EditorTemplates.PuckPicker)]
        
        public List<PuckPicker> CarouselItems { get; set; }
    }
}