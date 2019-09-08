using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using puck.core.Base;
using puck.core.Events;

namespace puck.core.Abstract
{
    public interface I_Task_Dispatcher
    {
        List<BaseTask> Tasks { get; set; }
        void Start(CancellationToken ct);
        bool ShouldRunNow(BaseTask t);
        bool CanRun(BaseTask t);
        bool CatchUp{get;set;}
        void Stop(bool immediate);
        event EventHandler<DispatchEventArgs> TaskEnd;
        void HandleTaskEnd(object s,DispatchEventArgs e);
    }
}
