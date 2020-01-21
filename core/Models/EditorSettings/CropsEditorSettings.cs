using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Abstract;
using System.ComponentModel.DataAnnotations;
using puck.core.Attributes;

namespace puck.core.Models.EditorSettings
{
    [FriendlyClassName(Name="Crops Editor Settings")]
    [Display(Name = "Crops Editor Settings")]
    public class CropsEditorSettings:I_Puck_Editor_Settings
    {   
        [UIHint("SettingsCrops")]
        public List<CropInfo> Crops { get; set; }    
    }
    
}
