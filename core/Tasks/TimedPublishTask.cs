using puck.core.Base;
using puck.core.Constants;
using puck.core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using puck.core.State;
using Microsoft.Extensions.DependencyInjection;
using puck.core.Services;
using puck.core.Abstract;

namespace puck.core.Tasks
{
    class TimedPublishTask:BaseTask
    {
        public TimedPublishTask() {
            this.Id = -1;
            this.Recurring = true;
            this.IntervalSeconds = 60;
            this.RunOn = DateTime.Now;
        }
        public override async Task Run(CancellationToken t)
        {
            //PuckCache.PuckLog.Log(new Exception($"{DateTime.Now.ToString()}"));
            using (var scope = PuckCache.ServiceProvider.CreateScope())
            {
                await base.Run(t);
                var repo = scope.ServiceProvider.GetService<I_Puck_Repository>();
                var contentService = scope.ServiceProvider.GetService<I_Content_Service>();
                var publishMeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.TimedPublish && x.Dt.HasValue && x.Dt.Value <= DateTime.Now).ToList();
                var unpublishMeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.TimedUnpublish && x.Dt.HasValue && x.Dt.Value <= DateTime.Now).ToList();
                
                foreach (var meta in publishMeta)
                {
                    try
                    {
                        var id = Guid.Parse(meta.Key.Split(':')[0]);
                        var variant = meta.Key.Split(':')[1];
                        var descendantVariants = (meta.Value ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        await contentService.Publish(id, variant, descendantVariants, userName: meta.UserName);
                    }
                    catch (Exception ex) {
                        PuckCache.PuckLog.Log($"timed publish failed {meta.Key??""}. {ex.Message}", ex.StackTrace, level: "error", exceptionType: ex.GetType());
                    }
                }

                foreach (var meta in unpublishMeta)
                {
                    try
                    {
                        var id = Guid.Parse(meta.Key.Split(':')[0]);
                        var variant = meta.Key.Split(':')[1];
                        var descendantVariants = new List<string>() { variant };
                        await contentService.UnPublish(id, variant, descendantVariants, userName: meta.UserName);
                    }
                    catch (Exception ex) {
                        PuckCache.PuckLog.Log($"timed unpublish failed {meta.Key ?? ""}. {ex.Message}", ex.StackTrace, level: "error", exceptionType: ex.GetType());
                    }
                }

                publishMeta.ForEach(x => repo.DeleteMeta(x));
                unpublishMeta.ForEach(x => repo.DeleteMeta(x));
                repo.SaveChanges();
            }
        }
    }
}
