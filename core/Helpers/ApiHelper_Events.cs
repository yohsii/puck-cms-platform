using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using puck.core.Base;
using System.Web;
using System.Threading.Tasks;
using puck.core.Abstract;
using puck.core.Concrete;
using System.Text.RegularExpressions;
using puck.core.Models;
using puck.core.Constants;
using System.Globalization;
using Newtonsoft.Json;
using puck.core.Entities;
using puck.core.Exceptions;
using puck.core.Events;
namespace puck.core.Helpers
{
    public partial class ApiHelper
    {
        public static event EventHandler<AfterEditorSettingsSaveEventArgs> AfterEditorSettingsSave;
        public static event EventHandler<AfterEditorSettingsDeleteEventArgs> AfterEditorSettingsDelete;
        public static void OnAfterSettingsSave(object s, AfterEditorSettingsSaveEventArgs args)
        {
            if (AfterEditorSettingsSave != null)
                AfterEditorSettingsSave(s, args);
        }

        public static void OnAfterSettingsDelete(object s, AfterEditorSettingsDeleteEventArgs args)
        {
            if (AfterEditorSettingsDelete != null)
                AfterEditorSettingsDelete(s, args);
        }
    }
}
