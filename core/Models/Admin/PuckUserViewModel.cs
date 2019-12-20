using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using puck.core.Entities;
using puck.core.Models.EditorSettings.Attributes;

namespace puck.core.Models.Admin
{
    public class PuckUserViewModel
    {
        public string StartPath { get; set; }
        public PuckUser User { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string LastLoginDateString { get; set; }
        //[Required]
        [Display(Name ="Username")]
        public string UserName { get; set; }
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string CurrentEmail { get; set; }

        [System.ComponentModel.DataAnnotations.EmailAddress]
        [Required]
        public string Email { get; set; }
        [Display(Name ="First Name")]
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string Surname { get; set; }
        [UIHint("SettingsRoles")]
        public List<string> Roles { get; set; }

        [UIHint("SettingsUserVariant")]
        [Display(Name="User Language")]
        public string UserVariant { get; set;}

        [UIHint("PuckPicker")]
        [PuckPickerEditorSettings(MaxPick = 1)]
        [Display(Name ="Start Node")]
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
