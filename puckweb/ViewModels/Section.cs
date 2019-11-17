using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace puckweb.ViewModels
{
    public class Section : Page
    {
        [Display(Name ="Section Name",GroupName ="Content")]
        public string SectionName { get; set; }
    }
}