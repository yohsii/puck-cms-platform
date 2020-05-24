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
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using StackExchange.Profiling;
using System.Data.SqlClient;
using puck.core.Tasks;
using puck.core.State;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using LinqKit;

namespace puck.core.Helpers
{
    public partial class ApiHelper : I_Api_Helper
    {
        private static readonly object _savelck = new object();
        public RoleManager<PuckRole> roleManager { get; set; }
        public UserManager<PuckUser> userManager { get; set; }
        public I_Puck_Repository repo { get; set; }
        public I_Task_Dispatcher tdispatcher { get; set; }
        public I_Content_Indexer indexer { get; set; }
        public I_Log logger { get; set; }
        public IConfiguration config { get; set; }
        public ApiHelper(RoleManager<PuckRole> RoleManager, UserManager<PuckUser> UserManager, I_Puck_Repository Repo, I_Task_Dispatcher TaskDispatcher, I_Content_Indexer Indexer, I_Log Logger,IConfiguration config)
        {
            this.roleManager = RoleManager;
            this.userManager = UserManager;
            this.repo = Repo;
            this.tdispatcher = TaskDispatcher;
            this.indexer = Indexer;
            this.logger = Logger;
            this.config = config;
        }

        public async Task<Tuple<int,int>> GetCurrentWorkflowItemId(string username,int? since=null) {
            var user = await userManager.FindByNameAsync(username);

            var userGroups = user.PuckUserGroups?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };

            var predicate = PredicateBuilder.New<PuckWorkflowItem>();

            foreach (var group in userGroups)
            {
                predicate = predicate.Or(x => x.Group.Equals(group));
            }

            predicate.Or(x => x.Assignees.Contains(user.UserName));

            var model = repo.GetPuckWorkflowItem().AsExpandable().Where(predicate).Where(x => !x.Complete);

            if (since.HasValue)
                model = model.Where(x => x.Id > since);

            var count = model.Count();

            int currentId = model.OrderByDescending(x => x.Timestamp).FirstOrDefault()?.Id ?? 0;

            return new Tuple<int, int>(currentId,count);
        }

