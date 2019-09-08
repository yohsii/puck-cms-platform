using Microsoft.AspNetCore.Mvc.Rendering;
using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace puck.core.Models
{
    public class ChangeType
    {
        public Guid ContentId { get; set; }
        public PuckRevision Revision { get; set; }
        public Type ContentType { get; set; }
        public Type NewType { get; set; }
        public List<PropertyInfo> ContentProperties { get; set; }
        public List<PropertyInfo> NewTypeProperties { get; set; }
        public List<FileInfo> Templates { get; set; }
        public List<SelectListItem> TemplatesSelectListItems { get; set; }
        [Required]
        public string _SelectedTemplate { get; set; }
    }
}
