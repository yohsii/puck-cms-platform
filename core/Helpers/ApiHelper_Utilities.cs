using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using puck.core.Base;
using System.Web;
using puck.core.Abstract;
using System.Text.RegularExpressions;
using puck.core.Constants;
using Newtonsoft.Json;
using puck.core.Entities;
//using System.Net.Mail;
using puck.core.State;
using System.ComponentModel.DataAnnotations;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Dynamic;

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
            if (!PuckCache.IsEditServer && PuckCache.UseAzureDirectory)
            {
                result += "-" + PuckCache.AzureMachineNameIdentifier;
            }
            return result;
        }
        public static string DirOfPath(string s)
        {
            if (s.EndsWith("/"))
                return s;
            string result = s.Substring(0, s.LastIndexOf("/") + 1);
            return result;
        }
        static Regex contentRootPathRegex = null;
        public static string ToVirtualPath(string p)
        {
            if(contentRootPathRegex==null) contentRootPathRegex = new Regex(Regex.Escape(ApiHelper.MapPath("~/")), RegexOptions.Compiled);
            p = contentRootPathRegex.Replace(p, "~/", 1).Replace("\\", "/");
            return p;
        }
        public static Type GetTypeFromName(string name,bool defaultToBaseModel=false) {
            if (PuckCache.ModelNameToType.ContainsKey(name))
                return PuckCache.ModelNameToType[name];
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
            foreach (var gen in PuckCache.IGeneratedToModel??new Dictionary<string, Type>())
            {
                var t = ApiHelper.GetType(gen.Key);
                result = result.Replace(gen.Value.FullName, t.FullName);
            }
            return result;
        }

        private static List<Type> DoTypeChainType(Type type, List<Type> types=null)
        {
            if (types == null)
                types = new List<Type>();
            types.Add(type);
            if (type.BaseType != null && type.BaseType != typeof(Object))
                types = DoTypeChainType(type.BaseType, types);
            return types;
        }
        public static List<Type> TypeChainType(Type type)
        {
            var result = DoTypeChainType(type);
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
        public static List<Type> GetModelTypes(bool inclusive = false)
        {
            var excluded = new List<Type>() { typeof(PuckRevision) };
            var result = FindDerivedClasses(typeof(BaseModel), excluded, inclusive).ToList();
            return result;
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
        public static bool ExpandoHasProperty(ExpandoObject e,string propertyName) {
            return ((IDictionary<string, object>)e).ContainsKey(propertyName);
        }
        public static Object GetExpandoProperty(ExpandoObject e,string propertyName) {
            return ((IDictionary<string, object>)e)[propertyName];
        }
        public static void SetExpandoProperty(ExpandoObject e, string propertyName,object value)
        {
            ((IDictionary<string, object>)e)[propertyName]=value;
        }
        public static Type ConcreteType(Type t)
        {
            if (t == null) return null;
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
            //var asmNames = DependencyContext.Default.GetDefaultAssemblyNames();
            var assembly = Assembly.GetEntryAssembly();
            var assemblyNames = assembly.GetReferencedAssemblies();
            var assemblies = new List<Assembly>() { assembly};
            foreach (var assemblyName in assemblyNames)
            {
                assembly = Assembly.Load(assemblyName);
                assemblies.Add(assembly);
            }
            //foreach (var name in asmNames) {
            //    try {
            //        var assembly = Assembly.Load(name);
            //        assemblies.Add(assembly);
            //    } catch (Exception ex) { }
            //}
            var types = assemblies
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
            //var att = t.GetCustomAttribute<FriendlyClassNameAttribute>();
            var att = t.GetCustomAttribute<DisplayAttribute>(false);
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
            var SmtpHost = PuckCache.Configuration.GetValue<string>("SmtpHost");
            var SmtpPort = PuckCache.Configuration.GetValue<int?>("SmtpPort");
            var SmtpUserName = PuckCache.Configuration.GetValue<string>("SmtpUserName");
            var SmtpPassword = PuckCache.Configuration.GetValue<string>("SmtpPassword");
            var SmtpUseSsl = PuckCache.Configuration.GetValue<bool>("SmtpUseSsl");
            from = from ?? PuckCache.Configuration.GetValue<string>("SmtpFrom");
            if (string.IsNullOrEmpty(SmtpHost) || SmtpPort == null || string.IsNullOrEmpty(SmtpUserName) || string.IsNullOrEmpty(SmtpPassword))
            {
                PuckCache.PuckLog.Log(new Exception("cannot send email, Smtp configuration is not set."));
                return;
            }

            MimeMessage message = new MimeMessage();

            MailboxAddress fromAddress = new MailboxAddress(from);
            message.From.Add(fromAddress);

            var toArr = to.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            toArr.ToList().ForEach(x=>message.To.Add(new MailboxAddress(x)));
            
            message.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder();
            if(isHtml)
                bodyBuilder.HtmlBody = body;
            else
                bodyBuilder.TextBody = body;
            message.Body = bodyBuilder.ToMessageBody();

            using (SmtpClient client = new SmtpClient())
            {
                client.Connect(SmtpHost, SmtpPort.Value, SmtpUseSsl);
                client.Authenticate(SmtpUserName, SmtpPassword);
                client.Send(message);
                client.Disconnect(true);
            }
        }
        public static string EmailTransform(string template, BaseModel model,NotifyActions action) {
            string date = DateTime.Now.ToShortDateString();
            string time = DateTime.Now.ToShortTimeString();
            string user = "";
            string editUrl = "";
            try {
                var hcontext = HttpContext.Current;
                user = hcontext?.User?.Identity?.Name??"";
                var uri = hcontext?.Request?.GetUri() ?? PuckCache.FirstRequestUrl;
                editUrl = uri.Scheme +"://" 
                    + uri.Host 
                    + (uri.Port!=80 ?(":" + uri.Port):"")
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
