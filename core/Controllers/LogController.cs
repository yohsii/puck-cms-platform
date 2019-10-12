using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using puck.core.Base;
using puck.core.Helpers;
using puck.core.Controllers;
using puck.core.Abstract;
using puck.core.Constants;
using Newtonsoft.Json;
using puck.core.Entities;
using puck.core.Filters;
using puck.core.Models;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using puck.core.Attributes;
using puck.core.State;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using puck.core.Models.Logging;

namespace puck.core.Controllers
{
    [Area("puck")]
    [SetPuckCulture]
    [Authorize(Roles=PuckRoles.Tasks,AuthenticationSchemes =Mvc.AuthenticationScheme)]
    public class LogController : BaseController
    {
        I_Log_Helper logHelper;
        public LogController(I_Log_Helper lh) {
            logHelper = lh;
        }

        public ActionResult Machines() {
            var success = true;
            var message = "";
            var result = new List<string>();
            try
            {
                result = logHelper.ListMachines();
            }
            catch (Exception ex) {
                success = false;
                message = ex.Message;
            }
            return Json(new {machines=result,success=success,message=message });
        }

        public ActionResult Logs(string machine=null)
        {
            var success = true;
            var message = "";
            var result = new List<string>();
            try
            {
                result = logHelper.ListLogs(machineName:machine);
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { logs = result, success = success, message = message });
        }

        public ActionResult Log(string machine = null,string name=null)
        {
            var success = true;
            var message = "";
            string machineName = machine ?? ApiHelper.ServerName();
            string logName = name ?? DateTime.Now.ToString("yyyy-MM-dd");
            var result = new List<LogEntry>();
            try
            {
                result = logHelper.GetLog(machineName:machine,logName:name);
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { entries = result ,machine=machineName ,name=logName ,success = success, message = message });
        }

    }
}
