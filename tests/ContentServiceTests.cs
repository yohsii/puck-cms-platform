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
using puck.tests.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace puck.tests
{
    //NOTE: to run tests you will need to set correct values for appSettings.json (in this test project not in web project)
    //specifically, you need to set "LuceneIndexPath", "LogPath" and "ContentRootPath"
    public class ContentServiceTests
    {
        Dictionary<string, Services> ServiceDictionary { get; set; }
        string uname = "darkezmo@hotmail.com";
        public I_Puck_Repository NewRepo(string type) {
            var services = ServiceDictionary[type];
            var context = new PuckContext(services.DbContextOptionsBuilder.Options);
            var repo = new Puck_Repository(context);
            return repo;
        }
        [OneTimeSetUp]
        public void Setup() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ServiceDictionary = new Dictionary<string, Services>();
        }
        [OneTimeTearDown]
        public void Cleanup() {
            foreach(var item in ServiceDictionary) {
                item.Value.Repo.Context.Database.EnsureDeleted();
            }
        }
        public Services GetServices(string type)
        {
            if (ServiceDictionary.ContainsKey(type))
            {
                PuckCache.ServiceProvider = MockHelpers.MockServiceProvider(ServiceDictionary[type].Repo).Object;
                return ServiceDictionary[type];
            }

            var services = new Models.Services();
            
            var config = TestsHelper.GetConfig();
            var env = TestsHelper.GetEnvironment();
            
            var connectionString = config.GetConnectionString(type);
            var logPath = config.GetValue<string>("LogPath");
            
            services.Logger = new Logger(logPath);
            services.TDispatcher = new Dispatcher();
            if (type == DbConstants.SQLite)
            {
                services.Con = new SqliteConnection(connectionString);
                services.Con.Open();
                services.DbContextOptionsBuilder = new DbContextOptionsBuilder<PuckContext>();
                services.DbContextOptionsBuilder
                    .UseSqlite(services.Con);
            }else if (type == DbConstants.MySql)
            {
                services.DbContextOptionsBuilder = new DbContextOptionsBuilder<PuckContext>();
                services.DbContextOptionsBuilder
                    .UseMySql(connectionString);
            }else if (type == DbConstants.PostgreSQL)
            {
                services.DbContextOptionsBuilder = new DbContextOptionsBuilder<PuckContext>();
                services.DbContextOptionsBuilder
                    .UseNpgsql(connectionString);
            }else if (type == DbConstants.SQLServer)
            {
                services.DbContextOptionsBuilder = new DbContextOptionsBuilder<PuckContext>();
                services.DbContextOptionsBuilder
                    .UseSqlServer(connectionString);
            }
            services.Context = new PuckContext(services.DbContextOptionsBuilder.Options);
            services.Repo = new Puck_Repository(services.Context);

            if (services.Repo.Context.Database.GetService<IRelationalDatabaseCreator>().Exists()) {
                var dbDeleted = services.Repo.Context.Database.EnsureDeleted();
            }
            var dbCreated = services.Repo.Context.Database.EnsureCreated();
            var indexerSearcher = new Content_Indexer_Searcher(services.Logger, config, env);
            services.Indexer = indexerSearcher;
            services.Searcher = indexerSearcher;
            services.RoleManager = MockHelpers.MockRoleManager<PuckRole>().Object;
            services.UserManager = MockHelpers.MockUserManager().Object;
            services.ApiHelper = new ApiHelper(services.RoleManager, services.UserManager, services.Repo, services.TDispatcher, services.Indexer, services.Logger);
            services.ContentService = new ContentService(config, services.RoleManager, services.UserManager, services.Repo,services.TDispatcher, services.Indexer, services.Logger,services.ApiHelper);
            PuckCache._puckSearcher = services.Searcher;
            PuckCache.ServiceProvider = MockHelpers.MockServiceProvider(services.Repo).Object;
            PuckCache.MaxRevisions = 20;
            TestsHelper.SetAQNMappings();
            TestsHelper.SetAnalyzerMappings();
            services.Indexer.DeleteAll();
            ServiceDictionary[type] = services;
            return services;
        }

        [Test]
        [TestCase(DbConstants.SQLite)]
        [TestCase(DbConstants.MySql)]
        [TestCase(DbConstants.SQLServer)]
        [TestCase(DbConstants.PostgreSQL)]
        public async Task IdPath(string type) {
            var s = GetServices(type);
            var level1 = await s.ContentService.Create<Folder>(Guid.Empty, "en-gb", "idPathRoot", template: "~/views/home/homepage.cshtml", published: true, userName: "darkezmo@hotmail.com");
            await s.ContentService.SaveContent(level1,triggerEvents:false,userName:uname);
            var level2_1 = await s.ContentService.Create<Folder>(level1.Id, "en-gb", "level2_1", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(level2_1, triggerEvents: false, userName: uname);
            var level3_1 = await s.ContentService.Create<Folder>(level2_1.Id, "en-gb", "level3_1", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(level3_1, triggerEvents: false, userName: uname);
            
            var level3_1_idPath = s.ContentService.GetIdPath(level3_1);
            var idPath = $"{level1.Id},{level2_1.Id},{level3_1.Id}";
            Assert.That(level3_1_idPath == idPath);
        }

        [Test]
        [TestCase(DbConstants.SQLite)]
        [TestCase(DbConstants.MySql)]
        [TestCase(DbConstants.SQLServer)]
        [TestCase(DbConstants.PostgreSQL)]
        public async Task NameAndPathChanges(string type) {
            var s = GetServices(type);
            // home
            var homePage = await s.ContentService.Create<Folder>(Guid.Empty, "en-gb", "homeChangeName", template: "~/views/home/homepage.cshtml", published: true, userName: "darkezmo@hotmail.com");
            await s.ContentService.SaveContent(homePage, triggerEvents: false, userName: uname);
            
            // home/news
            var newsPageEn = await s.ContentService.Create<Folder>(homePage.Id, "en-gb", "news", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(newsPageEn, triggerEvents: false, userName: uname);
            var newsPageJp = await s.ContentService.Create<Folder>(homePage.Id, "ja-jp", "news", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            newsPageJp.Id = newsPageEn.Id;
            await s.ContentService.SaveContent(newsPageJp, triggerEvents: false, userName: uname);
            
            // home/news/images
            var imagesPageEn = await s.ContentService.Create<Folder>(newsPageEn.Id, "en-gb", "images", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(imagesPageEn, triggerEvents: false, userName: uname);
            var imagesPageJp = await s.ContentService.Create<Folder>(newsPageEn.Id, "ja-jp", "images", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            imagesPageJp.Id = imagesPageEn.Id;
            await s.ContentService.SaveContent(imagesPageJp, triggerEvents: false, userName: uname);

            // home/news/images/tokyo
            var tokyoPageEn = await s.ContentService.Create<Folder>(imagesPageEn.Id, "en-gb", "tokyo", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(tokyoPageEn, triggerEvents: false, userName: uname);
            
            // home/news/images/london
            var londonPageEn = await s.ContentService.Create<Folder>(imagesPageEn.Id, "en-gb", "london", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(londonPageEn, triggerEvents: false, userName: uname);

            var repo2 = NewRepo(type);

            var londonRevision = repo2.CurrentRevision(londonPageEn.Id,londonPageEn.Variant);

            Assert.That(londonRevision.Path=="/homechangename/news/images/london");

            //add new ru-ru translation for imagesPage with different name
            var imagesPageRu = await s.ContentService.Create<Folder>(newsPageEn.Id, "ru-ru", "images1", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            imagesPageRu.Id = imagesPageEn.Id;
            await s.ContentService.SaveContent(imagesPageRu, triggerEvents: false, userName: uname);

            //since imagesPageRu was not published, name change should only affect unpublished variants and not published variants or any descendant paths
            //NOTE - name changes should be synced across variants/translations

            // images-jp, since it was unpublished, should have its nodename changed to images1
            var _imagesJp = NewRepo(type).CurrentRevision(imagesPageJp.Id, imagesPageJp.Variant);
            Assert.That(
                _imagesJp.NodeName=="images1"
                );
            // images-en, since it was published, should have its nodename unchanged
            Assert.That(
                NewRepo(type).CurrentRevision(imagesPageEn.Id, imagesPageEn.Variant).NodeName == "images"
                );
            // since imagesPageRu was unpublished, descendant content should have its path unchanged
            Assert.That(
                NewRepo(type).CurrentRevision(londonPageEn.Id, londonPageEn.Variant).Path == "/homechangename/news/images/london"
                );

            // now publish images-jp
            var _imageJpMod = _imagesJp.ToBaseModel();
            _imageJpMod.Published = true;
            await s.ContentService.SaveContent(_imageJpMod,triggerEvents:false,userName:uname);
            
            var _imagesEn = NewRepo(type).CurrentRevision(imagesPageEn.Id, imagesPageEn.Variant);
            // images-en should now have its nodename changed
            Assert.That(
                _imagesEn.NodeName == "images1"
                );

            // since the nodename change is now published, descendant content should have its path changed
            var _londonEn = NewRepo(type).CurrentRevision(londonPageEn.Id, londonPageEn.Variant);
            Assert.That(
                _londonEn.Path == "/homechangename/news/images1/london"
                );
            var _tokyoEn = NewRepo(type).CurrentRevision(tokyoPageEn.Id, tokyoPageEn.Variant);
            Assert.That(
                _tokyoEn.Path == "/homechangename/news/images1/tokyo"
                );
        }

        [Test]
        [TestCase(DbConstants.SQLite)]
        [TestCase(DbConstants.MySql)]
        [TestCase(DbConstants.SQLServer)]
        [TestCase(DbConstants.PostgreSQL)]
        public async Task RepublishEntireSite2(string type)
        {
            var s = GetServices(type);
            // home
            var homePage = await s.ContentService.Create<Folder>(Guid.Empty, "en-gb", "homeRepublish", template: "~/views/home/homepage.cshtml", published: true, userName: "darkezmo@hotmail.com");
            await s.ContentService.SaveContent(homePage, triggerEvents: false, userName: uname);

            // home/news
            var newsPageEn = await s.ContentService.Create<Folder>(homePage.Id, "en-gb", "news", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(newsPageEn, triggerEvents: false, userName: uname);
            var newsPageJp = await s.ContentService.Create<Folder>(homePage.Id, "ja-jp", "news", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            newsPageJp.Id = newsPageEn.Id;
            await s.ContentService.SaveContent(newsPageJp, triggerEvents: false, userName: uname);

            // home/news/images
            var imagesPageEn = await s.ContentService.Create<Folder>(newsPageEn.Id, "en-gb", "images", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(imagesPageEn, triggerEvents: false, userName: uname);
            var imagesPageJp = await s.ContentService.Create<Folder>(newsPageEn.Id, "ja-jp", "images", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            imagesPageJp.Id = imagesPageEn.Id;
            await s.ContentService.SaveContent(imagesPageJp, triggerEvents: false, userName: uname);

            // home/news/images/tokyo
            var tokyoPageEn = await s.ContentService.Create<Folder>(imagesPageEn.Id, "en-gb", "tokyo", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(tokyoPageEn, triggerEvents: false, userName: uname);

            // home/news/images/london
            var londonPageEn = await s.ContentService.Create<Folder>(imagesPageEn.Id, "en-gb", "london", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(londonPageEn, triggerEvents: false, userName: uname);

            var repo2 = NewRepo(type);

            var londonRevision = repo2.CurrentRevision(londonPageEn.Id, londonPageEn.Variant);

            //add new ru-ru translation for imagesPage with different name
            var imagesPageRu = await s.ContentService.Create<Folder>(newsPageEn.Id, "ru-ru", "images1", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            imagesPageRu.Id = imagesPageEn.Id;
            await s.ContentService.SaveContent(imagesPageRu, triggerEvents: false, userName: uname);

            // images-jp, since it was unpublished, should have its nodename changed to images1
            var _imagesJp = NewRepo(type).CurrentRevision(imagesPageJp.Id, imagesPageJp.Variant);
            
            // now publish images-jp
            var _imageJpMod = _imagesJp.ToBaseModel();
            _imageJpMod.Published = true;
            await s.ContentService.SaveContent(_imageJpMod, triggerEvents: false, userName: uname);

            var _imagesEn = NewRepo(type).CurrentRevision(imagesPageEn.Id, imagesPageEn.Variant);
            // images-en should now have its nodename changed
            
            // since the nodename change is now published, descendant content should have its path changed
            var _londonEn = NewRepo(type).CurrentRevision(londonPageEn.Id, londonPageEn.Variant);

            await s.ContentService.RePublishEntireSite2();
            Assert.Pass();
        }

        [Test]
        [TestCase(DbConstants.SQLite)]
        [TestCase(DbConstants.MySql)]
        [TestCase(DbConstants.SQLServer)]
        [TestCase(DbConstants.PostgreSQL)]
        public async Task Move(string type)
        {
            var s = GetServices(type);
            var rootName = "homemove";
            // home
            var homePage = await s.ContentService.Create<Folder>(Guid.Empty, "en-gb", rootName, template: "~/views/home/homepage.cshtml", published: true, userName: "darkezmo@hotmail.com");
            await s.ContentService.SaveContent(homePage, triggerEvents: false, userName: uname);

            // home/news
            var newsPageEn = await s.ContentService.Create<Folder>(homePage.Id, "en-gb", "news", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(newsPageEn, triggerEvents: false, userName: uname);
            var newsPageJp = await s.ContentService.Create<Folder>(homePage.Id, "ja-jp", "news", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            newsPageJp.Id = newsPageEn.Id;
            await s.ContentService.SaveContent(newsPageJp, triggerEvents: false, userName: uname);

            // home/news/images
            var imagesPageEn = await s.ContentService.Create<Folder>(newsPageEn.Id, "en-gb", "images", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(imagesPageEn, triggerEvents: false, userName: uname);
            var imagesPageJp = await s.ContentService.Create<Folder>(newsPageEn.Id, "ja-jp", "images", template: "~/views/home/homepage.cshtml", published: false, userName: uname);
            imagesPageJp.Id = imagesPageEn.Id;
            await s.ContentService.SaveContent(imagesPageJp, triggerEvents: false, userName: uname);

            // home/news/images/tokyo
            var tokyoPageEn = await s.ContentService.Create<Folder>(imagesPageEn.Id, "en-gb", "tokyo", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(tokyoPageEn, triggerEvents: false, userName: uname);

            // home/news/images/london
            var londonPageEn = await s.ContentService.Create<Folder>(imagesPageEn.Id, "en-gb", "london", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(londonPageEn, triggerEvents: false, userName: uname);

            // home/news/images/london/camden
            var camdenPageEn = await s.ContentService.Create<Folder>(londonPageEn.Id, "en-gb", "camden", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(camdenPageEn, triggerEvents: false, userName: uname);

            //before move
            var _londonEnRev1 = NewRepo(type).CurrentRevision(londonPageEn.Id, londonPageEn.Variant);
            var _camdenPageEnRev1 = NewRepo(type).CurrentRevision(camdenPageEn.Id, camdenPageEn.Variant);

            Assert.That(_londonEnRev1.IdPath == $"{homePage.Id},{newsPageEn.Id},{imagesPageEn.Id},{londonPageEn.Id}");
            Assert.That(_camdenPageEnRev1.IdPath == $"{homePage.Id},{newsPageEn.Id},{imagesPageEn.Id},{londonPageEn.Id},{camdenPageEn.Id}");

            Assert.That(_londonEnRev1.Path == $"/{rootName}/news/images/london");
            Assert.That(_camdenPageEnRev1.Path == $"/{rootName}/news/images/london/camden");

            await s.ContentService.Move(londonPageEn.Id, newsPageEn.Id,userName:uname);

            //after move
            var _londonEnRev2 = NewRepo(type).CurrentRevision(londonPageEn.Id,londonPageEn.Variant);
            var _camdenPageEnRev2 = NewRepo(type).CurrentRevision(camdenPageEn.Id,camdenPageEn.Variant);

            var _londonEnRev2_ = NewRepo(type).GetPuckRevision().Where(x => x.Id == londonPageEn.Id && x.Variant.ToLower().Equals(londonPageEn.Variant.ToLower())).FirstOrDefault();

            Assert.That(_londonEnRev2.IdPath == $"{homePage.Id},{newsPageEn.Id},{londonPageEn.Id}");
            Assert.That(_camdenPageEnRev2.IdPath == $"{homePage.Id},{newsPageEn.Id},{londonPageEn.Id},{camdenPageEn.Id}");

            Assert.That(_londonEnRev2.Path == $"/{rootName}/news/london");
            Assert.That(_camdenPageEnRev2.Path == $"/{rootName}/news/london/camden");

        }


        [Test]
        [TestCase(DbConstants.SQLite)]
        [TestCase(DbConstants.MySql)]
        [TestCase(DbConstants.SQLServer)]
        [TestCase(DbConstants.PostgreSQL)]
        public void ConnectionsSame(string type)
        {
            var s = GetServices(type);
            var con = s.Repo.Context.Database.GetDbConnection();
            var con2 = s.Repo.Context.Database.GetDbConnection();
            Assert.That(con==con2);
        }

        [Test]
        [TestCase(DbConstants.SQLite)]
        [TestCase(DbConstants.MySql)]
        [TestCase(DbConstants.SQLServer)]
        [TestCase(DbConstants.PostgreSQL)]
        public async Task CreateRoot(string type)
        {
            var s = GetServices(type);
            var model = await s.ContentService.Create<Folder>(Guid.Empty, "en-gb", "home", template: "~/views/home/homepage.cshtml", published: true, userName: uname);
            await s.ContentService.SaveContent(model,triggerEvents:false,userName:uname);

            var newRepo = NewRepo(type);
            var savedModel = newRepo.CurrentRevision(model.Id,model.Variant);
            Assert.That(savedModel!=null);
        }
    }
}