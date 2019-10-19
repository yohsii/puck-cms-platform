using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using puck.core.Base;
using System.Web;
using System.Threading.Tasks;
using puck.core.Abstract;
using puck.core.Concrete;
using System.Text.RegularExpressions;
using puck.core.Models;
using puck.core.Constants;
using System.Globalization;
using Newtonsoft.Json;
using puck.core.Entities;
using puck.core.Exceptions;
using puck.core.Events;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using StackExchange.Profiling;
using System.Data.SqlClient;
using puck.core.Tasks;
using puck.core.State;
using puck.core.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace puck.core.Services
{
    public class ContentService : I_Content_Service
    {
        private static readonly object _savelck = new object();
        public RoleManager<PuckRole> roleManager { get; set; }
        public UserManager<PuckUser> userManager { get; set; }
        public I_Puck_Repository repo { get; set; }
        public I_Task_Dispatcher tdispatcher { get; set; }
        public I_Content_Indexer indexer { get; set; }
        public I_Log logger { get; set; }
        public IConfiguration config { get; set; }
        private static SemaphoreSlim slock1 = new SemaphoreSlim(1);
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

        private static void DelegateBeforeMoveEvent(Dictionary<string, Tuple<Type, Action<object, BeforeMoveEventArgs>, bool>> list, object n, BeforeMoveEventArgs e)
        {
            var type = e.Nodes.First().GetType();
            //refactor:can probably use is operator to implement event propagation
            var types = ApiHelper.BaseTypes(type);
            types.Add(type);
            list.Where(x => x.Value.Item1 == type || (x.Value.Item3 && types.Contains(x.Value.Item1)))
                .ToList().ForEach(x =>
                {
                    x.Value.Item2(n, e);
                });
        }
        private static void DelegateAfterMoveEvent(Dictionary<string, Tuple<Type, Action<object, MoveEventArgs>, bool>> list, object n, MoveEventArgs e)
        {
            var type = e.Nodes.First().GetType();
            //refactor:can probably use is operator to implement event propagation
            var types = ApiHelper.BaseTypes(type);
            types.Add(type);
            list.Where(x => x.Value.Item1 == type || (x.Value.Item3 && types.Contains(x.Value.Item1)))
                .ToList().ForEach(x =>
                {
                    x.Value.Item2(n, e);
                });
        }

        private static void DelegateBeforeSave(object n, BeforeIndexingEventArgs e)
        {
            DelegateBeforeEvent(BeforeSaveActionList, n, e);
        }
        private static void DelegateAfterSave(object n, IndexingEventArgs e)
        {
            DelegateAfterEvent(AfterSaveActionList, n, e);
        }
        private static void DelegateBeforeDelete(object n, BeforeIndexingEventArgs e)
        {
            DelegateBeforeEvent(BeforeDeleteActionList, n, e);
        }
        private static void DelegateAfterDelete(object n, IndexingEventArgs e)
        {
            DelegateAfterEvent(AfterDeleteActionList, n, e);
        }
        private static void DelegateBeforeMove(object n, BeforeMoveEventArgs e)
        {
            DelegateBeforeMoveEvent(BeforeMoveActionList, n, e);
        }
        private static void DelegateAfterMove(object n, MoveEventArgs e)
        {
            DelegateAfterMoveEvent(AfterMoveActionList, n, e);
        }

        public static Dictionary<string, Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>> BeforeSaveActionList =
            new Dictionary<string, Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>>();

        public static Dictionary<string, Tuple<Type, Action<object, IndexingEventArgs>, bool>> AfterSaveActionList =
            new Dictionary<string, Tuple<Type, Action<object, IndexingEventArgs>, bool>>();

        public static Dictionary<string, Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>> BeforeDeleteActionList =
            new Dictionary<string, Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>>();

        public static Dictionary<string, Tuple<Type, Action<object, IndexingEventArgs>, bool>> AfterDeleteActionList =
            new Dictionary<string, Tuple<Type, Action<object, IndexingEventArgs>, bool>>();

        public static Dictionary<string, Tuple<Type, Action<object, BeforeMoveEventArgs>, bool>> BeforeMoveActionList =
            new Dictionary<string, Tuple<Type, Action<object, BeforeMoveEventArgs>, bool>>();

        public static Dictionary<string, Tuple<Type, Action<object, MoveEventArgs>, bool>> AfterMoveActionList =
            new Dictionary<string, Tuple<Type, Action<object, MoveEventArgs>, bool>>();

        public static event EventHandler<BeforeIndexingEventArgs> BeforeSave;
        public static event EventHandler<IndexingEventArgs> AfterSave;
        public static event EventHandler<BeforeIndexingEventArgs> BeforeDelete;
        public static event EventHandler<IndexingEventArgs> AfterDelete;
        public static event EventHandler<BeforeMoveEventArgs> BeforeMove;
        public static event EventHandler<MoveEventArgs> AfterMove;
        public static event EventHandler<CreateEventArgs> AfterCreate;
        public static void RegisterBeforeSaveHandler<T>(string Name, Action<object, BeforeIndexingEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            BeforeSaveActionList.Add(Name, new Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>(typeof(T), Handler, Propagate));
        }
        public static void RegisterAfterSaveHandler<T>(string Name, Action<object, IndexingEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            AfterSaveActionList.Add(Name, new Tuple<Type, Action<object, IndexingEventArgs>, bool>(typeof(T), Handler, Propagate));
        }
        public static void RegisterBeforeDeleteHandler<T>(string Name, Action<object, BeforeIndexingEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            BeforeDeleteActionList.Add(Name, new Tuple<Type, Action<object, BeforeIndexingEventArgs>, bool>(typeof(T), Handler, Propagate));
        }
        public static void RegisterAfterDeleteHandler<T>(string Name, Action<object, IndexingEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            AfterDeleteActionList.Add(Name, new Tuple<Type, Action<object, IndexingEventArgs>, bool>(typeof(T), Handler, Propagate));
        }
        public static void RegisterBeforeMoveHandler<T>(string Name, Action<object, BeforeMoveEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            BeforeMoveActionList.Add(Name, new Tuple<Type, Action<object, BeforeMoveEventArgs>, bool>(typeof(T), Handler, Propagate));
        }
        public static void RegisterAfterMoveHandler<T>(string Name, Action<object, MoveEventArgs> Handler, bool Propagate = false) where T : BaseModel
        {
            AfterMoveActionList.Add(Name, new Tuple<Type, Action<object, MoveEventArgs>, bool>(typeof(T), Handler, Propagate));
        }

        public static void UnRegisterBeforeSaveHandler(string Name)
        {
            BeforeSaveActionList.Remove(Name);
        }
        public static void UnRegisterAfterSaveHandler(string Name)
        {
            AfterSaveActionList.Remove(Name);
        }
        public static void UnRegisterBeforeDeleteHandler(string Name)
        {
            BeforeDeleteActionList.Remove(Name);
        }
        public static void UnRegisterAfterDeleteHandler(string Name)
        {
            AfterDeleteActionList.Remove(Name);
        }
        public static void UnRegisterBeforeMoveHandler(string Name)
        {
            BeforeMoveActionList.Remove(Name);
        }
        public static void UnRegisterAfterMoveHandler(string Name)
        {
            AfterMoveActionList.Remove(Name);
        }
        public static void OnCreate(object s, CreateEventArgs args)
        {
            if (AfterCreate != null)
                AfterCreate(s, args);
        }
        public static void OnBeforeSave(object s, BeforeIndexingEventArgs args)
        {
            if (BeforeSave != null)
                BeforeSave(s, args);
        }

        public static void OnAfterSave(object s, IndexingEventArgs args)
        {
            if (AfterSave != null)
                AfterSave(s, args);
        }

        public static void OnBeforeDelete(object s, BeforeIndexingEventArgs args)
        {
            if (BeforeDelete != null)
                BeforeDelete(s, args);
        }

        public static void OnAfterDelete(object s, IndexingEventArgs args)
        {
            if (AfterDelete != null)
                AfterDelete(s, args);
        }

        public static void OnBeforeMove(object s, BeforeMoveEventArgs args)
        {
            if (BeforeMove != null)
                BeforeMove(s, args);
        }

        public static void OnAfterMove(object s, MoveEventArgs args)
        {
            if (AfterMove != null)
                AfterMove(s, args);
        }


        static ContentService()
        {
            BeforeSave += new EventHandler<BeforeIndexingEventArgs>(DelegateBeforeSave);
            AfterSave += new EventHandler<IndexingEventArgs>(DelegateAfterSave);
            BeforeDelete += new EventHandler<BeforeIndexingEventArgs>(DelegateBeforeDelete);
            AfterDelete += new EventHandler<IndexingEventArgs>(DelegateAfterDelete);
            BeforeMove += new EventHandler<BeforeMoveEventArgs>(DelegateBeforeMove);
            AfterMove += new EventHandler<MoveEventArgs>(DelegateAfterMove);
        }
        public ContentService(IConfiguration config, RoleManager<PuckRole> RoleManager, UserManager<PuckUser> UserManager, I_Puck_Repository Repo, I_Task_Dispatcher TaskDispatcher, I_Content_Indexer Indexer, I_Log Logger)
        {
            this.roleManager = RoleManager;
            this.userManager = UserManager;
            this.repo = Repo;
            this.tdispatcher = TaskDispatcher;
            this.indexer = Indexer;
            this.logger = Logger;
            this.config = config;
        }

        public void Sort(Guid parentId, List<Guid> ids)
        {
            var qh = new QueryHelper<BaseModel>();
            var indexItems = qh.And().Field(x => x.ParentId, parentId.ToString()).GetAllNoCast();
            var dbItems = repo.CurrentRevisionsByParentId(parentId).ToList();
            indexItems.ForEach(n =>
            {
                for (var i = 0; i < ids.Count; i++)
                {
                    if (ids[i].Equals(n.Id))
                    {
                        n.SortOrder = i;
                    }
                }
            });
            var c = 0;
            var indexItemsNotListed = indexItems.Where(x => !ids.Contains(x.Id)).ToList();
            indexItemsNotListed.ForEach(x => { x.SortOrder = ids.Count + c; c++; });
            dbItems.ForEach(n =>
            {
                for (var i = 0; i < ids.Count; i++)
                {
                    if (ids[i].Equals(n.Id))
                    {
                        if (!n.IsPublishedRevision) {
                            var publishedRevision = repo.PublishedRevision(n.Id, n.Variant);
                            if (publishedRevision != null) {
                                publishedRevision.SortOrder = i;
                            }
                        }
                        n.SortOrder = i;
                    }
                }
            });
            var dbItemsNotListed = dbItems.Where(x => !ids.Contains(x.Id)).ToList();
            dbItemsNotListed.ForEach(x => { x.SortOrder = ids.Count + c; c++; });
            repo.SaveChanges();
            indexer.Index(indexItems);
        }

        public int UpdateDescendantHasNoPublishedRevision(string path, string value, List<string> descendantVariants)
        {
            int rowsAffected = 0;
            
            var sql = $"update PuckRevision set [HasNoPublishedRevision] = {value} where [IdPath] LIKE '{path+"%"}'";
            if (descendantVariants.Any())
            {
                sql += " and (";
                for (var i = 0; i < descendantVariants.Count; i++)
                {
                    var variant = descendantVariants[i];
                    if (i > 0)
                        sql += " or ";
                    sql += $"[Variant] = '{variant}'";
                }
                sql += ")";
            }
            rowsAffected = repo.Context.Database.ExecuteSqlRaw(sql);
            
            return rowsAffected;
        }
        public int UpdateDescendantIsPublishedRevision(string path, string value, bool addWhereIsCurrentClause, List<string> descendantVariants)
        {
            int rowsAffected = 0;
            var sql = $"update PuckRevision set [Published] = {value} , [IsPublishedRevision] = {value} where [IdPath] LIKE '{path+"%"}'";
            if (addWhereIsCurrentClause)
                sql += " and [Current] = 1";
            if (descendantVariants.Any())
            {
                sql += " and (";
                for (var i = 0; i < descendantVariants.Count; i++)
                {
                    var variant = descendantVariants[i];
                    if (i > 0)
                        sql += " or ";
                    sql += $"[Variant] = '{variant}'";
                }
                sql += ")";
            }
            rowsAffected = repo.Context.Database.ExecuteSqlRaw(sql);
            return rowsAffected;
        }
        public async Task Publish(Guid id, string variant, List<string> descendantVariants, string userName = null)
        {
            //await slock1.WaitAsync();
            try
            {
                PuckUser user = null;
                if (!string.IsNullOrEmpty(userName))
                {
                    user = await userManager.FindByNameAsync(userName);
                    if (user == null)
                        throw new UserNotFoundException("there is no user for provided username");
                }
                else
                    userName = HttpContext.Current.User.Identity.Name;

                var currentRevision = repo.CurrentRevision(id, variant);
                if (currentRevision.ParentId != Guid.Empty)
                {
                    var publishedParentRevisions = repo.PublishedRevisions(currentRevision.ParentId);
                    if (publishedParentRevisions.Count() == 0)
                    {
                        throw new Exception("You cannot publish this node because its parent is not published.");
                    }
                }
                var mod = ApiHelper.RevisionToBaseModel(currentRevision);
                mod.Published = true;
                await SaveContent(mod, makeRevision: false, userName: userName);
                var affected = 0;
                string notes = "";
                if (descendantVariants.Any())
                {
                    //set descendants to have HasNoPublishedRevision set to false
                    affected = UpdateDescendantHasNoPublishedRevision(currentRevision.IdPath + ",", "0", descendantVariants);
                    //set descendants to have IsPublishedRevision set to false
                    affected = UpdateDescendantIsPublishedRevision(currentRevision.IdPath + ",", "0", false, descendantVariants);
                    //set current descendants to have IsPublishedRevision set to true, since we're publishing Current descendants
                    affected = UpdateDescendantIsPublishedRevision(currentRevision.IdPath + ",", "1", true, descendantVariants);
                    var descendantVariantsLowerCase = descendantVariants.Select(x => x.ToLower()).ToList();
                    var descendantRevisions = repo.CurrentRevisionDescendants(currentRevision.IdPath).Where(x => descendantVariantsLowerCase.Contains(x.Variant.ToLower())).ToList();
                    var descendantModels = descendantRevisions.Select(x => ApiHelper.RevisionToBaseModel(x)).ToList();
                    AddPublishInstruction(descendantModels);
                    indexer.Index(descendantModels);
                    if (descendantModels.Any())
                        notes = $"{descendantModels.Count} descendant items also published";
                }
                AddAuditEntry(mod.Id, mod.Variant, AuditActions.Publish, notes, userName);
            }
            finally
            {
                //slock1.Release(); 
            }
        }
        public async Task UnPublish(Guid id, string variant, List<string> descendantVariants, string userName = null)
        {
            //await slock1.WaitAsync();
            try
            {
                PuckUser user = null;
                if (!string.IsNullOrEmpty(userName))
                {
                    user = await userManager.FindByNameAsync(userName);
                    if (user == null)
                        throw new UserNotFoundException("there is no user for provided username");
                }
                else
                    userName = HttpContext.Current.User.Identity.Name;

                var toIndex = new List<BaseModel>();
                var currentRevision = repo.CurrentRevision(id, variant);
                var publishedRevision = repo.PublishedRevision(id, variant);
                var mod = ApiHelper.RevisionToBaseModel(currentRevision);
                mod.Published = false;
                await SaveContent(mod, makeRevision: false, userName: userName);
                toIndex.Add(mod);
                var publishedVariants = repo.PublishedRevisionVariants(id, variant).ToList();
                var affected = 0;
                if (publishedVariants.Count() == 0)
                {
                    if (!currentRevision.IsPublishedRevision && publishedRevision != null && !currentRevision.Path.ToLower().Equals(publishedRevision.Path.ToLower()))
                    {
                        //since we're unpublishing the published revision (which descendant paths are based on), we should set descendant paths to be based off of the current revision
                        affected = UpdateDescendantPaths(publishedRevision.Path + "/", currentRevision.Path + "/");
                        UpdatePathRelatedMeta(publishedRevision.Path, currentRevision.Path);
                    }
                }
                var notes = "";
                if (descendantVariants.Any())
                {
                    //set descendants to have HasNoPublishedRevision set to true
                    affected = UpdateDescendantHasNoPublishedRevision(currentRevision.IdPath + ",", "1", descendantVariants);
                    //set descendants to have IsPublishedRevision set to false
                    affected = UpdateDescendantIsPublishedRevision(currentRevision.IdPath + ",", "0", false, descendantVariants);
                    var descendantVariantsLowerCase = descendantVariants.Select(x => x.ToLower()).ToList();
                    var descendantRevisions = repo.CurrentRevisionDescendants(currentRevision.IdPath).Where(x => descendantVariantsLowerCase.Contains(x.Variant.ToLower())).ToList();
                    var descendantModels = descendantRevisions.Select(x => ApiHelper.RevisionToBaseModel(x)).ToList();
                    toIndex.AddRange(descendantModels);
                    if (descendantModels.Any())
                        notes = $"{descendantModels.Count} descendant items also unpublished";
                }
                AddPublishInstruction(toIndex);
                indexer.Index(toIndex);
                AddAuditEntry(mod.Id, mod.Variant, AuditActions.Unpublish, notes, userName);
            }
            finally
            {
                //slock1.Release();
            }
        }
        public void AddPublishInstruction(List<BaseModel> toIndex)
        {
            if (toIndex.Count > 0)
            {
                var instruction = new PuckInstruction() { InstructionKey = InstructionKeys.Publish, Count = toIndex.Count, ServerName = ApiHelper.ServerName() };
                string instructionDetail = "";
                toIndex.ForEach(x => instructionDetail += $"{x.Id.ToString()}:{x.Variant},");
                instructionDetail = instructionDetail.TrimEnd(',');
                instruction.InstructionDetail = instructionDetail;
                repo.AddPuckInstruction(instruction);
                repo.SaveChanges();
            }
        }
        public void Publish(Guid id, string variant, List<string> descendants, bool publish)
        {
            //get items to delete
            var repoQ = repo.GetPuckRevision().Where(x => x.Id == id && x.Current);
            if (!string.IsNullOrEmpty(variant))
                repoQ = repoQ.Where(x => x.Variant.ToLower().Equals(variant.ToLower()));
            //return as models
            var repoItems = repoQ.ToList().Select(x =>
            {
                var mod = x.ToBaseModel();
                mod.Published = publish;
                x.Published = publish;
                x.Value = JsonConvert.SerializeObject(mod);
                return mod;
            }).ToList();

            if (repoItems.Count == 0)
                throw new Exception("no results with ID:" + id + " Variant:" + variant + " to publish");

            var parentVariants = repo.CurrentRevisionParent(repoItems.First().Path).ToList();
            //can't publish if parent not published unless root item
            if (parentVariants.Count == 0 && repoItems.First().Path.Count(c => c == '/') > 1)
                throw new NoParentExistsException("you cannot publish an item if it has no parent, unless it's a root node");

            if (descendants.Count > 0)
                repoItems.AddRange(
                    repo.CurrentRevisionDescendants(repoItems.First().Path).Where(x => descendants.Contains(x.Variant.ToLower())).ToList().Select(x =>
                    {
                        var mod = x.ToBaseModel();
                        mod.Published = publish;
                        x.Published = publish;
                        x.Value = JsonConvert.SerializeObject(mod);
                        return mod;
                    }).ToList());
            repo.SaveChanges();
            indexer.Index(repoItems);
        }
        public int DeleteRevisions(List<Guid> ids,int step=100) {
            int affected = 0;
            if (ids.Count == 0) return affected;
            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                int skip = 0;
                int take = step;
                var toDelete = ids.Skip(skip).Take(take);
                con.Open();
                while (toDelete.Count() > 0) {
                    var sql = "delete from PuckRevision where id in(";
                    foreach (var id in toDelete) { 
                        sql += $"'{id.ToString()}',";
                    }
                    sql = sql.TrimEnd(',');
                    sql +=")";
                    var com = new SqlCommand(sql, con);
                    affected += com.ExecuteNonQuery();
                    skip += take;
                    toDelete = ids.Skip(skip).Take(take);
                }
            }
            return affected;
        }
        public async Task Delete(Guid id, string variant = null, string userName = null)
        {
            PuckUser user = null;
            if (!string.IsNullOrEmpty(userName))
            {
                user = await userManager.FindByNameAsync(userName);
                if (user == null)
                    throw new UserNotFoundException("there is no user for provided username");
            }
            else
                userName = HttpContext.Current.User.Identity.Name;
            string notes = "";
            //remove from index
            var qh = new QueryHelper<BaseModel>(prependTypeTerm:false);
            qh.ID(id);
            if (!string.IsNullOrEmpty(variant))
                qh.And().Field(x => x.Variant, variant);
            var toDelete = qh.GetAll();
            var addDescendants = false;
            var variants = new List<BaseModel>();
            if (toDelete.Count > 0)
            {
                variants = toDelete.First().Variants<BaseModel>();
                if (variants.Count == 0 || string.IsNullOrEmpty(variant))
                {
                    addDescendants = true;
                    //var descendants = toDelete.First().Descendants<BaseModel>();
                    //toDelete.AddRange(descendants);
                }
            }
            var deleteQuery = new QueryHelper<BaseModel>(prependTypeTerm: false);
            var innerQ = deleteQuery.New().ID(id);
            if (!string.IsNullOrEmpty(variant))
                innerQ.And().Field(x => x.Variant, variant);
            deleteQuery.Group(
                innerQ
            );
            if (addDescendants)
                deleteQuery.Descendants(toDelete.First().Path, must: false);
            Guid? ParentId = null;
            //indexer.Delete(deleteQuery.ToString());
            var cancelled = new List<BaseModel>();
            //remove from repo
            var repoItemsQ = repo.GetPuckRevision().Where(x => x.Id == id && x.Current);
            if (!string.IsNullOrEmpty(variant))
                repoItemsQ = repoItemsQ.Where(x => x.Variant.ToLower().Equals(variant.ToLower()));
            var repoItems = repoItemsQ.ToList();
            ParentId = repoItems.FirstOrDefault()?.ParentId;
            var repoVariants = new List<PuckRevision>();
            var descendants = new List<PuckRevision>();
            if (repoItems.Count > 0)
            {
                repoVariants = repo.CurrentRevisionVariants(repoItems.First().Id, repoItems.First().Variant).ToList();
                if (repoVariants.Count == 0 || string.IsNullOrEmpty(variant))
                {
                    descendants = repo.CurrentRevisionDescendants(repoItems.First().IdPath).ToList();
                    repoItems.AddRange(descendants);
                    if (descendants.Any())
                        notes = $"{descendants.Count} descendant items also deleted";
                }
            }
            repoItems.ForEach(x =>
            {
                var args = new BeforeIndexingEventArgs() { Node = x.ToBaseModel(), Cancel = false };
                OnBeforeDelete(this, args);
                if (args.Cancel)
                {
                    cancelled.Add(x);
                    return;
                }
                toDelete.Add(args.Node);
                repo.DeleteRevision(x);
            });
            
            //deletes only happening on current revisions. delete all revisions. this is too costly to do with EF for descendants, we'll handle descendants using sql
            if (!string.IsNullOrEmpty(variant))
            {
                var itemToDelete = toDelete.FirstOrDefault(x => x.Id == id && x.Variant.ToLower().Equals(variant.ToLower()));
                if (itemToDelete != null) {
                    repo.GetPuckRevision().Where(x=>x.Id==id &&x.Variant.ToLower().Equals(variant.ToLower())).ToList().ForEach(x=>repo.DeleteRevision(x));
                }
            }
            else {
                var itemsToDelete = toDelete.Where(x => x.Id == id).ToList();
                foreach (var item in itemsToDelete) {
                    repo.GetPuckRevision().Where(x => x.Id == item.Id && x.Variant.ToLower().Equals(item.Variant.ToLower())).ToList().ForEach(x => repo.DeleteRevision(x));
                }
            }
            repo.SaveChanges();
            indexer.Delete(toDelete);
            DeleteRevisions(descendants.Select(x=>x.Id).ToList());
            repoItems
                    .Where(x => !cancelled.Contains(x))
                    .ToList()
                    .ForEach(x => { OnAfterDelete(this, new IndexingEventArgs() { Node = x }); });

            //remove localisation setting
            string lookUpPath = string.Empty;
            if (repoItems.Any())
                lookUpPath = repoItems.First().Path;
            else if (toDelete.Any())
                lookUpPath = toDelete.First().Path;

            if (!string.IsNullOrEmpty(lookUpPath))
            {
                var lmeta = new List<PuckMeta>();
                var dmeta = new List<PuckMeta>();
                var cmeta = new List<PuckMeta>();
                var nmeta = new List<PuckMeta>();
                //if descendants are being deleted - descendants are included if there are no variants for the deleted node (therefore orphaning descendants) or if variant argument is not present (which means you wan't all variants deleted)
                if (repoVariants.Any() && !string.IsNullOrEmpty(variant))
                {
                    //lmeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.PathToLocale && x.Key.ToLower().Equals(lookUpPath.ToLower())).ToList();
                    //dmeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.DomainMapping && x.Key.ToLower().Equals(lookUpPath.ToLower())).ToList();
                    //cmeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.CacheExclude && x.Key.ToLower().Equals(lookUpPath.ToLower())).ToList();
                }
                else
                {
                    lmeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.PathToLocale && x.Key.ToLower().StartsWith(lookUpPath.ToLower())).ToList();
                    dmeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.DomainMapping && x.Key.ToLower().StartsWith(lookUpPath.ToLower())).ToList();
                    cmeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.CacheExclude && x.Key.ToLower().StartsWith(lookUpPath.ToLower())).ToList();
                    nmeta = repo.GetPuckMeta().Where(x => x.Name.StartsWith(DBNames.Notify) && (
                         x.Key.ToLower().Equals(lookUpPath.ToLower())
                        || (lookUpPath.ToLower().StartsWith(x.Key.ToLower()) && x.Name.Contains(":*:"))
                        )).ToList();
                }
                lmeta.ForEach(x => { repo.DeleteMeta(x); });
                dmeta.ForEach(x => { repo.DeleteMeta(x); });
                cmeta.ForEach(x => { repo.DeleteMeta(x); });
                nmeta.ForEach(x => { repo.DeleteMeta(x); });
            }
            repo.SaveChanges();
            StateHelper.UpdateDomainMappings(true);
            StateHelper.UpdatePathLocaleMappings(true);
            
            var instruction = new PuckInstruction() { InstructionKey = InstructionKeys.Delete, Count = 1, ServerName = ApiHelper.ServerName() };
            instruction.InstructionDetail = deleteQuery.ToString();
            repo.AddPuckInstruction(instruction);
            
            repo.SaveChanges();
            AddAuditEntry(id, variant ?? "", AuditActions.Delete, notes, userName);
            var hasChildren = repo.GetPuckRevision().Count(x=>x.ParentId==ParentId && x.Current)>0;
            var parentRevisions = repo.GetPuckRevision().Where(x => x.Id == ParentId && x.Current).ToList();
            parentRevisions.ForEach(x=>x.HasChildren=hasChildren);
            repo.SaveChanges();
        }
        public string GetLiveOrCurrentPath(Guid id)
        {
            var node = repo.GetPuckRevision()
                .Where(x => x.Id == id && ((x.HasNoPublishedRevision && x.Current) || x.IsPublishedRevision)).OrderByDescending(x => x.Updated).FirstOrDefault();
            return node?.Path;
        }
        public async Task<T> Create<T>(Guid parentId, string variant, string name, string template = null, bool published = false, string userName = null) where T : BaseModel
        {
            var instance = (T)ApiHelper.CreateInstance(typeof(T));
            if (parentId != Guid.Empty)
            {
                var parent = repo.GetPuckRevision().FirstOrDefault(x => x.Id == parentId && x.Current);
                if (parent == null)
                    throw new Exception("could not find parent node");
                var slug = ApiHelper.Slugify(name);
                instance.Path = ""; //$"{parent.Path}/";
                instance.ParentId = parentId;
            }
            else
                instance.Path = ""; //$"/";
            if (!string.IsNullOrEmpty(template))
                instance.TemplatePath = template;
            else
            {
                var meta = repo.GetPuckMeta()
                    .Where(x => x.Name == DBNames.TypeAllowedTemplates && x.Key == typeof(T).Name)
                    .ToList();
                if (meta == null || meta.Count == 0)
                {
                    throw new Exception($"you've not specified a template parameter. tried to pick one from allowable templates (set in settings section) but none have been set for type:{ApiHelper.FriendlyClassName(typeof(T))}");
                }
                instance.TemplatePath = meta.FirstOrDefault().Value;
            }
            instance.NodeName = name;
            instance.Variant = variant;
            instance.TypeChain = ApiHelper.TypeChain(typeof(T));
            instance.Type = typeof(T).Name;
            if (string.IsNullOrEmpty(userName))
            {
                instance.CreatedBy = HttpContext.Current.User.Identity.Name;
            }
            else
            {
                var user = await userManager.FindByNameAsync(userName);
                if (user == null) throw new UserNotFoundException("there is no user for provided username");
                instance.CreatedBy = userName;
            }

            instance.LastEditedBy = instance.CreatedBy;
            instance.Published = published;
            return instance;
        }
        public string GetIdPath(BaseModel mod)
        {
            if (mod.ParentId == Guid.Empty)
            {
                return mod.Id.ToString();
            }
            var chain = new List<string>();
            chain.Add(mod.Id.ToString());
            var currentRevision = repo.GetPuckRevision().FirstOrDefault(x => x.Id == mod.ParentId && x.Current);
            chain.Add(currentRevision.Id.ToString());
            while (currentRevision.ParentId != Guid.Empty)
            {
                currentRevision = repo.GetPuckRevision().FirstOrDefault(x => x.Id == currentRevision.ParentId && x.Current);
                chain.Add(currentRevision.Id.ToString());
            }
            chain.Reverse();
            var result = string.Join(",", chain);
            return result;
        }
        public int UpdateDescendantPaths(string oldPath, string newPath)
        {
            int rowsAffected = 0;
            
            var sql = $"update PuckRevision set [Path] = '{newPath}' + SUBSTRING([Path], LEN('{oldPath}')+1,8000) where [Path] LIKE '{oldPath+"%"}'";
            rowsAffected = repo.Context.Database.ExecuteSqlRaw(sql);
            return rowsAffected;
        }
        public int UpdateDescendantIdPaths(string oldPath, string newPath)
        {
            int rowsAffected = 0;
            var sql = $"update PuckRevision set [IdPath] = '{newPath}' + SUBSTRING([IdPath], LEN('{oldPath}')+1,8000) where [IdPath] LIKE '{oldPath+"%"}'";
            rowsAffected = repo.Context.Database.ExecuteSqlRaw(sql);
            return rowsAffected;
        }
        public void UpdatePathRelatedMeta(string oldPath, string newPath)
        {
            var regex = new Regex(Regex.Escape(oldPath), RegexOptions.Compiled);

            var lmeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.PathToLocale && x.Key.ToLower().Equals(oldPath.ToLower())).ToList();
            lmeta.ForEach(x => x.Key = newPath);
            var lmetaD = repo.GetPuckMeta().Where(x => x.Name == DBNames.PathToLocale && x.Key.ToLower().StartsWith(oldPath.ToLower() + "/")).ToList();
            lmetaD.ForEach(x => x.Key = regex.Replace(x.Key, newPath, 1));

            var dmeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.DomainMapping && x.Key.ToLower().Equals(oldPath.ToLower())).ToList();
            dmeta.ForEach(x => x.Key = newPath);

            var cmeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.CacheExclude && x.Key.ToLower().Equals(oldPath.ToLower())).ToList();
            cmeta.ForEach(x => x.Key = newPath);
            var cmetaD = repo.GetPuckMeta().Where(x => x.Name == DBNames.CacheExclude && x.Key.ToLower().StartsWith(oldPath.ToLower() + "/")).ToList();
            cmetaD.ForEach(x => x.Key = regex.Replace(x.Key, newPath, 1));

            var nmeta = repo.GetPuckMeta().Where(x => x.Name.StartsWith(DBNames.Notify) && x.Key.ToLower().Equals(oldPath.ToLower())).ToList();
            nmeta.ForEach(x => x.Key = newPath);
            var nmetaD = repo.GetPuckMeta().Where(x => x.Name.StartsWith(DBNames.Notify) && x.Key.ToLower().StartsWith(oldPath.ToLower() + "/")).ToList();
            nmetaD.ForEach(x => x.Key = regex.Replace(x.Key, newPath, 1));

            repo.SaveChanges();

        }
        public async Task<List<BaseModel>> SaveContent<T>(T mod, bool makeRevision = true, string userName = null, bool handleNodeNameExists = true, int nodeNameExistsCounter = 0,bool triggerEvents=true,bool triggerIndexEvents=true,bool shouldIndex=true) where T : BaseModel
        {
            await slock1.WaitAsync();
            try
            {
                PuckUser user = null;
                if (!string.IsNullOrEmpty(userName))
                {
                    user = await userManager.FindByNameAsync(userName);
                    if (user == null)
                        throw new UserNotFoundException("there is no user for provided username");
                }
                await ObjectDumper.Transform(mod, int.MaxValue);

                //get sibling nodes
                //var nodeDirectory = mod.Path.Substring(0, mod.Path.LastIndexOf('/') + 1);

                var nodesAtPath = repo.CurrentRevisionsByParentId(mod.ParentId).Where(x => x.Id != mod.Id);
                    
                //check node name is unique at path
                if (nodesAtPath.Any(x => x.NodeName.ToLower().Equals(mod.NodeName.ToLower())))
                {
                    if (handleNodeNameExists)
                    {
                        if (nodeNameExistsCounter == 0)
                        {
                            mod.NodeName = mod.NodeName + " (1)";
                        }
                        else
                        {
                            var regex = new Regex(@"\(\d+\)$");
                            var newName = regex.Replace(mod.NodeName, $"({nodeNameExistsCounter + 1})");
                            mod.NodeName = newName;
                        }
                        return await SaveContent(mod, makeRevision: makeRevision, userName: userName, handleNodeNameExists: handleNodeNameExists, nodeNameExistsCounter: nodeNameExistsCounter + 1,triggerEvents:triggerEvents,triggerIndexEvents:triggerIndexEvents,shouldIndex:shouldIndex);
                    }
                    else
                    {
                        throw new NodeNameExistsException($"Nodename:{mod.NodeName} already exists with same parent id:{mod.ParentId}, choose another nodename.");
                    }
                }
                //set sort order for new content
                if (mod.SortOrder == -1)
                    mod.SortOrder = nodesAtPath.Count();
                if (triggerEvents)
                {
                    var beforeArgs = new BeforeIndexingEventArgs { Node = mod };
                    OnBeforeSave(this, beforeArgs);
                    if (beforeArgs.Cancel)
                        throw new SaveCancelledException("Saving was cancelled by a custom event handler");
                }
                var revisions = repo.GetPuckRevision().Where(x => x.Id.Equals(mod.Id) && x.Variant.ToLower().Equals(mod.Variant.ToLower()));
                if (makeRevision)
                {
                    if (revisions.Count() == 0)
                        mod.Revision = 1;
                    else
                        mod.Revision = revisions.Max(x => x.Revision) + 1;
                }
                mod.Updated = DateTime.Now;
                //get parent check published
                var parentVariants = repo.GetPuckRevision().Where(x => x.Id == mod.ParentId && (x.IsPublishedRevision || (x.Current && x.HasNoPublishedRevision)));
                var parentVariantsCount = parentVariants.Count();
                if (mod.ParentId != Guid.Empty && parentVariantsCount == 0)
                    throw new NoParentExistsException("this is not a root node yet doesn't have a parent");
                var hasPublishedParent = parentVariants.Any(x=>x.Published);
                //if (!hasPublishedParent) {
                //    hasPublishedParent = repo.PublishedRevisions(mod.ParentId).Count()>0;
                //}
                //can't publish if parent not published
                //var publishedParentVariants = repo.PublishedRevisions(mod.ParentId).ToList();
                if (mod.ParentId != Guid.Empty && !hasPublishedParent)//!parentVariants.Any(x => x.Published /*&& x.Variant.ToLower().Equals(mod.Variant.ToLower())*/))
                    mod.Published = false;

                //check this is an update or create
                var original = repo.CurrentRevision(mod.Id, mod.Variant);
                var publishedRevision = repo.PublishedRevision(mod.Id, mod.Variant);
                //could be published revision, or even a published variant
                var publishedRevisionOrVariant = repo.PublishedRevisions(mod.Id).OrderByDescending(x => x.Updated).FirstOrDefault();
                var toIndex = new List<BaseModel>();
                //toIndex.Add(mod);
                bool nameChanged = false;
                bool nameDifferentThanCurrent = false;
                bool parentChanged = false;
                string currentRevisionPath = string.Empty;
                bool nameDifferentThanPublished = false;
                string publishedRevisionPath = string.Empty;
                string originalPath = string.Empty;
                bool publishedRevisionRepublished = false;
                if (original != null)
                {//this must be an edit
                 //if (!original.NodeName.ToLower().Equals(mod.NodeName.ToLower()))
                    if (original.ParentId != mod.ParentId)
                    {
                        parentChanged = true;
                    }
                    if (!original.Path.ToLower().Equals(mod.Path.ToLower())
                        || !original.NodeName.ToLower().Equals(mod.NodeName.ToLower())
                        || parentChanged)
                    {
                        nameChanged = true;
                        nameDifferentThanCurrent = true;
                        currentRevisionPath = original.Path;
                        originalPath = original.Path;
                    }
                }
                if (publishedRevisionOrVariant != null)
                {
                    if (publishedRevisionOrVariant.ParentId != mod.ParentId)
                    {
                        parentChanged = true;
                    }
                    //if (!original.NodeName.ToLower().Equals(mod.NodeName.ToLower()))
                    if (!publishedRevisionOrVariant.Path.ToLower().Equals(mod.Path.ToLower())
                        || !publishedRevisionOrVariant.NodeName.ToLower().Equals(mod.NodeName.ToLower())
                        || parentChanged)
                    {
                        nameChanged = true;
                        nameDifferentThanPublished = true;
                        publishedRevisionPath = publishedRevisionOrVariant.Path;
                        originalPath = original.Path;
                    }
                }
                var idPath = "";// = original?.IdPath ?? GetIdPath(mod);
                if (original == null || nameChanged || parentChanged)
                    idPath = GetIdPath(mod);
                else {
                    idPath = original.IdPath;
                }
                var currentVariantsDb = repo.CurrentRevisionVariants(mod.Id, mod.Variant).ToList();
                var publishedVariantsDb = repo.PublishedRevisionVariants(mod.Id, mod.Variant).ToList();
                var hasNoPublishedVariants = publishedVariantsDb.Count == 0;
                //if (variantsDb.Any(x => !x.NodeName.ToLower().Equals(mod.NodeName.ToLower())))
                if (currentVariantsDb.Any(x => x.ParentId != mod.ParentId))
                {//update parentId of variants
                    currentVariantsDb.ForEach(x => { x.ParentId = mod.ParentId; x.IdPath = idPath; });
                }
                if (publishedVariantsDb.Any(x => x.ParentId != mod.ParentId))
                {//update parentId of variants
                    publishedVariantsDb.ForEach(x => { x.ParentId = mod.ParentId; x.IdPath = idPath; });
                }
                bool updateCurrentVariantsDb = false;
                bool updatePublishedVariantsDb = false;
                if (!mod.Published)
                {
                    if (currentVariantsDb.Where(x => !x.Published).Any(x => !x.NodeName.ToLower().Equals(mod.NodeName.ToLower())))
                    {//update path of variants
                        nameChanged = true;
                        if (string.IsNullOrEmpty(originalPath))
                            originalPath = currentVariantsDb.First().Path;
                        updateCurrentVariantsDb = true;
                    }
                }
                else
                {
                    if (publishedVariantsDb.Any(x => !x.NodeName.ToLower().Equals(mod.NodeName.ToLower())))
                    {//update path of published variants
                        nameChanged = true;
                        if (string.IsNullOrEmpty(originalPath))
                            originalPath = publishedVariantsDb.First().Path;
                        updatePublishedVariantsDb = true;
                    }
                }
                var pAffected = 0;
                var affected = 0;
                if (nameChanged || parentChanged || string.IsNullOrEmpty(mod.Path))
                {
                    if (mod.ParentId == Guid.Empty)
                    {
                        mod.Path = "/" + ApiHelper.Slugify(mod.NodeName);
                    }
                    else
                    {
                        var parentPath = GetLiveOrCurrentPath(mod.ParentId);
                        mod.Path = $"{parentPath}/{ApiHelper.Slugify(mod.NodeName)}";
                    }
                }
                if (parentChanged)
                {
                    pAffected = UpdateDescendantIdPaths(original.IdPath, idPath);
                    if (!string.IsNullOrEmpty(publishedRevisionPath))
                    {
                        //update descendant paths(publishedRevisionPath)
                        affected = UpdateDescendantPaths(publishedRevisionPath + "/", mod.Path + "/");
                        UpdatePathRelatedMeta(publishedRevisionPath, mod.Path);
                    }
                    else
                    {
                        //update descendant paths(currentRevisionPath)
                        affected = UpdateDescendantPaths(currentRevisionPath + "/", mod.Path + "/");
                        UpdatePathRelatedMeta(currentRevisionPath, mod.Path);
                    }
                    var descendants = repo.CurrentOrPublishedDescendants(idPath).ToList().Select(x => ApiHelper.RevisionToBaseModel(x)).ToList();
                    var variantsToIndex = new List<BaseModel>();
                    variantsToIndex.AddRange(currentVariantsDb.Select(x => ApiHelper.RevisionToBaseModel(x)).ToList());
                    variantsToIndex.AddRange(publishedVariantsDb.Select(x => ApiHelper.RevisionToBaseModel(x)).ToList());
                    toIndex.AddRange(descendants);
                    toIndex.AddRange(variantsToIndex);
                    //if current model is not set to publish, update the currently published revision to reflect parent id being changed
                    if (publishedRevision != null && !mod.Published)
                    {
                        publishedRevision.IdPath = idPath;
                        publishedRevision.Path = mod.Path;
                        toIndex.Add(publishedRevision.ToBaseModel());
                    }
                    else
                        toIndex.Add(mod);
                }
                else
                {
                    if (original != null && original.HasNoPublishedRevision && hasNoPublishedVariants && !mod.Published && nameDifferentThanCurrent)
                    {
                        //update descendant paths
                        affected = UpdateDescendantPaths(original.Path + "/", mod.Path + "/");
                        UpdatePathRelatedMeta(original.Path, mod.Path);
                    }
                    if (mod.Published && (nameDifferentThanCurrent || nameDifferentThanPublished))
                    {
                        if (!string.IsNullOrEmpty(publishedRevisionPath))
                        {
                            //update descendant paths(publishedRevisionPath)
                            affected = UpdateDescendantPaths(publishedRevisionPath + "/", mod.Path + "/");
                            UpdatePathRelatedMeta(publishedRevisionPath, mod.Path);
                        }
                        else
                        {
                            //update descendant paths(currentRevisionPath)
                            affected = UpdateDescendantPaths(currentRevisionPath + "/", mod.Path + "/");
                            UpdatePathRelatedMeta(currentRevisionPath, mod.Path);
                        }
                    }
                }
                if (updateCurrentVariantsDb)
                    currentVariantsDb.Where(x => !x.Published).ToList().ForEach(x => { x.NodeName = mod.NodeName; x.Path = mod.Path; });
                if (updatePublishedVariantsDb)
                    publishedVariantsDb.ToList().ForEach(x => { x.NodeName = mod.NodeName; x.Path = mod.Path; });

                /*if (nameChanged)
                {
                    var regex = new Regex(Regex.Escape(originalPath), RegexOptions.Compiled);
                    //update path of decendants
                    var descendantsDb = repo.CurrentRevisionDescendants(originalPath).ToList();
                    descendantsDb.ForEach(x => { x.Path = regex.Replace(x.Path, mod.Path, 1); });
                    repo.GetPuckMeta().Where(x => x.Name.StartsWith(DBNames.Notify))
                            .Where(x => x.Key.ToLower().Equals(originalPath.ToLower())
                            || originalPath.ToLower().StartsWith(x.Key.ToLower()) && x.Name.Contains(":*:"))
                            .ToList()
                            .ForEach(x => x.Key = mod.Path);
                }
                */
                string username = string.Empty;
                if (user == null)
                    username = HttpContext.Current.User.Identity.Name;
                else
                    username = user.UserName;

                //add revision
                PuckRevision revision;
                if (makeRevision)
                {
                    revision = new PuckRevision();
                    //revisions
                    //    .ForEach(x => x.Current = false);
                    if (original != null)
                        original.Current = false;
                    repo.AddRevision(revision);
                }
                else
                {
                    revision = original;
                    if (revision == null)
                    {
                        revision = new PuckRevision();
                        repo.AddRevision(revision);
                    }
                }
                revision.IdPath = idPath;
                revision.LastEditedBy = username;
                revision.CreatedBy = mod.CreatedBy;
                revision.Created = mod.Created;
                revision.Id = mod.Id;
                revision.NodeName = mod.NodeName;
                revision.Path = mod.Path;
                revision.Published = mod.Published;
                revision.Revision = mod.Revision;
                revision.SortOrder = mod.SortOrder;
                revision.TemplatePath = mod.TemplatePath;
                revision.Type = mod.Type;
                revision.TypeChain = mod.TypeChain;
                revision.Updated = mod.Updated;
                revision.Variant = mod.Variant;
                revision.Current = true;
                revision.ParentId = mod.ParentId;
                revision.Value = JsonConvert.SerializeObject(mod);
                if (!makeRevision && !revision.Published) {
                    if (revision.IsPublishedRevision) { 
                        revision.IsPublishedRevision = false;
                        revision.HasNoPublishedRevision = true;
                        //revisions.ForEach(x => x.HasNoPublishedRevision = true);
                        int _affected = UpdateHasNoPublishedRevisionAndIsPublishedRevision(mod.Id, mod.Variant, true, null);    
                        indexer.Delete(mod);
                    }
                }
                else if (mod.Published)
                {
                    //if published, set the currently published revision. this requires unsetting any previously set publishedrevision flag
                    //revisions.ForEach(x => x.IsPublishedRevision = false);
                    revision.IsPublishedRevision = true;
                    //if this revision or any previous revisions have a published revision, HasNoPublishedRevision must be false
                    revision.HasNoPublishedRevision = false;
                    //revisions.ForEach(x => x.HasNoPublishedRevision = false);
                    int? isPublishedIgnoreRevisionId = makeRevision ? (int?)null : revision.RevisionID;
                    int _affected = UpdateHasNoPublishedRevisionAndIsPublishedRevision(mod.Id, mod.Variant, false, false,isPublishedRevisionIgnoreRevisionId:isPublishedIgnoreRevisionId);
                }
                else if (publishedRevision == null)
                {
                    revision.HasNoPublishedRevision = true;
                }
                
                ////if published, set the currently published revision. this requires unsetting any previously set publishedrevision flag
                //if (mod.Published)
                //{
                //    revisions.ForEach(x => x.IsPublishedRevision = false);
                //    revision.IsPublishedRevision = true;
                //}
                ////if this revision or any previous revisions have a published revision, HasNoPublishedRevision must be false
                //if (revision.IsPublishedRevision || revisions.Any(x => x.IsPublishedRevision))
                //{
                //    revision.HasNoPublishedRevision = false;
                //    revisions.ForEach(x => x.HasNoPublishedRevision = false);
                //}
                //else
                //{//current revision or previous revisions don't have have IsPublishedRevision set so this must mean there is no published revision
                //    revision.HasNoPublishedRevision = true;
                //    revisions.ForEach(x => x.HasNoPublishedRevision = true);
                //}
                //prune old revisions
                revisions.OrderByDescending(x => x.RevisionID).Skip(PuckCache.MaxRevisions).ToList().ForEach(x=>repo.DeleteRevision(x));
                var shouldUpdateDomainMappings = false;
                var shouldUpdatePathLocaleMappings = false;
                //if first time node saved and is root node - set locale for path
                if (currentVariantsDb.Count == 0 && (original == null) && mod.ParentId == Guid.Empty)
                {
                    var lMeta = new PuckMeta()
                    {
                        Name = DBNames.PathToLocale,
                        Key = mod.Path,
                        Value = mod.Variant
                    };
                    repo.AddMeta(lMeta);
                    //if first item - set wildcard domain mapping
                    if (nodesAtPath.Count() == 0)
                    {
                        var dMeta = new PuckMeta()
                        {
                            Name = DBNames.DomainMapping,
                            Key = mod.Path,
                            Value = "*"
                        };
                        repo.AddMeta(dMeta);
                    }
                    shouldUpdatePathLocaleMappings = true;
                    shouldUpdateDomainMappings = true;
                }
                var hasChildren = repo.GetPuckRevision().Count(x => x.ParentId.Equals(mod.Id) && x.Current)>0;
                revision.HasChildren = hasChildren;
                if (currentVariantsDb.Any(x => x.HasChildren != hasChildren))
                    currentVariantsDb.ForEach(x=>x.HasChildren=hasChildren);
                if (parentVariants.Any(x => !x.HasChildren))
                    parentVariants.ToList().ForEach(x => x.HasChildren = true);

                repo.SaveChanges();
                
                //index related operations
                var qh = new QueryHelper<BaseModel>();
                //get current indexed node with same ID and VARIANT
                var currentMod = qh.And().Field(x => x.Variant, mod.Variant)
                    .ID(mod.Id)
                    .Get();
                //if parent changed, we index regardless of if the model being saved is set to publish or not. moves are always published immediately
                if (parentChanged)
                {
                    if(shouldIndex)
                        indexer.Index(toIndex,triggerEvents:triggerIndexEvents);
                }
                else if (mod.Published || currentMod == null)//add to lucene index if published or no such node exists in index
                                                             /*note that you can only have one node with particular id/variant in index at any one time
                                                             * the reason that you want to add node to index when it's not published but there is no such node currently in index
                                                             * is to make sure there is always at least one version of the node in the index for back office search operations
                                                             */
                {
                    toIndex.Add(mod);
                    var changed = false;
                    var indexOriginalPath = string.Empty;
                    //if node exists in index
                    if (currentMod != null)
                    {
                        //and that node currently has a different path than the node we're indexing to replace it
                        if (!mod.Path.ToLower().Equals(currentMod.Path.ToLower()))
                        {
                            //means we have changed the path - by changing the nodename
                            changed = true;
                            //set the original path so we can use it for regex replace operation for changing descendants who will otherwise have incorrect paths
                            indexOriginalPath = currentMod.Path;
                        }
                    }
                    //get nodes currently indexed which have the same ID but different VARIANT
                    var variants = mod.Variants<BaseModel>(noCast: true);
                    if (variants.Any(x => x.ParentId != mod.ParentId))
                    {
                        variants.ForEach(x => { x.ParentId = mod.ParentId; toIndex.Add(x); });
                    }
                    //if any of the variants have different path to the current node
                    if (variants.Any(x => !x.Path.ToLower().Equals(mod.Path.ToLower())))
                    {
                        //means we have changed the path - by changing the nodename
                        changed = true;
                        //if the original path hasn't been set already, set it for use in a regex replace operation
                        if (string.IsNullOrEmpty(indexOriginalPath))
                            indexOriginalPath = variants.First().Path;
                    }
                    //if there was a change in the path
                    if (changed && mod.Published)
                    {
                        //sync up all the variants so they have the same nodename and path
                        variants.ForEach(x =>
                        {
                            x.NodeName = mod.NodeName; x.Path = mod.Path;
                            if (!toIndex.Contains(x))
                                toIndex.Add(x);
                        });
                        //new regex which searches for the current indexed path so it can be replaced with the new one
                        var regex = new Regex(Regex.Escape(indexOriginalPath), RegexOptions.Compiled);
                        var descendants = new List<BaseModel>();
                        //get descendants - either from currently indexed version of the node we're currently saving (which may be new variant and so not currently indexed) or from its variants.
                        if (currentMod != null)
                            descendants = currentMod.Descendants<BaseModel>(currentLanguage: false, noCast: true);
                        else if (variants.Any())
                            descendants = variants.First().Descendants<BaseModel>(currentLanguage: false, noCast: true);
                        //replace portion of path that has changed
                        descendants.ForEach(x => { x.Path = regex.Replace(x.Path, mod.Path, 1); toIndex.Add(x); });
                        //delete previous meta binding
                        repo.GetPuckMeta().Where(x => x.Name == DBNames.PathToLocale && x.Key.ToLower().Equals(originalPath.ToLower())).ToList()
                            .ForEach(x => x.Key = mod.Path);
                        repo.GetPuckMeta().Where(x => x.Name == DBNames.DomainMapping && x.Key.ToLower().Equals(originalPath.ToLower())).ToList()
                            .ForEach(x => x.Key = mod.Path);
                        repo.SaveChanges();
                        shouldUpdateDomainMappings = true;
                        shouldUpdatePathLocaleMappings = true;
                    }
                    if(shouldIndex)
                        indexer.Index(toIndex,triggerEvents:triggerIndexEvents);
                }
                if (triggerEvents)
                {
                    var afterArgs = new IndexingEventArgs { Node = mod };
                    OnAfterSave(this, afterArgs);
                }
                AddPublishInstruction(toIndex);

                string auditAction = mod.Published ? AuditActions.Publish : AuditActions.Save;
                if (original == null) auditAction = AuditActions.Create;
                AddAuditEntry(mod.Id, mod.Variant, auditAction, "", username);
                if(shouldUpdateDomainMappings)
                    StateHelper.UpdateDomainMappings(true);
                if(shouldUpdatePathLocaleMappings)
                    StateHelper.UpdatePathLocaleMappings(true);
                return toIndex;
            }
            finally
            {
                slock1.Release();
            }
        }
        int UpdateHasNoPublishedRevisionAndIsPublishedRevision(Guid id, string variant, bool? hasNoPublishedRevision,
                    bool? isPublishedRevision, int? hasNoPublishedRevisionIgnoreRevisionId = null, int? isPublishedRevisionIgnoreRevisionId = null)
        {
            var affected = 0;
            string hasNoPublishedRevisionSql = "";
            if (hasNoPublishedRevision.HasValue)
            {
                var value = hasNoPublishedRevision.Value ? 1 : 0;
                hasNoPublishedRevisionSql = $"update PuckRevision set [HasNoPublishedRevision]={value}"
                    + $" where [Id]='{id}' and [Variant]='{variant}' and [HasNoPublishedRevision]!={value}"
                    + (hasNoPublishedRevisionIgnoreRevisionId.HasValue ? $" and [RevisionID]!={hasNoPublishedRevisionIgnoreRevisionId}" : "");
            }
            string isPublishedRevisionSql = "";
            if (isPublishedRevision.HasValue)
            {
                var value = isPublishedRevision.Value ? 1 : 0;
                isPublishedRevisionSql = $"update PuckRevision set [IsPublishedRevision]={value}"
                    + $" where [Id]='{id}' and [Variant]='{variant}' and [IsPublishedRevision]!={value}"
                    + (isPublishedRevisionIgnoreRevisionId.HasValue ? $" and [RevisionID]!={isPublishedRevisionIgnoreRevisionId}" : "");
            }
            var batchedSql = "";
            if (!string.IsNullOrEmpty(hasNoPublishedRevisionSql) && !string.IsNullOrEmpty(isPublishedRevisionSql))
                batchedSql = hasNoPublishedRevisionSql + ";" + isPublishedRevisionSql;
            else if (string.IsNullOrEmpty(hasNoPublishedRevisionSql))
                batchedSql = isPublishedRevisionSql;
            else if (string.IsNullOrEmpty(isPublishedRevisionSql))
                batchedSql = hasNoPublishedRevisionSql;

            affected = repo.Context.Database.ExecuteSqlRaw(batchedSql);
            return affected;
        }
        public void AddAuditEntry(Guid id, string variant, string action, string notes, string username)
        {
            var audit = new PuckAudit();
            audit.ContentId = id;
            audit.Variant = variant;
            audit.Action = action;
            audit.Notes = notes;
            audit.Username = username;
            repo.AddPuckAudit(audit);
            repo.SaveChanges();
        }
        public async Task RePublishEntireSite()
        {
            await slock1.WaitAsync();
            try
            {
                //((Puck_Repository)repo).repo.Configuration.AutoDetectChangesEnabled = false;
                var toIndex = new List<BaseModel>();
                using (MiniProfiler.Current.Step("get all models"))
                {
                    toIndex = repo.GetPuckRevision().Where(x => x.Current).ToList().Select(x => x.ToBaseModel()).ToList();
                }
                var qh = new QueryHelper<BaseModel>(prependTypeTerm: false);
                qh.And().Field(x => x.TypeChain, typeof(BaseModel).FullName.Wrap());
                var query = qh.ToString();
                using (MiniProfiler.Current.Step("delete models"))
                {
                    indexer.Delete(query, reloadSearcher: false);
                }
                using (MiniProfiler.Current.Step("index models"))
                {
                    indexer.Index(toIndex);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                slock1.Release();
                //((Puck_Repository)repo).repo.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public async Task RePublishEntireSite2()
        {
            var errored = false;
            var errorMsg = string.Empty;
            PuckCache.IsRepublishingEntireSite = true;
            await slock1.WaitAsync();
            try
            {
                var values = new List<string>();
                var models = new List<BaseModel>();
                var typeAndValues = new List<KeyValuePair<string, string>>();
                using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    PuckCache.IndexingStatus = $"retrieving records to republish";
                    var sql = "SELECT Path,Type,Value,TypeChain,SortOrder,ParentId,TemplatePath FROM PuckRevision where ([IsPublishedRevision] = 1 OR ([HasNoPublishedRevision]=1 AND [Current] = 1))";
                    var com = new SqlCommand(sql, con);
                    //com.Parameters.AddWithValue("@pricePoint", paramValue);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    using (MiniProfiler.Current.Step("get all models"))
                    {
                        while (reader.Read())
                        {
                            var aqn = reader.GetString(1);
                            var value = reader.GetString(2);
                            //var type = ApiHelper.GetType(aqn);
                            var type = ApiHelper.GetTypeFromName(aqn);
                            if (type == null) continue;
                            var model = JsonConvert.DeserializeObject(value, type) as BaseModel;
                            model.Type = aqn;
                            model.Path = reader.GetString(0);
                            model.TypeChain = reader.GetString(3);
                            model.SortOrder = reader.GetInt32(4);
                            model.ParentId = reader.GetGuid(5);
                            model.TemplatePath = reader.GetString(6);
                            models.Add(model);
                            //typeAndValues.Add(new KeyValuePair<string, string>(aqn, value));
                            //values.Add(reader.GetString(2));
                        }
                    }
                    reader.Close();
                }
                //using (MiniProfiler.Current.Step("deserialize"))
                //{
                //    PuckCache.IndexingStatus = $"deserializing records";
                //    foreach (var item in typeAndValues)
                //    {
                //        try
                //        {
                //            //var model = JsonConvert.DeserializeObject(item.Value, ApiHelper.GetType(item.Key)) as BaseModel;
                //            //models.Add(model);
                //            //values.Add(reader.GetString(2));
                //        }
                //        catch (Exception ex) { }

                //    }
                //}
                //var qh = new QueryHelper<BaseModel>(prependTypeTerm: false);
                //qh.And().Field(x => x.TypeChain, typeof(BaseModel).Name.Wrap());
                //var query = qh.ToString();
                PuckCache.IndexingStatus = $"deleting all indexed items";
                using (MiniProfiler.Current.Step("delete models"))
                {
                    //indexer.Delete(query, reloadSearcher: false);
                    indexer.DeleteAll();
                }
                using (MiniProfiler.Current.Step("index models"))
                {
                    indexer.Index(models, triggerEvents: false);
                }
            }
            catch (Exception ex)
            {
                //PuckCache.IsRepublishingEntireSite = false;
                //PuckCache.IndexingStatus = "";
                logger.Log(ex);
                errored = true;
                errorMsg = ex.Message;
            }
            finally
            {
                slock1.Release();
                PuckCache.IsRepublishingEntireSite = false;
                PuckCache.IndexingStatus = "";
                if (errored)
                {
                    PuckCache.IndexingStatus = errorMsg;
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(15000);
                        if (PuckCache.IndexingStatus == errorMsg)
                            PuckCache.IndexingStatus = "";
                    });
                }
            }
        }
        public async Task Copy(Guid id, Guid parentId, bool includeDescendants, string userName = null)
        {
            PuckUser user = null;
            if (!string.IsNullOrEmpty(userName))
            {
                user = await userManager.FindByNameAsync(userName);
                if (user == null)
                    throw new UserNotFoundException("there is no user for provided username");
            }
            else
                userName = HttpContext.Current.User.Identity.Name;

            string notes = "";

            var itemsToCopy = repo.GetPuckRevision().Where(x => x.Id == id && x.Current).ToList();
            if (itemsToCopy.Count == 0) return;
            var descendants = new List<PuckRevision>();
            if (includeDescendants)
            {
                descendants = repo.CurrentRevisionDescendants(itemsToCopy.First().IdPath).ToList();
                if (descendants.Any())
                    notes = $"{descendants.Count} descendant items also copied";
            }

            var allItemsToCopy = new List<PuckRevision>();
            allItemsToCopy.AddRange(itemsToCopy);
            allItemsToCopy.AddRange(descendants);

            var ids = allItemsToCopy.Select(x => x.Id).Distinct();

            var idMap = new Dictionary<Guid, Guid>();

            foreach (var guid in ids)
            {
                idMap[guid] = Guid.NewGuid();
            }

            foreach (var item in allItemsToCopy)
            {
                if (idMap.ContainsKey(item.Id))
                    item.Id = idMap[item.Id];
                if (idMap.ContainsKey(item.ParentId))
                    item.ParentId = idMap[item.ParentId];
            }

            itemsToCopy.ForEach(x => x.ParentId = parentId);

            async Task SaveCopies(Guid pId, List<PuckRevision> items)
            {
                var children = items.Where(x => x.ParentId == pId).ToList();
                var childrenGroupedById = children.GroupBy(x => x.Id);
                foreach (var group in childrenGroupedById)
                {
                    foreach (var revision in group)
                    {
                        var model = ApiHelper.RevisionToBaseModel(revision);
                        await SaveContent(model);
                    }
                    await SaveCopies(group.Key, items);
                }
            }

            await SaveCopies(parentId, allItemsToCopy);
            AddAuditEntry(id, "", AuditActions.Copy, notes, userName);
        }
        public async Task Move(Guid nodeId, Guid destinationId, string userName = null)
        {
            PuckUser user = null;
            if (!string.IsNullOrEmpty(userName))
            {
                user = await userManager.FindByNameAsync(userName);
                if (user == null)
                    throw new UserNotFoundException("there is no user for provided username");
            }
            else
                userName = HttpContext.Current.User.Identity.Name;
            Guid? parentId = null;
            var startRevisions = repo.GetPuckRevision().Where(x => x.Id == nodeId && x.Current).ToList();
            parentId = startRevisions.FirstOrDefault()?.ParentId;
            var destinationRevisions = repo.GetPuckRevision().Where(x => x.Id == destinationId && x.Current).ToList();
            if (startRevisions.Count == 0) throw new Exception("cannot find start node");
            if (destinationRevisions.Count == 0) throw new Exception("cannot find destination node");
            if (destinationRevisions.FirstOrDefault().IdPath.ToLower().StartsWith(startRevisions.FirstOrDefault().IdPath.ToLower()))
                throw new Exception("cannot move parent node to child");
            if (startRevisions.FirstOrDefault().ParentId == Guid.Empty)
                throw new Exception("cannot move root node");
            var startNodes = startRevisions.Select(x => x.ToBaseModel()).ToList();
            var destinationNodes = destinationRevisions.Select(x => x.ToBaseModel()).ToList();
            BaseModel startNode = null;
            var beforeArgs = new BeforeMoveEventArgs
            {
                Nodes = startNodes
                ,
                DestinationNodes = destinationNodes
            };
            OnBeforeMove(null, beforeArgs);
            if (!beforeArgs.Cancel)
            {
                startNodes.ForEach(x => x.ParentId = destinationId);
                startNode = startNodes.FirstOrDefault();
                await SaveContent(startNode, makeRevision: false);
                var afterArgs = new MoveEventArgs { Nodes = startNodes, DestinationNodes = startNodes };
                OnAfterMove(null, afterArgs);
            }
            else
            {
                throw new Exception("Move cancelled by custom event handler.");
            }
            if (startNodes.Any())
                AddAuditEntry(startNodes.First().Id, startNodes.First().Variant, AuditActions.Move, "", userName);
            if (parentId.HasValue) {
                var parentRevisions = repo.GetPuckRevision().Where(x => x.Id == parentId).ToList();
                var hasChildren = repo.GetPuckRevision().Count(x=>x.ParentId.Equals(parentId)&&x.Current)>0;
                parentRevisions.ForEach(x=>x.HasChildren=hasChildren);
                repo.SaveChanges();
            }
        }
        public async Task Move(string start, string destination)
        {
            if (destination.ToLower().StartsWith(start.ToLower()))
                throw new Exception("cannot move parent node to child");
            if (start.Count(x => x == '/') == 1)
                throw new Exception("cannot move root node");
            var toMove = repo.CurrentRevisionsByPath(start).FirstOrDefault();
            if (!destination.EndsWith("/"))
                destination += "/";
            toMove.Path = destination + toMove.NodeName;

            var startRevisions = repo.CurrentRevisionsByPath(start).ToList().Cast<BaseModel>().ToList();
            var destinationRevisions = repo.CurrentRevisionsByPath(destination.TrimEnd('/')).ToList().Cast<BaseModel>().ToList();
            var beforeArgs = new BeforeMoveEventArgs { Nodes = startRevisions, DestinationNodes = destinationRevisions };
            OnBeforeMove(null, beforeArgs);
            if (!beforeArgs.Cancel)
            {
                await SaveContent(toMove, makeRevision: false);
                startRevisions = repo.CurrentRevisionsByPath(toMove.Path).ToList().Cast<BaseModel>().ToList();
                var afterArgs = new MoveEventArgs { Nodes = startRevisions, DestinationNodes = destinationRevisions };
                OnAfterMove(null, afterArgs);
            }
        }
        public int UpdateTypeAndTypeChain(string oldType, string newType, string newTypeChain)
        {
            int rowsAffected = 0;
            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                var sql = "update PuckRevision set [Type] = @newType, [TypeChain] = @newTypeChain where [Type] = @oldType";
                var com = new SqlCommand(sql, con);
                com.Parameters.AddWithValue("@oldType", oldType);
                com.Parameters.AddWithValue("@newType", newType);
                com.Parameters.AddWithValue("@newTypeChain", newTypeChain);
                con.Open();
                rowsAffected = com.ExecuteNonQuery();
            }
            return rowsAffected;
        }
        public int RenameOrphaned2(string orphanTypeName, string newTypeName)
        {
            var newType = ApiHelper.GetTypeFromName(newTypeName);
            var newTypeChain = ApiHelper.TypeChain(newType);

            List<BaseModel> toIndex = null;

            var affected = UpdateTypeAndTypeChain(orphanTypeName, newTypeName, newTypeChain);

            var revisions = repo.GetPuckRevision().Where(x => x.Type.Equals(newTypeName) && (x.IsPublishedRevision || (x.Current && x.HasNoPublishedRevision)))
                .ToList();

            toIndex = revisions.Select(x => x.ToBaseModel()).Where(x => x != null).ToList();
            
            //var qh = new QueryHelper<BaseModel>(prependTypeTerm: false);
            //qh.Must().Field(x => x.Type, orphanTypeName);
            //toIndex = qh.GetAllNoCast(limit: int.MaxValue,typeOverride:newType);

            //toIndex.ForEach(x => { x.Type = newTypeName; x.TypeChain = newTypeChain; });
            AddPublishInstruction(toIndex);
            indexer.Index(toIndex);

            //update relevant meta entries
            var metaTypeAllowedTypes = repo.GetPuckMeta().Where(x => x.Name == DBNames.TypeAllowedTypes && (x.Key.Equals(orphanTypeName) || x.Value.Equals(orphanTypeName))).ToList();
            metaTypeAllowedTypes.ForEach(x =>
            {
                if (x.Key.Equals(orphanTypeName))
                    x.Key = newTypeName;
                if (x.Value.Equals(orphanTypeName))
                    x.Value = newTypeName;
            });

            var metaEditorSettings = repo.GetPuckMeta().Where(x => x.Name == DBNames.EditorSettings && x.Key.Equals(orphanTypeName)).ToList();
            metaEditorSettings.ForEach(x =>
            {
                x.Key = newTypeName;
            });

            var metaTypeAllowedTemplates = repo.GetPuckMeta().Where(x => x.Name == DBNames.TypeAllowedTemplates && x.Key.Equals(orphanTypeName)).ToList();
            metaTypeAllowedTemplates.ForEach(x =>
            {
                x.Key = newTypeName;
            });

            var metaFieldGroups = repo.GetPuckMeta().Where(x => x.Name.StartsWith(DBNames.FieldGroups + orphanTypeName)).ToList();
            metaFieldGroups.ForEach(x =>
            {
                x.Name = DBNames.FieldGroups + newTypeName;
            });

            repo.SaveChanges();
            return affected;
        }
        public void RenameOrphaned(string orphanTypeName, string newTypeName)
        {
            //var newType = ApiHelper.GetType(newTypeName);
            var newType = ApiHelper.GetTypeFromName(newTypeName);
            var newTypeChain = ApiHelper.TypeChain(newType);
            var indexChecked = new HashSet<string>();
            //determines how many db revisions to get at once and also the reindex threshhold - useful for handling large amount of data without raping server resources.
            var step = 1000;
            var toIndex = new List<BaseModel>();
            //we're doing this in chunks - while there are still chunks to process
            while (repo.GetPuckRevision().Where(x => x.Type.Equals(orphanTypeName)).Count() > 0)
            {
                //get next chunk from database
                var records = repo.GetPuckRevision().Where(x => x.Type.Equals(orphanTypeName)).Take(step).ToList();
                var recordCounter = 0;
                records.ForEach(x =>
                {
                    try
                    {
                        //update json string
                        var valueobj = JsonConvert.DeserializeObject(x.Value, ApiHelper.ConcreteType(newType)) as BaseModel;
                        //set database revision type to new type
                        x.Type = newTypeName;
                        //update typechain
                        x.TypeChain = newTypeChain;
                        valueobj.Type = x.Type;
                        valueobj.TypeChain = x.TypeChain;
                        x.Value = JsonConvert.SerializeObject(valueobj);
                        //update indexed values, check this hasn't been indexed before
                        if (!indexChecked.Contains(string.Concat(x.Id.ToString(), x.Variant)))
                        {
                            var results = puck.core.Helpers.QueryHelper<BaseModel>.Query(
                                string.Concat("+", FieldKeys.ID, ":", x.Id.ToString(), " +", FieldKeys.Variant, ":", x.Variant)
                                );
                            var result = results.FirstOrDefault();
                            if (result != null)
                            {
                                var indexNode = JsonConvert.DeserializeObject(result[FieldKeys.PuckValue], ApiHelper.ConcreteType(newType)) as BaseModel;
                                //basically grab currently indexed node, change type information and add to reindex list
                                indexNode.TypeChain = x.TypeChain;
                                indexNode.Type = x.Type;
                                toIndex.Add(indexNode);
                                indexChecked.Add(string.Concat(x.Id.ToString(), x.Variant));
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        var exc = new Exception(ex.Message + string.Format(" -- errored on record {0} with id {1} and variant {2}", recordCounter, records[recordCounter].Id, records[recordCounter].Variant), ex);
                        logger.Log(exc);
                    }
                    finally
                    {
                        recordCounter++;
                    }
                });
                //commit current chunk to db
                repo.SaveChanges();
                //since committing index is slow, only commit once reindex list grows to certain size to avoid frequent expensive operations on index
                if (toIndex.Count >= step)
                {
                    indexer.Index(toIndex);
                    toIndex.Clear();
                }
            }

            //update relevant meta entries
            var metaTypeAllowedTypes = repo.GetPuckMeta().Where(x => x.Name == DBNames.TypeAllowedTypes && (x.Key.Equals(orphanTypeName) || x.Value.Equals(orphanTypeName))).ToList();
            metaTypeAllowedTypes.ForEach(x =>
            {
                if (x.Key.Equals(orphanTypeName))
                    x.Key = newTypeName;
                if (x.Value.Equals(orphanTypeName))
                    x.Value = newTypeName;
            });

            var metaEditorSettings = repo.GetPuckMeta().Where(x => x.Name == DBNames.EditorSettings && x.Key.Equals(orphanTypeName)).ToList();
            metaEditorSettings.ForEach(x =>
            {
                x.Key = newTypeName;
            });

            var metaTypeAllowedTemplates = repo.GetPuckMeta().Where(x => x.Name == DBNames.TypeAllowedTemplates && x.Key.Equals(orphanTypeName)).ToList();
            metaTypeAllowedTemplates.ForEach(x =>
            {
                x.Key = newTypeName;
            });

            var metaFieldGroups = repo.GetPuckMeta().Where(x => x.Name.StartsWith(DBNames.FieldGroups + orphanTypeName)).ToList();
            metaFieldGroups.ForEach(x =>
            {
                x.Name = DBNames.FieldGroups + newTypeName;
            });

            repo.SaveChanges();

            //if there's anything left to reindex, reindex.
            if (toIndex.Count > 0)
                indexer.Index(toIndex);

        }

    }
}
