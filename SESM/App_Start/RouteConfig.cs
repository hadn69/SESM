using System.Web.Mvc;
using System.Web.Routing;

namespace SESM
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // === API ===

            // Misc API
            routes.MapRoute(
                name: "APIMisc",
                url: "API/Misc/{action}",
                defaults: new { controller = "APIMisc" },
                constraints: new
                {
                    action = "IsCronValid|" +
                             "GetNextCronDates"
                }
            );

            // Settings API
            routes.MapRoute(
                name: "APISettings",
                url: "API/Settings/{action}",
                defaults: new { controller = "APISettings" },
                constraints: new
                {
                    action = "GetSESMSettings|" +
                             "SetSESMSettings|" +
                             "GetBackupsSettings|" +
                             "SetBackupsSettings|" +

                             "GetSEStatus|" +
                             "GetSEVersion|" +
                             "GetSESettings|" +
                             "SetSESettings|" +
                             "UploadSE|" +
                             "UpdateSE|" + 

                             "GetSESEStatus|" +
                             "GetSESEVersion|" +
                             "GetSESESettings|" +
                             "SetSESESettings|" +
                             "UploadSESE|" +
                             "UpdateSESE|" +
                             "DeleteSESE"
                }
            );

            // Server API
            routes.MapRoute(
                name: "APIServer",
                url: "API/Server/{action}",
                defaults: new { controller = "APIServer" },
                constraints: new
                {
                    action = "GetServers|" +
                             "GetServer|" +
                             "CreateServer|" +
                             "DeleteServers|" +

                             "GetSettings|" +
                             "SetSettings|" +
                             "GetSESESettings|" +
                             "SetSESESettings|" +
                             "GetJobsSettings|" +
                             "SetJobsSettings|" +
                             "GetBackupsSettings|" +
                             "SetBackupsSettings|" +
                             "GetAccessSettings|" +
                             "SetAccessSettings|" +

                             "GetConfiguration|" +
                             "GetConfigurationRights|" +
                             "SetConfiguration|" +
                             "StartServers|" +
                             "StopServers|" +
                             "RestartServers|" +
                             "KillServers"
                }
            );

            // Map API
            routes.MapRoute(
                name: "APIMap",
                url: "API/Map/{action}",
                defaults: new { controller = "APIMap" },
                constraints: new
                {
                    action = "GetMaps|" +
                             "SelectMap|" +
                             "DeleteMap|" +
                             "DownloadMap"
                }
            );

            // Explorer API
            routes.MapRoute(
                name: "APIExplorer",
                url: "API/Explorer/{action}",
                defaults: new { controller = "APIExplorer" },
                constraints: new
                {
                    action = "GetDirectoryContent|" +
                             "GetDirectoryDirectories|" +
                             "Delete|" +
                             "Rename|" +
                             "Download|" +
                             "NewFolder|" +
                             "NewFile|" +
                             "Upload|" +
                             "GetFileContent|" +
                             "SetFileContent"
                }
            );

            // Account API
            routes.MapRoute(
                name: "APIAccount",
                url: "API/Account/{action}",
                defaults: new { controller = "APIAccount" },
                constraints: new
                {
                    action = "GetChallenge|" +
                             "LogOut|" +
                             "Authenticate|" +
                             "Register|" +
                             "GetDetails|" +
                             "SetDetails"
                }
            );

            // API 404
            routes.MapRoute(
                name: "404API",
                url: "API/{*url}",
                defaults: new { controller = "Error", action = "404API" }
            );

            // === GUI ===

            // Home
            routes.MapRoute(
                name: "Home",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );

            // Server List
            routes.MapRoute(
                name: "ServerList",
                url: "Servers",
                defaults: new { action = "Index", controller = "Server" }
            );

            // Account
            routes.MapRoute(
                name: "Account",
                url: "Account/{action}",
                defaults: new {controller = "Account" },
                constraints: new
                {
                    action = "Login|" +
                             "Manage"
                }
            );

            // Settings
            routes.MapRoute(
                name: "Settings",
                url: "Settings/{action}",
                defaults: new { controller = "Settings", action ="Index"},
                constraints: new
                {
                    action = "Index|" +
                             "SE|" +
                             "SESE"
                }
            );

            // Other
            routes.MapRoute(
                name: "Other",
                url: "{id}/{action}",
                defaults: new { action = "Dashboard", controller = "Server" },
                constraints: new
                {
                    action = "Dashboard|" +
                             "Configuration|" +
                             "Settings|" +
                             "Maps|" +
                             "Explorer"
                }
            );

            // 404
            routes.MapRoute(
                name: "404",
                url: "{*url}",
                defaults: new { controller = "Error", action = "404" }
            );
        }
    }
}
