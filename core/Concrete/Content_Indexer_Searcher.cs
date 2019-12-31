using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using System.Web;
using Lucene.Net.Documents;
using System.IO;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using puck.core.Abstract;
using Lucene.Net.Store;
using puck.core.Constants;
using puck.core.Helpers;
using Newtonsoft.Json;
using Lucene.Net.Analysis;
using puck.core.Base;
using puck.core.Events;
using Spatial4n.Core.Context;
using Lucene.Net.Spatial.Vector;
using Lucene.Net.Spatial.Queries;
using puck.core.PuckLucene;
using StackExchange.Profiling;
using System.Threading.Tasks;
using puck.core.State;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Util;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Spatial4n.Core.Shapes;
using System.Globalization;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Prefix;
using System.Configuration;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Lucene.Net.Store.Azure;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace puck.core.Concrete
{
    public class Content_Indexer_Searcher : I_Content_Indexer, I_Content_Searcher
    {
        private StandardAnalyzer StandardAnalyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(LuceneVersion.LUCENE_48);
        private KeywordAnalyzer KeywordAnalyzer = new KeywordAnalyzer();
        public readonly SpatialContext ctx = SpatialContext.GEO;
        private Lucene.Net.Store.Directory Directory = null;
        private Regex regexIndexPathReplaceMachineName = new Regex(Regex.Escape("{MachineName}"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private Regex regexIndexPathReplaceContentRootPath = new Regex(Regex.Escape("{ContentRootPath}"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private string INDEXPATH
        {
            get
            {
                string path = null;
                if (PuckCache.UseAzureLucenePath)
                {
                    path = ReplacePathTokens(config.GetValue<string>("LuceneAzureIndexPath"));
                }
                else
                {
                    path = ReplacePathTokens(config.GetValue<string>("LuceneIndexPath"));
                }
                return path;
            }
        }
        private string[] NoToken = new string[] { FieldKeys.ID.ToString(), FieldKeys.Path.ToString() };
        private IndexSearcher Searcher = null;
        private IndexWriter Writer = null;
        private Object write_lock = new Object();
        private I_Log logger;
        private IConfiguration config = null;
        public bool CanWrite { get; set; } = true;
        public bool UseAzureDirectory { get; set; } = false;
        public IHostEnvironment env { get; set; }
        public bool handleQueueStarted = false;
        public Content_Indexer_Searcher(I_Log Logger, IConfiguration configuration, IHostEnvironment env)
        {
            this.logger = Logger;
            this.config = configuration;
            this.env = env;
            UseAzureDirectory = config.GetValue<bool?>("UseAzureDirectory") ?? false;
            if (UseAzureDirectory && !config.GetValue<bool>("IsEditServer"))
                CanWrite = false;

            Ini();

            BeforeIndex += new EventHandler<BeforeIndexingEventArgs>(DelegateBeforeIndexing);
            AfterIndex += new EventHandler<IndexingEventArgs>(DelegateAfterIndexing);
            BeforeDelete += new EventHandler<BeforeIndexingEventArgs>(DelegateBeforeDelete);
            AfterDelete += new EventHandler<IndexingEventArgs>(DelegateAfterDelete);
        }
        private string ReplacePathTokens(string path)
        {
            path = regexIndexPathReplaceMachineName
                .Replace(path, ApiHelper.ServerName());
            path = regexIndexPathReplaceContentRootPath
                .Replace(path, env.ContentRootPath);
            return path;
        }
        public void Ini()
        {
            if (UseAzureDirectory)
            {
                var azureBlobConnectionString = config.GetValue<string>("AzureDirectoryConnectionString");
                var azureDirectoryCachePath = ReplacePathTokens(config.GetValue<string>("AzureDirectoryCachePath"));
                var azureDirectoryContainerName = config.GetValue<string>("AzureDirectoryContainerName");
                //if empty string, ensure value is null
                if (string.IsNullOrEmpty(azureDirectoryContainerName)) azureDirectoryContainerName = null;
                var cloudStorageAccount = CloudStorageAccount.Parse(azureBlobConnectionString);
                Directory = new AzureDirectory(cloudStorageAccount, azureDirectoryCachePath, containerName: azureDirectoryContainerName);
            }
            else
            {
                if (!System.IO.Directory.Exists(INDEXPATH))
                {
                    System.IO.Directory.CreateDirectory(INDEXPATH);
                }
                Directory = FSDirectory.Open(INDEXPATH);
            }
            bool create = !DirectoryReader.IndexExists(Directory);

            lock (write_lock)
            {
                try
                {
                    if (CanWrite) { 
                        SetWriter(create);
                        if (Writer != null && !UseAzureDirectory) CloseWriter();
                        //Writer.Optimize();
                    }
                }
                catch (Lucene.Net.Store.LockObtainFailedException ex)
                {
                    logger.Log(ex);
                }
                catch (Exception ex)
                {
                    throw;
                    //logger.Log(ex);
                }
                finally
                {
                    //CloseWriter();
                }
            }
            SetSearcher();
        }
        public void SetWriter(bool create)
        {
            if (Writer == null)
            {
                //var dir = FSDirectory.Open(INDEXPATH);
                var config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, StandardAnalyzer);
                config.OpenMode = OpenMode.CREATE_OR_APPEND;
                Writer = new IndexWriter(Directory, config);
            }

        }
        public void CloseWriter()
        {
            if (Writer != null && CanWrite && !UseAzureDirectory)
            {
                Writer.Dispose(false);
                Writer = null;
            }
        }
        public void EnsureSearcher()
        {
            try
            {
                if (Searcher != null)
                    return;
                var indexReader = DirectoryReader.Open(Directory);
                Searcher = new Lucene.Net.Search.IndexSearcher(indexReader);
            }
            catch (Lucene.Net.Index.IndexNotFoundException ex)
            {
                logger.Log(ex);
            }
            catch (Exception ex) { throw; }
        }
        public void SetSearcher()
        {
            try
            {
                var oldSearcher = Searcher;
                //var dir = FSDirectory.Open(INDEXPATH);
                var indexReader = DirectoryReader.Open(Directory);
                Searcher = new Lucene.Net.Search.IndexSearcher(indexReader);
                //kill old searcher
                if (oldSearcher != null)
                {
                    oldSearcher.IndexReader.Dispose();
                }
                oldSearcher = null;
            }
            catch (Lucene.Net.Index.IndexNotFoundException ex)
            {
                logger.Log(ex);
            }
            catch (Exception ex) { throw; }
        }
        private static void DelegateBeforeEvent(Dictionary<string, Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>> list, object n, BeforeIndexingEventArgs e)
        {
            var type = e.Node.GetType();
            //refactor:can probably use is operator to implement event propagation
            var types = ApiHelper.BaseTypes(type);
            types.Add(type);
            list.Where(x => x.Value.Item1 == type || (x.Value.Item3 && types.Contains(x.Value.Item1)))
                .ToList().ForEach(x =>
                {
                    x.Value.Item2(n, e);
                });
        }
        private static void DelegateAfterEvent(Dictionary<string, Tuple<Type, Action<object, IndexingEventArgs>, bool>> list, object n, IndexingEventArgs e)
        {
            var type = e.Node.GetType();
            //refactor:can probably use is operator to implement event propagation
            var types = ApiHelper.BaseTypes(type);
            types.Add(type);
            list.Where(x => x.Value.Item1 == type || (x.Value.Item3 && types.Contains(x.Value.Item1)))
                .ToList().ForEach(x =>
                {
                    x.Value.Item2(n, e);
                });
        }
        private static void DelegateBeforeIndexing(object n, BeforeIndexingEventArgs e)
        {
            DelegateBeforeEvent(BeforeIndexActionList, n, e);
        }
        private static void DelegateAfterIndexing(object n, IndexingEventArgs e)
        {
            DelegateAfterEvent(AfterIndexActionList, n, e);
        }
        private static void DelegateBeforeDelete(object n, BeforeIndexingEventArgs e)
        {
            DelegateBeforeEvent(BeforeDeleteActionList, n, e);
        }
        private static void DelegateAfterDelete(object n, IndexingEventArgs e)
        {
            DelegateAfterEvent(AfterIndexActionList, n, e);
        }

        public static Dictionary<string, Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>> BeforeIndexActionList =
            new Dictionary<string, Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>>();

        public static Dictionary<string, Tuple<Type, Action<object, IndexingEventArgs>, bool>> AfterIndexActionList =
            new Dictionary<string, Tuple<Type, Action<object, IndexingEventArgs>, bool>>();

        public static Dictionary<string, Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>> BeforeDeleteActionList =
            new Dictionary<string, Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>>();

        public static Dictionary<string, Tuple<Type, Action<object, IndexingEventArgs>, bool>> AfterDeleteActionList =
            new Dictionary<string, Tuple<Type, Action<object, IndexingEventArgs>, bool>>();

        public event EventHandler<BeforeIndexingEventArgs> BeforeIndex;
        public event EventHandler<IndexingEventArgs> AfterIndex;
        public event EventHandler<BeforeIndexingEventArgs> BeforeDelete;
        public event EventHandler<IndexingEventArgs> AfterDelete;

        public void RegisterBeforeIndexHandler<T>(string Name, Action<object, BeforeIndexingEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            BeforeIndexActionList.Add(Name, new Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>(typeof(T), Handler, Propagate));
        }
        public void RegisterAfterIndexHandler<T>(string Name, Action<object, IndexingEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            AfterIndexActionList.Add(Name, new Tuple<Type, Action<object, IndexingEventArgs>, bool>(typeof(T), Handler, Propagate));
        }
        public void RegisterBeforeDeleteHandler<T>(string Name, Action<object, BeforeIndexingEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            BeforeDeleteActionList.Add(Name, new Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>(typeof(T), Handler, Propagate));
        }
        public void RegisterAfterDeleteHandler<T>(string Name, Action<object, IndexingEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            AfterDeleteActionList.Add(Name, new Tuple<Type, Action<object, IndexingEventArgs>, bool>(typeof(T), Handler, Propagate));
        }

        public void UnRegisterBeforeIndexHandler(string Name)
        {
            BeforeIndexActionList.Remove(Name);
        }
        public void UnRegisterAfterIndexHandler(string Name)
        {
            AfterIndexActionList.Remove(Name);
        }
        public void UnRegisterBeforeDeleteHandler(string Name)
        {
            BeforeDeleteActionList.Remove(Name);
        }
        public void UnRegisterAfterDeleteHandler(string Name)
        {
            AfterDeleteActionList.Remove(Name);
        }

        protected void OnBeforeIndex(object s, BeforeIndexingEventArgs args)
        {
            if (BeforeIndex != null)
                BeforeIndex(s, args);
        }

        protected void OnAfterIndex(object s, IndexingEventArgs args)
        {
            if (AfterIndex != null)
                AfterIndex(s, args);
        }

        protected void OnBeforeDelete(object s, BeforeIndexingEventArgs args)
        {
            if (BeforeDelete != null)
                BeforeDelete(s, args);
        }

        protected void OnAfterDelete(object s, IndexingEventArgs args)
        {
            if (AfterDelete != null)
                AfterDelete(s, args);
        }

        public void GetFieldSettings(List<FlattenedObject> props, Document doc, List<KeyValuePair<string, Analyzer>> analyzers)
        {
            foreach (var p in props)
            {
                if (analyzers != null)
                {
                    if (p.Analyzer != null)
                    {
                        analyzers.Add(new KeyValuePair<string, Analyzer>(p.Key, p.Analyzer));
                    }
                }
                if (doc != null)
                {
                    if (p.Value is int)
                    {
                        var nf = new Int32Field(p.Key, int.Parse(p.Value.ToString()), p.FieldStoreSetting);
                        doc.Add(nf);
                    }
                    else if (p.Value is long)
                    {
                        var nf = new Int64Field(p.Key, long.Parse(p.Value.ToString()), p.FieldStoreSetting);
                        doc.Add(nf);
                    }
                    else if (p.Value is float)
                    {
                        var nf = new SingleField(p.Key, float.Parse(p.Value.ToString()), p.FieldStoreSetting);
                        doc.Add(nf);
                    }
                    else if (p.Value is double)
                    {
                        var nf = new DoubleField(p.Key, double.Parse(p.Value.ToString()), p.FieldStoreSetting);
                        doc.Add(nf);
                    }
                    else if (p.Spatial)
                    {
                        if (p.Value == null || string.IsNullOrEmpty(p.Value.ToString()))
                            continue;
                        var name = p.Key;// p.Key.IndexOf('.')>-1?p.Key.Substring(0,p.Key.LastIndexOf('.')):p.Key;
                        int maxLevels = 11;
                        SpatialPrefixTree grid = new GeohashPrefixTree(ctx, maxLevels);
                        var strat = new RecursivePrefixTreeStrategy(grid, name);

                        //var strat = new PointVectorStrategy(ctx,name);
                        var yx = p.Value.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => double.Parse(x)).ToList();
                        var point = ctx.MakePoint(yx[1], yx[0]);
                        //var point = ctx.ReadShape(p.Value.ToString());
                        var fields = strat.CreateIndexableFields(point);
                        fields.ToList().ForEach(x => doc.Add(x));

                        IPoint pt = (IPoint)point;
                        //doc.Add(new StoredField(strat.FieldName, pt.X.ToString(CultureInfo.InvariantCulture) + " " + pt.Y.ToString(CultureInfo.InvariantCulture)));

                    }
                    else
                    {
                        string value = p.Value == null ? null : (p.KeepValueCasing ? p.Value.ToString() : p.Value.ToString().ToLower());
                        Field f = null;
                        if (p.FieldIndexSetting == Field.Index.ANALYZED || p.FieldIndexSetting == Field.Index.ANALYZED_NO_NORMS)
                            f = new TextField(p.Key, value ?? string.Empty, p.FieldStoreSetting);
                        else
                            f = new StringField(p.Key, value ?? string.Empty, p.FieldStoreSetting);
                        doc.Add(f);
                    }
                }
            }
        }
        public void HandleIndexQueue(bool triggerEvents=true,bool delete=true) {
            try
            {
                var itemsToIndex = new List<BaseModel>();
                try
                {
                    while (PuckCache.PublishQueue.Count > 0)
                    {
                        List<BaseModel> items;
                        if (PuckCache.PublishQueue.TryDequeue(out items))
                        {
                            itemsToIndex.AddRange(items);
                        }
                    }
                }
                catch (Exception ex) {
                    handleQueueStarted = false;
                    throw;
                }
                handleQueueStarted = false;
                if (itemsToIndex.Count > 0)
                    Index(itemsToIndex, triggerEvents: triggerEvents, delete: delete);
            }
            catch (Exception ex) {
                logger.Log(ex);
            }
        }
        public bool IsBusy()
        {
            bool taken = false;
            try
            {
                Monitor.TryEnter(write_lock, 0, ref taken);
            }
            catch (Exception ex)
            {
                PuckCache.PuckLog.Log(ex);
            }
            finally
            {
                if (taken)
                    Monitor.Exit(write_lock);
            }
            return !taken;
        }
        public bool Index<T>(List<T> models, bool triggerEvents = true,bool delete=true,bool queueIfBusy=false) where T : BaseModel
        {
            if (models.Count == 0) return true;
            bool taken = false;
            Exception caughtException = null;
            try
            {
                var timeout = queueIfBusy ? TimeSpan.FromMilliseconds(0) : TimeSpan.FromMilliseconds(-1);
                Monitor.TryEnter(write_lock, timeout, ref taken);
                if (!taken && queueIfBusy) {
                    PuckCache.PublishQueue.Enqueue(models as List<BaseModel>);
                    return false;
                }
                var cancelled = new List<BaseModel>();
                var count = 1;
                
                if (!CanWrite) return true;
                SetWriter(false);
                //Writer.Flush(true, true, true);
                Parallel.ForEach(models, (m, state, index) => {
                    PuckCache.IndexingStatus = $"indexing item {count} of {models.Count}";
                    //var type = ApiHelper.GetType(m.Type);
                    //if (type == null)
                    //    type = typeof(BaseModel);
                    var type = ApiHelper.GetTypeFromName(m.Type, defaultToBaseModel: true);
                    var analyzer = PuckCache.AnalyzerForModel[type];
                    var parser = new PuckQueryParser<T>(Lucene.Net.Util.LuceneVersion.LUCENE_48, FieldKeys.PuckDefaultField, analyzer);
                    if (triggerEvents)
                    {
                        var args = new BeforeIndexingEventArgs() { Node = m, Cancel = false };
                        OnBeforeIndex(this, args);
                        if (args.Cancel)
                        {
                            cancelled.Add(m);
                            return;
                        }
                    }
                    if (delete)
                    {
                        //delete doc
                        string removeQuery = "+" + FieldKeys.ID + ":" + m.Id.ToString() + " +" + FieldKeys.Variant + ":" + m.Variant.ToLower();
                        var q = parser.Parse(removeQuery);
                        Writer.DeleteDocuments(q);
                    }
                    Document doc = new Document();
                    //get fields to index
                    List<FlattenedObject> props = null;
                    using (MiniProfiler.Current.CustomTiming("get properties", ""))
                    {
                        props = ObjectDumper.Write(m, int.MaxValue);
                    }
                    using (MiniProfiler.Current.CustomTiming("add fields to doc", ""))
                    {
                        GetFieldSettings(props, doc, null);
                    }//add cms properties
                    if (!PuckCache.StoreReferences)
                        m.References = new List<string>();
                    string jsonDoc = JsonConvert.SerializeObject(m);
                    //doc in json form for deserialization later
                    doc.Add(new StringField(FieldKeys.PuckValue, jsonDoc, Field.Store.YES));
                    using (MiniProfiler.Current.CustomTiming("add document", ""))
                    {
                        Writer.AddDocument(doc, analyzer);
                    }
                    count++;
                });

                //Writer.Flush(true,true,true);
                using (MiniProfiler.Current.CustomTiming("commit", ""))
                {
                    Writer.Commit();
                }
                CloseWriter();
                SetSearcher();
                if (triggerEvents)
                {
                    models
                        .Where(x => !cancelled.Contains(x))
                        .ToList()
                        .ForEach(x => { OnAfterIndex(this, new IndexingEventArgs() { Node = x }); });
                }
                if (!handleQueueStarted)
                {
                    handleQueueStarted = true;
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(2000);
                        HandleIndexQueue();
                    });
                }
                //Optimize();
            }
            catch (Exception ex)
            {
                logger.Log(ex);
                caughtException = ex;
            }
            finally
            {
                if (taken)
                    Monitor.Exit(write_lock);
            }
            if (caughtException != null) throw caughtException;
            return true;
        }
        public void Delete<T>(List<T> toDelete) where T : BaseModel
        {
            lock (write_lock)
            {
                try
                {
                    if (!CanWrite) return;
                    var analyzer = PuckCache.AnalyzerForModel[typeof(T)];
                    var parser = new PuckQueryParser<T>(Lucene.Net.Util.LuceneVersion.LUCENE_48, FieldKeys.PuckDefaultField, analyzer);
                    SetWriter(false);
                    Writer.Flush(true, true);
                    var cancelled = new List<BaseModel>();
                    foreach (var m in toDelete)
                    {
                        var args = new BeforeIndexingEventArgs() { Node = m, Cancel = false };
                        OnBeforeDelete(this, args);
                        if (args.Cancel)
                        {
                            cancelled.Add(m);
                            continue;
                        }
                        string removeQuery = "+" + FieldKeys.ID + ":" + m.Id.ToString() + " +" + FieldKeys.Variant + ":" + m.Variant;
                        var q = parser.Parse(removeQuery);
                        Writer.DeleteDocuments(q);
                    }
                    Writer.Flush(true, true);
                    Writer.Commit();
                    toDelete
                        .Where(x => !cancelled.Contains(x))
                        .ToList()
                        .ForEach(x => { OnAfterDelete(this, new IndexingEventArgs() { Node = x }); });
                }
                catch (Exception ex)
                {
                    throw;
                    //logger.Log(ex);
                }
                finally
                {
                    CloseWriter();
                    SetSearcher();
                }
            }
        }
        public void Delete<T>(T toDelete) where T : BaseModel
        {
            if (toDelete != null)
                Delete<T>(new List<T> { toDelete });
        }
        public void Index<T>(T model) where T : BaseModel
        {
            if (model != null)
                Index(new List<T> { model });
        }

        public void Index(List<Dictionary<string, string>> values)
        {
            foreach (var dict in values)
            {
                this.Index(dict);
            }
        }

        public void Index(Dictionary<string, string> values)
        {
            lock (write_lock)
            {
                try
                {
                    if (!CanWrite) return;
                    SetWriter(false);
                    var id = values.Where(x => x.Key.Equals(FieldKeys.ID)).FirstOrDefault().Value;
                    Writer.DeleteDocuments(new Term(FieldKeys.ID, id));
                    Document doc = new Document();
                    foreach (var nv in values)
                    {
                        Field field;
                        if (NoToken.Contains(nv.Key.ToLower()))
                        {
                            field = new StringField(nv.Key.ToLower(), nv.Value, Field.Store.YES);
                        }
                        else
                        {
                            field = new TextField(nv.Key.ToLower(), nv.Value, Field.Store.YES);
                        }
                        doc.Add(field);
                    }
                    Writer.AddDocument(doc);
                    Writer.Commit();
                    Optimize();
                }
                catch (Exception ex)
                {
                    throw;
                    //logger.Log(ex);
                }
                finally
                {
                    CloseWriter();
                    SetSearcher();
                }
            }
        }
        public void DeleteAll(bool reloadSearcher = true,bool commit = true)
        {
            lock (write_lock)
            {
                try
                {
                    if (CanWrite)
                    {
                        SetWriter(false);
                        Writer.DeleteAll();
                        if(commit)
                            Writer.Commit();
                    }
                }
                catch (Exception ex)
                {
                    throw;
                    //logger.Log(ex);
                }
                finally
                {
                    CloseWriter();
                    if (reloadSearcher)
                        SetSearcher();
                }
            }
        }
        public void Delete(string terms, bool reloadSearcher = true)
        {
            lock (write_lock)
            {
                try
                {
                    var parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "text", StandardAnalyzer);
                    var contentQuery = parser.Parse(terms);
                    if (CanWrite)
                    {
                        SetWriter(false);
                        Writer.DeleteDocuments(contentQuery);
                        Writer.Commit();
                    }
                }
                catch (Exception ex)
                {
                    throw;
                    //logger.Log(ex);
                }
                finally
                {
                    CloseWriter();
                    if (reloadSearcher)
                        SetSearcher();
                }
            }
        }
        //optimize seems to be dropped in lucene 4.8
        public void Optimize()
        {
            throw new NotImplementedException();
            lock (write_lock)
            {
                try
                {
                    SetWriter(false);
                    //Writer.Optimize();
                }
                catch (OutOfMemoryException ex)
                {
                    CloseWriter();
                    throw;
                }
                catch (Exception ex)
                {
                    throw;
                    //logger.Log(ex);
                }
                finally
                {
                    SetSearcher();
                }
            }
        }
        public IList<Dictionary<string, string>> Query(Query contentQuery, HashSet<string> fieldsToLoad = null, int limit = 500)
        {
            EnsureSearcher();
            var hits = Searcher.Search(contentQuery, limit).ScoreDocs;

            var result = new List<Dictionary<string, string>>();
            for (var i = 0; i < hits.Count(); i++)
            {
                if (fieldsToLoad == null)
                {
                    var doc = Searcher.Doc(hits[i].Doc);
                    var d = new Dictionary<string, string>();
                    d.Add(FieldKeys.ID, doc.GetValues(FieldKeys.ID).FirstOrDefault() ?? "");
                    d.Add(FieldKeys.PuckType, doc.GetValues(FieldKeys.PuckType).FirstOrDefault() ?? "");
                    d.Add(FieldKeys.PuckValue, doc.GetValues(FieldKeys.PuckValue).FirstOrDefault() ?? "");
                    d.Add(FieldKeys.Path, doc.GetValues(FieldKeys.Path).FirstOrDefault() ?? "");
                    d.Add(FieldKeys.Variant, doc.GetValues(FieldKeys.Variant).FirstOrDefault() ?? "");
                    d.Add(FieldKeys.TemplatePath, doc.GetValues(FieldKeys.TemplatePath).FirstOrDefault() ?? "");
                    d.Add(FieldKeys.Score, hits[i].Score.ToString());
                    result.Add(d);
                }
                else
                {
                    var doc = Searcher.Doc(hits[i].Doc, fieldsToLoad);
                    var d = new Dictionary<string, string>();
                    foreach (var key in fieldsToLoad)
                    {
                        d.Add(key, doc.GetValues(key).FirstOrDefault() ?? "");
                    }
                    result.Add(d);
                }
            }
            return result;
        }
        public IList<Dictionary<string, string>> Query(string terms)
        {
            return Query(terms, null);
        }
        public IList<Dictionary<string, string>> Query(string terms, string typeName, HashSet<string> fieldsToLoad = null, int limit = 500)
        {
            QueryParser parser;
            if (!string.IsNullOrEmpty(typeName))
            {
                //var type = ApiHelper.GetType(typeName);
                var type = ApiHelper.GetTypeFromName(typeName);
                var analyzer = PuckCache.AnalyzerForModel[type];
                parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, FieldKeys.PuckDefaultField, analyzer);
            }
            else
            {
                parser = new QueryParser(LuceneVersion.LUCENE_48, "text", KeywordAnalyzer);
            }

            var contentQuery = parser.Parse(terms);
            return Query(contentQuery, fieldsToLoad: fieldsToLoad, limit: limit);
        }
        public IList<T> QueryNoCast<T>(string qstr) where T : BaseModel
        {
            int total;
            return QueryNoCast<T>(qstr, null, null, out total);
        }
        public IList<T> QueryNoCast<T>(string qstr, Filter filter, Sort sort, out int total, int limit = 500, int skip = 0, Type typeOverride = null, bool fallBackToBaseModel = false) where T : BaseModel
        {
            EnsureSearcher();
            var analyzer = PuckCache.AnalyzerForModel[typeof(T)];
            var parser = new PuckQueryParser<T>(LuceneVersion.LUCENE_48, FieldKeys.PuckDefaultField, analyzer);
            var q = parser.Parse(qstr);
            TopDocs docs;
            if (sort == null)
                docs = Searcher.Search(q, filter, limit);
            else
            {
                sort = sort.Rewrite(Searcher);
                docs = Searcher.Search(q, filter, limit, sort);
            }
            total = docs.TotalHits;
            var results = new List<T>();
            for (var i = skip; i < docs.ScoreDocs.Count(); i++)
            {
                var doc = Searcher.Doc(docs.ScoreDocs[i].Doc);
                //var type = ApiHelper.GetType(doc.GetValues(FieldKeys.PuckType).FirstOrDefault());
                Type type;
                if (typeOverride == null)
                {
                    type = ApiHelper.GetTypeFromName(doc.GetValues(FieldKeys.PuckType).FirstOrDefault());
                    if (type == null && fallBackToBaseModel)
                        type = typeof(BaseModel);
                    else if (type == null) continue;
                }
                else type = typeOverride;
                T result = (T)JsonConvert.DeserializeObject(doc.GetValues(FieldKeys.PuckValue)[0], type);
                results.Add(result);
            }
            return results;
        }
        public IList<T> Query<T>(string qstr) where T : BaseModel
        {
            int total;
            return Query<T>(qstr, null, null, out total);
        }
        public int Count<T>(string qstr) where T : BaseModel
        {
            int total;
            var result = Query<T>(qstr, null, null, out total, limit: 1);
            return total;
        }
        public int DocumentCount()
        {
            EnsureSearcher();
            var totalHitsCollector = new TotalHitCountCollector();
            Searcher?.Search(new MatchAllDocsQuery(), totalHitsCollector);
            return totalHitsCollector.TotalHits;
        }
        public IList<T> Query<T>(string qstr, Filter filter, Sort sort, out int total, int limit = 500, int skip = 0) where T : BaseModel
        {
            EnsureSearcher();
            var analyzer = PuckCache.AnalyzerForModel[typeof(T)];
            var parser = new PuckQueryParser<T>(LuceneVersion.LUCENE_48, FieldKeys.PuckDefaultField, analyzer);
            var q = parser.Parse(qstr);
            TopDocs docs;
            if (sort == null)
                docs = Searcher.Search(q, filter, limit);
            else
            {
                sort = sort.Rewrite(Searcher);
                docs = Searcher.Search(q, filter, limit, sort);
            }
            total = docs.TotalHits;
            var results = new List<T>();
            for (var i = skip; i < docs.ScoreDocs.Count(); i++)
            {
                var doc = Searcher.Doc(docs.ScoreDocs[i].Doc);
                T result = JsonConvert.DeserializeObject<T>(doc.GetValues(FieldKeys.PuckValue)[0]);
                results.Add(result);
            }
            return results;
        }
        public IList<T> Get<T>()
        {
            return Get<T>(int.MaxValue);
        }
        public IList<T> Get<T>(int limit)
        {
            EnsureSearcher();
            var t = new Term(FieldKeys.PuckTypeChain, typeof(T).FullName);
            var q = new TermQuery(t);
            var hits = Searcher.Search(q, limit).ScoreDocs;
            var results = new List<T>();
            for (var i = 0; i < hits.Count(); i++)
            {
                var doc = Searcher.Doc(hits[i].Doc);
                T result = JsonConvert.DeserializeObject<T>(doc.GetValues(FieldKeys.PuckValue)[0]);
                results.Add(result);
            }
            return results;
        }

    }
}
