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
    /*Image View Model, you might want to keep this as there's a handy image picker that by default can select content using this viewmodel*/
    public class ImageVM:BaseModel
    {
        [Display(GroupName ="Content")]
        [PuckImageTransformer()]
        public PuckImage Image { get; set; }
    }
}