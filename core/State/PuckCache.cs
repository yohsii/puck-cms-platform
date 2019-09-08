using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Models;
using Lucene.Net.Analysis;
using puck.core.Helpers;
using puck.core.Abstract;
using Lucene.Net.Documents;
using puck.core.Attributes;
using System.Configuration;
using puck.core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using puck.core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace puck.core.State
{
    public static class PuckCache
    {
        public static void Configure(IConfiguration config, IHostEnvironment env,IServiceProvider serviceProvider)
        {
            Path404 = string.IsNullOrEmpty(config.GetValue<string>("Puck404Path")) ? "~/views/Errors/Puck404.cshtml" : config.GetValue<string>("Puck404Path");
            Path500 = string.IsNullOrEmpty(config.GetValue<string>("Puck500Path")) ? "~/views/Errors/Puck500.cshtml" : config.GetValue<string>("Puck500Path");
            Debug = config.GetValue<bool>("PuckDebug");
            UpdateTaskLastRun = config.GetValue<bool>("PuckUpdateTaskLastRun");
            UpdateRecurringTaskLastRun = config.GetValue<bool>("PuckUpdateRecurringTaskLastRun");
            TaskCatchUp = config.GetValue<bool>("PuckTaskCatchUp");
            IsEditServer = config.GetValue<bool>("IsEditServer");
            UseAzureLucenePath = config.GetValue<bool>("UseAzureLucenePath");
            ServiceProvider = serviceProvider;
            ContentRootPath = env.ContentRootPath;
            Configuration = config;
        }
        public static IConfiguration Configuration;
        public static string ContentRootPath="";
        public static string SmtpFrom = "";
        public static string SmtpHost = "localhost";
        public static string EmailTemplatePublishPath = "~/app_data/notification_publish_template.txt";
        public static string EmailTemplateEditPath = "~/app_data/notification_edit_template.txt";
        public static string EmailTemplateDeletePath = "~/app_data/notification_delete_template.txt";
        public static string EmailTemplateMovePath = "~/app_data/notification_move_template.txt";
        public static string TemplateDirectory = "~/views/";
        public static string Path404 = null;
        public static string Path500 = null;
        public static bool Debug = false;
        public static bool UpdateTaskLastRun = true;
        public static bool UpdateRecurringTaskLastRun = false;
        public static bool TaskCatchUp = true;
        public static bool IsEditServer = true;
        public static bool UseAzureLucenePath = false;
        public static int RedirectOuputCacheMinutes = 1;
        public static int DefaultOutputCacheMinutes = 0;
        public static int DisplayModesCacheMinutes = 10;
        public static int MaxSyncInstructions = 100;
        public static string SystemVariant = "en-GB";
        public static Uri FirstRequestUrl = null;
        public static bool IsRepublishingEntireSite { get; set; }
        public static bool ShouldSync { get; set; }
        public static bool IsSyncQueued { get; set; }
        public static string IndexingStatus { get; set; }
        public static List<Variant> Variants { get; set; }
        public static Dictionary<string, string> DomainRoots { get; set; }
        public static Dictionary<string, string> PathToLocale { get; set; }
        public static Dictionary<string, Analyzer> TypeAnalyzers { get; set; }
        public static Dictionary<string, string> Redirect301 { get; set; }
        public static Dictionary<string, string> Redirect302 { get; set; }
        public static Dictionary<string, int> TypeOutputCache { get; set; }
        public static Dictionary<string, Type> IGeneratedToModel { get; set; }
        public static Dictionary<string, Dictionary<string, string>> TypeFields { get; set; }
        public static Dictionary<string,List<Type>> ModelDerivedModels { get; set; }
        //map model type fullname to asssembly qualified name
        public static Dictionary<string, string> ModelNameToAQN { get; set; }
        public static Dictionary<string, CropInfo> CropSizes { get; set; }
        public static HashSet<string> OutputCacheExclusion { get; set; }
        public static IServiceProvider ServiceProvider { get; set; }
        public static I_Task_Dispatcher PuckDispatcher { get { return ServiceProvider.GetService<I_Task_Dispatcher>(); } }
        public static I_Content_Searcher PuckSearcher { get { return ServiceProvider.GetService<I_Content_Searcher>(); } }
        public static I_Content_Indexer PuckIndexer { get { return ServiceProvider.GetService<I_Content_Indexer>(); } }
        public static I_Puck_Repository PuckRepo { get { return ServiceProvider.GetService<I_Puck_Repository>(); } }
        public static UserManager<PuckUser> PuckUserManager { get { return ServiceProvider.GetService<UserManager<PuckUser>>(); } }
        public static RoleManager<PuckRole> PuckRoleManager { get { return ServiceProvider.GetService<RoleManager<PuckRole>>(); } }
        public static ApiHelper ApiHelper { get { return ServiceProvider.GetService<ApiHelper>(); } }
        public static ContentService ContentService { get { return ServiceProvider.GetService<ContentService>(); } }
        public static IMemoryCache Cache { get { return ServiceProvider.GetService<IMemoryCache>(); } }
        public static I_Log PuckLog { get { return ServiceProvider.GetService<I_Log>(); } }
        public static List<Analyzer> Analyzers { get; set; }
        public static Dictionary<Type, Analyzer> AnalyzerForModel { get; set; }

    }
}
