using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using puck.core.Abstract;
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
        private readonly I_Puck_Repository repo;
        public PuckClaimsPrincipalFactory(UserManager<PuckUser> userManager,RoleManager<PuckRole> roleManager,IOptions<IdentityOptions> optionsAccessor,I_Puck_Repository repo) 
            : base(userManager, roleManager, optionsAccessor){
            this.repo = repo;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(PuckUser user){
            var principal = await base.CreateAsync(user);
            if (string.IsNullOrEmpty(user.PuckStartNodeIds)) return principal;

            var ids = user.PuckStartNodeIds.Split(',', System.StringSplitOptions.RemoveEmptyEntries).Select(x => Guid.Parse(x));
            var validIds = repo.GetPuckRevision().Where(x => ids.Contains(x.Id) && x.Current).Select(x => x.Id).Distinct().ToList();
            
            if (validIds.Count == 0) return principal;

            ((ClaimsIdentity)principal.Identity).AddClaims(
                validIds
                    .Select(x=>new Claim(Claims.PuckStartId,x.ToString())).ToArray()
            );
            return principal;
        }
    }
}
