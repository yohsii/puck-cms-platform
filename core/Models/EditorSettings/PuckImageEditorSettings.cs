using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Abstract;
using System.ComponentModel.DataAnnotations;
using puck.core.Attributes;

namespace puck.core.Models.EditorSettings
{
    [FriendlyClassName(Name="Puck Image Editor Settings")]
    public class PuckImageEditorSettings:I_Puck_Editor_Settings
    {   
        [UIHint("PuckCrops")]
        public List<CropInfo> Crops { get; set; }    
    }
    
}
