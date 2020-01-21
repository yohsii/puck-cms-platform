using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace puck.core.Models.EditorSettings.Attributes
{
    [Display(Name ="Text Area Editor Settings")]
    public class TextAreaEditorSettings : Attribute
    {
        public string Width { get; set; }
        public string Height { get; set; }
    }
}
