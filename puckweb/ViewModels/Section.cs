using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace puckweb.ViewModels
{
    /*You don't need this viewmodel (or any of them, although i recommend keeping ImageVM), feel free to modify/delete and make your own*/
    public class Section : Page
    {
        [Display(Name ="Section Name",GroupName ="Content")]
        public string SectionName { get; set; }
    }
}