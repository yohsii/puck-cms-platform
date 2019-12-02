using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using puck.core.Constants;
using puck.core.Models;
using puck.core.Models.EditorSettings.Attributes;

namespace puckweb.ViewModels
{
    /*You don't need this viewmodel (or any of them, although i recommend keeping ImageVM), feel free to modify/delete and make your own*/
    public class Homepage:Page
    {
        [Display(Name="Carousel Items",GroupName ="Content")]
        [PuckPickerEditorSettings(MaxPick =5)]
        [UIHint(EditorTemplates.PuckPicker)]
        
        public List<PuckPicker> CarouselItems { get; set; }
    }
}