using Microsoft.AspNet.Identity;
using Services.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace DanceClass.Utils
{
    public class ChangePasswordPromptFilterAttribute : ActionFilterAttribute
    {
        private readonly IMemberService _memberService;

        public ChangePasswordPromptFilterAttribute(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IPrincipal principal = filterContext.HttpContext.User;
            bool isAdmin = principal.IsInRole("Admin");
            if (principal.Identity.IsAuthenticated && !isAdmin)
            {
                string userId = principal.Identity.GetUserId();
                if (_memberService.IsNeedToChangePassword(int.Parse(userId)) && filterContext.HttpContext.Request.RawUrl != "/Account/ResetPassword")
                {
                    filterContext.Result = new RedirectResult("/Account/ResetPassword");
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}