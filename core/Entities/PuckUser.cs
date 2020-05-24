using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace puck.core.Entities
{
    public class PuckUser : IdentityUser
    {
        public PuckUser() {
            
        }
        public DateTime? PuckLastLoginDate { get; set; }
        public string PuckFirstName { get; set; }
        public string PuckSurname { get; set; }
        [UIHint("SettingsUserVariant")]
        [Display(Name="User Language")]
        public string PuckUserVariant { get; set;}
        public string PuckStartNodeIds { get; set; }
        public string PuckUserGroups { get; set; }

        public virtual ICollection<PuckUserRole> Roles { get; } = new List<PuckUserRole>();
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; } = new List<IdentityUserClaim<string>>();
        public virtual ICollection<IdentityUserLogin<string>> Logins { get; } = new List<IdentityUserLogin<string>>();
    }
    public class PuckRole : IdentityRole { 
        public virtual ICollection<PuckUserRole> UserRoles { get; set; }
    }
    public class PuckUserRole : IdentityUserRole<string>
    {
        public virtual PuckUser User { get; set; }
        public virtual PuckRole Role { get; set; }
    }

}
