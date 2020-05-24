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
using LinqKit;
using System.Threading;

namespace puck.core.Controllers
{
    [Area("puck")]
    public class WorkflowController : BaseController
    {
        private static SemaphoreSlim slock1 = new SemaphoreSlim(1);
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
        public WorkflowController(I_Api_Helper ah, I_Content_Service cs, I_Content_Indexer i, I_Content_Searcher s, I_Log l, I_Puck_Repository r, RoleManager<PuckRole> rm, UserManager<PuckUser> um, SignInManager<PuckUser> sm, IHostEnvironment env, IConfiguration config, IMemoryCache cache)
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

        [HttpGet]
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<IActionResult> Notifications(int since)
        {
            var success = false;
            var message = "";
            var count = 0;
            var id = 0;
            try
            {
                var notifications = await apiHelper.GetCurrentWorkflowItemId(User.Identity.Name, since: since);
                id = notifications.Item1;
                count = notifications.Item2;

                success = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                log.Log(ex);
            }
            return Json(new { success = success, message = message, id = id, count=count });
        }

        [HttpPost]
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<IActionResult> Create(PuckWorkflowItem model) {
            var success = false;
            var message = "";
            bool lockTaken = false;
            try
            {
                if (model != null)
                    model.AddedBy = User.Identity.Name;
                if (ModelState.ContainsKey("AddedBy"))
                {
                    ModelState["AddedBy"].Errors.Clear();
                    ModelState["AddedBy"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
                }
                if (!ModelState.IsValid)
                {
                    message = string.Join(",", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                    return Json(new { success = success, message = message });
                }
                //await slock1.WaitAsync();
                lockTaken = true;

                var existingItems = repo.GetPuckWorkflowItem().Where(x => x.ContentId == model.ContentId && x.Variant == model.Variant && !x.Complete).ToList();
                existingItems.ForEach(x => { x.Complete = true; x.CompleteDate = DateTime.Now; });

                repo.AddPuckWorkflowItem(model);
                repo.SaveChanges();

                success = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                log.Log(ex);
            }
            finally {
                //if(lockTaken)
                //    slock1.Release();
            }

            return Json(new {success=success,message=message });
        }

        [HttpPost]
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<IActionResult> Complete(Guid contentId,string variant,string status)
        {
            var success = false;
            var message = "";

            try
            {
                var existingItems = repo.GetPuckWorkflowItem().Where(x => x.ContentId == contentId && x.Variant == variant && !x.Complete).ToList();
                existingItems.ForEach(x => { x.Complete = true; x.CompleteDate = DateTime.Now;});

                repo.SaveChanges();

                success = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                log.Log(ex);
            }

            return Json(new { success = success, message = message });
        }

        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<IActionResult> Index()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);

            var userGroups = user.PuckUserGroups?.Split(',', StringSplitOptions.RemoveEmptyEntries)??new string[] { };

            var predicate = PredicateBuilder.New<PuckWorkflowItem>();

            foreach (var group in userGroups) {
                predicate = predicate.Or(x=>x.Group.Equals(group));
            }

            predicate.Or(x => x.Assignees.Contains(user.UserName));

            var model = repo.GetPuckWorkflowItem().AsExpandable().Where(predicate).Where(x=>!x.Complete).ToList();

            model.AddRange(repo.GetPuckWorkflowItem().Where(x=>x.Complete).OrderByDescending(x=>x.CompleteDate).Take(10).ToList());

            var ids = model.Select(x => x.ContentId);

            var names = repo.GetPuckRevision().Where(x => x.Current && ids.Contains(x.Id)).Select(x => new PuckRevision { NodeName = x.NodeName, Id=x.Id,Variant=x.Variant }).ToList();

            var nameDict = new Dictionary<Guid, string>();

            names.ForEach(x=>nameDict[x.Id]=x.NodeName+$" - {x.Variant}");

            ViewBag.Names = nameDict;

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<IActionResult> Lock(Guid contentId, string variant,string until)
        {
            var success = false;
            var message = "";

            try
            {
                var existingItems = repo.GetPuckWorkflowItem().Where(x => x.ContentId == contentId && x.Variant == variant && !x.Complete).ToList();

                DateTime lockedUntil = DateTime.Now;

                switch (until) {
                    case "10 mins":
                        lockedUntil = DateTime.Now.AddMinutes(10);
                        break;
                    case "30 mins":
                        lockedUntil = DateTime.Now.AddMinutes(30);
                        break;
                    case "1 hour":
                        lockedUntil = DateTime.Now.AddHours(1);
                        break;
                    case "2 hours":
                        lockedUntil = DateTime.Now.AddHours(2);
                        break;
                    case "5 hours":
                        lockedUntil = DateTime.Now.AddHours(5);
                        break;
                    case "8 hour":
                        lockedUntil = DateTime.Now.AddHours(8);
                        break;
                }
                
                existingItems.ForEach(x => { x.LockedUntil=lockedUntil; x.LockedBy = User.Identity.Name; });

                repo.SaveChanges();

                success = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                log.Log(ex);
            }

            return Json(new { success = success, message = message });
        }

        [HttpPost]
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<IActionResult> Unlock(Guid contentId, string variant)
        {
            var success = false;
            var message = "";

            try
            {
                var existingItems = repo.GetPuckWorkflowItem().Where(x => x.ContentId == contentId && x.Variant == variant).ToList();

                existingItems.ForEach(x=> { x.LockedBy = string.Empty;x.LockedUntil = null; });

                repo.SaveChanges();
                
                success = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                log.Log(ex);
            }

            return Json(new { success = success, message = message });
        }

        [HttpPost]
        [Authorize(Roles = PuckRoles.Puck, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<IActionResult> Rollback(Guid contentId, string variant)
        {
            var success = false;
            var message = "";

            try
            {
                var currentItems = repo.GetPuckWorkflowItem().Where(x => x.ContentId == contentId && x.Variant == variant && !x.Complete).ToList();

                var lastComplete = repo.GetPuckWorkflowItem().Where(x => x.ContentId == contentId && x.Variant == variant && x.Complete).OrderByDescending(x => x.CompleteDate).FirstOrDefault();

                currentItems.ForEach(x => { repo.DeletePuckWorkflowItem(x); });

                if (lastComplete != null) {
                    lastComplete.Complete = false;
                    lastComplete.CompleteDate = null;
                }

                repo.SaveChanges();

                success = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                log.Log(ex);
            }

            return Json(new { success = success, message = message });
        }

    }
}
