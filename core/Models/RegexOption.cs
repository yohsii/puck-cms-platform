using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using puck.core.Abstract;

namespace puck.core.Models
{
    [puck.core.Attributes.FriendlyClassName(Name="Regex Option")]
    [Display(Name = "Regex Option")]
    public class RegexOption : I_GeneratedOption
    {
        [Display(Name="Regex Rule")]
        public string RegexString { get; set; }

        string I_GeneratedOption.OutputString()
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(RegexString))
                result = string.Concat("[RegularExpression(\"", RegexString, "\")]");
            return result;
        }
        
    }
}
