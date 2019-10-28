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
using puck.tests.Helpers;
using puck.tests.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Linq;

namespace puck.tests
{
    //NOTE: to run tests you will need to set correct values for appSettings.json (in this test project not in web project)
    //specifically, you need to set "LuceneIndexPath", "LogPath" and "ContentRootPath"
    public class ContentServiceTests
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
        string uname = "darkezmo@hotmail.com";
        public I_Puck_Repository NewRepo() {
            var context = new PuckContext(dbContextOptionsBuilder.Options);
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
            contentService = new ContentService(config, roleManager, userManager, repo,tDispatcher, indexer, logger,apiHelper);
            PuckCache._puckSearcher = searcher;
            PuckCache.ServiceProvider = MockHelpers.MockServiceProvider(repo).Object;
            PuckCache.MaxRevisions = 20;
            TestsHelper.SetAQNMappings();
            TestsHelper.SetAnalyzerMappings();
            indexer.DeleteAll();
        }

        [Test]
        public async Task IdPath() {
            var level1 = await contentService.Create<Folder>(Guid.Empty, "en-gb", "idPathRoot", template: "~/views/home/homepage.cshtml", published: true, userName: "darkezmo@hotmail.com");
            await contentService.SaveContent(level1,triggerEvents:false,userName:uname);
            var level2_1 = await contentService.Create<Folder>(level1.Id, "en-gb", "level2_1", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(level2_1, triggerEvents: false, userName: uname);
            var level3_1 = await contentService.Create<Folder>(level2_1.Id, "en-gb", "level3_1", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(level3_1, triggerEvents: false, userName: uname);
            
            var level3_1_idPath = contentService.GetIdPath(level3_1);
            var idPath = $"{level1.Id},{level2_1.Id},{level3_1.Id}";
            Assert.That(level3_1_idPath == idPath);
        }

        [Test]
        public async Task NameAndPathChanges() {
            // home
            var homePage = await contentService.Create<Folder>(Guid.Empty, "en-gb", "homeChangeName", template: "~/views/home/homepage.cshtml", published: true, userName: "darkezmo@hotmail.com");
            await contentService.SaveContent(homePage, triggerEvents: false, userName: uname);
            
            // home/news
            var newsPageEn = await contentService.Create<Folder>(homePage.Id, "en-gb", "news", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(newsPageEn, triggerEvents: false, userName: uname);
            var newsPageJp = await contentService.Create<Folder>(homePage.Id, "ja-jp", "news", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            newsPageJp.Id = newsPageEn.Id;
            await contentService.SaveContent(newsPageJp, triggerEvents: false, userName: uname);
            
            // home/news/images
            var imagesPageEn = await contentService.Create<Folder>(newsPageEn.Id, "en-gb", "images", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(imagesPageEn, triggerEvents: false, userName: uname);
            var imagesPageJp = await contentService.Create<Folder>(newsPageEn.Id, "ja-jp", "images", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            imagesPageJp.Id = imagesPageEn.Id;
            await contentService.SaveContent(imagesPageJp, triggerEvents: false, userName: uname);

            // home/news/images/tokyo
            var tokyoPageEn = await contentService.Create<Folder>(imagesPageEn.Id, "en-gb", "tokyo", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(tokyoPageEn, triggerEvents: false, userName: uname);
            
            // home/news/images/london
            var londonPageEn = await contentService.Create<Folder>(imagesPageEn.Id, "en-gb", "london", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(londonPageEn, triggerEvents: false, userName: uname);

            var repo2 = NewRepo();

            var londonRevision = repo2.CurrentRevision(londonPageEn.Id,londonPageEn.Variant);

            Assert.That(londonRevision.Path=="/homechangename/news/images/london");

            //add new ru-ru translation for imagesPage with different name
            var imagesPageRu = await contentService.Create<Folder>(newsPageEn.Id, "ru-ru", "images1", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            imagesPageRu.Id = imagesPageEn.Id;
            await contentService.SaveContent(imagesPageRu, triggerEvents: false, userName: uname);

            //since imagesPageRu was not published, name change should only affect unpublished variants and not published variants or any descendant paths
            //NOTE - name changes should be synced across variants/translations

            // images-jp, since it was unpublished, should have its nodename changed to images1
            var _imagesJp = NewRepo().CurrentRevision(imagesPageJp.Id, imagesPageJp.Variant);
            Assert.That(
                _imagesJp.NodeName=="images1"
                );
            // images-en, since it was published, should have its nodename unchanged
            Assert.That(
                NewRepo().CurrentRevision(imagesPageEn.Id, imagesPageEn.Variant).NodeName == "images"
                );
            // since imagesPageRu was unpublished, descendant content should have its path unchanged
            Assert.That(
                NewRepo().CurrentRevision(londonPageEn.Id, londonPageEn.Variant).Path == "/homechangename/news/images/london"
                );

            // now publish images-jp
            var _imageJpMod = _imagesJp.ToBaseModel();
            _imageJpMod.Published = true;
            await contentService.SaveContent(_imageJpMod,triggerEvents:false,userName:uname);
            
            var _imagesEn = NewRepo().CurrentRevision(imagesPageEn.Id, imagesPageEn.Variant);
            // images-en should now have its nodename changed
            Assert.That(
                _imagesEn.NodeName == "images1"
                );

            // since the nodename change is now published, descendant content should have its path changed
            var _londonEn = NewRepo().CurrentRevision(londonPageEn.Id, londonPageEn.Variant);
            Assert.That(
                _londonEn.Path == "/homechangename/news/images1/london"
                );
            var _tokyoEn = NewRepo().CurrentRevision(tokyoPageEn.Id, tokyoPageEn.Variant);
            Assert.That(
                _tokyoEn.Path == "/homechangename/news/images1/tokyo"
                );
        }

        [Test]
        public async Task Move()
        {
            var rootName = "homemove";
            // home
            var homePage = await contentService.Create<Folder>(Guid.Empty, "en-gb", rootName, template: "~/views/home/homepage.cshtml", published: true, userName: "darkezmo@hotmail.com");
            await contentService.SaveContent(homePage, triggerEvents: false, userName: uname);

            // home/news
            var newsPageEn = await contentService.Create<Folder>(homePage.Id, "en-gb", "news", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(newsPageEn, triggerEvents: false, userName: uname);
            var newsPageJp = await contentService.Create<Folder>(homePage.Id, "ja-jp", "news", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            newsPageJp.Id = newsPageEn.Id;
            await contentService.SaveContent(newsPageJp, triggerEvents: false, userName: uname);

            // home/news/images
            var imagesPageEn = await contentService.Create<Folder>(newsPageEn.Id, "en-gb", "images", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(imagesPageEn, triggerEvents: false, userName: uname);
            var imagesPageJp = await contentService.Create<Folder>(newsPageEn.Id, "ja-jp", "images", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            imagesPageJp.Id = imagesPageEn.Id;
            await contentService.SaveContent(imagesPageJp, triggerEvents: false, userName: uname);

            // home/news/images/tokyo
            var tokyoPageEn = await contentService.Create<Folder>(imagesPageEn.Id, "en-gb", "tokyo", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(tokyoPageEn, triggerEvents: false, userName: uname);

            // home/news/images/london
            var londonPageEn = await contentService.Create<Folder>(imagesPageEn.Id, "en-gb", "london", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(londonPageEn, triggerEvents: false, userName: uname);

            // home/news/images/london/camden
            var camdenPageEn = await contentService.Create<Folder>(londonPageEn.Id, "en-gb", "camden", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(camdenPageEn, triggerEvents: false, userName: uname);

            //before move
            var _londonEnRev1 = NewRepo().CurrentRevision(londonPageEn.Id, londonPageEn.Variant);
            var _camdenPageEnRev1 = NewRepo().CurrentRevision(camdenPageEn.Id, camdenPageEn.Variant);

            Assert.That(_londonEnRev1.IdPath == $"{homePage.Id},{newsPageEn.Id},{imagesPageEn.Id},{londonPageEn.Id}");
            Assert.That(_camdenPageEnRev1.IdPath == $"{homePage.Id},{newsPageEn.Id},{imagesPageEn.Id},{londonPageEn.Id},{camdenPageEn.Id}");

            Assert.That(_londonEnRev1.Path == $"/{rootName}/news/images/london");
            Assert.That(_camdenPageEnRev1.Path == $"/{rootName}/news/images/london/camden");

            await contentService.Move(londonPageEn.Id, newsPageEn.Id,userName:uname);

            //after move
            var _londonEnRev2 = NewRepo().CurrentRevision(londonPageEn.Id,londonPageEn.Variant);
            var _camdenPageEnRev2 = NewRepo().CurrentRevision(camdenPageEn.Id,camdenPageEn.Variant);

            var _londonEnRev2_ = NewRepo().GetPuckRevision().Where(x => x.Id == londonPageEn.Id && x.Variant.ToLower().Equals(londonPageEn.Variant.ToLower())).FirstOrDefault();

            Assert.That(_londonEnRev2.IdPath == $"{homePage.Id},{newsPageEn.Id},{londonPageEn.Id}");
            Assert.That(_camdenPageEnRev2.IdPath == $"{homePage.Id},{newsPageEn.Id},{londonPageEn.Id},{camdenPageEn.Id}");

            Assert.That(_londonEnRev2.Path == $"/{rootName}/news/london");
            Assert.That(_camdenPageEnRev2.Path == $"/{rootName}/news/london/camden");

        }


        [Test]
        public void ConnectionsSame()
        {
            var con = repo.Context.Database.GetDbConnection();
            var con2 = repo.Context.Database.GetDbConnection();
            var parameter = repo.Context.Database.GetDbConnection().CreateCommand().CreateParameter();
            Assert.That(con==con2);
        }

        [Test]
        public async Task CreateRoot()
        {
            var model = await contentService.Create<Folder>(Guid.Empty, "en-gb", "home", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await contentService.SaveContent(model,triggerEvents:false,userName:uname);

            var newRepo = NewRepo();
            var savedModel = newRepo.CurrentRevision(model.Id,model.Variant);
            Assert.That(savedModel!=null);
        }
    }
}