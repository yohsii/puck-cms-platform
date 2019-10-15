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
    public enum PuckPickerSelectionType { node, variant, both };
    [Display(Name = "Puck Picker Editor Settings")]
    public class PuckPickerEditorSettingsAttribute: Attribute, I_Puck_Picker_Settings
    {
        public string StartPathId { get; set; }
        public int MaxPick { get; set; }
        //public string SelectionType { get; set; }
        public bool AllowUnpublished { get; set; }
        //public bool AllowDuplicates { get; set; }
        public List<PuckPicker> StartPath { get; set; }
    }
}
