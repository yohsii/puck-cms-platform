using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Abstract;
using puck.core.Constants;
using puck.core.Helpers;
using puck.core.Events;
using Newtonsoft.Json;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using puck.core.Entities;
using System.Web;
using puck.core.Models.EditorSettings;
using puck.core.State;
using puck.core.Services;
using puck.core.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace puck.core
{
    public static class Bootstrap
    {
        public static void Ini(IConfiguration config,IHostEnvironment env,IServiceProvider serviceProvider
            , IHttpContextAccessor httpContextAccessor
            ,Dictionary<string,Func<Microsoft.AspNetCore.Http.HttpContext,bool>> displayModes=null) {
            System.Web.HttpContext.Configure(httpContextAccessor);
            puck.core.State.PuckCache.Configure(config, env, serviceProvider);
            PuckCache.DisplayModes = displayModes;
            var seedTask= StateHelper.SeedDb(config, env, serviceProvider);
            seedTask.Wait();
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<PuckContext, puck.core.Migrations.Configuration>());
            StateHelper.SetGeneratedMappings();
            StateHelper.UpdateDomainMappings();
            StateHelper.UpdatePathLocaleMappings();
            StateHelper.UpdateTaskMappings();
            StateHelper.UpdateDefaultLanguage();
            StateHelper.UpdateCacheMappings();
            StateHelper.UpdateRedirectMappings();
            PuckCache.Analyzers = new List<Lucene.Net.Analysis.Analyzer>();
            PuckCache.AnalyzerForModel = new Dictionary<Type,Lucene.Net.Analysis.Analyzer>();
            PuckCache.TypeFields = new Dictionary<string, Dictionary<string,string>>();
            PuckCache.ModelNameToAQN = new Dictionary<string, string>();
            //PuckCache.SmtpFrom = "puck@"+PuckCache.SmtpHost;
            //sets mapping between type fullname and assembly qualified name for all models
            StateHelper.UpdateAQNMappings();
            StateHelper.UpdateAnalyzerMappings();
            //update typechains which may have changed since last run
            //StateHelper.UpdateTypeChains();
            /*will likely get rid of typechains*/
            StateHelper.UpdateCrops();
            StateHelper.SetModelDerivedMappings();
            //figure out whether or not to republish entire site / ie coldboot
            var shouldColdBoot=SyncHelper.InitializeSync();
            //var qh = new QueryHelper<BaseModel>(prependTypeTerm: false);
            //qh.And().Field(x => x.TypeChain, typeof(BaseModel).FullName.Wrap());
            //var query = qh.ToString();
            //var documentCount = PuckCache.PuckSearcher.Count<BaseModel>(query);
            var documentCount = PuckCache.PuckSearcher.DocumentCount();
            if (shouldColdBoot || documentCount==0) {
                if (!PuckCache.IsRepublishingEntireSite)
                {
                    PuckCache.IsRepublishingEntireSite = true;
                    PuckCache.IndexingStatus = "republish entire site task queued";
                    //HostingEnvironment.QueueBackgroundWorkItem(ct => contentService.RePublishEntireSite2());
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var contentService = scope.ServiceProvider.GetService<ContentService>();
                        var republishTask = contentService.RePublishEntireSite2();
                        republishTask.Wait();
                    }
                }
            }

            //bind notification handlers
            //publish
            ApiHelper.AfterEditorSettingsSave += (object o,puck.core.Events.AfterEditorSettingsSaveEventArgs args)=> {
                if (args.Setting is PuckImageEditorSettings) {
                    StateHelper.UpdateCrops(addInstruction:true);
                }
            };
            PuckCache.PuckIndexer.RegisterAfterIndexHandler<puck.core.Base.BaseModel>("puck_publish_notification",async (object o, puck.core.Events.IndexingEventArgs args)=>
            {
                try
                {
                    using (var scope = PuckCache.ServiceProvider.CreateScope())
                    {
                        var apiHelper = scope.ServiceProvider.GetService<ApiHelper>();
                        var usersToNotify = await apiHelper.UsersToNotify(args.Node.Path, NotifyActions.Publish);
                        if (usersToNotify.Count == 0) return;
                        var subject = string.Concat("content published - ", args.Node.NodeName, " - ", args.Node.Path);
                        var template = System.IO.File.ReadAllText(ApiHelper.MapPath(PuckCache.EmailTemplatePublishPath));
                        template = ApiHelper.EmailTransform(template, args.Node, NotifyActions.Publish);
                        var emails = string.Join(";", usersToNotify.Select(x => x.Email)).TrimEnd(';');
                        ApiHelper.Email(emails, subject, template);
                    }
                }
                catch (Exception ex) {
                    PuckCache.PuckLog.Log(ex);
                }
            }, Propagate:true);
            //edit
            ContentService.RegisterAfterSaveHandler<puck.core.Base.BaseModel>("puck_edit_notification",async (object o, puck.core.Events.IndexingEventArgs args) =>
            {
                try
                {
                    using (var scope = PuckCache.ServiceProvider.CreateScope())
                    {
                        var apiHelper = scope.ServiceProvider.GetService<ApiHelper>();
                        var usersToNotify = await apiHelper.UsersToNotify(args.Node.Path, NotifyActions.Edit);
                        if (usersToNotify.Count == 0) return;
                        var subject = string.Concat("content edited - ", args.Node.NodeName, " - ", args.Node.Path);
                        var template = System.IO.File.ReadAllText(ApiHelper.MapPath(PuckCache.EmailTemplateEditPath));
                        template = ApiHelper.EmailTransform(template, args.Node, NotifyActions.Edit);
                        var emails = string.Join(";", usersToNotify.Select(x => x.Email)).TrimEnd(';');
                        ApiHelper.Email(emails, subject, template);
                    }
                }
                catch (Exception ex)
                {
                    PuckCache.PuckLog.Log(ex);
                }
            }, true);
            //delete
            ContentService.RegisterAfterDeleteHandler<puck.core.Base.BaseModel>("puck_delete_notification",async (object o, puck.core.Events.IndexingEventArgs args) =>
            {
                try
                {
                    using (var scope = PuckCache.ServiceProvider.CreateScope())
                    {
                        var apiHelper = scope.ServiceProvider.GetService<ApiHelper>();
                        var usersToNotify = await apiHelper.UsersToNotify(args.Node.Path, NotifyActions.Delete);
                        if (usersToNotify.Count == 0) return;
                        var subject = string.Concat("content deleted - ", args.Node.NodeName, " - ", args.Node.Path);
                        var template = System.IO.File.ReadAllText(ApiHelper.MapPath(PuckCache.EmailTemplateDeletePath));
                        template = ApiHelper.EmailTransform(template, args.Node, NotifyActions.Delete);
                        var emails = string.Join(";", usersToNotify.Select(x => x.Email)).TrimEnd(';');
                        ApiHelper.Email(emails, subject, template);
                    }
                }
                catch (Exception ex)
                {
                    PuckCache.PuckLog.Log(ex);
                }
            }, true);
            //move
            ContentService.RegisterAfterMoveHandler<puck.core.Base.BaseModel>("puck_move_notification",async (object o, puck.core.Events.MoveEventArgs args) =>
            {
                try
                {
                    using (var scope = PuckCache.ServiceProvider.CreateScope())
                    {
                        var apiHelper = scope.ServiceProvider.GetService<ApiHelper>();
                        var node = args.Nodes.FirstOrDefault();
                        var usersToNotify = await apiHelper.UsersToNotify(node.Path, NotifyActions.Move);
                        if (usersToNotify.Count == 0) return;
                        var subject = string.Concat("content move - ", node.NodeName, " - ", node.Path);
                        var template = System.IO.File.ReadAllText(ApiHelper.MapPath(PuckCache.EmailTemplateMovePath));
                        template = ApiHelper.EmailTransform(template, node, NotifyActions.Move);
                        var emails = string.Join(";", usersToNotify.Select(x => x.Email)).TrimEnd(';');
                        ApiHelper.Email(emails, subject, template);
                    }
                }
                catch (Exception ex)
                {
                    PuckCache.PuckLog.Log(ex);
                }
            }, true);
            
            //DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
            

        }
        
    }
}
