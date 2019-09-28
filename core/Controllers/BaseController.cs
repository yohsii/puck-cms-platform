using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json;
using puck.core.Base;
using puck.core.Constants;
using puck.core.Helpers;
using StackExchange.Profiling;
using puck.core.State;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Localization;

namespace puck.core.Controllers
{
    public class BaseController : Controller
    {

        public ActionResult Puck(string variant=null)
        {
            try
            {
                StateHelper.SetFirstRequestUrl();
                if (PuckCache.ShouldSync&&!PuckCache.IsSyncQueued)
                {
                    PuckCache.IsSyncQueued = true;
                    //was using HostingEnvironment.QueueBackgroundWorkItem and passing in cancellation token
                    //can't do that in asp.net core so passing in a new cancellation token which is a bit pointless
                    System.Threading.Tasks.Task.Factory.StartNew(()=> SyncHelper.Sync(new CancellationToken()));
                }
                var uri = Request.GetUri();
                string path = uri.AbsolutePath.ToLower();

                var dmode = this.GetDisplayModeId();
                                
                if (path=="/")
                    path = string.Empty;                
                
                string domain = uri.Host.ToLower();
                string searchPathPrefix;
                if (!PuckCache.DomainRoots.TryGetValue(domain, out searchPathPrefix))
                {
                    if (!PuckCache.DomainRoots.TryGetValue("*", out searchPathPrefix))
                        throw new Exception("domain roots not set. DOMAIN:" + domain);
                }
                string searchPath = searchPathPrefix.ToLower() + path;

                //do redirects
                string redirectUrl;
                if (PuckCache.Redirect301.TryGetValue(searchPath, out redirectUrl) )
                {
                    Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(PuckCache.RedirectOuputCacheMinutes)
                    };
                    Response.Redirect(redirectUrl, true);
                }
                if (PuckCache.Redirect302.TryGetValue(searchPath, out redirectUrl))
                {
                    Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(PuckCache.RedirectOuputCacheMinutes)
                    };
                    Response.Redirect(redirectUrl, false);
                }
                var requestCultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
                variant = requestCultureFeature.RequestCulture.Culture.Name.ToLower();

                //if (string.IsNullOrEmpty(variant))
                //{
                //    variant = ApiHelper.GetRequestVariant(searchPath);
                //}
                //set thread culture for future api calls on this thread
                //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(variant);
                IList<Dictionary<string, string>> results;
#if DEBUG
                using (MiniProfiler.Current.Step("lucene"))
                {
                    results = puck.core.Helpers.QueryHelper<BaseModel>.Query(
                        string.Concat("+",FieldKeys.Published,":true"," +", FieldKeys.Path, ":", $"\"{searchPath}\"", " +", FieldKeys.Variant, ":", variant)
                        );                    
                }
#else                
                results = puck.core.Helpers.QueryHelper<BaseModel>.Query(
                        string.Concat("+",FieldKeys.Published,":true"," +", FieldKeys.Path, ":", $"\"{searchPath}\"", " +", FieldKeys.Variant, ":", variant)
                        );           
#endif
                var result = results == null ? null : results.FirstOrDefault();
                BaseModel model = null;
                if (result != null)
                {
#if DEBUG
                    using (MiniProfiler.Current.Step("deserialize"))
                    {
                        model = JsonConvert.DeserializeObject(result[FieldKeys.PuckValue], ApiHelper.GetTypeFromName(result[FieldKeys.PuckType])) as BaseModel;
                    }
#else
                    model = JsonConvert.DeserializeObject(result[FieldKeys.PuckValue], ApiHelper.GetTypeFromName(result[FieldKeys.PuckType])) as BaseModel;
#endif
                    if (!PuckCache.OutputCacheExclusion.Contains(searchPath))
                    {
                        int cacheMinutes;
                        if (!PuckCache.TypeOutputCache.TryGetValue(result[FieldKeys.PuckType], out cacheMinutes))
                        {
                            if (!PuckCache.TypeOutputCache.TryGetValue(typeof(BaseModel).Name, out cacheMinutes))
                            {
                                cacheMinutes = PuckCache.DefaultOutputCacheMinutes;
                            }
                        }
                        Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromMinutes(cacheMinutes)
                        };
                    }
                }

                if (model == null)
                {
                    //404
                    return View(PuckCache.Path404);
                }
                var cache = PuckCache.Cache;
                object cacheValue;
                //string templatePath = result[FieldKeys.TemplatePath];
                string templatePath = model.TemplatePath;
                
                if (!string.IsNullOrEmpty(dmode))
                {
                    string cacheKey = CacheKeys.PrefixTemplateExist + dmode + templatePath;
                    if (cache.TryGetValue(cacheKey,out cacheValue))
                    {
                        templatePath = cacheValue as string;
                    }
                    else
                    {
                        string dpath = templatePath.Insert(templatePath.LastIndexOf('.') + 1, dmode + ".");
                        if (System.IO.File.Exists(ApiHelper.MapPath(dpath)))
                        {
                            templatePath = dpath;
                        }
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(PuckCache.DisplayModesCacheMinutes));
                        cache.Set(cacheKey, templatePath, cacheEntryOptions);
                    }
                }
                return View(templatePath, model);
            }
            catch (Exception ex)
            {
                PuckCache.PuckLog.Log(ex);
                ViewBag.Error = ex.Message;
                return View(PuckCache.Path500);
            }
        }
        
        protected string GetDisplayModeId() {
            var dmode = "";
            if (PuckCache.DisplayModes != null)
            {
                foreach (var mode in PuckCache.DisplayModes)
                {
                    if (mode.Value(HttpContext))
                    {
                        dmode = mode.Key;
                        break;
                    }
                }
            }
            return dmode;
        }
        
    }
}
