using System.Collections.Generic;
using puck.core.Models.Logging;

namespace puck.core.Abstract
{
    public interface I_Log_Helper
    {
        List<LogEntry> GetLog(string machineName = null, string logName = null);
        List<string> ListLogs(string machineName = null, int take = 30);
        List<string> ListMachines();
    }
}