using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using puck.core.Abstract;
using puck.core.Concrete;
using puck.core.Services;
using Microsoft.AspNetCore.Identity;
using puck.core.Entities;
using puck.core.Helpers;
using puck.core.State;
using puck.core.Base;
using Tests.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Data.Sqlite;

namespace Tests
{
    //NOTE: to run tests you will need to correct values for appSettings.json (in this test project not in web project
    //specifically, you need to set "LuceneIndexPath", "LogPath" and "ContentRootPath"
    public class Tests
    {
        I_Content_Service contentService;
        I_Content_Indexer indexer;
        I_Content_Searcher searcher;
        I_Puck_Repository repo;
        I_Log logger;
        I_Api_Helper apiHelper;
        I_Task_Dispatcher tDispatcher;
        RoleManager<PuckRole> roleManager;
        UserManager<PuckUser> userManager;
        SqliteConnection con;
        DbContextOptionsBuilder<PuckContext> dbContextOptionsBuilder;
        PuckContext context;
        public QueryHelper<T> NewQueryHelper<T>() where T:BaseModel {
            QueryHelper<T>.searcher = searcher;
            return new QueryHelper<T>();
        }
        public I_Puck_Repository NewRepo() {
            repo = new Puck_Repository(context);
            return repo;
        }

        [SetUp]
        public void Setup()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var config = TestsHelper.GetConfig();
            var env = TestsHelper.GetEnvironment();
            
            var connectionString = config.GetConnectionString("DefaultConnection");
            var logPath = config.GetValue<string>("LogPath");
            
            logger = new Logger(logPath);
            tDispatcher = new Dispatcher();
            con = new SqliteConnection("DataSource=:memory:");
            con.Open();
            dbContextOptionsBuilder = new DbContextOptionsBuilder<PuckContext>();
            dbContextOptionsBuilder
                .UseSqlite(con);
            context = new PuckContext(dbContextOptionsBuilder.Options);
            repo = new Puck_Repository(context);
            var dbCreated = repo.Context.Database.EnsureCreated();
            var indexerSearcher = new Content_Indexer_Searcher(logger, config, env);
            indexer = indexerSearcher;
            searcher = indexerSearcher;
            roleManager = MockHelpers.MockRoleManager<PuckRole>().Object;
            userManager = MockHelpers.MockUserManager().Object;
            apiHelper = new ApiHelper(roleManager, userManager, repo, tDispatcher, indexer, logger);
            contentService = new ContentService(config, roleManager, userManager, repo,tDispatcher, indexer, logger);
            PuckCache._puckSearcher = searcher;
            PuckCache.ServiceProvider = MockHelpers.MockServiceProvider(repo).Object;
            TestsHelper.SetAQNMappings();
            TestsHelper.SetAnalyzerMappings();
            indexer.DeleteAll();
        }

        [Test]
        public async Task CreateRoot()
        {
            var model = await contentService.Create<Folder>(Guid.Empty, "en-gb", "home", template: "~/views/home/homepage.cshtml", published: true, userName: "darkezmo@hotmail.com");
            await contentService.SaveContent(model,triggerEvents:false,userName:"darkezmo@hotmail.com");

            var newRepo = NewRepo();
            var savedModel = newRepo.CurrentRevision(model.Id,model.Variant);
            Assert.That(savedModel!=null);
        }
    }
}