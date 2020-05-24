using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using puck.core.Entities;
using puck.core.Models.EditorSettings.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace puck.core.Models.Admin
{
    public class PuckUserGroupViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [Required]
        [Display(Name = "User Group Name")]
        public string Name { get; set; }

        [Required]
        [UIHint("SettingsRoles")]
        [Display(Name = "Group Permissions")]
        public List<string> Roles { get; set; }

    }
}
