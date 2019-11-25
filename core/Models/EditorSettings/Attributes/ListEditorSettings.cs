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
    [Display(Name = "List Editor Settings")]
    public class ListEditorSettingsAttribute: Attribute
    {
        public string StringEditorTemplate { get; set; }
    }
}
