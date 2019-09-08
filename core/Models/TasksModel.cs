using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using puck.core.Base;
using puck.core.Entities;
using puck.core.Helpers;

namespace puck.core.Models
{
    public class TasksModel
    {
        public List<BaseTask> Tasks {get;set;}

        [Display(Name = "Generated Models")]
        [UIHint("SettingsGeneratedModels")]
        public List<GeneratedModel> GeneratedModels { get; set; }
        
    }
}
