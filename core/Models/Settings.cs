using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace puck.core.Models
{
    public class Settings
    {
        [Display(Name="UI Language")]
        [UIHint("SettingsDefaultLanguage")]
        public String DefaultLanguage { get; set; }
        
        [Display(Name="Languages")]
        [UIHint("SettingsLanguages")]
        public List<String> Languages { get; set; }

        [Display(Name="SettingsRedirect",ResourceType=typeof(puck.core.Globalisation.PuckLabels))]
        [UIHint("SettingsRedirect")]
        public Dictionary<string, string> Redirect { get; set; }

        [Display(Name="Path to locale mapping")]
        public Dictionary<string, string> PathToLocale { get; set; }

        [Display(Name="Enable Locale Prefix")]
        public bool EnableLocalePrefix { get; set; }

        [Display(Name="Organize fields into Groups")]
        [UIHint("SettingsFieldGroups")]
        public List<string> TypeGroupField { get; set; }
        
        [Display(Name="Allowed child models for a given model")]
        [UIHint("SettingsTypeAllowedTypes")]
        public List<string> TypeAllowedTypes { get; set; }

        [Display(Name = "Allowed templates for a given model")]
        [UIHint("SettingsAllowedTemplates")]
        public List<string> TypeAllowedTemplates { get; set; }

        [Display(Name="Editor Parameters")]
        [UIHint("SettingsEditorParameters")]
        public List<string> EditorParameters { get; set; }

        [Display(Name="Cache Policy")]
        [UIHint("SettingsCachePolicy")]
        public List<string> CachePolicy { get; set; }

        [Display(Name = "Orphaned Models")]
        [UIHint("SettingsOrphans")]
        public Dictionary<string,string> Orphans { get; set; }

    }
}
