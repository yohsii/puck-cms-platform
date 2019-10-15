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
using Microsoft.Data.SqlClient;

namespace puck.core.Tasks
{
    class SyncCheckTask:BaseTask
    {
        public SyncCheckTask() {
            this.ID = -2;
            this.Recurring = true;
            this.IntervalSeconds = 6;
            this.RunOn = DateTime.Now;
        }
        public override async Task Run(CancellationToken t)
        {
            await base.Run(t);
            try
            {
                var repo = PuckCache.PuckRepo;
                var serverName = ApiHelper.ServerName();
                var lastSyncMeta = repo.GetPuckMeta().Where(x => x.Name == DBNames.SyncId && x.Key == serverName).FirstOrDefault();
                if (lastSyncMeta == null) return;
                var syncId = int.Parse(lastSyncMeta.Value);
                if (repo.GetPuckInstruction().Any(x => x.Id > syncId && x.ServerName != serverName))
                    PuckCache.ShouldSync = true;
                else
                    PuckCache.ShouldSync = false;
            }//suppress occasional db connection related exceptions from filling up the logs, especially since this task is run so frequently
            catch (SqlException sqlEx) { }
        }
    }
}
