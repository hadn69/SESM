using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Server;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    
    public class ServerController : Controller
    {
        readonly DataContext _context = new DataContext();

        #region Index

        //
        // GET: Server
        // This page show the list of all the servers the current user is authorised to see
        [HttpGet]
        public ActionResult Index()
        {
            EntityUser user = Session["User"] as EntityUser;

            ServerProvider srvPrv = new ServerProvider(_context);

            List<EntityServer> serverList = srvPrv.GetServers(user);

            ViewData["AccessLevel"] = srvPrv.GetHighestAccessLevel(serverList, user);
            ViewData["ServerList"] = serverList;
            ViewData["StateList"] = srvPrv.GetState(serverList);
            
            
            return View();
        }

        #endregion

        #region Create
        //
        // GET: Server/Create
        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult Create()
        {
            NewServerViewModel model = new NewServerViewModel();
            return View(model);
        }

        //
        // POST: Server/Create
        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult Create(NewServerViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);

            if (ModelState.IsValid)
            {

                if (!srvPrv.CheckPortAvailability(model.ServerPort))
                {
                    ModelState.AddModelError("PortUnavailable", "The server port is already used (on " + srvPrv.GetServerByPort(model.ServerPort).Name + ")");
                    return View(model);
                }



                string[] webAdminsSplitted = model.WebAdministrators != null ? model.WebAdministrators.Split(';') : new string[0];
                string[] webManagerSplitted = model.WebManagers != null ? model.WebManagers.Split(';') : new string[0];
                string[] webUsersSplitted = model.WebUsers != null ? model.WebUsers.Split(';') : new string[0];
                UserProvider usrPrv = new UserProvider(_context);
                bool errorFlag = false;
                foreach (string item in webAdminsSplitted.Where(item => !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("adm" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach (string item in webManagerSplitted.Where(item => !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("man" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach (string item in webUsersSplitted.Where(item => !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("usr" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }

                if (errorFlag)
                    return View(model);

                EntityServer serv = new EntityServer();


                serv.Ip = model.IP;
                serv.Port = model.ServerPort;
                serv.Name = model.Name;
                serv.IsPublic = model.IsPublic;

                srvPrv.AddAdministrator(webAdminsSplitted, serv);
                srvPrv.AddManagers(webManagerSplitted, serv);
                srvPrv.AddUsers(webUsersSplitted, serv);

                srvPrv.CreateServer(serv);

                Directory.CreateDirectory(PathHelper.GetSavesPath(serv));
                Directory.CreateDirectory(PathHelper.GetInstancePath(serv) + @"Mods");

                ServerConfigHelper configHelper = new ServerConfigHelper();
                configHelper.ParseIn(model);
                configHelper.Save(serv);
                ServiceHelper.RegisterService(ServiceHelper.GetServiceName(serv));

                return RedirectToAction("Index","Map", new { id = serv.Id });
            }
            return View(model);
        }

        #endregion

        #region Status

        //
        // GET: Server/Status/5
        [HttpGet]
        [CheckAuth]
        public ActionResult Status(int? id)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            int serverId = id ?? 0;

            EntityServer serv = srvPrv.GetServer(serverId);

            ServerConfigHelper serverConfig = new ServerConfigHelper();
            serverConfig.Load(PathHelper.GetConfigurationFilePath(serv));

            ServerViewModel serverView = new ServerViewModel();
            serverView = serverConfig.ParseOut(serverView);
            serverView.Name = serv.Name;

            ViewData["State"] = srvPrv.GetState(serv);
            if (user == null)
                ViewData["AccessLevel"] = AccessLevel.Guest;
            else
                ViewData["AccessLevel"] = srvPrv.GetAccessLevel(user.Id, serv.Id);
            ViewData["ID"] = id;
            Response.AddHeader("Refresh", "10");
            return View(serverView);
        }

        #endregion

        

        // GET: Server/Delete/5
        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [SuperAdmin]
        public ActionResult Delete(int id)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityServer serv = srvPrv.GetServer(id);
            if (serv != null)
            {
                ServiceHelper.StopServiceAndWait(ServiceHelper.GetServiceName(serv));
                ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(serv));
                if (Directory.Exists(PathHelper.GetInstancePath(serv)))
                    Directory.Delete(PathHelper.GetInstancePath(serv), true);
                srvPrv.RemoveServer(serv);
            }

            return RedirectToAction("Index");
        }

        #region Start Stop Restart Kill

        #region Start
        //
        // GET: Server/Start/5
        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Start(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            ServiceHelper.StartService(ServiceHelper.GetServiceName(serv));

            return RedirectToAction("Status", new { id = id });
        }

        //
        // GET: Server/StartAll
        [HttpGet]
        [LoggedOnly]
        public ActionResult StartAll()
        {
            EntityUser user = Session["User"] as EntityUser;

            ServerProvider srvPrv = new ServerProvider(_context);
            List<EntityServer> serverList = srvPrv.GetServers(user);
            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                    ServiceHelper.StartService(ServiceHelper.GetServiceName(item));
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Stop
        //
        // GET: Server/Stop/5
        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Stop(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            ServiceHelper.StopService(ServiceHelper.GetServiceName(serv));

            return RedirectToAction("Status", new {id = id});
        }

        //
        // GET: Server/StopAll
        [HttpGet]
        [LoggedOnly]
        public ActionResult StopAll()
        {
            EntityUser user = Session["User"] as EntityUser;

            ServerProvider srvPrv = new ServerProvider(_context);
            List<EntityServer> serverList = srvPrv.GetServers(user);
            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                    ServiceHelper.StopService(ServiceHelper.GetServiceName(item));
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Restart
        //
        // GET: Server/Restart/5
        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Restart(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            ServiceHelper.RestartService(ServiceHelper.GetServiceName(serv));

            return RedirectToAction("Status", new { id = id });
        }

        //
        // GET: Server/RestartAll
        [HttpGet]
        [LoggedOnly]
        public ActionResult RestartAll()
        {
            EntityUser user = Session["User"] as EntityUser;

            ServerProvider srvPrv = new ServerProvider(_context);
            List<EntityServer> serverList = srvPrv.GetServers(user);
            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                    ServiceHelper.StopService(ServiceHelper.GetServiceName(item));
            }

            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                    ServiceHelper.WaitForStopped(ServiceHelper.GetServiceName(item));
            }

            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                    ServiceHelper.StartService(ServiceHelper.GetServiceName(item));
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Kill
        //
        // GET: Server/Kill/5
        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Kill(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);
            ServiceHelper.KillService(ServiceHelper.GetServiceName(serv));

            return RedirectToAction("Status", new { id = id });
        }

        //
        // GET: Server/KillAll
        [HttpGet]
        [LoggedOnly]
        public ActionResult KillAll()
        {
            EntityUser user = Session["User"] as EntityUser;

            ServerProvider srvPrv = new ServerProvider(_context);
            List<EntityServer> serverList = srvPrv.GetServers(user);
            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                    ServiceHelper.KillService(ServiceHelper.GetServiceName(item));
            }

            return RedirectToAction("Index");
        }

        #endregion

        #endregion
        //
        // GET: Server/Details/5
        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Details(int id)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            ViewData["ID"] = id;
            EntityServer serv = srvPrv.GetServer(id);
            AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, serv.Id);
            
            ViewData["AccessLevel"] = accessLevel;

            ServerConfigHelper serverConfig = new ServerConfigHelper();
            serverConfig.Load(PathHelper.GetConfigurationFilePath(serv));
            
            ServerViewModel serverView = new ServerViewModel();
            serverView = serverConfig.ParseOut(serverView);
            serverView.Name = serv.Name;
            serverView.IsPublic = serv.IsPublic;

            serverView.WebAdministrators = string.Join(";", serv.Administrators.Select(item => item.Login).ToList());
            serverView.WebManagers = string.Join(";", serv.Managers.Select(item => item.Login).ToList());
            serverView.WebUsers = string.Join(";", serv.Users.Select(item => item.Login).ToList());
            return View(serverView);
        }

        //
        // POST: Server/Details/5
        [HttpPost]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        [MultipleButton(Name = "action", Argument = "SaveRestartDetails")]
        public ActionResult DetailsSaveRestart(int? id, ServerViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            int serverId = id ?? 0;
            ViewData["ID"] = serverId;
            EntityServer serv = srvPrv.GetServer(serverId);

            AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, serv.Id);
            ViewData["AccessLevel"] = accessLevel;
            

            if (ModelState.IsValid)
            {

                ServerConfigHelper serverConfig = new ServerConfigHelper();
                serverConfig.Load(PathHelper.GetConfigurationFilePath(serv));

                List<string> WebAdminsList = serv.Administrators.Select(item => item.Login).ToList();
                List<string> WebManagersList = serv.Managers.Select(item => item.Login).ToList();
                List<string> WebUsersList = serv.Users.Select(item => item.Login).ToList();

                if ((accessLevel == AccessLevel.Manager || accessLevel == AccessLevel.Admin)
                    && model.WebAdministrators != string.Join(";", WebAdminsList))
                {
                    if (!(string.IsNullOrEmpty(model.WebAdministrators) &&
                          string.IsNullOrEmpty(string.Join(";", WebAdminsList))))
                    {
                        ModelState.AddModelError("AdminModified", "You can't modify the Web Administrator list");
                        return View("Details", model);
                    }
                }

                if (accessLevel == AccessLevel.Manager
                    && (model.Name != serv.Name
                    || model.ServerName != serverConfig.ServerName
                    || model.IP != serverConfig.IP
                    || model.ServerPort != serverConfig.ServerPort
                    || model.SteamPort != serverConfig.SteamPort
                    || model.GameMode != serverConfig.GameMode
                    || model.MaxPlayers != serverConfig.MaxPlayers
                    || model.MaxFloatingObjects != serverConfig.MaxFloatingObjects))
                {
                    ModelState.AddModelError("ManagerModified", "You can't modify the greyed fields, bad boy !");
                    return View("Details", model);
                }

                if (serv.Port != model.ServerPort && !srvPrv.CheckPortAvailability(model.ServerPort))
                {
                    ModelState.AddModelError("PortUnavailable", "The server port is already in use (on " + srvPrv.GetServerByPort(model.ServerPort).Name + ")");
                    return View("Details", model);
                }



                string[] webAdminsSplitted = model.WebAdministrators != null? model.WebAdministrators.Split(';'): new string[0];
                string[] webManagerSplitted = model.WebManagers != null ? model.WebManagers.Split(';') : new string[0];
                string[] webUsersSplitted = model.WebUsers != null ? model.WebUsers.Split(';') : new string[0];
                UserProvider usrPrv = new UserProvider(_context);
                bool errorFlag = false;
                foreach (string item in webAdminsSplitted.Where(item => !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("adm" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach (string item in webManagerSplitted.Where(item => !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("man" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach (string item in webUsersSplitted.Where(item => !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("usr" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }

                if (errorFlag)
                    return View("Details", model);

                serv.Ip = model.IP;
                serv.Port = model.ServerPort;
                serv.IsPublic = model.IsPublic;

                srvPrv.AddAdministrator(webAdminsSplitted, serv);
                srvPrv.AddManagers(webManagerSplitted, serv);
                srvPrv.AddUsers(webUsersSplitted, serv);

                srvPrv.UpdateServer(serv);

                ServiceHelper.StopServiceAndWait(ServiceHelper.GetServiceName(serv));

                if (model.Name != serv.Name)
                {
                    ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(serv));
                    
                    string oldPath = PathHelper.GetInstancePath(serv);
                    serv.Name = model.Name;
                    srvPrv.UpdateServer(serv);
                    if (Directory.Exists(oldPath))
                        Directory.Move(oldPath, PathHelper.GetInstancePath(serv));

                    ServiceHelper.RegisterService(ServiceHelper.GetServiceName(serv));
                }

                model.SaveName = serverConfig.SaveName;
                model.ScenarioType = serverConfig.ScenarioType;

                serverConfig.ParseIn(model);

                serverConfig.Save(serv);

                ServiceHelper.StartService(ServiceHelper.GetServiceName(serv));
                return RedirectToAction("Status", new { id = id });
            }
            return View("Details", model);
        }

        //
        // POST: Server/Details/5
        [HttpPost]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        [MultipleButton(Name = "action", Argument = "SaveDetails")]
        public ActionResult DetailsSave(int id, ServerViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            ViewData["ID"] = id;
            EntityServer serv = srvPrv.GetServer(id);

            AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, serv.Id);
            ViewData["AccessLevel"] = accessLevel;


            if (ModelState.IsValid)
            {

                ServerConfigHelper serverConfig = new ServerConfigHelper();
                serverConfig.Load(PathHelper.GetConfigurationFilePath(serv));

                List<string> WebAdminsList = serv.Administrators.Select(item => item.Login).ToList();
                List<string> WebManagersList = serv.Managers.Select(item => item.Login).ToList();
                List<string> WebUsersList = serv.Users.Select(item => item.Login).ToList();




                if ((accessLevel == AccessLevel.Manager || accessLevel == AccessLevel.Admin)
                    && model.WebAdministrators != string.Join(";", WebAdminsList))
                {
                    if (!(string.IsNullOrEmpty(model.WebAdministrators) &&
                          string.IsNullOrEmpty(string.Join(";", WebAdminsList))))
                    {
                        ModelState.AddModelError("AdminModified", "You can't modify the Web Administrator list");
                        return View("Details", model);
                    }
                }
                if (accessLevel == AccessLevel.Manager
                    && (model.Name != serv.Name
                    || model.ServerName != serverConfig.ServerName
                    || model.IP != serverConfig.IP
                    || model.ServerPort != serverConfig.ServerPort
                    || model.SteamPort != serverConfig.SteamPort
                    || model.GameMode != serverConfig.GameMode
                    || model.MaxPlayers != serverConfig.MaxPlayers
                    || model.MaxFloatingObjects != serverConfig.MaxFloatingObjects))
                {
                    ModelState.AddModelError("ManagerModified", "You can't modify the greyed fields, bad boy !");
                    return View("Details", model);
                }

                if (serv.Port != model.ServerPort && !srvPrv.CheckPortAvailability(model.ServerPort))
                {
                    ModelState.AddModelError("PortUnavailable", "The server port is already in use (on " + srvPrv.GetServerByPort(model.ServerPort).Name + ")");
                    return View("Details", model);
                }

                string[] webAdminsSplitted = model.WebAdministrators != null ? model.WebAdministrators.Split(';') : new string[0];
                string[] webManagerSplitted = model.WebManagers != null ? model.WebManagers.Split(';') : new string[0];
                string[] webUsersSplitted = model.WebUsers != null ? model.WebUsers.Split(';') : new string[0];
                UserProvider usrPrv = new UserProvider(_context);
                bool errorFlag = false;
                foreach (string item in webAdminsSplitted.Where(item => !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("adm" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach (string item in webManagerSplitted.Where(item => !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("man" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach (string item in webUsersSplitted.Where(item => !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("usr" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }

                if (errorFlag)
                    return View("Details", model);

                serv.Ip = model.IP;
                serv.Port = model.ServerPort;
                serv.IsPublic = model.IsPublic;

                srvPrv.AddAdministrator(webAdminsSplitted, serv);
                srvPrv.AddManagers(webManagerSplitted, serv);
                srvPrv.AddUsers(webUsersSplitted, serv);

                srvPrv.UpdateServer(serv);

                if (model.Name != serv.Name)
                {
                    ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(serv));

                    string oldPath = PathHelper.GetInstancePath(serv);
                    serv.Name = model.Name;
                    srvPrv.UpdateServer(serv);
                    if (Directory.Exists(oldPath))
                        Directory.Move(oldPath, PathHelper.GetInstancePath(serv));

                    ServiceHelper.RegisterService(ServiceHelper.GetServiceName(serv));
                }

                model.SaveName = serverConfig.SaveName;
                model.ScenarioType = serverConfig.ScenarioType;

                serverConfig.ParseIn(model);

                serverConfig.Save(serv);
                return RedirectToAction("Status", new {id = id});
            }
            return View("Details", model);
        }

        //
        // GET: Server/StatsHourly/5
        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult HourlyStats(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);
            ViewData["ID"] = id;
            List<EntityPerfEntry> perfEntries = serv.PerfEntries.Where(x => x.Timestamp >= DateTime.Now.AddHours(-2)).OrderBy(x => x.Timestamp).ToList();
            ViewData["perfEntries"] = perfEntries;
            return View();
        }

        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult GlobalStats(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            List<EntityPerfEntry> perfEntries = serv.PerfEntries.Where(x => x.CPUUsagePeak != null).OrderBy(x => x.Timestamp).ToList();
            ViewData["perfEntries"] = perfEntries;
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        
    }
}
