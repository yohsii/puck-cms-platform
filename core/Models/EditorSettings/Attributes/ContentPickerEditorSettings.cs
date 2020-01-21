using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Abstract;
using System.ComponentModel.DataAnnotations;
using puck.core.Attributes;
using Microsoft.AspNetCore.Mvc;
using puck.core.Abstract.EditorSettings;

namespace puck.core.Models.EditorSettings.Attributes
{
    public enum ContentPickerSelectionType { node, variant, both };
    [Display(Name = "Content Picker Editor Settings")]
    public class ContentPickerEditorSettingsAttribute: Attribute, I_Content_Picker_Settings
    {
        public string StartPathId { get; set; }
        public int MaxPick { get; set; }
        //public string SelectionType { get; set; }
        public bool AllowUnpublished { get; set; }
        //public bool AllowDuplicates { get; set; }
        public List<PuckReference> StartPath { get; set; }
        public string AllowedTypes { get; set; }
        public Type[] Types { get; set; }
    }
}
