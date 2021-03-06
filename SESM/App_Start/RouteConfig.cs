﻿using System.Web.Mvc;
using System.Web.Routing;

namespace SESM
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            #region API

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

            // Perf Mon API
            routes.MapRoute(
                name: "APIPerfMon",
                url: "API/PerfMon/{action}",
                defaults: new { controller = "APIPerfMon" },
                constraints: new
                {
                    action = "GetPerfData|" +
                             "CleanPerfData"
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

                             "GetMEStatus|" +
                             "GetMEVersion|" +
                             "GetMESettings|" +
                             "SetMESettings|" +
                             "UploadME|" +
                             "UpdateME|" +

                             "GetSESEStatus|" +
                             "GetSESEVersion|" +
                             "GetSESESettings|" +
                             "SetSESESettings|" +
                             "UploadSESE|" +
                             "UpdateSESE|" +
                             "DeleteSESE|" +

                             "GetCachedVersions"
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
                             "GetServerRoles|" +
                             "GetServerRoleAccess|" +
                             "GetServerPermissions|" +
                             "GetServerRoleDetails|" +
                             "SetServerRoleDetails|" +
                             "GetUsers|" +

                             "SEGetConfiguration|" +
                             "SEGetConfigurationRights|" +
                             "SESetConfiguration|" +

                             "MEGetConfiguration|" +
                             "MEGetConfigurationRights|" +
                             "MESetConfiguration|" +

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
                             "DeleteMaps|" +
                             "DownloadMaps|" +
                             "UploadMap|" +
                             "SECreateMap|" +
                             "MECreateMap"
                }
            );

            // Access API
            routes.MapRoute(
                name: "APIAccess",
                url: "API/Access/{action}",
                defaults: new { controller = "APIAccess" },
                constraints: new
                {
                    action = "GetHostRoles|" +
                             "GetHostRoleAccess|" +
                             "GetHostPermissions|" +
                             "GetHostRoleDetails|" +
                             "SetHostRoleDetails|" +
                             "CreateHostRole|" +
                             "DeleteHostRole|" +

                             "GetServerRoles|" +
                             "GetServerRoleAccess|" +
                             "GetServerPermissions|" +
                             "GetServerRoleDetails|" +
                             "SetServerRoleDetails|" +
                             "CreateServerRole|" +
                             "DeleteServerRole|" +
                             "GetUsers"
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
                             "SetFileContent|" +
                             "GetDllVersion"
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
                             "GetHostPerms|" +
                             "SetDetails"
                }
            );

            // Users API
            routes.MapRoute(
                name: "APIUsers",
                url: "API/Users/{action}",
                defaults: new { controller = "APIUsers" },
                constraints: new
                {
                    action = "GetUsers|" +
                             "SetUser|" +
                             "DeleteUsers|" +
                             "CreateUser"
                }
            );

            // SESE API
            routes.MapRoute(
                name: "APISESE",
                url: "API/SESE/{action}",
                defaults: new { controller = "APISESE" },
                constraints: new
                {
                    action = "TestWCF" 
                }
            );

            // API 404
            routes.MapRoute(
                name: "404API",
                url: "API/{*url}",
                defaults: new { controller = "Error", action = "404API" }
            );
            #endregion

            // === GUI ===

            // Account
            routes.MapRoute(
                name: "Account",
                url: "Account/{action}",
                defaults: new { controller = "Account" },
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
                defaults: new { controller = "Settings", action = "Index" },
                constraints: new
                {
                    action = "Index|" +
                             "SE|" +
                             "ME|" +
                             "SESE"
                }
            );

            // Users
            routes.MapRoute(
                name: "Users",
                url: "Users",
                defaults: new { controller = "Users", action = "Index" }
            );

            // Access
            routes.MapRoute(
                name: "Access",
                url: "Access",
                defaults: new { controller = "Access", action = "Index" }
            );

            // Stats
            routes.MapRoute(
                name: "Monitor",
                url: "Monitor",
                defaults: new { controller = "PerfMon", action = "Index" }
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
                             "Monitor|" +
                             "Access|" +
                             "Explorer",
                    id = @"\d+"
                }
            );

            // Home
            routes.MapRoute(
                name: "Home",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );

            // Version
            routes.MapRoute(
                name: "Versions",
                url: "Versions",
                defaults: new { controller = "Home", action = "Versions" }
            );

            // Server List
            routes.MapRoute(
                name: "ServerList",
                url: "Servers",
                defaults: new { controller = "Server", action = "Index" }
            );

            // 404
            routes.MapRoute(
                name: "404",
                url: "{*url}",
                defaults: new { controller = "Error", action = "404" }
            );
            routes.MapRoute(
                name: "404URL",
                url: "Error/404",
                defaults: new { controller = "Error", action = "404" }
            );
        }
    }
}
