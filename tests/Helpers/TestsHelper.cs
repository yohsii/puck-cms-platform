using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using puck.core.Base;
using puck.core.Helpers;
using puck.core.State;
using System;
using System.Collections.Generic;
using System.Linq;
using puck.tests.ViewModels;
using puck.tests.Models;
using System.Threading.Tasks;
using puck.core.Abstract;

namespace puck.tests.Helpers
{
    public static class TestsHelper
    {
        public static IConfiguration Config = GetConfig();
        public static IConfiguration GetConfig() {
            return new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .AddEnvironmentVariables()
                .Build();
        }
        public static IHostEnvironment GetEnvironment() {
            var mock = new Mock<IHostEnvironment>();
            mock.Setup(x => x.ContentRootPath)
                .Returns(GetConfig().GetValue<string>("ContentRootPath"));
            return mock.Object;
        }
        public static List<Type> testModelTypes = new List<Type> {typeof(BaseModel), typeof(Folder),typeof(ModelWithReferences) };
        public static void SetAQNMappings()
        {
            PuckCache.ModelNameToAQN = new Dictionary<string, string>();
            foreach (var t in testModelTypes)
            {
                if (PuckCache.ModelNameToAQN.ContainsKey(t.Name))
                    throw new Exception($"there is more than one ViewModel with the name:{t.Name}. ViewModel names must be unique!");
                PuckCache.ModelNameToAQN[t.Name] = t.AssemblyQualifiedName;
            }
        }
        public static void SetAnalyzerMappings()
        {
            var panalyzers = new List<Analyzer>();
            var analyzerForModel = new Dictionary<Type, Analyzer>();
            PuckCache.TypeFields = new Dictionary<string, Dictionary<string, string>>();
            foreach (var t in testModelTypes)
            {
                var instance = ApiHelper.CreateInstance(t);
                try
                {
                    ObjectDumper.SetPropertyValues(instance);
                }
                catch (Exception ex)
                {
                    throw;
                };

                var dmp = ObjectDumper.Write(instance, int.MaxValue);
                var analyzers = new Dictionary<string, Analyzer>();
                PuckCache.TypeFields[t.AssemblyQualifiedName] = new Dictionary<string, string>();
                foreach (var p in dmp)
                {
                    if (!PuckCache.TypeFields[t.AssemblyQualifiedName].ContainsKey(p.Key))
                        PuckCache.TypeFields[t.AssemblyQualifiedName].Add(p.Key, p.Type.AssemblyQualifiedName);
                    if (p.Analyzer == null)
                        continue;
                    if (!panalyzers.Any(x => x.GetType() == p.Analyzer.GetType()))
                    {
                        panalyzers.Add(p.Analyzer);
                    }
                    analyzers.Add(p.Key, panalyzers.Where(x => x.GetType() == p.Analyzer.GetType()).FirstOrDefault());
                }
                var pfAnalyzer = new PerFieldAnalyzerWrapper(new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48), analyzers);
                analyzerForModel.Add(t, pfAnalyzer);
            }
            PuckCache.Analyzers = panalyzers;
            PuckCache.AnalyzerForModel = analyzerForModel;
        }
        public static async Task<SiteTree> CreateSite(I_Content_Service cs,I_Puck_Repository repo,string rootName,List<string> variants,List<bool> variantIsPublished,int levels,int branches,string username) {
            var tree = new SiteTree();
            tree.Level = 1;
            tree.Branch = 1;
            for (var i = 0; i < variants.Count; i++) {
                var published = variantIsPublished[i];
                var root = await cs.Create<Folder>(Guid.Empty, variants[i], rootName, template: "template.cshtml", published: published, userName: username);
                await cs.SaveContent(root,triggerEvents:false,userName:username);
                var revision = repo.CurrentRevision(root.Id,root.Variant);
                tree.Variants.Add(revision);
            }
            
            async Task CreateLevel(int level,SiteTree ctree) {
                if (level > levels) return;
                for (var j = 0; j < branches; j++)
                {
                    var btree = new SiteTree();
                    btree.Level = level;
                    btree.Branch = j + 1;
                    for (var k = 0; k < variants.Count; k++)
                    {
                        var published = variantIsPublished[k];
                        var model = await cs.Create<Folder>(ctree.Variants.First().Id, variants[k], (j+1).ToString(), template: "template.cshtml", published: published, userName: username);
                        await cs.SaveContent(model, triggerEvents: false, userName: username);
                        var revision = repo.CurrentRevision(model.Id, model.Variant);
                        btree.Variants.Add(revision);
                    }
                    ctree.Children.Add(btree);
                    await CreateLevel(level+1,btree);
                }
            }
            await CreateLevel(2,tree);
            return tree;
        }
    }
}
