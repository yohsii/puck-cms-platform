using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Abstract;
using System.ComponentModel.DataAnnotations;
using puck.core.Attributes;
using Microsoft.AspNetCore.Mvc;
using puck.core.Abstract.EditorSettings;

namespace puck.core.Models.EditorSettings
{
    [Display(Name= "Image Picker Editor Settings")]
    public class ImagePickerEditorSettings:I_Puck_Editor_Settings, I_Image_Picker_Settings
    {
        public ImagePickerEditorSettings()
        {
            if (MaxPick == 0) MaxPick = 1;
        }

        [Display(Name ="Max Pick")]
        public int MaxPick { get; set; }
        
        [UIHint(puck.core.Constants.EditorTemplates.ContentPicker)]
        [Display(Name ="Start Path")]
        [Attributes.ContentPickerEditorSettings(MaxPick =1)]
        public List<PuckReference> StartPath { get; set; }
        
        [Display(Name ="Start Path Id")]
        [HiddenInput(DisplayValue =false)]
        public string StartPathId { get; set; }
    }
}
