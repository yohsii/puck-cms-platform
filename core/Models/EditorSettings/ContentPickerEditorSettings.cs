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
    public enum ContentPickerSelectionType { node, variant, both };
    [Display(Name = "Content Picker Editor Settings")]
    public class ContentPickerEditorSettings:I_Puck_Editor_Settings, I_Content_Picker_Settings
    {
        public ContentPickerEditorSettings() {
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

        [UIHint(puck.core.Constants.EditorTemplates.ContentPicker)]
        [Display(Name="Start Path")]
        [Attributes.ContentPickerEditorSettings(MaxPick =1)]
        public List<PuckReference> StartPath { get; set; }
        
        [HiddenInput(DisplayValue =false)]
        [Display(Name = "Start Path Id")]
        public string StartPathId { get; set; }

        [Display(Name ="Allowed Types",Description ="Comma-separated")]
        public string AllowedTypes { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Type[] Types { get; set; }
    }
}
