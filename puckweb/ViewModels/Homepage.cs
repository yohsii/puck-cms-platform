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
        //this is a content picker, the settings attribute specifies maximum number of selections and the types of content you're allowed to select
        [PuckPickerEditorSettings(MaxPick =5,Types =new Type[] {typeof(Homepage),typeof(Section),typeof(Page)})]
        [UIHint(EditorTemplates.PuckPicker)]
        public List<PuckPicker> CarouselItems { get; set; }

        [Display(GroupName = "Selects")]
        [UIHint(EditorTemplates.SelectList)]
        //select item Label and Value are separated with ":" by default, you can change this using the Separator property of the SelectListSettings attribute below
        //if the Label and Value are the same, you can simply specify the Label only
        [SelectListSettings(Values = new string[] { "London:UK", "Paris", "Tokyo:Japan" })]
        public string City { get; set; }

        [Display(GroupName = "Selects")]
        [UIHint(EditorTemplates.MultiSelectList)]
        [SelectListSettings(Values = new string[] { "London:UK", "Paris", "Tokyo:Japan" })]
        public List<string> Cities { get; set; }

    }
}