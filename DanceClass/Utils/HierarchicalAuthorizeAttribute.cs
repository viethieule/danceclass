using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace DanceClass.Utils
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class HierarchicalAuthorizeAttribute : AuthorizeAttribute
    {
        public AuthorizationLevel AuthorizationLevel { get; set; }
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (this.AuthorizationLevel == AuthorizationLevel.MemberAndHigher)
            {
                if (!IsInRoles(actionContext, "Member", "Receptionist", "Collaborator", "Admin"))
                {
                    return false;
                }
            }
            else if (this.AuthorizationLevel == AuthorizationLevel.ReceptionistAndHigher)
            {
                if (!IsInRoles(actionContext, "Receptionist", "Collaborator", "Admin"))
                {
                    return false;
                }
            }
            else if (this.AuthorizationLevel == AuthorizationLevel.CollaboratorAndHigher)
            {
                if (!IsInRoles(actionContext, "Collaborator", "Admin"))
                {
                    return false;
                }
            }
            else if (this.AuthorizationLevel == AuthorizationLevel.AdminOnly)
            {
                if (!IsInRoles(actionContext, "Admin"))
                {
                    return false;
                }
            }

            return base.IsAuthorized(actionContext);
        }

        private bool IsInRoles(HttpActionContext actionContext, params string[] roles)
        {
            return roles.Any(role => actionContext.RequestContext.Principal.IsInRole(role));
        }
    }

    public enum AuthorizationLevel
    {
        MemberAndHigher,
        ReceptionistAndHigher,
        CollaboratorAndHigher,
        AdminOnly
    }
}