using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using puck.core.Constants;
using puck.core.Abstract;
using Newtonsoft.Json;
using puck.core.Controllers;
using System.Linq.Expressions;
using puck.core.State;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using puck.core.Entities;

namespace puck.core.Extensions
{
    public static class ViewExtensions
    {
        public static HtmlString InputName<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression) 
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var fullHtmlFieldName = htmlHelper
                .ViewContext
                .ViewData
                .TemplateInfo
                .GetFullHtmlFieldName(name);
            return new HtmlString(fullHtmlFieldName);
        }
        public static T GetPropertyAttribute<T>(this ModelMetadata instance)
        {
            if (instance.ContainerType == null || string.IsNullOrEmpty(instance.PropertyName))
                return default(T);
            var result = instance.ContainerType
              .GetProperty(instance.PropertyName)
              .GetCustomAttributes(typeof(T), false)
              .Select(a =>a)
              .FirstOrDefault(a => a != null);
            if (result == null) return default(T);
            return (T)result;
        }
        public static T PuckEditorSettings<T>(this RazorPageBase page,string propertyName="",bool inherit=true,Type modelTypeOverride=null,bool attributeOnly=false) {
            int cacheMinutes = 30;
            if (page.ViewContext.ViewData.ModelMetadata!= null) {
                var settingsAttribute = page.ViewContext.ViewData.ModelMetadata.GetPropertyAttribute<T>();
                if (settingsAttribute != null)
                    return (T)settingsAttribute;
            }
            if (attributeOnly)
                return default(T);

            if (string.IsNullOrEmpty(propertyName))
                propertyName = page.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
            using (var scope = PuckCache.ServiceProvider.CreateScope())
            {
                var repo = scope.ServiceProvider.GetService<I_Puck_Repository>();
                var cache = scope.ServiceProvider.GetService<IMemoryCache>();
                
                var modelType = modelTypeOverride==null? 
                    page.ViewBag.Level0Type as Type
                    :modelTypeOverride;
                if (modelType == null)
                    return default(T);
                var settingsType = typeof(T);
                //var propertyName =ExpressionMetadataProvider.FromStringExpression("", page.ViewData,).PropertyName;
                var type = modelType;
                var cachePrefix = "editor_settings_";
                while (type != typeof(object))
                {
                    var key = string.Concat(settingsType.FullName, ":", type.Name, ":", propertyName);
                    var cacheKey = cachePrefix  + key;
                    var meta = cache.Get<PuckMeta>(cacheKey);
                    if (meta == null) {
                        if (!cache.Get<bool?>("null_" + cacheKey).HasValue)
                        {
                            meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.EditorSettings && x.Key.Equals(key)).FirstOrDefault();
                            if (meta != null)
                            {
                                cache.Set(cacheKey, meta, TimeSpan.FromMinutes(cacheMinutes));
                            }
                            else {
                                bool? nullValue = true;
                                cache.Set("null_" + cacheKey, nullValue, TimeSpan.FromMinutes(cacheMinutes));
                            }
                        }
                        if (meta == null)
                        {
                            key = string.Concat(settingsType.FullName, ":", type.Name, ":");
                            cacheKey = cachePrefix + key;
                            meta = cache.Get<PuckMeta>(cacheKey);
                            if (meta == null)
                            {
                                if (!cache.Get<bool?>("null_" + cacheKey).HasValue)
                                {
                                    meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.EditorSettings && x.Key.Equals(key)).FirstOrDefault();
                                    if (meta != null)
                                    {
                                        cache.Set(cacheKey, meta, TimeSpan.FromMinutes(cacheMinutes));
                                    }
                                    else
                                    {
                                        bool? nullValue = true;
                                        cache.Set("null_" + cacheKey, nullValue, TimeSpan.FromMinutes(cacheMinutes));
                                    }
                                }
                            }
                        }
                    }
                    if (meta != null)
                    {
                        var data = JsonConvert.DeserializeObject(meta.Value, settingsType);
                        return data == null ? default(T) : (T)data;
                    }
                    if (inherit) type = type.BaseType;
                    else type = typeof(object);
                }
                return default(T);
            }
        }
        
    }
}
