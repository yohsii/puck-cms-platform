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
    [Display(Name= "Puck Image Picker Editor Settings")]
    public class PuckImagePickerEditorSettings:I_Puck_Editor_Settings, I_Puck_Image_Picker_Settings
    {
        public PuckImagePickerEditorSettings()
        {
            if (MaxPick == 0) MaxPick = 1;
        }

        [Display(Name ="Max Pick")]
        public int MaxPick { get; set; }
        
        [UIHint("PuckPicker")]
        [Display(Name ="Start Path")]
        [Attributes.PuckPickerEditorSettings(MaxPick =1)]
        public List<PuckPicker> StartPath { get; set; }
        
        [Display(Name ="Start Path Id")]
        [HiddenInput(DisplayValue =false)]
        public string StartPathId { get; set; }
    }
}
