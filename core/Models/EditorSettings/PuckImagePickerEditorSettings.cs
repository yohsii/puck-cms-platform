using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Abstract;
using System.ComponentModel.DataAnnotations;
using puck.core.Attributes;

namespace puck.core.Models.EditorSettings
{
    [FriendlyClassName(Name="Puck Image Picker Editor Settings")]
    [Display(Name= "Puck Image Picker Editor Settings")]
    public class PuckImagePickerEditorSettings:Attribute,I_Puck_Editor_Settings
    {   
        [UIHint("PuckPicker")]
        public List<PuckPicker> StartPath { get; set; }
        public int MaxPick { get; set; }
    }
}
