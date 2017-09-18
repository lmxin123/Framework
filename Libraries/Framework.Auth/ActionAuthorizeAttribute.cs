using System;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

using Framework.Common.Json;
using Framework.Common;

namespace Framework.Auth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ActionAuthorizeAttribute : AuthorizeAttribute
    {
        public ActionTypes ActionType { get; set; } = ActionTypes.Default;
        const string failActionName = "AuthActionFaild";

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // base.OnAuthorization(filterContext);

            //var controllerType = filterContext.Controller.GetType();
            //var controllerContext = filterContext.Controller.ControllerContext;
            //var actionName = filterContext.RouteData.Values["action"].ToString();
            //var controllerDescriptor = new ReflectedControllerDescriptor(controllerType);
            //var actionDescriptor = controllerDescriptor.FindAction(controllerContext, actionName);

            //var r = filterContext.HttpContext.Request.QueryString["r"];
            //if (string.IsNullOrEmpty(r) && filterContext.ActionDescriptor.ActionName.ToLower() == failActionName.ToLower())
            //{
            //    filterContext.RouteData.Values.Add("r", DateTime.Now.Ticks);
            //    filterContext.Result = new RedirectResult(filterContext.HttpContext.Request.UrlReferrer.ToString());
            //}
            //else
            //{
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
                       || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);

            if (skipAuthorization)
            {
                return;
            }

            if (!Utility.IsUrl(filterContext.RequestContext.HttpContext.Request.Url.AbsoluteUri))
            {
                var url = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" +
                        filterContext.ActionDescriptor.ActionName;
                filterContext.Result = new RedirectResult(url, true);
            }

            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
            else if (!AuthorizeCore(filterContext.HttpContext))
            {
                HandleUnauthorizedRequest(filterContext);
            }
            //}
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("filterContext");

            IIdentity identity = httpContext.User.Identity;
            var userRights = identity.GetUserRight();
            var routeValues = httpContext.Request.RequestContext.RouteData.Values;
            string controller = routeValues["controller"].ToString().ToLower(),
                action = routeValues["action"].ToString().ToLower(),
                rightCode = httpContext.Request["rightCode"],
               module = string.Empty;

            if (string.IsNullOrEmpty(rightCode))
            {
                module = string.Concat("/", controller, "/", action);
            }
            else
            {
                var right = userRights.FirstOrDefault(a => a.SubMenuList.Exists(b => b.SubCode == rightCode))?
                     .SubMenuList.FirstOrDefault(a => a.SubCode == rightCode);
                if (right != null)
                {
                    module = right.Url;
                }
                else
                {
                    throw new ArgumentNullException($"权限编码无效{rightCode}");
                }
            }
            foreach (var right in userRights)
            {
                if (!string.IsNullOrEmpty(right.Url))
                {
                    string[] paths = right.Url.Split('/').Where(r => !string.IsNullOrEmpty(r)).ToArray();
                    if (controller == paths[0] && right.All) return true;
                }
                else
                {
                    foreach (var subRight in right.SubMenuList)
                    {
                        if (module.ToLower() == subRight.Url.ToLower())
                        {
                            return IdentityHelpers.SwitchActionType(subRight, ActionType, action);
                        }
                    }
                }
            }
            return false;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (filterContext.HttpContext.Request.HttpMethod != "POST")
            {
                if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                }
                else
                {
                    string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    filterContext.Result = new RedirectResult($"/{controllerName}/{failActionName}", true);
                }
            }
            else
            {
                var resp = new GeneralResponseModel<object>
                {
                    Success = false,
                    Message = "操作失败：您的权限不足！",
                    MessageCode = (int)ErrorCode.AuthFailError
                };
                string jsonStr = JsonConvert.SerializeObject(resp, Formatting.None);
                filterContext.Result = new JsonResult
                {
                    Data = resp,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    ContentType = "application/json",
                };
            }
        }
    }
}
