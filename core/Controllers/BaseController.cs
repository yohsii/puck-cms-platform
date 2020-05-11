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
using puck.core.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.ResponseCaching;
using System.Reflection;
using System.Dynamic;
using Lucene.Net.Analysis;

namespace puck.core.Controllers
{
    public class BaseController : Controller
    {

        public IActionResult Puck(string path = null, string variant=null)
        {
            try
            {
                StateHelper.SetFirstRequestUrl();
                SyncIfNecessary();

                var uri = Request.GetUri();
                
                if(string.IsNullOrEmpty(path))
                    path = uri.AbsolutePath.ToLower().TrimEnd('/');

                var dmode = this.GetDisplayModeId();
                
                string domain = uri.Host.ToLower();
                string searchPathPrefix;
                if (!PuckCache.DomainRoots.TryGetValue(domain, out searchPathPrefix))
                {
                    if (!PuckCache.DomainRoots.TryGetValue("*", out searchPathPrefix))
                    {
                        var ex = new Exception($"domain root not set, likely because there is no content. DOMAIN:{domain} - visit the backoffice to set up your site");
                        if (PuckCache.JustSeeded) {
                            PuckCache.JustSeeded = false;
                            return Redirect("/puck"); 
                        }
                        else
                            throw ex;
                    }
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
                    Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new string[] { "Accept-Encoding" };
                    Response.Redirect(redirectUrl, true);
                }
                if (PuckCache.Redirect302.TryGetValue(searchPath, out redirectUrl))
                {
                    Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(PuckCache.RedirectOuputCacheMinutes)
                    };
                    Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new string[] { "Accept-Encoding" };
                    Response.Redirect(redirectUrl, false);
                }
                
                if (string.IsNullOrEmpty(variant))
                {
                    variant = GetVariant(searchPath);
                }
                HttpContext.Items["variant"] = variant;
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
                        if (cacheMinutes != 0)
                        {
                            Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                            {
                                Public = true,
                                MaxAge = TimeSpan.FromMinutes(cacheMinutes)
                            };
                            Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new string[] { "Accept-Encoding" };
                            var varyByQs = string.Empty;
                            if (PuckCache.TypeOutputCacheVaryByQueryString.TryGetValue(result[FieldKeys.PuckType], out varyByQs)) {
                                var responseCachingFeature = HttpContext.Features.Get<IResponseCachingFeature>();
                                if (responseCachingFeature != null && !string.IsNullOrEmpty(varyByQs) && !string.IsNullOrWhiteSpace(varyByQs))
                                {
                                    responseCachingFeature.VaryByQueryKeys = varyByQs
                                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                        .Where(x=>!string.IsNullOrEmpty(x)&&!string.IsNullOrWhiteSpace(x))
                                        .ToArray();
                                }
                            }
                        }
                    }
                }

