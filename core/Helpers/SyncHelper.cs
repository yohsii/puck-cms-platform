using puck.core.Abstract;
using puck.core.Base;
using puck.core.Constants;
using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using puck.core.State;
using puck.core.Services;
using puck.core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace puck.core.Helpers
{
    public static class SyncHelper
    {
        public static event EventHandler<AfterSyncEventArgs> AfterSync;

        public static void OnAfterSync(object s, AfterSyncEventArgs args)
        {
            if (AfterSync != null)
                AfterSync(s, args);
        }

        private static object lck = new object();
        private static int lock_wait = 1;
        public static I_Puck_Repository Repo {get{return PuckCache.PuckRepo;}}
        public static I_Content_Indexer Indexer { get { return PuckCache.PuckIndexer; } }
        public static I_Content_Service ContentService { get { return PuckCache.ContentService; } } 
        public static bool InitializeSync() {
            var repo = Repo;
            using (var scope = PuckCache.ServiceProvider.CreateScope())
            {
                var contentService = scope.ServiceProvider.GetService<I_Content_Service>();
                var serverName = ApiHelper.ServerName();
                var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.SyncId && x.Key == serverName).FirstOrDefault();
                if (meta == null)
                {
                    var newMeta = new PuckMeta();
                    newMeta.Name = DBNames.SyncId;
                    newMeta.Key = ApiHelper.ServerName();
                    int? maxId = repo.GetPuckInstruction().Max(x => (int?)x.Id);
                    newMeta.Value = (maxId ?? 0).ToString();
                    repo.AddMeta(newMeta);
                    repo.SaveChanges();
                    return true;
                }
            }
            return false;
        }
        public static void Sync(CancellationToken ct)
        {
            bool taken = false;
            using (var scope = PuckCache.ServiceProvider.CreateScope())
            {
                var contentService = scope.ServiceProvider.GetService<I_Content_Service>();
                var searcher = scope.ServiceProvider.GetService<I_Content_Searcher>();
                var config = scope.ServiceProvider.GetService<IConfiguration>();
                try
                {
                    Monitor.TryEnter(lck, lock_wait, ref taken);
                    if (!taken)
                        return;
                    var repo = Repo;
                    var serverName = ApiHelper.ServerName();
                    var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.SyncId && x.Key == serverName).FirstOrDefault();
                    if (meta == null)
                        return;
                    var syncId = int.Parse(meta.Value);
                    var instructions = repo.GetPuckInstruction().Where(x => x.Id > syncId && x.ServerName != serverName).ToList();
                    if (instructions.Count == 0)
                        return;
                    //dosync
                    var hasPublishInstruction = false;
                    var instructionTotal = 0;
                    instructions.ForEach(x => instructionTotal += x.Count);
                    if (instructionTotal > PuckCache.MaxSyncInstructions)
                    {
                        //todo, update settings and republish entire site
                        if (!PuckCache.IsRepublishingEntireSite)
                        {
                            PuckCache.IsRepublishingEntireSite = true;
                            var republishTask = contentService.RePublishEntireSite2();
                            republishTask.GetAwaiter().GetResult();
                        }
                        StateHelper.UpdateTaskMappings();
                        StateHelper.UpdateRedirectMappings();
                        StateHelper.UpdatePathLocaleMappings();
                        StateHelper.UpdateDomainMappings();
                        StateHelper.UpdateCacheMappings();
                        StateHelper.UpdateCrops();
                    }
                    else
                    {
                        foreach (var instruction in instructions)
                        {
                            if (instruction.InstructionKey == InstructionKeys.SetSearcher)
                            {
                                searcher.SetSearcher();
                            }
                            if (instruction.InstructionKey == InstructionKeys.Delete)
                            {
                                var qh = new QueryHelper<BaseModel>(prependTypeTerm: false);
                                qh.SetQuery(instruction.InstructionDetail);
                                var models = qh.GetAll(limit:int.MaxValue);
                                Indexer.Delete(models);
                            }
                            if (instruction.InstructionKey == InstructionKeys.RepublishSite)
                            {
                                if (!PuckCache.IsRepublishingEntireSite)
                                {
                                    PuckCache.IsRepublishingEntireSite = true;
                                    var republishTask = contentService.RePublishEntireSite2();
                                    republishTask.GetAwaiter().GetResult();
                                }
                            }
                            else if (instruction.InstructionKey == InstructionKeys.Publish)
                            {
                                hasPublishInstruction = true;
                                var toIndex = new List<BaseModel>();
                                //instruction detail holds comma separated list of ids and variants in format id:variant,id:variant
                                var idList = instruction.InstructionDetail.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var idAndVariant in idList)
                                {
                                    var idAndVariantArr = idAndVariant.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                    var id = Guid.Parse(idAndVariantArr[0]);
                                    var variant = idAndVariantArr[1];
                                    var publishedOrCurrentRevision = repo.PublishedOrCurrentRevision(id, variant);
                                    if (publishedOrCurrentRevision != null)
                                    {
                                        var model = publishedOrCurrentRevision.ToBaseModel();
                                        toIndex.Add(model);
                                    }
                                }
                                Indexer.Index(toIndex);
                            }
                            else if (instruction.InstructionKey == InstructionKeys.UpdateCrops)
                            {
                                StateHelper.UpdateCrops();
                            }
                            else if (instruction.InstructionKey == InstructionKeys.UpdateCacheMappings)
                            {
                                StateHelper.UpdateCacheMappings();
                            }
                            else if (instruction.InstructionKey == InstructionKeys.UpdateDomainMappings)
                            {
                                StateHelper.UpdateDomainMappings();
                            }
                            else if (instruction.InstructionKey == InstructionKeys.UpdatePathLocales)
                            {
                                StateHelper.UpdatePathLocaleMappings();
                            }
                            else if (instruction.InstructionKey == InstructionKeys.UpdateRedirects)
                            {
                                StateHelper.UpdateRedirectMappings();
                            }
                            else if (instruction.InstructionKey == InstructionKeys.UpdateTaskMappings)
                            {
                                StateHelper.UpdateTaskMappings();
                            }

                        }
                        if (hasPublishInstruction) {
                            if (((config.GetValue<bool?>("UseAzureDirectory") ?? false) || (config.GetValue<bool?>("UseSyncDirectory") ?? false)) 
                                && config.GetValue<bool>("IsEditServer"))
                            {
                                var newInstruction = new PuckInstruction();
                                newInstruction.InstructionKey = InstructionKeys.SetSearcher;
                                newInstruction.Count = 1;
                                newInstruction.ServerName = ApiHelper.ServerName();
                                repo.AddPuckInstruction(newInstruction);
                                repo.SaveChanges();
                            }
                        }
                    }
                    //update syncId
                    //var maxInstructionId = instructions.Max(x => x.Id);
                    var maxInstructionIdOrDefault = repo.GetPuckInstruction().Max(x => (int?)x.Id);
                    var maxInstructionId = maxInstructionIdOrDefault.HasValue ? maxInstructionIdOrDefault.Value : 0;
                    meta.Value = maxInstructionId.ToString();
                    repo.SaveChanges();
                    OnAfterSync(null, new AfterSyncEventArgs { Instructions = instructions });
                }
                catch (Exception ex)
                {
                    PuckCache.PuckLog.Log(ex);
                }
                finally
                {
                    if (taken)
                        Monitor.Exit(lck);
                    PuckCache.IsSyncQueued = false;
                }
            }
        }
    }
}
