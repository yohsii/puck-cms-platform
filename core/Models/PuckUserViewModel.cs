using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using puck.core.Entities;

namespace puck.core.Models
{
    public class PuckUserViewModel
    {
        public PuckUser User { get; set; }
        
        //[Required]
        public string UserName { get; set; }

        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string CurrentEmail { get; set; }

        [System.ComponentModel.DataAnnotations.EmailAddress]
        [Required]
        public string Email { get; set; }
        
        [UIHint("SettingsRoles")]
        public List<string> Roles { get; set; }

        [UIHint("SettingsUserVariant")]
        [Display(Name="User Language")]
        public string UserVariant { get; set;}

        [UIHint("PuckPicker")]
        public List<PuckPicker> StartNode { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password")]
        public string PasswordConfirm { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword")]
        public string NewPasswordConfirm { get; set; }

    }
}
