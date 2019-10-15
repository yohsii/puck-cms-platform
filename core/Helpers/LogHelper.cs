using puck.core.Abstract;
using puck.core.Models.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace puck.core.Helpers
{
    public class LogHelper : I_Log_Helper
    {
        string basePath = ApiHelper.MapPath($"~/App_Data/Log/");
        public LogHelper()
        {

        }

        public List<string> ListMachines()
        {
            List<string> result = new List<string>();

            if (!Directory.Exists(basePath))
                return result;

            var directory = new DirectoryInfo(basePath);

            result = directory.GetDirectories().Select(x => x.Name).ToList();

            return result;
        }

        public List<string> ListLogs(string machineName = null, int take = 30)
        {
            if (machineName == null)
                machineName = ApiHelper.ServerName();
            List<string> result = new List<string>();
            var path = basePath + $"{machineName}";

            if (!Directory.Exists(path))
                return result;

            var directory = new DirectoryInfo(path);

            result = directory.GetFiles().OrderByDescending(x => x.LastWriteTime).Take(30).Select(x => x.Name).ToList();

            return result;
        }

        public List<LogEntry> GetLog(string machineName = null, string logName = null)
        {
            if (string.IsNullOrEmpty(logName))
            {
                logName = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (!logName.ToLower().EndsWith(".txt"))
                logName = logName += ".txt";
            if (machineName == null)
                machineName = ApiHelper.ServerName();
            List<LogEntry> result = new List<LogEntry>();

            var path = basePath + $"{machineName}\\{logName}";
            if (!File.Exists(path))
                return result;
            var txt = File.ReadAllText(path);
            var rows = txt.Split("\n\n",StringSplitOptions.RemoveEmptyEntries).Where(x => x != "\r\n" && x!="\n" && x!="\r");
            foreach (var row in rows)
            {
                var fields = row.TrimStart('\r','\n').Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length < 5) continue;
                var model = new LogEntry();
                var dateStr = fields[0].TrimEnd('\r', '\n');
                model.Date = DateTime.ParseExact(dateStr, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                model.Level = fields[1].TrimEnd('\r', '\n');
                model.ExceptionType = fields[2].TrimEnd('\r', '\n');
                model.Message = fields[3].TrimEnd('\r', '\n');
                model.StackTrace = fields[4].TrimStart().TrimEnd('\r', '\n');
                result.Add(model);
            }
            result.Reverse();
            return result;
        }

    }
}
