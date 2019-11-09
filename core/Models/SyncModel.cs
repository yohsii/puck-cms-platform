using puck.core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace puck.core.Models
{
    public class SyncModel
    {
        public SyncModel() {
            //IncludeVariants = true;
            OnlyOverwriteIfNewer = true;
        }
        public List<ConfigContainer> Configs { get; set; }
        public BaseModel Model { get; set; }
        
        //[Display(Name ="Include Variants")]
        //public bool IncludeVariants { get; set; }
        
        [Display(Name = "Include Descendants")]
        public bool IncludeDescendants { get; set; }
        
        [Display(Name ="Only Overwrite If Newer")]
        public bool OnlyOverwriteIfNewer { get; set; }

        public string SelectedConfig { get; set; }

        public Guid Id { get; set; }
    }
}
