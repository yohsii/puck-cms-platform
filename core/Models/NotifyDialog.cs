using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Models
{
    public class Notify
    {
        public List<string> Actions { get; set; }
        public bool Recursive { get; set; }
        public string Path { get; set; }
        public List<string> Users { get; set; }
        //public List<string> AllUsers { get; set; }
        //public List<string> AllActions { get; set; }
        public IEnumerable<SelectListItem> AllUsers { get; set; }
        public IEnumerable<SelectListItem> AllActions { get; set; }
    }
}
