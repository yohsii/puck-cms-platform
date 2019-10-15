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
    public enum PuckPickerSelectionType { node, variant, both };
    [Display(Name = "Puck Picker Editor Settings")]
    public class PuckPickerEditorSettings:I_Puck_Editor_Settings, I_Puck_Picker_Settings
    {
        public PuckPickerEditorSettings() {
            if (MaxPick == 0) MaxPick = 1;
        }
        [Display(Name ="Max Pick")]
        public int MaxPick { get; set; }
        
        //[Display(Name ="Selection Type")]
        //[UIHint("PuckPickerSelectionType")]
        //public string SelectionType { get; set; }
        
        [Display(Name ="Allow Unpublished")]
        public bool AllowUnpublished { get; set; }
        //public bool AllowDuplicates { get; set; }

        [UIHint("PuckPicker")]
        [Display(Name="Start Path")]
        [Attributes.PuckPickerEditorSettings(MaxPick =1)]
        public List<PuckPicker> StartPath { get; set; }
        
        [HiddenInput(DisplayValue =false)]
        [Display(Name = "Start Path Id")]
        public string StartPathId { get; set; }
    }
}
