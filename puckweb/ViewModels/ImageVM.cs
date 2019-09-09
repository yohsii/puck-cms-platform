using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using puck.core.Attributes;
using puck.core.Base;
using puck.Models;
namespace puckweb.ViewModels
{
    [FriendlyClassName(Name="Image")]
    /*Image View Model*/
    public class ImageVM:BaseModel
    {
        [Display(GroupName ="Content")]
        public PuckImage Image { get; set; }
    }
}