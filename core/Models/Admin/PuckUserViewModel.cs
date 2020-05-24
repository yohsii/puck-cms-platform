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
        public string StartPaths { get; set; }
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
        
        public List<PuckUserGroupViewModel> CurrentUserGroups { get; set; }

        [UIHint("SettingsUserGroups")]
        [Display(Name = "User Groups")]
        public string UserGroups { get; set; }

        [Required]
        [UIHint("SettingsRoles")]
        [Display(Name = "Permissions")]
        public List<string> Roles { get; set; }

        [UIHint("SettingsUserVariant")]
        [Display(Name="User Language")]
        public string UserVariant { get; set;}

        [UIHint(puck.core.Constants.EditorTemplates.ContentPicker)]
        [ContentPickerEditorSettings(MaxPick = 100)]
        [Display(Name ="Start Nodes")]
        public List<PuckReference> StartNodes { get; set; }
        
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
