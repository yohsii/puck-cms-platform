using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using puck.core.Abstract;
using Microsoft.Extensions.Hosting;
using puck.core.Helpers;

namespace puck.core.Concrete
{
    public class Logger:I_Log
    {
        public string DATADIRECTORY { get { return ApiHelper.MapPath("~/App_Data/Log"); } }
        private static Object log_lock = new Object();
        public void Log(Exception e)
        {
            lock (log_lock)
            {
                var dname = DATADIRECTORY;
                var fname = "log";
                var ext = ".txt";
                var maxlen = 5000000;
                if (!Directory.Exists(dname))
                {
                    Directory.CreateDirectory(dname);
                }
                var di = new DirectoryInfo(dname);
                var fc = di.GetFiles().Length;
                var lfpath = dname + "\\" + fname + (fc == 0 ? 0 : fc - 1).ToString() + ext;
                StreamWriter sw = null;
                if (!File.Exists(lfpath))
                {
                    sw = File.CreateText(lfpath);
                }
                var fi = new FileInfo(lfpath);
                if (fi.Length > maxlen)
                {
                    lfpath = dname + "\\" + fname + fc.ToString() + ext;
                    sw = File.CreateText(lfpath);
                }
                if (sw == null) sw = File.AppendText(lfpath);
                sw.WriteLine("date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                sw.WriteLine("message: " + e.Message);
                sw.WriteLine("stacktrace: " + e.StackTrace);
                sw.WriteLine("\n");
                sw.Close();
            }
        }
    }
}