        public string GetConnectionStringName() {
            var connectionStringName = "";
            if (config.GetValue<bool?>("UseSQLServer") ?? false)
            {
                connectionStringName = "SQLServer";
            }
            else if (config.GetValue<bool?>("UsePostgreSQL") ?? false)
            {
                connectionStringName = "PostgreSQL";
            }
            else if (config.GetValue<bool?>("UseMySQL") ?? false)
            {
                connectionStringName = "MySQL";
            }
            else if (config.GetValue<bool?>("UseSQLite") ?? false)
            {
                connectionStringName = "SQLite";
            }
            return connectionStringName;
        }
        public List<ConfigContainer> GetConfigs() {
            var result = new List<ConfigContainer>();
            var rootDir = new DirectoryInfo(ApiHelper.MapPath("~/"));
            var configFiles = rootDir.GetFiles().Where(x=>x.Name.ToLower().StartsWith("appsettings.")&&x.Name.ToLower().EndsWith(".json") && !x.Name.ToLower().Equals("appsettings.json"));
            foreach (var configFile in configFiles) {
                var configContainer = new ConfigContainer();
                configContainer.Config = new ConfigurationBuilder()
                    .SetBasePath(ApiHelper.MapPath("~/"))
                    .AddJsonFile(configFile.Name)
                    .Build();
                configContainer.Name = configFile.Name;
                var ename = configFile.Name.Substring(configFile.Name.IndexOf(".")+1);
                ename = ename.Substring(0,ename.IndexOf("."));
                configContainer.EnvironmentName = ename;
                result.Add(configContainer);
            }
            return result;
        }
        public void AddRedirect(string from, string to, string type) {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || string.IsNullOrEmpty(type))
                return;
            from = from.Trim().ToLower();
            to = to.Trim().ToLower();
            type = type.Trim();
            var redirect = repo.GetPuckRedirect().FirstOrDefault(x => x.From.Equals(from));
            if (redirect == null) {
                redirect = new PuckRedirect() {
                    From = from
                };
                repo.AddPuckRedirect(redirect);
            }
            redirect.To = to;
            redirect.Type = type;
            repo.SaveChanges();
        }
        public void DeleteRedirect(string from) {
            if (string.IsNullOrEmpty(from))
                return;
            from = from.Trim().ToLower();
            var redirect = repo.GetPuckRedirect().FirstOrDefault(x=>x.From.Equals(from));
            if (redirect != null)
            {
                repo.DeletePuckRedirect(redirect);
                repo.SaveChanges();
            }
        }
        public void AddTag(string tag,string category) {
            if (string.IsNullOrEmpty(tag))
                return;
            if (string.IsNullOrEmpty(category))
                category = "";
            var tagEntity = repo.GetPuckTag().FirstOrDefault(x=>x.Tag.ToLower().Equals(tag.ToLower()) && x.Category.ToLower().Equals(category.ToLower()));
            if (tagEntity == null)
            {
                tagEntity = new PuckTag() { Tag = tag, Category = category ?? "", Count = 0 };
                repo.AddPuckTag(tagEntity);
            }
            tagEntity.Count++;
            repo.SaveChanges();
        }
        public void DeleteTag(string tag, string category)
        {
            if (string.IsNullOrEmpty(tag))
                return;
            if (string.IsNullOrEmpty(category))
                category = "";
            var tagEntity = repo.GetPuckTag().FirstOrDefault(x => x.Tag.ToLower().Equals(tag.ToLower()) && x.Category.ToLower().Equals(category.ToLower()));
            if (tagEntity == null)
            {
                return;
            }
            repo.DeletePuckTag(tagEntity);
            repo.SaveChanges();
        }
        public string UserVariant()
        {
            string variant;
            if (System.Web.HttpContext.Current.Session.GetString("language") != null)
            {
                variant = System.Web.HttpContext.Current.Session.GetString("language");
            }
            else
            {
                var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.UserVariant && x.Key == System.Web.HttpContext.Current.User.Identity.Name).FirstOrDefault();
                if (meta != null && !string.IsNullOrEmpty(meta.Value))
                {
                    variant = meta.Value;
                    System.Web.HttpContext.Current.Session.SetString("language", meta.Value);
                }
                else
                {
                    variant = PuckCache.SystemVariant;
                }
            }
            return variant;
        }


        public void SetDomain(string path, string domains)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("path null or empty");
            var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.DomainMapping).ToList();

            if (string.IsNullOrEmpty(domains))
            {
                var m = meta.Where(x => x.Key == path).ToList();
                m.ForEach(x =>
                {
                    repo.DeletePuckMeta(x);
                });
                if (m.Count > 0)
                    repo.SaveChanges();
            }
            else
            {
                var d = domains.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x=>!string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x))
                    .Select(x=>x.ToLower())
                    .ToList();
                d.ForEach(dd =>
                {
                    if (meta.Where(x => x.Value == dd && !x.Key.Equals(path)).Count() > 0)
                        throw new Exception($"domain {dd} already mapped to another node, unset first.");
                });
                var m = meta.Where(x => x.Key == path).ToList();
                m.ForEach(x =>
                {
                    repo.DeletePuckMeta(x);
                });
                d.ForEach(x =>
                {
                    var newMeta = new PuckMeta();
                    newMeta.Name = DBNames.DomainMapping;
                    newMeta.Key = path;
                    newMeta.Value = x;
                    repo.AddPuckMeta(newMeta);
                });
                repo.SaveChanges();
            }
            StateHelper.UpdateDomainMappings(true);
        }
        public void SetLocalisation(string path, string variant)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("path null or empty");
            if (string.IsNullOrEmpty(variant))
            {
                var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.PathToLocale && x.Key == path).ToList();
                meta.ForEach(x =>
                {
                    repo.DeletePuckMeta(x);
                });
                if (meta.Count > 0)
                    repo.SaveChanges();
            }
            else
            {
                var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.PathToLocale && x.Key == path).ToList();
                meta.ForEach(x => repo.DeletePuckMeta(x));

                var newMeta = new PuckMeta();
                newMeta.Name = DBNames.PathToLocale;
                newMeta.Key = path;
                newMeta.Value = variant;
                repo.AddPuckMeta(newMeta);
                repo.SaveChanges();
            }
            StateHelper.UpdatePathLocaleMappings(true);
        }

        public List<BaseTask> Tasks()
        {
            var result = new List<BaseTask>();
            var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.Tasks).ToList();
            var toRemove = new List<PuckMeta>();
            meta.ForEach(x =>
            {
                //var type = Type.GetType(x.Key);
                var type = TaskTypes().FirstOrDefault(xx => xx.FullName.Equals(x.Key));
                if (type == null)
                {
                    toRemove.Add(x);
                    return;
                }
                var instance = JsonConvert.DeserializeObject(x.Value, type) as BaseTask;
                instance.Id = x.Id;
                if (!tdispatcher.CanRun(instance))
                {
                    toRemove.Add(x);
                    return;
                }
                result.Add(instance);
            });
            toRemove.ForEach(x => repo.DeletePuckMeta(x));
            repo.SaveChanges();
            return result;
        }
        public List<Type> TaskTypes(bool ignoreSystemTasks = true, bool ignoreBaseTask = true)
        {
            var excludedTypes = new List<Type>();
            if (ignoreSystemTasks)
            {
                excludedTypes.AddRange(SystemTasks().Select(x => x.GetType()));
            }
            if (ignoreBaseTask)
            {
                excludedTypes.Add(typeof(BaseTask));
            }
            var tasks = FindDerivedClasses(typeof(BaseTask), null, false).ToList();
            var result = tasks.Where(x => !excludedTypes.Contains(x)).ToList();
            return result;
        }
        public List<BaseTask> SystemTasks()
        {
            var result = new List<BaseTask>();
            result.Add(new SyncCheckTask());
            result.Add(new KeepAliveTask());
            if (PuckCache.IsEditServer)
                result.Add(new TimedPublishTask());
            return result;
        }

        public String PathLocalisation(string path)
        {
            var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.PathToLocale && (path+"/").StartsWith(x.Key+"/")).OrderByDescending(x => x.Key.Length).FirstOrDefault();
            return meta == null ? null : meta.Value;
        }
        public String DomainMapping(string path)
        {
            var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.DomainMapping && x.Key == path).ToList();
            return meta.Count == 0 ? string.Empty : string.Join(",", meta.Select(x => x.Value));
        }
        public Notify NotifyModel(string path)
        {
            //:actions
            //save
            //publish
            //delete
            //move

            //:target
            //recursive
            //path

            //:filter
            //users            

            //DBNAME
            //notify:admin:save(|publish|delete|move|)
            //DBKEY
            //|user|user1|etc|
            //VALUE
            //content/home/*
            var username = System.Web.HttpContext.Current.User.Identity.Name;
            var model = new Notify { Path = path, Actions = new List<string>(), Users = new List<string>() };
            var notify = repo.GetPuckMeta()
                .Where(x => x.Name.StartsWith(DBNames.Notify))
                .Where(x => x.Key.Equals(path))
                .Where(x => x.Value == username)
                .FirstOrDefault();
            if (notify != null)
            {
                var actions = notify.Name.Substring((DBNames.Notify + ":*").Length);
                var actionList = actions.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                //var usersList = notify.Value.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                model.Actions = actionList;
                //model.Users = usersList;
                model.Recursive = notify.Name.Contains(":*");
            }
            model.AllActions = Enum.GetNames(typeof(NotifyActions)).Select(x => new SelectListItem() { Text = x, Value = x, Selected = model.Actions.Contains(x) });
            //model.AllUsers = Roles.GetUsersInRole(PuckRoles.Puck).ToList().Select(x => new SelectListItem() { Text = x, Value = x, Selected = model.Users.Contains(x) });
            return model;
        }
        public void SetNotify(Notify model)
        {
            model.Actions = model.Actions ?? new List<string>();
            model.Users = model.Users ?? new List<string>();
            var username = System.Web.HttpContext.Current.User.Identity.Name;
            var dbname = string.Concat(DBNames.Notify, ":", model.Recursive ? "*" : ".", string.Join("", model.Actions.Select(x => ":" + x)));
            var dbkey = model.Path;
            var dbvalue = username;
            repo.GetPuckMeta()
                .Where(x => x.Name.StartsWith(DBNames.Notify))
                .Where(x => x.Key.Equals(model.Path))
                .Where(x => x.Value.Equals(username))
                .ToList()
                .ForEach(x => repo.DeletePuckMeta(x));
            var newMeta = new PuckMeta
            {
                Key = dbkey,
                Name = dbname,
                Value = dbvalue
            };
            repo.AddPuckMeta(newMeta);
            repo.SaveChanges();
        }
        public async Task<List<PuckUser>> UsersToNotify(string path, NotifyActions action)
        {
            //var user = HttpContext.Current.User.Identity.Name;
            var strAction = action.ToString();
            var metas = repo.GetPuckMeta()
                .Where(x => x.Name.Contains(":" + strAction /*+ ":"*/))
                .Where(
                    x => x.Key.Equals(path) && x.Name.Contains(":.:")
                    ||
                    x.Key.StartsWith(path) && x.Name.Contains(":*:")
                )
                .ToList();
            var usernames = metas.Select(x => x.Value).ToList();
            var users = new List<PuckUser>();
            foreach (var username in usernames)
            {
                var user = await userManager.FindByNameAsync(username);
                if (user != null)
                    users.Add(user);
            }
            return users;
        }
        public List<GeneratedProperty> AllProperties(GeneratedModel model)
        {
            var result = new List<GeneratedProperty>();
            var mod = model;
            do
            {
                result.AddRange(mod.Properties.ToList());
                if (!string.IsNullOrEmpty(mod.Inherits))
                    mod = repo.GetGeneratedModel().Where(x => x.IFullName == mod.Inherits).SingleOrDefault();
                else
                    mod = null;
            } while (mod != null);
            return result;
        }
        public List<string> FieldGroups(string type = null)
        {
            var result = new List<string>();
            var fieldGroups = repo.GetPuckMeta().Where(x => x.Name.StartsWith(DBNames.FieldGroups)).ToList();
            fieldGroups.ForEach(x =>
            {
                string typeName = x.Name.Replace(DBNames.FieldGroups, "");
                string groupName = x.Key;
                string FieldName = x.Value;
                result.Add(string.Concat(typeName, ":", groupName, ":", FieldName));
            });
            if (!string.IsNullOrEmpty(type))
            {
                //var targetType = ApiHelper.GetType(type);
                var targetType = ApiHelper.GetTypeFromName(type);
                var baseTypes = BaseTypes(targetType);
                baseTypes.Add(targetType);
                result = result
                    .Where(x => baseTypes
                        .Any(xx => xx.Name.Equals(x.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[0])))
                        .ToList();
            }
            return result;
        }

        public List<Variant> Variants()
        {
            var allVariants = AllVariants();
            var results = new List<Variant>();
            var allLanguageMetas = repo.GetPuckMeta().Where(x => x.Name == DBNames.Settings && x.Key == DBKeys.Languages).ToList().OrderBy(x=>x.Dt??DateTime.Now).ToList();
            for (var i = 0; i < allLanguageMetas.Count; i++)
            {
                var language = allLanguageMetas[i];
                if (language != null)
                {
                    var variant = allVariants.Where(x => x.Key.ToLower().Equals(language.Value.ToLower())).FirstOrDefault();
                    if (variant != null)
                    {
                        variant.IsDefault = i == 0;
                        results.Add(variant);
                    }
                }
            }
            return results;
        }
        public List<Variant> AllVariants()
        {
            var results = new List<Variant>();
            foreach (var ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                string specName = "(none)";
                try
                {
                    specName = CultureInfo.CreateSpecificCulture(ci.Name).Name;
                }
                catch { }
                results.Add(new Variant { FriendlyName = ci.EnglishName, IsDefault = false, Key = ci.Name.ToLower() });
            }
            return results;
        }

        public List<FileInfo> AllowedViews(string type, string[] excludePaths = null)
        {
            var results = new List<FileInfo>();
            var paths = repo.GetPuckMeta().Where(x => x.Name == DBNames.TypeAllowedTemplates && x.Key.Equals(type))
                .ToList()
                .OrderBy(x=>x.Dt??DateTime.Now)
                .Select(x => x.Value)
                .ToList();
            if (paths.Count == 0) return results;
            var views = Views(excludePaths);
            foreach (var path in paths) {
                var view = views.FirstOrDefault(x=>path == ToVirtualPath(x.FullName));
                if(view!=null)
                    results.Add(view);
            }
            return results;
        }
        public List<FileInfo> Views(string[] excludePaths = null)
        {
            if (excludePaths == null)
                excludePaths = new string[] { };
            for (var i = 0; i < excludePaths.Length; i++)
            {
                excludePaths[i] = ApiHelper.MapPath(excludePaths[i]);
            }
            var templateDirPath = ApiHelper.MapPath("~/Views");
            var viewFiles = new DirectoryInfo(templateDirPath).EnumerateFiles("*.cshtml", SearchOption.AllDirectories)
                .Where(x => !excludePaths.Any(y => x.FullName.ToLower().StartsWith(y.ToLower())))
                .ToList();
            return viewFiles;
        }

        public List<I_Puck_Editor_Settings> EditorSettings()
        {
            var result = new List<I_Puck_Editor_Settings>();
            var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.EditorSettings).ToList();
            meta.ForEach(x =>
            {
                //key - settingsType:modelType:propertyName
                var keys = x.Key.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                //var type = Type.GetType(keys[0]);
                var type = ApiHelper.EditorSettingTypes().FirstOrDefault(xx => xx.FullName.Equals(keys[0]));
                var instance = JsonConvert.DeserializeObject(x.Value, type) as I_Puck_Editor_Settings;
                result.Add(instance);
            });
            return result;
        }
        public List<Type> AllowedTypes(string typeName)
        {
            var meta = repo.GetPuckMeta()
                .Where(x => x.Name == DBNames.TypeAllowedTypes && x.Key.Equals(typeName))
                .ToList()
                .OrderBy(x=>x.Dt??DateTime.Now)
                .ToList();
            //var result = meta.Select(x=>ApiHelper.GetType(x.Value)).ToList();
            var result = meta.Select(x => ApiHelper.GetTypeFromName(x.Value)).Where(x=>x!=null).ToList();
            return result;
        }

        public List<Type> AllModels(bool inclusive = false)
        {
            var models = Models(inclusive);
            //var gmodels = GeneratedModelTypes();
            //models.AddRange(gmodels);
            return models;
        }
        /*
        public static List<Type> GeneratedModels() {
            var models = new List<Type>();
            Repo.GetGeneratedModel().ToList()
                .ForEach(x=>models.Add(Type.GetType(x.IFullName)));
            return models;
        } 
        */
        public List<GeneratedModel> GeneratedModels()
        {
            var models = repo.GetGeneratedModel().ToList();
            return models;
        }
        public List<Type> GeneratedModelTypes(List<Type> excluded = null)
        {
            var models = repo.GetGeneratedModel()
                .Where(x => !string.IsNullOrEmpty(x.IFullName))
                .ToList()
                .Select(x => GetType(x.IFullName))
                .Where(x => x != null)
                .ToList();
            if (excluded != null)
            {
                models = models.Except(excluded).ToList();
            }
            return models;
        }
        public List<Type> Models(bool inclusive = false)
        {
            var excluded = new List<Type>() { typeof(PuckRevision) };
            //var igenerated = FindDerivedClasses(typeof(I_Generated)).Where(x=>x.IsInterface);
            //var generated = new List<Type>();
            //igenerated.ToList().ForEach(x => {
            //    var concrete = FindDerivedClasses(x);
            //    generated.AddRange(concrete);
            //});
            //excluded.AddRange(generated);
            var result = FindDerivedClasses(typeof(BaseModel), excluded, inclusive).ToList();
            //result.AddRange(igenerated);
            return result;
        }
        public List<string> OrphanedTypeNames()
        {
            var loadedTypes = Models().Select(x => x.Name).ToList();
            var names = repo.GetPuckRevision().Where(x => x.Current && !loadedTypes.Contains(x.Type)).Select(x => x.Type).Distinct().ToList();
            return names;
        }

    }
}
