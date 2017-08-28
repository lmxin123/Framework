using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;

using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

using Newtonsoft.Json;
using Framework.Common.Extensions;
using System.Threading.Tasks;

namespace Framework.Auth
{
    /// <summary>
    /// 操作方式
    /// </summary>
    public enum ActionTypes
    {
        Select,
        Create,
        Update,
        Delete,
        Auditing,
        Default
    }

    public static class IdentityHelpers
    {
        public static List<ParentMenuItem> menus = new List<ParentMenuItem>();//当缓存用
        public static string CACHE_USERID = string.Empty;
        static ApplicationUserManager _userManager;

        static ApplicationUserManager UserManager
        {
            get
            {
                 _userManager= _userManager?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                return _userManager;
            }
        }

        public static MvcHtmlString GetUserName(this HtmlHelper html, string id)
        {
            var user = UserManager.FindById(id);

            return new MvcHtmlString(user?.UserName);
        }

        public static MvcHtmlString ClaimType(this HtmlHelper html, string claimType)
        {
            FieldInfo[] fields = typeof(ClaimTypes).GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.GetValue(null).ToString() == claimType)
                {
                    return new MvcHtmlString(field.Name);
                }
            }
            return new MvcHtmlString(string.Format("{0}",
                 claimType.Split('/', '.').Last()));
        }

        public static List<ParentMenuItem> GetUserRight(this IIdentity identity, string userId = null, bool requireNew = false)
        {
            if (!identity.IsAuthenticated && string.IsNullOrEmpty(userId))
            {
                //未登录或者登录信息己过期，请重新登录！
                HttpContext.Current.Response.RedirectPermanent(AuthSetting.LoginUrl, true);
                menus.Clear();
                return menus;
            }

            string id = userId ?? identity.GetUserId();

            if (CACHE_USERID != id || requireNew)
            {
                CACHE_USERID = id;//这里一定要赋值

                using (var db = new AuthDbContext())
                {
                    //   var role =Applicat db.FirstOrDefault(item => item.UserId == CACHE_USERID);
                    var roles = UserManager.GetRoles(CACHE_USERID);
                    if (roles == null || roles.Count == 0)
                        throw new Exception("用户缺少权限！");

                    var roleRight = db.Roles.FirstOrDefault(item => item.Name == roles.FirstOrDefault());

                    if (string.IsNullOrEmpty(roleRight.Rights))
                        throw new Exception($"{roleRight.Name}角色还没有分配过任何权限！");

                    var userRight = JsonConvert.DeserializeObject<List<ParentRightItem>>(roleRight.Rights);
                    menus.Clear();
                    menus.AddRange(MenuManager.GetAllMenus());

                    menus.ForEach(menu =>
                    {
                        var right = userRight.FirstOrDefault(item => item.Code == menu.Code);
                        menu.All = right.All;

                        menu.SubMenuList.ForEach(subMenu =>
                        {
                            var subRight = right.SubRightList.FirstOrDefault(subItem => subItem.SubCode == subMenu.SubCode);

                            subMenu.Auditing = subRight.Auditing ?? false;
                            subMenu.Create = subRight.Create ?? false;
                            subMenu.Delete = subRight.Delete ?? false;
                            subMenu.Select = subRight.Select ?? false;
                            subMenu.Update = subRight.Update ?? false;
                        });
                    });
                }
            }
            return menus;
        }

        public static bool CheckActionRight(this IIdentity identity, string code, ActionTypes type)
        {
            if (menus.Count == 0)
            {
                menus = GetUserRight(identity);
            }
            bool hasRight = false;

            foreach (var right in menus)
            {
                var item = right.SubMenuList.FirstOrDefault(subRight => subRight.SubCode == code);

                if (item != null)
                {
                    hasRight = SwitchActionType(item, type);
                    break;
                }
            }

            return hasRight;
        }

        public static bool SwitchActionType(SubMenuItem subRight, ActionTypes actionType = ActionTypes.Default, string actionName = "")
        {
            if (actionType != ActionTypes.Default)
            {
                switch (actionType)
                {
                    case ActionTypes.Select:
                        return subRight.Select;
                    case ActionTypes.Create:
                        return subRight.Create;
                    case ActionTypes.Update:
                        return subRight.Update;
                    case ActionTypes.Delete:
                        return subRight.Delete;
                    case ActionTypes.Auditing:
                        return subRight.Auditing;
                }
            }
            else
            {
                if (new string[] { "index", "get", "validdate" }.StartsWith(actionName))
                    return subRight.Select;
                if (new string[] { "update", "change" }.StartsWith(actionName))
                    return subRight.Update;
                if (new string[] { "delete", "del" }.StartsWith(actionName))
                    return subRight.Delete;
                if (new string[] { "create", "register", "save" }.StartsWith(actionName))
                    return subRight.Create;
                if (new string[] { "check", "auditing" }.StartsWith(actionName))
                    return subRight.Auditing;
            }

            return false;
        }
    }
}
