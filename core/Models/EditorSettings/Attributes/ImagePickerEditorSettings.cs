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
    [Display(Name= "Image Picker Editor Settings")]
    public class ImagePickerEditorSettingsAttribute:Attribute, I_Image_Picker_Settings
    {
        public string StartPathId { get; set; }
        public int MaxPick { get; set; }
        public List<PuckReference> StartPath { get; set; }
    }
}
