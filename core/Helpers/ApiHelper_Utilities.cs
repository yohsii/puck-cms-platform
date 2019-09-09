using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using puck.core.Base;
using System.Web;
using System.Threading.Tasks;
using puck.core.Abstract;
using puck.core.Concrete;
using System.Text.RegularExpressions;
using puck.core.Models;
using puck.core.Constants;
using System.Globalization;
using Newtonsoft.Json;
using puck.core.Entities;
using puck.core.Exceptions;
using puck.core.Events;
using System.Net.Mail;
using puck.core.Attributes;
using puck.core.State;
using Microsoft.Extensions.DependencyModel;

namespace puck.core.Helpers
{
    public partial class ApiHelper
    {
        public static string MapPath(string vpath) {
            var path = vpath.Replace("~", "").Replace("/","\\");
            var absPath = PuckCache.ContentRootPath + path;
            return absPath;
        }
        public static string ServerName() {
            var result = Environment.MachineName;// +HttpRuntime.AppDomainAppId.Replace("/","_");
            return result;
        }
        public static object RevisionToModel(PuckRevision revision)
        {
            try
            {
                //var model = JsonConvert.DeserializeObject(revision.Value, ConcreteType(ApiHelper.GetType(revision.Type)));
                var model = JsonConvert.DeserializeObject(revision.Value, ConcreteType(ApiHelper.GetTypeFromName(revision.Type)));
                var mod = model as BaseModel;
                mod.ParentId = revision.ParentId;
                mod.Path = revision.Path; mod.SortOrder = revision.SortOrder; mod.NodeName = revision.NodeName; mod.Published = revision.Published;
                mod.Type = revision.Type;
                mod.TypeChain = revision.TypeChain;
                return model;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static BaseModel RevisionToBaseModel(PuckRevision revision)
        {
            try
            {
                //var model = JsonConvert.DeserializeObject(revision.Value, ConcreteType(ApiHelper.GetType(revision.Type)));
                var model = JsonConvert.DeserializeObject(revision.Value, ConcreteType(ApiHelper.GetTypeFromName(revision.Type)));
                var mod = model as BaseModel;
                mod.Id = revision.Id;
                mod.ParentId = revision.ParentId;
                mod.Path = revision.Path; mod.SortOrder = revision.SortOrder; mod.NodeName = revision.NodeName; mod.Published = revision.Published;
                mod.Type = revision.Type;
                mod.TypeChain = revision.TypeChain;
                return mod;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static BaseModel RevisionToBaseModelCast(PuckRevision revision)
        {
            try
            {
                var model = JsonConvert.DeserializeObject(revision.Value, typeof(BaseModel));
                var mod = model as BaseModel;
                mod.ParentId = revision.ParentId;
                mod.Path = revision.Path; mod.SortOrder = revision.SortOrder; mod.NodeName = revision.NodeName; mod.Published = revision.Published;
                mod.Type = revision.Type;
                mod.TypeChain = revision.TypeChain;
                return mod;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string DirOfPath(string s)
        {
            if (s.EndsWith("/"))
                return s;
            string result = s.Substring(0, s.LastIndexOf("/") + 1);
            return result;
        }
        public static string ToVirtualPath(string p)
        {
            Regex r = new Regex(Regex.Escape(ApiHelper.MapPath("~/")), RegexOptions.Compiled);
            p = r.Replace(p, "~/", 1).Replace("\\", "/");
            return p;
        }
        public static Type GetTypeFromName(string name,bool defaultToBaseModel=false) {
            if (PuckCache.ModelNameToAQN.ContainsKey(name))
                return ApiHelper.GetType(PuckCache.ModelNameToAQN[name]);
            if (defaultToBaseModel)
                return typeof(BaseModel);
            return null;
        }
        private static string DoTypeChain(Type type, string chain = "")
        {
            //chain += type.FullName + " ";
            chain += type.Name + " ";
            if (type.BaseType != null && type.BaseType != typeof(Object))
                chain = DoTypeChain(type.BaseType, chain);
            return chain.TrimEnd();
        }
        public static string TypeChain(Type type, string chain = "")
        {
            var result = DoTypeChain(type, chain);
            foreach (var gen in PuckCache.IGeneratedToModel)
            {
                var t = ApiHelper.GetType(gen.Key);
                result = result.Replace(gen.Value.FullName, t.FullName);
            }
            return result;
        }
        public static List<Type> BaseTypes(Type start, List<Type> result = null, bool excludeSystemObject = true)
        {
            result = result ?? new List<Type>();
            if (start.BaseType == null)
                return result;
            if (start.BaseType != typeof(Object) || !excludeSystemObject)
                result.Add(start.BaseType);
            return BaseTypes(start.BaseType, result);
        }
        public static void SetCulture(string path = null)
        {
            if (path == null)
                path = HttpContext.Current.Request.GetUri().AbsolutePath;
        }
        //public static List<Type> TaskTypes(bool ignoreSystemTasks=true,bool ignoreBaseTask=true)
        //{
        //    return FindDerivedClasses(typeof(BaseTask), null, false).ToList();
        //}
        public static List<Type> EditorSettingTypes()
        {
            return FindDerivedClasses(typeof(I_Puck_Editor_Settings)).ToList();
        }
        public static Type GetType(string assemblyQualifiedName)
        {
            var result = Type.GetType(assemblyQualifiedName);
            //if (result == null)
            //{
            //    try
            //    {
            //        //throws exception if type not found
            //        result = Type.GetType(
            //            assemblyQualifiedName,
            //            (name) =>
            //            {
            //                return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName).FirstOrDefault();
            //            },
            //            null,
            //            true
            //        );
            //    }
            //    catch (Exception ex)
            //    {
            //        return null;
            //    }
            //}
            return result;
        }
        public static Type ConcreteType(Type t)
        {
            Type result = null;
            if (t.IsInterface)
                result = PuckCache.IGeneratedToModel[t.AssemblyQualifiedName];
            else
                result = t;
            return result;
        }
        public static object CreateInstance(Type t)
        {
            Object result = Activator.CreateInstance(ConcreteType(t));
            return result;
        }
        public static IEnumerable<Type> FindDerivedClasses(Type baseType, List<Type> excluded = null, bool inclusive = false)
        {
            excluded = excluded ?? new List<Type>();
            var asmNames = DependencyContext.Default.GetDefaultAssemblyNames();
            var types = asmNames.Select(Assembly.Load)
                .SelectMany(x => x.GetTypes()).Where(x => (x != baseType || inclusive) && baseType.IsAssignableFrom(x) && !excluded.Contains(x));
            return types;
        }
        public static List<Type> GeneratedOptions()
        {
            var types = FindDerivedClasses(typeof(I_GeneratedOption)).ToList();
            return types;
        }
        public static string FriendlyClassName(Type t) {
            string name = t.Name;
            if (typeof(I_Generated).IsAssignableFrom(t)) {
                t = ApiHelper.ConcreteType(t);
            }
            var att = t.GetCustomAttribute<FriendlyClassNameAttribute>();
            if (att != null)
                name = att.Name;
            return name;
        }
        public static string SanitizeClassName(string name) {
            var result = Regex.Replace(name, @"\W", "");
            return result;
        }
        public static string SanitizePropertyName(string name)
        {
            var result = Regex.Replace(name, @"\W", "");
            return result;
        }
        public static string RemoveAccent(string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static string Slugify(string phrase)
        {
            string str = RemoveAccent(phrase).ToLower();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", ""); // Remove all non valid chars          
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space  
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-"); // //Replace spaces by dashes
            return str;
        }        
        public static string SanitizeUrl(string url) {
            var result = url;
            return result;
        }
        public static void Email(string to, string subject, string body, string host = null, string from = null,bool isHtml=true)
        {
            //host = host ?? PuckCache.SmtpHost;
            from = from ?? PuckCache.SmtpFrom;
            MailMessage mail = new MailMessage(from, to);
            SmtpClient client = new SmtpClient();
            //client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.UseDefaultCredentials = false;
            //client.Host = host;
            mail.IsBodyHtml = isHtml;
            mail.Subject = subject;
            mail.Body = body;
            client.Send(mail);
        }
        public static string EmailTransform(string template, BaseModel model,NotifyActions action) {
            string date = DateTime.Now.ToShortDateString();
            string time = DateTime.Now.ToShortTimeString();
            string user = "";
            string editUrl = "";
            try {
                var hcontext = HttpContext.Current;
                user = hcontext.User.Identity.Name;
                var uri = hcontext.Request.GetUri();
                editUrl = uri.Scheme +"://" + uri.Host + ":" + uri.Port
                    + "/puck?hash="
                    +HttpUtility.UrlEncode("content?id="+model.Id.ToString()+"&variant="+model.Variant);
            }
            catch (Exception ex) {
                PuckCache.PuckLog.Log(ex);
            }
            template = template
                .Replace("<!--Id-->", model.Id.ToString())
                .Replace("<!--NodeName-->", model.NodeName)
                .Replace("<!--LastEditedBy-->", model.LastEditedBy)
                .Replace("<!--CreatedBy-->", model.CreatedBy)
                .Replace("<!--Path-->", model.Path)
                .Replace("<!--Created-->", model.Created.ToString("dd/MM/yyyy hh:mm:ss"))
                .Replace("<!--Updated-->", model.Updated.ToString("dd/MM/yyyy hh:mm:ss"))
                .Replace("<!--Revision-->", model.Revision.ToString())
                .Replace("<!--Variant-->", model.Variant)
                .Replace("<!--Published-->", model.Published.ToString())
                .Replace("<!--SortOrder-->", model.SortOrder.ToString())
                .Replace("<!--TemplatePath-->", model.TemplatePath)
                .Replace("<!--TypeChain-->", model.TypeChain)
                .Replace("<!--Type-->", model.Type)
                .Replace("<!--__Verb__",action.ToString())
                .Replace("<!--EditUrl-->",editUrl)
                .Replace("<!--Date-->",date)
                .Replace("<!--Time-->",time)
                .Replace("<!--User-->",user)
                ;
            return template;
        }
    }
}
