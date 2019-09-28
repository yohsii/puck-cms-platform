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
    [Display(Name= "Puck Tags Editor Settings")]
    public class PuckTagsEditorSettings:Attribute,I_Puck_Editor_Settings
    {
        public PuckTagsEditorSettings()
        {
            if (string.IsNullOrEmpty(Category)) Category = "";
        }
        public string Category { get; set; }
    }
}
