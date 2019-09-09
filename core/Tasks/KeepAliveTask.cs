using puck.core.Base;
using puck.core.Constants;
using puck.core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using puck.core.State;
namespace puck.core.Tasks
{
    class KeepAliveTask:BaseTask
    {
        public KeepAliveTask() {
            this.ID = -3;
            this.Recurring = true;
            this.IntervalSeconds = 120;
            this.RunOn = DateTime.Now;
        }
        public override async Task Run(CancellationToken t)
        {
            await base.Run(t);
            if (PuckCache.FirstRequestUrl != null) {
                var uri = PuckCache.FirstRequestUrl;
                var url = $"{uri.Scheme}://{uri.Host}:{uri.Port}/puck/api/keepalive";
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                var content = "";
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }
            }
        }
    }
}
