using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Abstract;
using puck.core.Base;
using puck.core.Entities;

namespace puck.core.Events
{
    public class IndexingEventArgs : EventArgs
    {
        public BaseModel Node { get; set; }
    }
    public class BeforeIndexingEventArgs:IndexingEventArgs {
        public bool Cancel { get; set; }
    }
    public class DispatchEventArgs:EventArgs {
        public BaseTask Task { get; set; }
    }

    public class MoveEventArgs : EventArgs
    {
        public List<BaseModel> Nodes { get; set; }
        public List<BaseModel> DestinationNodes { get; set; }
    }
    public class BeforeMoveEventArgs : MoveEventArgs {
        public bool Cancel{get;set;}
    }
    public class AfterEditorSettingsSaveEventArgs {
        public I_Puck_Editor_Settings Setting { get; set; }
    }
    public class AfterSyncEventArgs {
        public List<PuckInstruction> Instructions { get; set; } 
    }
}
