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
namespace puck.core.Tasks
{
    class TimedPublishTask:BaseTask
    {
        public TimedPublishTask() {
            this.ID = -1;
            this.Recurring = true;
            this.IntervalSeconds = 60;
            this.RunOn = DateTime.Now;
        }
        public override void Run(CancellationToken t)
        {
            //PuckCache.PuckLog.Log(new Exception($"{DateTime.Now.ToString()}"));
            base.Run(t);
            var repo = PuckCache.PuckRepo;
            var contentService = PuckCache.ContentService;
            var publishMeta = repo.GetPuckMeta().Where(x=>x.Name==DBNames.TimedPublish && x.Dt.HasValue && x.Dt.Value<=DateTime.Now).ToList();
            var unpublishMeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.TimedUnpublish && x.Dt.HasValue && x.Dt.Value <= DateTime.Now).ToList();

            foreach (var meta in publishMeta) {
                var id =Guid.Parse(meta.Key.Split(':')[0]);
                var variant = meta.Key.Split(':')[1];
                var descendantVariants = (meta.Value ?? "").Split(new char[] { ','},StringSplitOptions.RemoveEmptyEntries).ToList();
                contentService.Publish(id, variant,descendantVariants,userName:meta.Username);
            }

            foreach (var meta in unpublishMeta)
            {
                var id = Guid.Parse(meta.Key.Split(':')[0]);
                var variant = meta.Key.Split(':')[1];
                var descendantVariants = new List<string>() {variant };
                contentService.UnPublish(id, variant, descendantVariants,userName:meta.Username);
            }

            publishMeta.ForEach(x => repo.DeleteMeta(x));
            unpublishMeta.ForEach(x=> repo.DeleteMeta(x));
            repo.SaveChanges();
        }
    }
}
