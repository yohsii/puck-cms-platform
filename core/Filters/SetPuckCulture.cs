using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using puck.core.Constants;
using puck.core.Helpers;
using puck.core.State;
using Microsoft.AspNetCore.Mvc.Filters;
using puck.core.Abstract;

namespace puck.core.Filters
{
    public class SetPuckCulture : Attribute, IActionFilter
    {

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var apiHelper = filterContext.HttpContext.RequestServices.GetService(typeof(I_Api_Helper)) as I_Api_Helper;
            string variant = apiHelper.UserVariant();
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(variant);
        }
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }

    }
}
