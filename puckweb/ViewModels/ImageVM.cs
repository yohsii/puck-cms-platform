using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using puck.core.Attributes;
using puck.core.Attributes.Transformers;
using puck.core.Base;
using puck.core.Models;
namespace puckweb.ViewModels
{
    [Display(Name="Image")]
    /*Image View Model*/
    public class ImageVM:BaseModel
    {
        [Display(GroupName ="Content")]
        [PuckImageTransformer()]
        public PuckImage Image { get; set; }
    }
}