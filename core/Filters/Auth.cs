//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Security;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.Owin;
//using Microsoft.Owin.Security;
//using Microsoft.Owin;
//using Newtonsoft.Json;
//using puck.core.Abstract;
//using puck.core.Constants;
//using puck.core.Identity;
//using puck.core.Models;
//using puck.core.State;
//namespace puck.core.Filters
//{
//    public class Auth:AuthorizeAttribute
//    {
//        private I_Puck_Repository repo = PuckCache.PuckRepo;
//        public PuckUserManager UserManager { get {
//                return HttpContext.Current.GetOwinContext().GetUserManager<PuckUserManager>();
//        } }
        
//        private bool CheckPath(string path,string username) {
//            var user = UserManager.FindByName(username);
//            if (user == null) return false;
//            if (user.StartNodeId != Guid.Empty) { 
//                var startNode = repo.GetPuckRevision().Where(x=>x.Id==user.StartNodeId&&x.Current).FirstOrDefault();
//                if (startNode != null) {
//                    return path.ToLower().StartsWith(startNode.Path.ToLower());
//                }
//            }
//            return true;
//        }
//        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
//        {
//            bool result = base.AuthorizeCore(httpContext);
//            if (httpContext.Request.QueryString.AllKeys.Contains("p_path"))
//            {
//                string username = httpContext.User.Identity.Name;
//                result = result && CheckPath(httpContext.Request["p_path"],username);
//            }
//            return result;
//        }
//        protected override void HandleUnauthorizedRequest(AuthorizationContext context) {
//            UrlHelper urlHelper = new UrlHelper(context.RequestContext);
//            if (context.HttpContext.Request.IsAjaxRequest())
//            {
//                if (context.HttpContext.Request.Headers["accept"] != null && !context.HttpContext.Request.Headers["accept"].ToLower().StartsWith("text/html"))
//                    context.Result = new JsonResult
//                    {
//                        Data = new
//                        {
//                            succes=false,
//                            message = "Not authorized - make sure your login is still valid and check with your admin that you have the necessary permissions to access this content.",
//                            LogOnUrl = urlHelper.Action("In","Account")
//                        }, JsonRequestBehavior = JsonRequestBehavior.AllowGet
//                    };
//                else
//                    context.Result = new ContentResult()
//                    {
//                        Content = "Not authorized - make sure your login is still valid and check with your admin that you have the necessary permissions to access this content.",
//                        ContentType="text/html"
//                    };
//            }
//            else {
//                context.RequestContext.HttpContext.Response.Redirect(urlHelper.Action("In", "Admin")
//                    +"?returnurl="+ HttpUtility.UrlEncode(context.RequestContext.HttpContext.Request.Url.PathAndQuery)
//                , true);
//            }
//        }
//    }
//}
