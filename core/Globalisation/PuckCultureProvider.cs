using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using puck.core.Controllers;
using puck.core.Helpers;
using puck.core.State;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace puck.core.Globalisation
{
    //public class PuckCultureProvider : RequestCultureProvider
    //{
    //    public override async Task<ProviderCultureResult> DetermineProviderCultureResult(Microsoft.AspNetCore.Http.HttpContext httpContext)
    //    {
    //        var uri = httpContext.Request.GetUri();
    //        string path = uri.AbsolutePath.ToLower();

    //        if (path == "/")
    //            path = string.Empty;

    //        string domain = uri.Host.ToLower();
    //        string searchPathPrefix;
    //        if (!PuckCache.DomainRoots.TryGetValue(domain, out searchPathPrefix))
    //        {
    //            if (!PuckCache.DomainRoots.TryGetValue("*", out searchPathPrefix))
    //                return null;
    //        }
    //        string searchPath = searchPathPrefix.ToLower() + path;
    //        string variant = ApiHelper.GetRequestVariant(searchPath);
    //        if (string.IsNullOrEmpty(variant))
    //            return null;
    //        var result = new ProviderCultureResult(variant);
    //        return result;
    //    }
    //}
}
