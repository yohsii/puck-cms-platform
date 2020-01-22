using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using puck.core.Constants;
using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Identity
{
    public class PuckClaimsPrincipalFactory : UserClaimsPrincipalFactory<PuckUser,PuckRole>
    {
        public PuckClaimsPrincipalFactory(UserManager<PuckUser> userManager,RoleManager<PuckRole> roleManager,IOptions<IdentityOptions> optionsAccessor) 
            : base(userManager, roleManager, optionsAccessor){

        }

        public async override Task<ClaimsPrincipal> CreateAsync(PuckUser user){
            var principal = await base.CreateAsync(user);
            if (string.IsNullOrEmpty(user.PuckStartNodeIds)) return principal;
            ((ClaimsIdentity)principal.Identity).AddClaims(
                user.PuckStartNodeIds.Split(',',StringSplitOptions.RemoveEmptyEntries)
                    .Select(x=>new Claim(Claims.PuckStartId,x)).ToArray()
            );
            return principal;
        }
    }
}
