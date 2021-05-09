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
    public class PreviewController : BaseController
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
        public PreviewController(I_Api_Helper ah, I_Content_Service cs, I_Content_Indexer i, I_Content_Searcher s, I_Log l, I_Puck_Repository r, RoleManager<PuckRole> rm, UserManager<PuckUser> um, SignInManager<PuckUser> sm, IHostEnvironment env, IConfiguration config, IMemoryCache cache)
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

        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<ActionResult> Preview(IFormCollection fc, string p_type)
        {

            var targetType = ApiHelper.ConcreteType(ApiHelper.GetTypeFromName(p_type));
            var model = ApiHelper.CreateInstance(targetType);
            BaseModel bmodel = null;
            try
            {
                if (await TryUpdateModelAsync(model, model.GetType(), ""))
                {
                    bmodel = model as BaseModel;

                }
                else
                {
                    //return preview failed view
                }
            }
            catch (Exception ex)
            {
                log.Log(ex);
                //return preview failed view
            }
            
            var dmode = this.GetDisplayModeId();
            string templatePath = bmodel.TemplatePath;
            if (!string.IsNullOrEmpty(dmode))
            {
                string dpath = templatePath.Insert(templatePath.LastIndexOf('.') + 1, dmode + ".");
                if (System.IO.File.Exists(ApiHelper.MapPath(dpath)))
                {
                    templatePath = dpath;
                }
            }
            
            var variant = GetVariant(bmodel.Path);
            HttpContext.Items["variant"] = variant;
            //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(threadVariant);
            return View(templatePath, model);
        }

        public ActionResult PreviewEditor(Guid id, string variant) {

            var model = repo.CurrentRevision(id, variant).ToBaseModel();

            return View(model);
        
        }

    }
}