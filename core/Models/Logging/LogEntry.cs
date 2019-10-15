using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace puck.core.Models.Logging
{
    public class LogEntry
    {
        public DateTime Date { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        [Display(Name ="Stack Trace")]
        public string StackTrace { get; set; }
        [Display(Name ="Type")]
        public string ExceptionType { get; set; }
    }
}
