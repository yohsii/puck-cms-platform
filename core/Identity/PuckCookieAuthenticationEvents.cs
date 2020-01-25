using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using puck.core.Constants;
using puck.core.Entities;

public class PuckCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly UserManager<PuckUser> userManager;
    private readonly IMemoryCache cache;
    public PuckCookieAuthenticationEvents(UserManager<PuckUser> userManager,IMemoryCache cache)
    {
        this.userManager = userManager;
        this.cache = cache;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!(cache.Get<bool?>($"renewPuckClaims{context.Principal.Identity.Name}") ?? false))
        {
            await SecurityStampValidator.ValidatePrincipalAsync(context);
            return;
        }

        cache.Remove($"renewPuckClaims{context.Principal.Identity.Name}");
        
        var claims = context.Principal.FindAll(Claims.PuckStartId)?.ToList();
        if (claims != null && claims.Any()) {
            for(var i=0;i<claims.Count;i++)
            {
                ((ClaimsIdentity)context.Principal.Identity).RemoveClaim(claims[i]);
            }
        }

        var user = await userManager.FindByNameAsync(context.Principal.Identity.Name);

        if (user != null && !string.IsNullOrEmpty(user.PuckStartNodeIds)) {
            foreach (var startNodeId in user.PuckStartNodeIds.Split(',', System.StringSplitOptions.RemoveEmptyEntries)) {
                ((ClaimsIdentity)context.Principal.Identity).AddClaim(new Claim(Claims.PuckStartId,startNodeId));
            }
        }
        context.ReplacePrincipal(context.Principal);
        context.ShouldRenew = true;
        
        await SecurityStampValidator.ValidatePrincipalAsync(context);
    }
}