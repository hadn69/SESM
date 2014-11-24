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
                name: "APIServer",
                url: "API/Server/{action}/{id}",
                defaults: new { controller = "APIServer", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "APIAccount",
                url: "API/Account/{action}/{id}",
                defaults: new { controller = "APIAccount", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
