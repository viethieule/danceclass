using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DanceClass
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            

            routes.MapRoute(
                name: "Member",
                url: "member/{username}",
                defaults: new { controller = "Members", action = "Index", username = "" },
                new string[] { "DanceClass.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Schedule", action = "Index", id = UrlParameter.Optional },
                new string[] { "DanceClass.Controllers" }
            );
        }
    }
}
