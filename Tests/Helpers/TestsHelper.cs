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

namespace puck.tests.Helpers
{
    public static class TestsHelper
    {
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
        public static List<Type> testModelTypes = new List<Type> {typeof(BaseModel), typeof(Folder) };
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
                PuckCache.TypeFields = new Dictionary<string, Dictionary<string, string>>();
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

    }
}
