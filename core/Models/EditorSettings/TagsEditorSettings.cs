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
    [Display(Name= "Tags Editor Settings")]
    public class TagsEditorSettings:Attribute,I_Puck_Editor_Settings
    {
        public TagsEditorSettings()
        {
            if (string.IsNullOrEmpty(Category)) Category = "";
        }
        public string Category { get; set; }
    }
}
