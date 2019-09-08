using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace puck.core.Models
{
    public class CreateTemplate
    {
        [RegularExpression(@"^[\w\-. \(\)]+$", ErrorMessage = "Invalid file name")]
        [Required]
        public string Name { get; set; }
        public string Path { get; set; }
        [Display(Name="Model")]
        public string TemplateModel { get; set; }
    }
    public class CreateFolder {
        [RegularExpression(@"^[\w\-. ]+$", ErrorMessage = "Invalid folder name")]
        [Required]
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
