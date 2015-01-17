using System.Web.Mvc;
using System.Web.Routing;

namespace SESM
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "APIMisc",
                url: "API/Misc/{action}",
                defaults: new { controller = "APIMisc", action = "Index" }
            );

            routes.MapRoute(
                name: "APISettings",
                url: "API/Settings/{action}",
                defaults: new { controller = "APISettings", action = "Index"}
            );

            routes.MapRoute(
                name: "APIServer",
                url: "API/Server/{action}/{id}",
                defaults: new { controller = "APIServer", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "APIAccount",
                url: "API/Account/{action}",
                defaults: new { controller = "APIAccount", action = "Index"}
            );

            routes.MapRoute(
                name: "Home",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                name: "Views",
                url: "{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                constraints: new { controller = "Home|Account|Server|Settings" }
            );


            // Show a 404 error page for API.
            routes.MapRoute(
                name: "404API",
                url: "API/{*url}",
                defaults: new { controller = "Error", action = "404API"}
            );

            // Show a 404 error page for anything else.
            routes.MapRoute(
                name: "404",
                url: "{*url}",
                defaults: new { controller = "Error", action = "404"}
            );
        }
    }
}