                if (model == null)
                {
                    //404
                    HttpContext.Response.StatusCode = 404;
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
                if (templatePath.ToLower().Equals(PuckCache.Path404?.ToLower() ?? "")) {
                    HttpContext.Response.StatusCode = 404;
                }
                return View(templatePath, model);
            }
            catch (Exception ex)
            {
                return ErrorPage(exception:ex);
            }
        }

        public string GetVariant(string searchPath)
        {
            string variant = null;
            if (!PuckCache.PathToLocale.TryGetValue(searchPath, out variant))
            {
                foreach (var entry in PuckCache.PathToLocale)
                {//PathToLocale dictionary ordered by depth descending (based on number of forward slashes in path) so it's safe to break after first match
                    if ((searchPath+"/").StartsWith(entry.Key+"/"))
                    {
                        variant = entry.Value;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(variant))
                    variant = PuckCache.SystemVariant;
            }
            return variant;
        }

        public string GetDisplayModeId() {
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

        protected void SyncIfNecessary() {
            if (PuckCache.ShouldSync && !PuckCache.IsSyncQueued)
            {
                PuckCache.IsSyncQueued = true;
                //was using HostingEnvironment.QueueBackgroundWorkItem and passing in cancellation token
                //can't do that in asp.net core so passing in a new cancellation token which is a bit pointless
                System.Threading.Tasks.Task.Factory.StartNew(() => SyncHelper.Sync(new CancellationToken()));
            }
        }

        protected IActionResult ErrorPage(Exception exception=null,bool log = true) {
            HttpContext.Response.StatusCode = 500;
            var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            if (exception == null)
            {
                var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandlerFeature != null)
                    model.Exception = exceptionHandlerFeature.Error;
            }
            else
                model.Exception = exception;

            if (log && model.Exception != null)
                PuckCache.PuckLog.Log(model.Exception);

            return View(PuckCache.Path500,model);
        }

        protected List<QueryResult> Query(List<QueryModel> queries,string cacheKey=null,int cacheMinutes=0) {
            var result = new List<QueryResult>();
            
            if (queries == null)
                return result;

            if (!string.IsNullOrEmpty(cacheKey)) {
                object o=null;
                if (PuckCache.Cache.TryGetValue(cacheKey, out o))
                {
                    var _res = o as List<QueryResult>;
                    if(_res!=null)
                        return _res;
                }
            }

            var models = ApiHelper.GetModelTypes();

            void DoIncludes(List<ExpandoObject> results, List<string> includes, int includesIndex = 0)
            {
                if (includesIndex > includes.Count - 1) return;
                var include = includes[includesIndex];
                var properties = include.Split(".", StringSplitOptions.RemoveEmptyEntries);
                void GetProperty(object currentProp, object _setProp,string _setPropName,string[] properties,int i) {
                    var prop = properties[i];
                    
                    if (ApiHelper.ExpandoHasProperty(currentProp as ExpandoObject, prop))
                    {
                        var obj = ApiHelper.GetExpandoProperty(currentProp as ExpandoObject, prop);
                        
                        if (obj == null) return;

                        var objType = obj.GetType();
                        if (objType.Equals(typeof(ExpandoObject)))
                        {
                            _setProp = currentProp;
                            _setPropName = prop;
                            currentProp = obj;
                            i++;
                            GetProperty(currentProp,_setProp,_setPropName,properties,i);
                        }
                        else if (i == properties.Length - 1 && objType.Equals(typeof(List<Object>))) {
                            //we have the List<PuckReference> property
                            _setProp = currentProp;
                            _setPropName = prop;
                            currentProp = obj;
                            i++;
                            DoGetFromReferences(currentProp, _setProp, _setPropName);
                        }
                        else if (objType.Equals(typeof(List<Object>))) {
                            _setProp = currentProp;
                            _setPropName = prop;
                            currentProp = obj;
                            i++;
                            var lprop = currentProp as List<object>;
                            foreach (var o in lprop) {
                                GetProperty(o,_setProp,_setPropName,properties,i);
                            }
                        }
                    }
                }

                void DoGetFromReferences(object currentProp,object _setProp,string _setPropName)
                {
                    if (currentProp != null && currentProp.GetType().Equals(typeof(List<object>)))
                    {
                        var lprop = currentProp as List<object>;
                        if (lprop.Any() && lprop[0].GetType().Equals(typeof(ExpandoObject)))
                        {
                            var refQuery = new QueryHelper<BaseModel>();
                            var qhinner1 = refQuery.New();
                            var hasQuery = false;
                            var j = 0;
                            var sortOrder = new Dictionary<Guid, int>();
                            foreach (var reference in lprop)
                            {
                                var eref = reference as ExpandoObject;
                                Guid id;
                                if (ApiHelper.ExpandoHasProperty(eref, "Id") && ApiHelper.ExpandoHasProperty(eref, "Variant") && Guid.TryParse(ApiHelper.GetExpandoProperty(eref, "Id").ToString(), out id))
                                {
                                    hasQuery = true;
                                    var qhinner2 = qhinner1.New().Id(ApiHelper.GetExpandoProperty(eref, "Id").ToString());
                                    qhinner2.Variant(ApiHelper.GetExpandoProperty(eref, "Variant").ToString().ToLower());
                                    qhinner1.Group(
                                        qhinner2
                                    );
                                    sortOrder[id] = j;
                                }
                                j++;
                            }
                            refQuery.And().Group(qhinner1);
                            List<ExpandoObject> unsortedRefResult = refQuery.GetAllExpando(limit: int.MaxValue);
                            var refResult = unsortedRefResult.OrderBy(x => sortOrder[Guid.Parse(ApiHelper.GetExpandoProperty(x, "Id").ToString())]).ToList();
                            ApiHelper.SetExpandoProperty(_setProp as ExpandoObject, _setPropName, refResult);
                            DoIncludes(refResult, includes, includesIndex: includesIndex + 1);
                        }
                    }
                }

                foreach (var item in results)
                {
                    GetProperty(item,null,"",properties,0);
                }
            }

            foreach (var query in queries) {
                if (string.IsNullOrEmpty(query.Type))
                {
                    result.Add(new QueryResult {Total=0,Results=new List<ExpandoObject>() });
                    continue;
                }
                Type type = null;
                if (!PuckCache.ModelNameToType.TryGetValue(query.Type, out type)) {
                    result.Add(new QueryResult { Total = 0, Results = new List<ExpandoObject>() });
                    continue;
                }

                var qht = typeof(QueryHelper<>);
                var typeArgs = new Type[] { type};
                var gtype = qht.MakeGenericType(typeArgs);
                
                var qho = Activator.CreateInstance(gtype,new object[] {true,true });

                var interfaceTypes = new List<Type>();
                var interfaceProperties = new Dictionary<string, FlattenedObject>();

                var fieldTypeMappings = new Dictionary<string, Type>();
                var fieldAnalyzerMappings = new Dictionary<string, Analyzer>();

                if (!string.IsNullOrEmpty(query.Implements)) {
                    var interfaceNames = query.Implements.Split(',',StringSplitOptions.RemoveEmptyEntries);
                    foreach (var interfaceName in interfaceNames) {
                        Tuple<Type,List<FlattenedObject>> vals = null;
                        if (PuckCache.InterfaceNameToType.TryGetValue(interfaceName, out vals)) {
                            interfaceTypes.Add(vals.Item1);
                            
                            foreach (var flattenedObject in vals.Item2) {
                                interfaceProperties[flattenedObject.Key] = flattenedObject;
                                fieldTypeMappings[flattenedObject.Key] = flattenedObject.Type;
                                if(flattenedObject.Analyzer!=null)
                                    fieldAnalyzerMappings[flattenedObject.Key] = flattenedObject.Analyzer;
                            }
                        }
                    }

                }

                if (interfaceTypes.Any()) {
                    var miImplements = gtype.GetMethod("Implements", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Type[]) }, null);
                    miImplements.Invoke(qho, new object[] { interfaceTypes.ToArray() });
                }
                
                if (!string.IsNullOrEmpty(query.Sorts)) {
                    var miSort = gtype.GetMethod("SortByField", BindingFlags.Instance | BindingFlags.NonPublic,Type.DefaultBinder,new Type[]{typeof(string),typeof(bool),typeof(Type)},null);
                    foreach (var sort in query.Sorts.Split(',', StringSplitOptions.RemoveEmptyEntries)) {
                        var sortParams = sort.Split(":",StringSplitOptions.RemoveEmptyEntries);
                        if (sortParams.Length == 0) continue;
                        var desc = sortParams[1] == "desc";
                        FlattenedObject flatObj = null;
                        Type propType = null;
                        if (interfaceProperties.TryGetValue(sortParams[0],out flatObj)) {
                            propType = flatObj.Type;
                        }
                        miSort.Invoke(qho,new object[] { sortParams[0], desc, propType });
                    }
                }
                if (!string.IsNullOrEmpty(query.Query))
                {
                    var miAppendQuery = gtype.GetMethod("AppendQuery");
                    miAppendQuery.Invoke(qho,new object[] {query.Query });
                }
                var miQueryNoCast = gtype.GetMethod("GetAllExpando");
                var qresult = miQueryNoCast.Invoke(qho,new object[] {query.Take,query.Skip,fieldTypeMappings,fieldAnalyzerMappings}) as List<ExpandoObject>;

                if (query.Include != null)
                {
                    foreach (var includes in query.Include) {
                        DoIncludes(qresult,includes,includesIndex: 0);
                    }
                }

                var totalHitsPi = qho.GetType().GetProperty("TotalHits");

                result.Add(new QueryResult {Total= (int)totalHitsPi.GetValue(qho), Results = qresult });
            }

            if (!string.IsNullOrEmpty(cacheKey)) {
                PuckCache.Cache.Set(cacheKey,result,TimeSpan.FromMinutes(cacheMinutes));
            }

            return result;
        }

    }
}
