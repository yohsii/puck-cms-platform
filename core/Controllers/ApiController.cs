using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using puck.core.Helpers;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using puck.core.Abstract;
using puck.core.Constants;
using puck.core.Base;
using puck.core.Entities;
using puck.core.Models;
using StackExchange.Profiling;
using System.Threading.Tasks;
using puck.core.State;
using puck.core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using puck.core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using puck.core.Concrete;
using Microsoft.Extensions.Caching.Memory;

namespace puck.core.Controllers
{
    [Area("puck")]
    //[SetPuckCulture]
    public class ApiController : BaseController
    {
        private static readonly object _savelck = new object();
        I_Content_Indexer indexer;
        I_Content_Searcher searcher;
        I_Log log;
        I_Puck_Repository repo;
        RoleManager<PuckRole> roleManager;
        UserManager<PuckUser> userManager;
        SignInManager<PuckUser> signInManager;
        I_Content_Service contentService;
        I_Api_Helper apiHelper;
        IHostEnvironment env;
        IConfiguration config;
        IMemoryCache cache;
        public ApiController(I_Api_Helper ah, I_Content_Service cs, I_Content_Indexer i, I_Content_Searcher s, I_Log l, I_Puck_Repository r, RoleManager<PuckRole> rm, UserManager<PuckUser> um, SignInManager<PuckUser> sm, IHostEnvironment env, IConfiguration config, IMemoryCache cache)
        {
            this.indexer = i;
            this.searcher = s;
            this.log = l;
            this.repo = r;
            this.roleManager = rm;
            this.userManager = um;
            this.signInManager = sm;
            this.contentService = cs;
            this.apiHelper = ah;
            this.env = env;
            this.config = config;
            this.cache = cache;
            StateHelper.SetFirstRequestUrl();
            SyncIfNecessary();
        }
        public ActionResult KeepAlive()
        {
            return base.Content("success");
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult GetCropSizes()
        {
            return Json(PuckCache.CropSizes);
        }

        [Authorize(Roles = PuckRoles.Settings, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult Redirects()
        {
            bool success = true;
            string message = "";
            var results = new List<PuckRedirect>();
            try
            {
                results = repo.GetPuckRedirect().ToList();
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { redirects = results, success = success, message = message });
        }
        [HttpPost]
        [Authorize(Roles = PuckRoles.Settings, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult AddRedirect(string from, string to, string type)
        {
            bool success = true;
            string message = "";
            try
            {
                apiHelper.AddRedirect(from, to, type);
                contentService.AddAuditEntry(Guid.Empty, "", AuditActions.AddRedirect, $"from:{from}, to:{to}, type:{type}", User.Identity.Name);
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }

        [HttpPost]
        [Authorize(Roles = PuckRoles.Settings, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult DeleteRedirect(string from)
        {
            bool success = true;
            string message = "";
            try
            {
                apiHelper.DeleteRedirect(from);
                contentService.AddAuditEntry(Guid.Empty, "", AuditActions.DeleteRedirect, $"from:{from}", User.Identity.Name);
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
        public ActionResult DevPage(string id = "0a2ebbd3-b118-4add-a219-4dbc54cd742a")
        {
            var guid = Guid.Parse(id);
            var revision = repo.GetPuckRevision().FirstOrDefault(x => x.Current && x.Id == guid);
            var model = revision.ToBaseModel();
            return View(model);
        }
        //[Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = "Identity.Application")]
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> UserLanguage()
        {
            string variant = PuckCache.SystemVariant;
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            if (!string.IsNullOrEmpty(user.PuckUserVariant))
                //var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.UserVariant && x.Key == User.Identity.Name).FirstOrDefault();
                //if (meta != null)
                variant = user.PuckUserVariant;
            return Json(variant);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> UserRoles()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            var roles = await userManager.GetRolesAsync(user);
            return Json(roles);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult FieldGroups(string type)
        {
            var cacheKey = "fieldGroups_" + type;
            var model = cache.Get<List<string>>(cacheKey);
            if (model == null)
            {
                model = apiHelper.FieldGroups(type);
                cache.Set(cacheKey, model, TimeSpan.FromMinutes(30));
            }
            return Json(model);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult CreateDialog(string type)
        {
            return View();
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult Variants()
        {
            var model = apiHelper.Variants();
            return Json(model);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult AllVariants()
        {
            var model = apiHelper.AllVariants();
            return Json(model);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult Preview(string path, string variant)
        {
            var model = repo.GetPuckRevision().Where(x => x.Current && x.Path.ToLower().Equals(path.ToLower()) && x.Variant.ToLower().Equals(variant.ToLower())).FirstOrDefault();
            return Preview(model);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult PreviewGuid(Guid id, string variant)
        {
            var model = repo.GetPuckRevision().Where(x => x.Current && x.Id == id && x.Variant.ToLower().Equals(variant.ToLower())).FirstOrDefault();
            return Preview(model);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        private ActionResult Preview(PuckRevision model)
        {
            var dmode = this.GetDisplayModeId();
            string templatePath = model.TemplatePath;
            if (!string.IsNullOrEmpty(dmode))
            {
                string dpath = templatePath.Insert(templatePath.LastIndexOf('.') + 1, dmode + ".");
                if (System.IO.File.Exists(ApiHelper.MapPath(dpath)))
                {
                    templatePath = dpath;
                }
            }
            var mod = model.ToBaseModel();
            var variant = GetVariant(mod.Path);
            HttpContext.Items["variant"] = variant;
            //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(threadVariant);
            return View(templatePath, mod);
        }
        [Authorize(Roles = PuckRoles.Notify, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult Notify(string p_path)
        {
            var model = apiHelper.NotifyModel(p_path);
            return Json(model);
        }
        [Authorize(Roles = PuckRoles.Notify, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult NotifyDialog(string p_path)
        {
            var model = apiHelper.NotifyModel(p_path);
            return View(model);
        }
        [Authorize(Roles = PuckRoles.Notify, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        [HttpPost]
        public JsonResult Notify(Notify model)
        {
            string message = "";
            bool success = false;
            try
            {
                apiHelper.SetNotify(model);
                success = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
        [Authorize(Roles = PuckRoles.Domain, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult DomainMappingDialog(string p_path)
        {
            var model = apiHelper.DomainMapping(p_path);
            return View((object)model);
        }
        [Authorize(Roles = PuckRoles.Domain, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult DomainMapping(string p_path)
        {
            var model = apiHelper.DomainMapping(p_path);
            return Json(model);
        }
        [Authorize(Roles = PuckRoles.Domain, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        [HttpPost]
        public JsonResult DomainMapping(string p_path, string domains)
        {
            string message = "";
            bool success = false;
            try
            {
                apiHelper.SetDomain(p_path, domains);
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                message = ex.Message;
            }
            return Json(new { message = message, success = success });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult GetCacheItem(string key)
        {
            var item = cache.Get(key);
            return Json(new { item = item });
        }
        [Authorize(Roles = PuckRoles.Sync, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<ActionResult> Sync(SyncModel syncModel)
        {
            string message = "";
            bool success = false;
            var cacheKey = Guid.NewGuid().ToString();
            var username = User.Identity.Name;
            try
            {
                PuckCache.Cache.Set(cacheKey, $"initializing sync");
                System.Threading.Tasks.Task.Factory.StartNew(async () =>
                {
                    using (var scope = PuckCache.ServiceProvider.CreateScope())
                    {
                        var tDispatcher = scope.ServiceProvider.GetService<I_Task_Dispatcher>();
                        var indexer = scope.ServiceProvider.GetService<I_Content_Indexer>();
                        var apiHelper = scope.ServiceProvider.GetService<I_Api_Helper>();
                        var repo = scope.ServiceProvider.GetService<I_Puck_Repository>();
                        var cService = scope.ServiceProvider.GetService<I_Content_Service>();
                        var config = scope.ServiceProvider.GetService<IConfiguration>();
                        I_Puck_Context context = null;
                        var roleManager = scope.ServiceProvider.GetService<RoleManager<PuckRole>>();
                        var userManager = scope.ServiceProvider.GetService<UserManager<PuckUser>>();
                        var logger = scope.ServiceProvider.GetService<I_Log>();
                        var configs = apiHelper.GetConfigs();
                        var configContainer = configs.FirstOrDefault(x => x.Name.ToLower().Equals(syncModel.SelectedConfig.ToLower()));
                        var connectionStringName = apiHelper.GetConnectionStringName();
                        var connectionString = configContainer.Config.GetConnectionString(connectionStringName);
                        var revision = repo.GetPuckRevision().FirstOrDefault(x => x.Id == syncModel.Id && x.Current);
                        PuckRevision parent = null;
                        if (revision.ParentId != Guid.Empty)
                            parent = repo.GetPuckRevision().FirstOrDefault(x => x.Id == revision.ParentId && x.Current);

                        switch (connectionStringName)
                        {
                            case "SQLServer":
                                context = new PuckContextSQLServer(configContainer.Config);
                                break;
                            case "PostgreSQL":
                                context = new PuckContextPostgreSQL(configContainer.Config);
                                break;
                            case "MySQL":
                                context = new PuckContextMySQL(configContainer.Config);
                                break;
                            case "SQLite":
                                context = new PuckContextSQLite(configContainer.Config);
                                break;
                        }
                        var destinationRepo = new Puck_Repository(context);
                        var destinationContentService = new ContentService(config, cService.roleManager, cService.userManager, destinationRepo, tDispatcher, indexer, logger, apiHelper, new MemoryCache(new MemoryCacheOptions() { SizeLimit = null }));
                        if (revision.ParentId != Guid.Empty && !destinationRepo.GetPuckRevision().Any(x => x.Id == revision.ParentId && x.Current))
                        {
                            PuckCache.Cache.Set(cacheKey, $"Error. Cannot sync, the destination database doesn't contain the parent element \"{parent.NodeName}\" with id {parent.Id}");
                            return;
                        }
                        try
                        {
                            await cService.Sync(revision.Id, revision.ParentId, syncModel.IncludeDescendants, syncModel.OnlyOverwriteIfNewer, destinationContentService, PuckCache.Cache, cacheKey, userName: username);
                        }
                        catch (Exception ex)
                        {
                            PuckCache.Cache.Set(cacheKey, $"Error. {ex.Message}");
                        }
                    }
                });
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                message = ex.Message;
            }
            return Json(new { message = message, success = success, cacheKey = cacheKey });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult GetReferencedContent(Guid id, string variant)
        {
            var revision = repo.CurrentRevision(id,variant);

            if (revision == null)
                return Json(new List<BaseModel>());

            var model = revision.ToBaseModel();

            var url = "";
            if (model.Path.Count(x => x == '/') == 1)
                url = "/";
            else{
                var trimmed = model.Path.TrimStart('/');
                url = trimmed.Substring(trimmed.IndexOf("/"));
            }

            var innerQuery = new QueryHelper<BaseModel>(publishedContentOnly: false)
                .Field(x => x.References, $"{id.ToString()}_{variant.ToLower()}")
                .Field(x => x.References, url.Replace("/", @"\/"));

            if (model.Type == "ImageVM") {
                var puckImageProperty = model.GetType().GetProperties().Where(x => x.PropertyType == typeof(PuckImage)).FirstOrDefault();
                if (puckImageProperty != null) {
                    var puckImageValue = puckImageProperty.GetValue(model) as PuckImage;
                    if (puckImageValue != null && !string.IsNullOrEmpty(puckImageValue.Path)) {
                        if (puckImageValue.Path.StartsWith("http")) {
                            var relativePath = new Uri(puckImageValue.Path).AbsolutePath;
                            if(!string.IsNullOrEmpty(relativePath))
                                innerQuery.Field(x=>x.References,relativePath.Wrap());
                        }else
                            innerQuery.Field(x => x.References, puckImageValue.Path.Wrap());
                    }
                }
            }

            var qh = new QueryHelper<BaseModel>(publishedContentOnly: false)
                .Must(innerQuery);

            var results = qh
                .GetAllNoCast()
                .Where(x => !(x.Id == id && x.Variant.ToLower().Equals(variant.ToLower())))
                .ToList();

            return Json(results);
        }
        [HttpPost]
        [Authorize(Roles = PuckRoles.Sync, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<ActionResult> CancelSync(string key)
        {
            cache.Set<bool?>($"cancel{key}", true);
            return Json(new { success = true, message = "sync cancelled." });
        }
        [Authorize(Roles = PuckRoles.Sync, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<ActionResult> SyncDialog(Guid id)
        {
            var model = new SyncModel();
            var revision = repo.GetPuckRevision().FirstOrDefault(x => x.Id == id && x.Current);

            model.PendingSyncs = new List<KeyValuePair<string, string>>();
            var syncKeysToRemove = new List<string>();
            foreach (var syncKey in PuckCache.SyncKeys)
            {
                var name = cache.Get<string>($"name{syncKey}");
                var status = cache.Get<string>(syncKey);
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(status))
                {
                    syncKeysToRemove.Add(syncKey);
                    continue;
                }
                var kvp = new KeyValuePair<string, string>($"Content: {name}. {status}", syncKey);
                model.PendingSyncs.Add(kvp);
            }
            syncKeysToRemove.ForEach(x => PuckCache.SyncKeys.RemoveAll(xx => xx.Equals(x)));

            model.Model = revision.ToBaseModel();
            model.Id = revision.Id;
            model.Configs = apiHelper.GetConfigs();
            model.Configs.RemoveAll(x => x.Name.ToLower().Equals($"appsettings.{env.EnvironmentName.ToLower()}.json"));
            var toRemove = new List<ConfigContainer>();
            var connectionStringName = apiHelper.GetConnectionStringName();
            foreach (var cc in model.Configs)
            {
                if (string.IsNullOrEmpty(cc.Config.GetConnectionString(connectionStringName)))
                {
                    toRemove.Add(cc);
                }
                else if (cc.Config.GetConnectionString(connectionStringName).ToLower().Equals(config.GetConnectionString(connectionStringName).ToLower()))
                {
                    toRemove.Add(cc);
                }
            }
            toRemove.ForEach(x => model.Configs.Remove(x));
            return View(model);
        }

        [Authorize(Roles = PuckRoles.Copy, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<ActionResult> Copy(Guid id, Guid parentId, bool includeDescendants)
        {
            string message = "";
            bool success = false;
            try
            {
                await contentService.Copy(id, parentId, includeDescendants);
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                message = ex.Message;
            }
            return Json(new { message = message, success = success });
        }

        [Authorize(Roles = PuckRoles.Move, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> Move(Guid startId, Guid destinationId)
        {
            string message = "";
            bool success = false;
            try
            {
                await contentService.Move(startId, destinationId);
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                message = ex.Message;
            }
            return Json(new { message = message, success = success });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult GetTags(string category)
        {
            bool success = true;
            string message = "";
            List<PuckTag> tags = null;
            try
            {
                tags = cache.Get<List<PuckTag>>("tags_" + category);
                if (tags == null)
                {
                    if (string.IsNullOrEmpty(category))
                        tags = repo.GetPuckTag().Where(x => string.IsNullOrEmpty(x.Category)).ToList();
                    else
                        tags = repo.GetPuckTag().Where(x => x.Category.ToLower().Equals(category.ToLower())).ToList();
                    cache.Set("tags_" + category, tags, TimeSpan.FromSeconds(10));
                }
            }
            catch (Exception ex)
            {
                success = false;
                log.Log(ex);
                message = ex.Message;
            }
            return Json(new { tags = tags.Select(x => x.Tag), success = success, message = message });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        [HttpPost]
        public ActionResult AddTag(string tag, string category)
        {
            bool success = true;
            string message = "";
            try
            {
                apiHelper.AddTag(tag, category);
            }
            catch (Exception ex)
            {
                success = false;
                log.Log(ex);
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        [HttpPost]
        public ActionResult DeleteTag(string tag, string category)
        {
            bool success = true;
            string message = "";
            try
            {
                apiHelper.DeleteTag(tag, category);
            }
            catch (Exception ex)
            {
                success = false;
                log.Log(ex);
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
        [Authorize(Roles = PuckRoles.Localisation, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult LocalisationDialog(string p_path)
        {
            var model = apiHelper.PathLocalisation(p_path);
            return View((object)model);
        }
        [Authorize(Roles = PuckRoles.Localisation, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult Localisation(string p_path)
        {
            var model = apiHelper.PathLocalisation(p_path);
            return Json(model);
        }
        [Authorize(Roles = PuckRoles.Localisation, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        [HttpPost]
        public JsonResult Localisation(string p_path, string variant)
        {
            string message = "";
            bool success = false;
            try
            {
                if (apiHelper.PathLocalisation(p_path) != variant)
                    apiHelper.SetLocalisation(p_path, variant);
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                message = ex.Message;
            }
            return Json(new { message = message, success = success });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult RootsLocalisations(string ids)
        {
            var guids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Guid.Parse(x));
            var result = new List<dynamic>();
            foreach (var guid in guids)
            {
                var revision = repo.PublishedOrCurrentRevisions(guid).FirstOrDefault();
                var variant = apiHelper.PathLocalisation(revision.Path);
                result.Add(new { path = revision.Path.ToLower(), variant = variant });
            }
            return Json(result);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult AllLocalisations()
        {
            var result = PuckCache.PathToLocale ?? new Dictionary<string, string>();
            return Json(result);
        }
        [Authorize(Roles = PuckRoles.ChangeType, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult ChangeTypeDialog(Guid id)
        {
            var revision = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).FirstOrDefault();
            PuckRevision parent = repo.GetPuckRevision().Where(x => x.Id == revision.ParentId && x.Current).FirstOrDefault();
            var children = repo.CurrentRevisionsByParentId(revision.Id).ToList();

            //only return allowed types
            List<Type> allowedTypes = null;
            if (parent == null)
                allowedTypes = apiHelper.Models();
            else
                allowedTypes = apiHelper.AllowedTypes(parent.Type);

            if (allowedTypes.Count == 0)
                allowedTypes = apiHelper.Models();

            //further filtering based on allowed types and the types of the children nodes
            var typesToRemove = new List<Type>();
            foreach (var type in allowedTypes)
            {
                var typeAllowedTypes = apiHelper.AllowedTypes(type.Name);
                if (typeAllowedTypes.Count == 0)
                    continue;
                foreach (var childRevision in children)
                {
                    if (!typeAllowedTypes.Any(x => x.Name == childRevision.Type))
                    {
                        typesToRemove.Add(type);
                    }
                }
            }
            typesToRemove.ForEach(x => allowedTypes.Remove(x));

            return View(allowedTypes);
        }
        [Authorize(Roles = PuckRoles.ChangeType, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult ChangeTypeMappingDialog(Guid id, string newType)
        {
            var revision = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).FirstOrDefault();
            //var tCurrentType = ApiHelper.GetType(revision.Type);
            var tCurrentType = ApiHelper.GetTypeFromName(revision.Type);
            //var tNewType = ApiHelper.GetType(newType);
            var tNewType = ApiHelper.GetTypeFromName(newType);

            var baseModelProperties = typeof(BaseModel).GetProperties().ToList();
            var currentTypeProperties = tCurrentType.GetProperties().Where(x => !baseModelProperties.Any(xx => xx.Name == x.Name)).ToList();
            var newTypeProperties = tNewType.GetProperties().Where(x => !baseModelProperties.Any(xx => xx.Name == x.Name)).ToList();
            var model = new ChangeType()
            {
                ContentId = id,
                ContentType = tCurrentType,
                Revision = revision,
                ContentProperties = currentTypeProperties,
                NewType = tNewType,
                NewTypeProperties = newTypeProperties
            };

            model.Templates = apiHelper.AllowedViews(tNewType.Name);
            if (model.Templates.Count == 0)
                model.Templates = apiHelper.Views();
            var selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "-- select template --", Value = "", Selected = true });
            foreach (var template in model.Templates.OrderBy(x => x.Name))
            {
                selectListItems.Add(new SelectListItem() { Text = template.Name, Value = ApiHelper.ToVirtualPath(template.FullName) });
            }
            model.TemplatesSelectListItems = selectListItems;


            return View(model);
        }
        [Authorize(Roles = PuckRoles.TimedPublish)]
        public ActionResult TimedPublish(TimedPublish model)
        {
            var success = false;
            var message = "";
            if (ModelState.IsValid)
            {
                var key = $"{model.Id.ToString()}:{model.Variant}";
                if (model.PublishAt.HasValue)
                {
                    var value = "";
                    if (model.PublishDescendantVariants != null)
                    {
                        value = string.Join(",", model.PublishDescendantVariants);
                    }
                    var meta = repo.GetPuckMeta().FirstOrDefault(x => x.Name == DBNames.TimedPublish && x.Key == key);
                    if (meta == null)
                    {
                        meta = new PuckMeta();
                        repo.AddPuckMeta(meta);
                    }
                    meta.Name = DBNames.TimedPublish;
                    meta.Key = key;
                    meta.Dt = model.PublishAt.Value;
                    meta.UserName = User.Identity.Name;
                    meta.Value = value;
                }
                else
                {
                    var meta = repo.GetPuckMeta().FirstOrDefault(x => x.Name == DBNames.TimedPublish && x.Key == key);
                    if (meta != null)
                    {
                        repo.DeletePuckMeta(meta);
                    }
                }
                if (model.UnpublishAt.HasValue)
                {
                    var meta = repo.GetPuckMeta().FirstOrDefault(x => x.Name == DBNames.TimedUnpublish && x.Key == key);
                    if (meta == null)
                    {
                        meta = new PuckMeta();
                        repo.AddPuckMeta(meta);
                    }
                    meta.Name = DBNames.TimedUnpublish;
                    meta.Key = key;
                    meta.Dt = model.UnpublishAt.Value;
                    meta.UserName = User.Identity.Name;
                }
                else
                {
                    var meta = repo.GetPuckMeta().FirstOrDefault(x => x.Name == DBNames.TimedUnpublish && x.Key == key);
                    if (meta != null)
                    {
                        repo.DeletePuckMeta(meta);
                    }
                }

                repo.SaveChanges();
                success = true;
                message = "Schedule set";
            }
            else
            {
                message = string.Join(". ", ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage)));
            }
            return Json(new { success = success, message = message });
        }
        [Authorize(Roles = PuckRoles.TimedPublish, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult TimedPublishDialog(Guid id, string variant)
        {
            var model = new TimedPublish();
            model.Id = id;
            model.Variant = variant;
            model.Variants = apiHelper.Variants();
            var key = $"{id}:{variant}";
            var publishMeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.TimedPublish && x.Key == key).FirstOrDefault();
            if (publishMeta != null)
            {
                if (publishMeta.Dt.HasValue)
                {
                    model.PublishAt = publishMeta.Dt.Value;
                    model.PublishDescendantVariants = publishMeta.Value?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    repo.DeletePuckMeta(publishMeta);
                }
            }
            var unPublishMeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.TimedUnpublish && x.Key == key).FirstOrDefault();
            if (unPublishMeta != null)
            {
                if (unPublishMeta.Dt.HasValue)
                {
                    model.UnpublishAt = unPublishMeta.Dt.Value;
                    model.UnpublishDescendantVariants = unPublishMeta.Value?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    repo.DeletePuckMeta(unPublishMeta);
                }
            }
            repo.SaveChanges();
            return View(model);
        }
        [Authorize(Roles = PuckRoles.Audit, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult AuditMarkup(Guid id, int page = 1, int pageSize = 10, string variant = null, string userName = null)
        {
            var revision = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).FirstOrDefault();
            if (revision != null)
                ViewData["nodeName"] = revision.NodeName;
            var audit = repo.GetPuckAudit().Where(x => x.ContentId == id);
            if (!string.IsNullOrEmpty(variant))
                audit = audit.Where(x => x.Variant.ToLower().Equals(variant.ToLower()));
            if (!string.IsNullOrEmpty(userName))
                audit = audit.Where(x => x.UserName.ToLower().Equals(userName.ToLower()));
            var count = audit.Count();
            var model = audit.OrderByDescending(x => x.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewData["count"] = count;
            ViewData["currentPage"] = page;
            ViewData["pageSize"] = pageSize;
            ViewData["variants"] = apiHelper.Variants();
            return View("Audit", model);
        }
        [Authorize(Roles = PuckRoles.ChangeType, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult> ChangeTypeMapping(Guid id, string newType, IFormCollection fc)
        {
            string message = "";
            bool success = false;
            try
            {
                var revisions = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).ToList();
                foreach (var revision in revisions)
                {
                    var model = revision.ToBaseModel();
                    //var tNewType = ApiHelper.GetType(newType);
                    var tNewType = ApiHelper.GetTypeFromName(newType);
                    var newModel = Activator.CreateInstance(tNewType);
                    var newModelAsBaseModel = newModel as BaseModel;
                    newModelAsBaseModel.Id = model.Id;
                    newModelAsBaseModel.Created = model.Created;
                    newModelAsBaseModel.CreatedBy = model.CreatedBy;
                    newModelAsBaseModel.LastEditedBy = User.Identity.Name;
                    newModelAsBaseModel.NodeName = model.NodeName;
                    newModelAsBaseModel.ParentId = model.ParentId;
                    newModelAsBaseModel.Path = model.Path;
                    newModelAsBaseModel.Published = model.Published;
                    newModelAsBaseModel.Revision = model.Revision;
                    newModelAsBaseModel.SortOrder = model.SortOrder;
                    newModelAsBaseModel.Type = newType;
                    newModelAsBaseModel.TypeChain = ApiHelper.TypeChain(tNewType);
                    newModelAsBaseModel.Updated = DateTime.Now;
                    newModelAsBaseModel.Variant = model.Variant;
                    newModelAsBaseModel.TemplatePath = fc["_SelectedTemplate"];
                    foreach (var currentPropertyName in fc.Keys)
                    {
                        var newPropertyName = fc[currentPropertyName];
                        if (string.IsNullOrEmpty(newPropertyName) || !model.GetType().GetProperties().Any(x => x.Name == currentPropertyName))
                            continue;

                        var currentValue = model.GetType().GetProperty(currentPropertyName).GetValue(model);

                        PropertyInfo prop = newModel.GetType().GetProperty(newPropertyName, BindingFlags.Public | BindingFlags.Instance);
                        if (null != prop && prop.CanWrite)
                        {
                            prop.SetValue(newModel, currentValue, null);
                        }

                    }
                    await contentService.SaveContent(newModelAsBaseModel);
                }
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult GetIdPath(Guid id)
        {
            var node = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).FirstOrDefault();
            string idPath = node == null ? string.Empty : node.IdPath;
            return Json(idPath);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult GetPath(Guid id)
        {
            var node = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).FirstOrDefault();
            string path = node == null ? string.Empty : node.Path;
            return Json(path);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> StartId()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            return Json(user.PuckStartNodeId ?? Guid.Empty);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> StartPath()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            if (user.PuckStartNodeId.HasValue && user.PuckStartNodeId != Guid.Empty)
            {
                var node = repo.GetPuckRevision().Where(x => x.Id == user.PuckStartNodeId && x.Current).FirstOrDefault();
                if (node != null)
                {
                    return Json(node.Path + "/");
                }
            }
            return Json("/");
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult SearchTypes(string root)
        {
            var typeGroups = repo.GetPuckRevision().Where(x => x.Path.StartsWith(root + "/") && x.Current).GroupBy(x => x.Type);
            var typeStrings = typeGroups.Select(x => x.Key);
            var result = new List<dynamic>();
            foreach (var typeString in typeStrings)
            {
                //var type = ApiHelper.GetType(typeString);
                var type = ApiHelper.GetTypeFromName(typeString);
                if (type != null)
                {
                    result.Add(new { Name = ApiHelper.FriendlyClassName(type), Type = typeString });
                }
            }
            return Json(result);
        }
        private List<BaseModel> DoSearch(string q, string type, string root)
        {
            var results = new List<Dictionary<string, string>>();

            var typeGroups = repo.GetPuckRevision().Where(x => x.Path.StartsWith(root + "/") && x.Current).GroupBy(x => x.Type);
            var typeStrings = typeGroups.Select(x => x.Key);

            if (string.IsNullOrEmpty(type))
            {
                foreach (var typeString in typeStrings)
                {
                    var _type = ApiHelper.GetTypeFromName(typeString);
                    var tqs = "(";
                    foreach (var t in PuckCache.TypeFields[_type.AssemblyQualifiedName])
                    {
                        if (tqs.IndexOf(" " + t.Key + ":") > -1 || tqs.IndexOf("(" + t.Key + ":") > -1)
                            continue;
                        tqs += string.Concat(t.Key, ":", q, " ");
                    }
                    tqs = tqs.Trim();
                    tqs += ")";
                    tqs += string.Concat(" AND ", FieldKeys.PuckType, ":", "\"", typeString, "\"");
                    if (!string.IsNullOrEmpty(root))
                    {
                        tqs = string.Concat(tqs, " AND ", FieldKeys.Path, ":", root.Replace("/", @"\/"), @"\/*");
                    }
                    results.AddRange(PuckCache.PuckSearcher.Query(tqs, typeString));
                }
                results = results.OrderByDescending(x => float.Parse(x[FieldKeys.Score])).ToList();
            }
            else
            {
                var _type = ApiHelper.GetTypeFromName(type);
                var tqs = "(";
                foreach (var f in PuckCache.TypeFields[_type.AssemblyQualifiedName])
                {
                    tqs += string.Concat(f.Key, ":", q, " ");
                }
                tqs = tqs.Trim();
                tqs += ")";
                tqs += string.Concat(" AND ", FieldKeys.PuckType, ":", "\"", type, "\"");
                if (!string.IsNullOrEmpty(root))
                {
                    tqs = string.Concat(tqs, " AND ", FieldKeys.Path, ":", root.Replace("/", @"\/"), @"\/*");
                }
                results.AddRange(PuckCache.PuckSearcher.Query(tqs, type));
            }

            var model = new List<BaseModel>();
            foreach (var res in results)
            {
                //var mod = JsonConvert.DeserializeObject(res[FieldKeys.PuckValue],ApiHelper.ConcreteType(ApiHelper.GetType(res[FieldKeys.PuckType]))) as BaseModel;
                var mod = JsonConvert.DeserializeObject(res[FieldKeys.PuckValue], ApiHelper.GetTypeFromName(res[FieldKeys.PuckType])) as BaseModel;
                model.Add(mod);
            }
            return model;
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult SearchView(string q, string type, string root)
        {
            var model = DoSearch(q, type, root);
            return View("search", model);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult Search(string q, string type, string root)
        {
            var model = DoSearch(q, type, root);
            var resultStr = JsonConvert.SerializeObject(model);
            return base.Content(resultStr, "application/json");
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult VariantsForNode(string path)
        {
            var nodes = repo.CurrentRevisionsByPath(path);
            var result = nodes.Select(x => new { Variant = x.Variant, Published = x.Published });
            return Json(result);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult VariantsForNodeById(Guid id)
        {
            var nodes = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).ToList();
            var result = nodes.Select(x => new { Variant = x.Variant, Published = x.Published });
            return Json(result);
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult Content(string path = "/")
        {
            //using path instead of p_path in the method sig means path won't be checked against user's start node - which we don't want for this method
            string p_path = path;
            List<PuckRevision> resultsRev;
#if DEBUG
            using (MiniProfiler.Current.Step("content by path from DB"))
            {
                resultsRev = repo.CurrentRevisionsByDirectory(p_path).ToList();
            }
#else
            resultsRev = repo.CurrentRevisionsByDirectory(p_path).ToList();
#endif
            var results = resultsRev.Select(x => x.ToBaseModel(cast: true)).ToList()
                .GroupByPath()
                .OrderBy(x => x.Value.First().Value.SortOrder)
                .ToDictionary(x => x.Key, x => x.Value);

            List<string> haveChildren = new List<string>();
            foreach (var k in results)
            {
                if (repo.CurrentRevisionChildren(k.Key).Count() > 0)
                    haveChildren.Add(k.Key);
            }
            var qh = new QueryHelper<BaseModel>();
            var publishedContent = qh.Directory(p_path).GetAll().GroupByPath();
            return Json(new { current = results, published = publishedContent, children = haveChildren });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult GetCurrentModel(Guid id, string variant = null)
        {
            var jsonStr = JsonConvert.SerializeObject(_GetCurrentModel(id, variant));
            return Content(jsonStr, "application/json");
        }
        //shouldn't be publically accessable as an action since it's protected but just incase
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        protected BaseModel _GetCurrentModel(Guid id, string variant = null)
        {
            BaseModel model;
            if (string.IsNullOrEmpty(variant))
                model = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).FirstOrDefault()?.ToBaseModel();
            else
                model = repo.CurrentRevision(id, variant)?.ToBaseModel();
            return model;
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult GetModels(string ids)
        {
            var guids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Guid.Parse(x));
            var models = new List<BaseModel>();
            foreach (var guid in guids)
            {
                var model = _GetCurrentModel(guid);
                if (model != null)
                    models.Add(model);
            }
            var jsonStr = JsonConvert.SerializeObject(models);
            return base.Content(jsonStr, "application/json");
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult SetHasChildren(Guid id)
        {
            if (id != Guid.Empty)
            {
                var revisions = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).ToList();
                if (revisions.Any())
                {
                    var hasChildren = repo.GetPuckRevision().Count(x => x.ParentId == id && x.Current) > 0;
                    if (revisions.Any(x => x.HasChildren != hasChildren))
                    {
                        revisions.ForEach(x => x.HasChildren = hasChildren);
                        repo.SaveChanges();
                    }
                }
            }
            return Json(new { success = true, message = "" });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult MinimumContentByParentId(Guid parentId = default(Guid),bool fullIndexContent=false)
        {
            //using path instead of p_path in the method sig means path won't be checked against user's start node - which we don't want for this method
            List<PuckRevision> resultsRev;
#if DEBUG
            using (MiniProfiler.Current.Step("content by path from DB"))
            {
                resultsRev = repo.CurrentRevisionsByParentId(parentId).Select(x => new PuckRevision
                {
                    TypeChain = x.TypeChain,
                    Type = x.Type,
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Path = x.Path,
                    NodeName = x.NodeName,
                    Variant = x.Variant,
                    Published = x.Published,
                    HasChildren = x.HasChildren,
                    SortOrder = x.SortOrder
                }).ToList();
            }
#else
            resultsRev = repo.CurrentRevisionsByParentId(parentId).Select(x => new PuckRevision
                {
                    TypeChain = x.TypeChain,
                    Type = x.Type,
                    Id=x.Id,
                    ParentId=x.ParentId,
                    Path=x.Path,
                    NodeName=x.NodeName,
                    Variant=x.Variant,
                    Published=x.Published,
                    HasChildren=x.HasChildren,
                    SortOrder=x.SortOrder
                }).ToList();
#endif
            var results = resultsRev.Select(x => x as BaseModel).ToList()
                .GroupById()
                .OrderBy(x => x.Value.First().Value.SortOrder)
                .ToDictionary(x => x.Key.ToString(), x => x.Value);

            List<string> haveChildren = new List<string>();
            foreach (var group in resultsRev.GroupBy(x => x.Id))
            {
                if (group.FirstOrDefault().HasChildren)
                    haveChildren.Add(group.FirstOrDefault().Id.ToString());
            }

            Dictionary<string, Dictionary<string, BaseModel>> publishedContent = null;
            if (fullIndexContent)
            {
                var qh = new QueryHelper<BaseModel>();
                publishedContent = qh.And().Field(x => x.ParentId, parentId.ToString()).GetAllNoCast(limit: int.MaxValue).GroupById().ToDictionary(x => x.Key.ToString(), x => x.Value);
            }
            else
            {
                var qh = new QueryHelper<BaseModel>().And().Field(x => x.ParentId, parentId.ToString());
                var publishedContentDictionaryList = searcher.Query(
                    qh.ToString(),
                    typeof(BaseModel).Name,
                    fieldsToLoad: new HashSet<string> { FieldKeys.ID, FieldKeys.Variant, FieldKeys.PuckType }
                    , limit: int.MaxValue);
                List<BaseModel> publishedBaseModels = new List<BaseModel>();
                foreach (var dict in publishedContentDictionaryList)
                {
                    string idStr = "";
                    string variant = "";
                    var mod = new BaseModel();
                    if (dict.TryGetValue(FieldKeys.ID, out idStr))
                    {
                        if (!string.IsNullOrEmpty(idStr))
                            mod.Id = Guid.Parse(idStr);
                        else continue;
                    }
                    else continue;
                    if (dict.TryGetValue(FieldKeys.Variant, out variant))
                    {
                        if (!string.IsNullOrEmpty(variant))
                            mod.Variant = variant;
                        else continue;
                    }
                    else continue;
                    publishedBaseModels.Add(mod);
                }
                publishedContent = publishedBaseModels.GroupById().ToDictionary(x => x.Key.ToString(), x => x.Value);
            }

            var jsonStr = JsonConvert.SerializeObject(new { current = results, published = publishedContent, children = haveChildren });
            return base.Content(jsonStr, "application/json");
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult ContentByParentId(Guid parentId = default(Guid), bool cast = true, bool fullPublishedContent = false)
        {
            //using path instead of p_path in the method sig means path won't be checked against user's start node - which we don't want for this method
            List<PuckRevision> resultsRev;
#if DEBUG
            using (MiniProfiler.Current.Step("content by path from DB"))
            {
                resultsRev = repo.CurrentRevisionsByParentId(parentId).ToList();
            }
#else
            resultsRev = repo.CurrentRevisionsByParentId(parentId).ToList();
#endif
            var results = resultsRev.Select(x => cast ? x.ToBaseModel(cast: true) : x.ToBaseModel()).Where(x=>x!=null).ToList()
                .GroupById()
                .OrderBy(x => x.Value.First().Value.SortOrder)
                .ToDictionary(x => x.Key.ToString(), x => x.Value);

            List<string> haveChildren = new List<string>();
            //foreach (var k in results)
            //{
            //    var id = Guid.Parse(k.Key);
            //    if (repo.CurrentRevisionChildren(id).Count() > 0)
            //        haveChildren.Add(k.Key);
            //}
            foreach (var group in resultsRev.GroupBy(x => x.Id))
            {
                if (group.FirstOrDefault().HasChildren)
                    haveChildren.Add(group.FirstOrDefault().Id.ToString());
            }
            Dictionary<string, Dictionary<string, BaseModel>> publishedContent = null;
            if (fullPublishedContent)
            {
                var qh = new QueryHelper<BaseModel>();
                publishedContent = qh.And().Field(x => x.ParentId, parentId.ToString()).GetAllNoCast(limit: int.MaxValue).GroupById().ToDictionary(x => x.Key.ToString(), x => x.Value);
            }
            else
            {
                var qh = new QueryHelper<BaseModel>().And().Field(x => x.ParentId, parentId.ToString());
                var publishedContentDictionaryList = searcher.Query(
                    qh.ToString(),
                    typeof(BaseModel).Name,
                    fieldsToLoad: new HashSet<string> { FieldKeys.ID, FieldKeys.Variant, FieldKeys.PuckType }
                    , limit: int.MaxValue);
                List<BaseModel> publishedBaseModels = new List<BaseModel>();
                foreach (var dict in publishedContentDictionaryList)
                {
                    string idStr = "";
                    string variant = "";
                    var mod = new BaseModel();
                    if (dict.TryGetValue(FieldKeys.ID, out idStr))
                    {
                        if (!string.IsNullOrEmpty(idStr))
                            mod.Id = Guid.Parse(idStr);
                        else continue;
                    }
                    else continue;
                    if (dict.TryGetValue(FieldKeys.Variant, out variant))
                    {
                        if (!string.IsNullOrEmpty(variant))
                            mod.Variant = variant;
                        else continue;
                    }
                    else continue;
                    publishedBaseModels.Add(mod);
                }
                publishedContent = publishedBaseModels.GroupById().ToDictionary(x => x.Key.ToString(), x => x.Value);
            }
            var jsonStr = JsonConvert.SerializeObject(new { current = results, published = publishedContent, children = haveChildren });
            return base.Content(jsonStr, "application/json");
        }
        [Authorize(Roles = PuckRoles.Sort, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult Sort(Guid parentId, List<Guid> items)
        {
            string message = "";
            bool success = false;
            try
            {
                contentService.Sort(parentId, items);
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
        [HttpPost]
        [Authorize(Roles = PuckRoles.Publish, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> RePublish(Guid id, string variants, string descendants = "")
        {
            var message = string.Empty;
            var success = false;
            try
            {
                var arrVariants = variants.Split(new char[] { ','},StringSplitOptions.RemoveEmptyEntries).ToList();
                var arrDescendants = descendants.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                await contentService.RePublish(id, arrVariants, arrDescendants);
                success = true;
            }
            catch (Exception ex)
            {
                //log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
        [HttpPost]
        [Authorize(Roles = PuckRoles.Publish, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> Publish(Guid id, string variants, string descendants = "")
        {
            var message = string.Empty;
            var success = false;
            try
            {
                var arrVariants = variants.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var arrDescendants = descendants.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                await contentService.Publish(id, arrVariants, arrDescendants);
                success = true;
            }
            catch (Exception ex)
            {
                //log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
        [HttpPost]
        [Authorize(Roles = PuckRoles.Unpublish, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> UnPublish(Guid id, string variants, string descendants = "")
        {
            var message = string.Empty;
            var success = false;
            try
            {
                var arrVariants = variants.Split(new char[] { ','},StringSplitOptions.RemoveEmptyEntries).ToList();
                var arrDescendants = descendants.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                await contentService.UnPublish(id, arrVariants, arrDescendants);
                success = true;
            }
            catch (Exception ex)
            {
                //log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }

        [Authorize(Roles = PuckRoles.Delete, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> Delete(Guid id, string variant = null)
        {
            var message = string.Empty;
            var success = false;
            try
            {
                await contentService.Delete(id, variant);
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult Models(string type)
        {
            if (string.IsNullOrEmpty(type))
                return Json(apiHelper.AllModels().Select(x =>
                    new { Name = ApiHelper.FriendlyClassName(x), AssemblyName = x.Name }
                    ));
            else
                return Json(apiHelper.AllowedTypes(type).Select(x =>
                    new { Name = ApiHelper.FriendlyClassName(x), AssemblyName = x.Name }
                    ));
        }
        public ActionResult ModelOptions(string type = "")
        {
            var models = apiHelper.AllModels();

            var modelMatches = models.Where(x => x.FullName.EndsWith(type)).ToList();
            var result = (modelMatches == null ? models : new List<Type>(modelMatches))
                .Select(x => new { Name = ApiHelper.FriendlyClassName(x), AssemblyName = x.Name })
                .ToList();
            return Json(result);
        }
        public ActionResult InspectModel(string type, string opath = "")
        {
            var isGenerated = false;
            //var tType = ApiHelper.GetType(type);
            var tType = ApiHelper.GetTypeFromName(type);
            var originalType = tType;
            if (typeof(I_Generated).IsAssignableFrom(tType))
            {
                isGenerated = true;
                tType = ApiHelper.ConcreteType(tType);
            }
            var props = tType.GetProperties();
            var parts = opath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var str = opath;

            var currentT = tType;

            var result = new List<dynamic>();

            parts.ToList().ForEach(x => {
                var t = currentT.GetProperties().Where(xx => xx.Name.Equals(x)).FirstOrDefault().PropertyType;
                currentT = t;
            });

            currentT.GetProperties().ToList().ForEach(x =>
            {
                var isArray = x.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null && !(x.PropertyType == typeof(string));
                result.Add(
                new
                {
                    Name = x.Name,
                    IsArray = isArray,
                    IsComplexType = x.PropertyType.IsClass && !isArray && !(x.PropertyType == typeof(string)),
                    Type = x.PropertyType.Name,
                    InsertString = "@Model." + (string.IsNullOrEmpty(opath) ? "" : opath + ".") + x.Name,
                    IterateString = string.Format("@foreach(var el in Model.{0}){{\n\n}}",
                        (string.IsNullOrEmpty(opath) ? "" : opath + ".") + x.Name),
                    InspectString = (string.IsNullOrEmpty(opath) ? "" : opath + ".") + x.Name
                });
            });

            return Json(new { Data = result, Path = opath, Type = type, Name = ApiHelper.FriendlyClassName(tType), FullName = originalType.FullName, IsGenerated = isGenerated });
        }

        [Authorize(Roles = PuckRoles.Edit, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult PrepopulatedEdit(string p_type, Guid? id)
        {
            if (string.IsNullOrEmpty(p_type))
            {
                var revision = repo.GetPuckRevision().FirstOrDefault(x => x.Id == id && x.Current);
                p_type = revision.Type;
            }
            ViewBag.ShouldBindListEditor = false;
            ViewBag.IsPrepopulated = true;
            ViewBag.TypeMissing = false;
            object model = null;
            //empty model of type
            //var modelType = ApiHelper.GetType(p_type);
            var modelType = ApiHelper.GetTypeFromName(p_type);
            if (modelType == null) return View("Edit", new BaseModel());
            var concreteType = ApiHelper.ConcreteType(modelType);
            model = ApiHelper.CreateInstance(concreteType);
            ObjectDumper.SetPropertyValues(model, onlyPopulateListEditorLists: true);
            var mod = model as BaseModel;
            mod.Type = p_type;
            return View("Edit", model);
        }

        [Authorize(Roles = PuckRoles.Edit, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult Edit(string p_type, Guid? parentId, Guid? contentId, string p_variant = "", string p_fromVariant = "", string p_path = "/")
        {
            if (p_variant == "null" || string.IsNullOrEmpty(p_variant))
                p_variant = PuckCache.SystemVariant;
            object model = null;
            ViewBag.TypeMissing = false;
            if (!string.IsNullOrEmpty(p_type))
            {
                //empty model of type
                //var modelType = ApiHelper.GetType(p_type);
                var modelType = ApiHelper.GetTypeFromName(p_type);
                var concreteType = ApiHelper.ConcreteType(modelType);
                model = ApiHelper.CreateInstance(concreteType);
                //if creating new, return early
                if (contentId == null)
                {
                    var parentPath = contentService.GetLiveOrCurrentPath(parentId.Value) ?? "";
                    var basemodel = (BaseModel)model;
                    basemodel.ParentId = parentId.Value;
                    basemodel.Path = "";
                    basemodel.Variant = p_variant;
                    basemodel.TypeChain = ApiHelper.TypeChain(concreteType);
                    basemodel.Type = modelType.Name;
                    basemodel.CreatedBy = User.Identity.Name;
                    basemodel.LastEditedBy = basemodel.CreatedBy;
                    ContentService.OnCreate(this, new CreateEventArgs { Node = basemodel, Type = basemodel.GetType() });
                    ViewBag.ShouldBindListEditor = true;
                    ViewBag.IsPrepopulated = false;
                    ViewBag.Level0Type = basemodel.GetType();
                    return View(model);
                }
            }
            //else we'll need to get current data to edit for node or return node to translate

            List<PuckRevision> results = null;
            //try get node by id with particular variant
            if (!string.IsNullOrEmpty(p_fromVariant) && p_fromVariant.Equals("none"))
                results = repo.GetPuckRevision().Where(x => x.Id == contentId.Value && x.Current).ToList();
            else if (string.IsNullOrEmpty(p_fromVariant))
                results = repo.GetPuckRevision().Where(x => x.Id == contentId.Value && x.Variant.ToLower().Equals(p_variant.ToLower()) && x.Current).ToList();
            else
                results = repo.GetPuckRevision().Where(x => x.Id == contentId.Value && x.Variant.ToLower().Equals(p_fromVariant.ToLower()) && x.Current).ToList();

            if (results.Count > 0)
            {
                var result = results.FirstOrDefault();
                if (ApiHelper.GetTypeFromName(result.Type) == null)
                {
                    ViewBag.TypeMissing = true;
                    ViewBag.MissingType = result.Type;
                    ViewBag.ShouldBindListEditor = true;
                    ViewBag.IsPrepopulated = false;
                    ViewBag.Level0Type = typeof(BaseModel);
                    return View(new BaseModel());
                }
                if (!string.IsNullOrEmpty(p_fromVariant) && p_fromVariant.Equals("none"))
                {//create blank new translation of existing content
                    var modelType = ApiHelper.GetTypeFromName(result.Type);
                    var concreteType = ApiHelper.ConcreteType(modelType);
                    model = ApiHelper.CreateInstance(concreteType);
                    var baseModel = model as BaseModel;
                    baseModel.Path = "";
                    baseModel.ParentId = result.ParentId;
                    baseModel.Id = result.Id;
                    baseModel.TemplatePath = result.TemplatePath;
                    baseModel.NodeName = result.NodeName;
                    baseModel.SortOrder = result.SortOrder;
                    baseModel.Type = result.Type;
                    baseModel.TypeChain = result.TypeChain;
                    baseModel.Variant = p_variant;
                    baseModel.Created = DateTime.Now;
                    baseModel.Updated = DateTime.Now;
                    baseModel.Published = false;
                    baseModel.Revision = 0;
                    baseModel.CreatedBy = User.Identity.Name;
                    baseModel.LastEditedBy = baseModel.CreatedBy;
                }
                else
                {
                    model = result.ToBaseModel();
                    if (!string.IsNullOrEmpty(p_fromVariant))
                    {
                        var mod = model as BaseModel;
                        mod.Variant = p_variant;
                        mod.Created = DateTime.Now;
                        mod.Updated = DateTime.Now;
                        mod.Published = false;
                        mod.Revision = 0;
                        mod.CreatedBy = User.Identity.Name;
                        mod.LastEditedBy = mod.CreatedBy;
                        mod.Path = "";
                    }
                }
            }
            ViewBag.ShouldBindListEditor = true;
            ViewBag.IsPrepopulated = false;
            ViewBag.Level0Type = model.GetType();
            return View(model);
        }

        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult GetRepublishEntireSiteStatus()
        {
            string message = "";
            if (!string.IsNullOrEmpty(PuckCache.RepublishEntireSiteError))
                message = "error: " + PuckCache.RepublishEntireSiteError;
            else if (PuckCache.IsRepublishingEntireSite)
                message = PuckCache.IndexingStatus;
            else
                message = "complete";
            return Json(new { Success = true, Message = message });
        }

        [Authorize(Roles = PuckRoles.Republish, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        [HttpPost]
        public ActionResult RepublishEntireSite()
        {
            var success = true;
            string message = "republish entire site started";
            if (!PuckCache.IsRepublishingEntireSite)
            {
                System.Threading.Tasks.Task.Factory.StartNew(async () => {
                    using (var scope = PuckCache.ServiceProvider.CreateScope())
                    {
                        var _repo = scope.ServiceProvider.GetService<I_Puck_Repository>();
                        var _contentService = scope.ServiceProvider.GetService<I_Content_Service>();
                        await _contentService.RePublishEntireSite2(addInstruction: true);
                        if (PuckCache.UseAzureDirectory || PuckCache.UseSyncDirectory && PuckCache.IsEditServer)
                        {
                            var instruction = new PuckInstruction() { InstructionKey = InstructionKeys.SetSearcher, Count = 1, ServerName = ApiHelper.ServerName() };
                            _repo.AddPuckInstruction(instruction);
                            _repo.SaveChanges();
                        }
                    }
                });
                PuckCache.IsRepublishingEntireSite = true;
                PuckCache.IndexingStatus = "republish entire site task queued";
            }
            else
            {
                success = false;
                message = "already republishing entire site";
            }
            return Json(new { Success = success, Message = message });
        }
        public ActionResult Ret401()
        {
            return Unauthorized();
        }
        [Authorize(Roles = PuckRoles.Edit, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        [HttpPost]
        public async Task<JsonResult> Edit(IFormCollection fc, string p_type, string p_path)
        {
            //var targetType = ApiHelper.ConcreteType(ApiHelper.GetType(p_type));
            var targetType = ApiHelper.ConcreteType(ApiHelper.GetTypeFromName(p_type));
            var model = ApiHelper.CreateInstance(targetType);
            string path = "";
            Guid parentId = Guid.Empty;
            Guid id = Guid.Empty;
            bool success = false;
            string message = "";
            try
            {
                if (await TryUpdateModelAsync(model, model.GetType(), ""))
                {
                    /*manually binding images is no longer necessary in aspnetcore 
                     as you are no longer relying on FormCollection to bind values
                     */
                    //ObjectDumper.BindImages(model, int.MaxValue, Request.Form.Files);
                    //ObjectDumper.Transform(model, int.MaxValue);
                    var mod = model as BaseModel;
                    path = mod.Path;
                    id = mod.Id;
                    parentId = mod.ParentId;
                    var saveResult = await contentService.SaveContent(mod,queueIfIndexerBusy:true);
                    message = saveResult.Message;
                    success = true;
                }
                else
                {
                    success = false;
                    message = string.Join(" ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
                log.Log(ex);
            }
            return Json(new { success = success, message = message, path = path, id = id, parentId = parentId });
        }

        [Authorize(Roles = PuckRoles.Cache, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult CacheInfo(string p_path)
        {
            bool success = false;
            string message = "";
            var model = false;
            try
            {
                var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.CacheExclude && x.Key.ToLower().Equals(p_path.ToLower())).FirstOrDefault();
                if (meta == null || !bool.TryParse(meta.Value, out model))
                    model = false;
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
                log.Log(ex);
            }
            return Json(new { result = model, success = success, message = message });
        }

        [Authorize(Roles = PuckRoles.Cache, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        [HttpPost]
        public JsonResult CacheInfo(string p_path, bool value)
        {
            bool success = false;
            string message = "";
            try
            {
                var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.CacheExclude && x.Key.ToLower().Equals(p_path.ToLower())).FirstOrDefault();
                if (meta != null)
                {
                    if (value)
                        meta.Value = value.ToString();
                    else
                        repo.DeletePuckMeta(meta);
                }
                else if(value)
                {
                    meta = new PuckMeta() { Name = DBNames.CacheExclude, Key = p_path, Value = value.ToString() };
                    repo.AddPuckMeta(meta);
                }
                repo.SaveChanges();
                StateHelper.UpdateCacheMappings(true);
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
                log.Log(ex);
            }
            return Json(new { success = success, message = message });
        }

        [Authorize(Roles = PuckRoles.Revert, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult Revisions(Guid id, string variant)
        {
            var model = repo.GetPuckRevision().Where(x => x.Id == id && x.Variant.ToLower().Equals(variant.ToLower())).OrderByDescending(x => x.Revision).ToList();
            return View(model);
        }
        [Authorize(Roles = PuckRoles.Revert, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult Compare(int id)
        {
            var compareTo = repo.GetPuckRevision().Where(x => x.RevisionId == id).FirstOrDefault();
            var current = repo.GetPuckRevision().Where(x => x.Id == compareTo.Id && x.Variant.ToLower().Equals(compareTo.Variant.ToLower()) && x.Current).FirstOrDefault();
            var model = new RevisionCompare { Current = null, Revision = null, RevisionId = -1 };
            if (compareTo != null && current != null)
            {
                //var mCompareTo = JsonConvert.DeserializeObject(compareTo.Value,ApiHelper.ConcreteType(ApiHelper.GetType(compareTo.Type))) as BaseModel;
                var mCompareTo = JsonConvert.DeserializeObject(compareTo.Value, ApiHelper.ConcreteType(ApiHelper.GetTypeFromName(compareTo.Type))) as BaseModel;
                //var mCurrent = JsonConvert.DeserializeObject(current.Value,ApiHelper.ConcreteType(ApiHelper.GetType(current.Type))) as BaseModel;
                var mCurrent = JsonConvert.DeserializeObject(current.Value, ApiHelper.ConcreteType(ApiHelper.GetTypeFromName(current.Type))) as BaseModel;
                model = new RevisionCompare { Current = mCurrent, Revision = mCompareTo, RevisionId = compareTo.RevisionId };
            }
            return View(model);
        }
        [Authorize(Roles = PuckRoles.Revert, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public ActionResult Revert(int id)
        {
            bool success = false;
            string message = "";
            string path = "";
            string type = "";
            string variant = "";
            Guid modelId = Guid.Empty;
            try
            {
                var rnode = repo.GetPuckRevision().Where(x => x.RevisionId == id).FirstOrDefault();
                if (rnode == null)
                    throw new Exception(string.Format("revision does not exist: id:{0}", id));
                var current = repo.GetPuckRevision().Where(x => x.Id == rnode.Id && x.Variant.ToLower().Equals(rnode.Variant.ToLower()) && x.Current).ToList();
                current.ForEach(x => x.Current = false);
                rnode.Current = true;
                if (current.Any())
                {
                    //don't want to revert change node/path because it has consequences for children/descendants
                    rnode.NodeName = current.FirstOrDefault().NodeName;
                    rnode.Path = current.FirstOrDefault().Path;
                    rnode.IdPath = current.FirstOrDefault().IdPath;
                    rnode.SortOrder = current.FirstOrDefault().SortOrder;
                    rnode.ParentId = current.FirstOrDefault().ParentId;
                    rnode.HasChildren = current.FirstOrDefault().HasChildren;
                    //rnode.Type = current.FirstOrDefault().Type;
                    //rnode.TypeChain = current.FirstOrDefault().TypeChain;
                }
                if (current.Any(x => x.Published))
                {
                    //var model = JsonConvert.DeserializeObject(rnode.Value,ApiHelper.ConcreteType(ApiHelper.GetType(rnode.Type))) as BaseModel;
                    var model = rnode.ToBaseModel(); //JsonConvert.DeserializeObject(rnode.Value, ApiHelper.ConcreteType(ApiHelper.GetTypeFromName(rnode.Type))) as BaseModel;
                    indexer.Index(new List<BaseModel>() { model });
                }
                path = rnode.Path;
                type = rnode.Type;
                modelId = rnode.Id;
                variant = rnode.Variant;
                repo.SaveChanges();
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
                log.Log(ex);
            }
            return Json(new { success = success, message = message, id = modelId, path = path, type = type, variant = variant });
        }
        [Authorize(Roles = PuckRoles.Revert, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public JsonResult DeleteRevision(int id)
        {
            var message = string.Empty;
            var success = false;
            try
            {
                repo.GetPuckRevision().Where(x => x.RevisionId == id).ToList().ForEach(x => repo.DeletePuckRevision(x));
                repo.SaveChanges();
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }

    }
}
