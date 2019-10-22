using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace puck.core.Abstract
{
    public interface I_Log
    {
        void Log(Exception ex);
        void Log(string message, string stackTrace, string level = "error", Type exceptionType = null);
    }
}
