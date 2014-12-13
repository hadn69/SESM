using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using NLog;
using Quartz;
using Quartz.Impl;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Server;
using SESM.Tools;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    [CheckLockout]
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
                    return View(model).Danger("The server port is already used (on " + srvPrv.GetServerByPort(model.ServerPort).Name + ")");
                }
                if(!srvPrv.CheckSESEPortAvailability(model.ServerExtenderPort))
                {
                    ModelState.AddModelError("PortUnavailable", "The server extender port is already in use (on " + srvPrv.GetServerBySESEPort(model.ServerExtenderPort).Name + ")");
                    return View(model).Danger("The server extender port is already in use (on " + srvPrv.GetServerBySESEPort(model.ServerExtenderPort).Name + ")");
                }

                string[] webAdminsSplitted = model.WebAdministrators != null ? model.WebAdministrators.Split(';') : new string[0];
                string[] webManagerSplitted = model.WebManagers != null ? model.WebManagers.Split(';') : new string[0];
                string[] webUsersSplitted = model.WebUsers != null ? model.WebUsers.Split(';') : new string[0];
                UserProvider usrPrv = new UserProvider(_context);
                bool errorFlag = false;
                foreach(string item in webAdminsSplitted.Where(item => !string.IsNullOrWhiteSpace(item) && !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("adm" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach(string item in webManagerSplitted.Where(item => !string.IsNullOrWhiteSpace(item) && !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("man" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach(string item in webUsersSplitted.Where(item => !string.IsNullOrWhiteSpace(item) && !usrPrv.UserExist(item)))
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
                serv.IsAutoRestartEnabled = model.AutoRestart;
                serv.AutoRestartCron = model.AutoRestartCron;
                serv.UseServerExtender = model.UseServerExtender;
                serv.ServerExtenderPort = model.ServerExtenderPort;
                serv.IsAutoStartEnabled = model.AutoStart;

                srvPrv.AddAdministrator(webAdminsSplitted, serv);
                srvPrv.AddManagers(webManagerSplitted, serv);
                srvPrv.AddUsers(webUsersSplitted, serv);

                srvPrv.CreateServer(serv);

                Directory.CreateDirectory(PathHelper.GetSavesPath(serv));
                Directory.CreateDirectory(PathHelper.GetInstancePath(serv) + @"Mods");

                ServerConfigHelper configHelper = new ServerConfigHelper();
                configHelper.ParseIn(model);
                configHelper.Save(serv);
                if(serv.UseServerExtender)
                    ServiceHelper.RegisterServerExtenderService(serv);
                else
                    ServiceHelper.RegisterService(ServiceHelper.GetServiceName(serv));

                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                if(serv.IsAutoRestartEnabled)
                {
                    IJobDetail autoRestartJob = JobBuilder.Create<AutoRestartJob>()
                        .WithIdentity("AutoRestart" + serv.Id + "Job", "AutoRestart")
                        .UsingJobData("id", serv.Id)
                        .Build();

                    ITrigger autoRestartTrigger = TriggerBuilder.Create()
                        .WithIdentity("AutoRestart" + serv.Id + "Trigger", "AutoRestart")
                        .WithCronSchedule(model.AutoRestartCron)
                        .StartNow()
                        .Build();

                    scheduler.ScheduleJob(autoRestartJob, autoRestartTrigger);
                }

                return RedirectToAction("Index","Map", new { id = serv.Id }).Success("Server created sucessfuly, you may now create a map or upload one in the map manager.");
            }
            return View(model);
        }

        #endregion

        #region Status

        //
        // GET: Server/Status/5
        [HttpGet]
        [CheckAuth]
        public ActionResult Status(int id)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityServer serv = srvPrv.GetServer(id);

            ServerConfigHelper serverConfig = new ServerConfigHelper();
            serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));

            ServerViewModel serverView = new ServerViewModel();
            serverView = serverConfig.ParseOut(serverView);
            serverView.Name = serv.Name;
            serverView.IsLvl1BackupEnabled = serv.IsLvl1BackupEnabled;
            serverView.IsLvl2BackupEnabled = serv.IsLvl2BackupEnabled;
            serverView.IsLvl3BackupEnabled = serv.IsLvl3BackupEnabled;

            ViewData["State"] = srvPrv.GetState(serv);
            if (user == null)
                ViewData["AccessLevel"] = AccessLevel.Guest;
            else
                ViewData["AccessLevel"] = srvPrv.GetAccessLevel(user.Id, serv.Id);
            ViewData["ID"] = id;

            if (SESMConfigHelper.StatusAutoRefresh)
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
                Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                serviceLogger.Info(serv.Name + " stopped by " + user.Login + " for server deletion");
                ServiceHelper.StopServiceAndWait(serv);
                ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(serv));
                if (Directory.Exists(PathHelper.GetInstancePath(serv)))
                    Directory.Delete(PathHelper.GetInstancePath(serv), true);
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                scheduler.DeleteJob(new JobKey("LowPriorityStart" + serv.Id + "Job", "LowPriorityStart"));
                scheduler.DeleteJob(new JobKey("AutoRestart" + serv.Id + "Job", "AutoRestart"));
                srvPrv.RemoveServer(serv);

                return RedirectToAction("Index").Success("Server Deleted");
            }

            return RedirectToAction("Index").Danger("Unknown Server");
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
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            serviceLogger.Info(serv.Name + " started by " + user.Login + " by start button");
            ServiceHelper.StartService(serv);

            return RedirectToAction("Status", new { id = id }).Success("Server Started");
        }

        //
        // GET: Server/StartAll
        [HttpGet]
        [LoggedOnly]
        public ActionResult StartAll()
        {
            EntityUser user = Session["User"] as EntityUser;
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            ServerProvider srvPrv = new ServerProvider(_context);
            List<EntityServer> serverList = srvPrv.GetServers(user);
            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                {
                    serviceLogger.Info(item.Name + " started by " + user.Login + " by startAll button");
                    ServiceHelper.StartService(item);
                }
            }

            return RedirectToAction("Index").Success("All Server Started");
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
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            serviceLogger.Info(serv.Name + " stopped by " + user.Login + " by stop button");
            ServiceHelper.StopService(serv);

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
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                {
                    serviceLogger.Info(item.Name + " stopped by " + user.Login + " by stopAll button");
                    ServiceHelper.StopService(item);
                }
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
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            serviceLogger.Info(serv.Name + " restarted by " + user.Login + " by restart button");
            ServiceHelper.RestartService(serv);

            return RedirectToAction("Status", new { id = id });
        }

        //
        // GET: Server/RestartAll
        [HttpGet]
        [LoggedOnly]
        public ActionResult RestartAll()
        {
            EntityUser user = Session["User"] as EntityUser;
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

            ServerProvider srvPrv = new ServerProvider(_context);
            List<EntityServer> serverList = srvPrv.GetServers(user);
            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                {
                    serviceLogger.Info(item.Name + " stopped by " + user.Login + " by restartAll button");
                    ServiceHelper.StopService(item);
                }
            }

            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                    ServiceHelper.WaitForStopped(item);
            }

            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                {
                    serviceLogger.Info(item.Name + " started by " + user.Login + " by restartAll button");
                    ServiceHelper.StartService(item);
                }
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
            EntityUser user = Session["User"] as EntityUser;
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);
            serviceLogger.Info(serv.Name + " killed by " + user.Login + " by kill button");
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
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            ServerProvider srvPrv = new ServerProvider(_context);
            List<EntityServer> serverList = srvPrv.GetServers(user);
            foreach (EntityServer item in serverList)
            {
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, item.Id);
                if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
                {
                    serviceLogger.Info(item.Name + " killed by " + user.Login + " by killAll button");
                    ServiceHelper.KillService(ServiceHelper.GetServiceName(item));
                }
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
            serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));
            
            ServerViewModel serverView = new ServerViewModel();
            serverView = serverConfig.ParseOut(serverView);
            serverView.Name = serv.Name;
            serverView.IsPublic = serv.IsPublic;
            serverView.IsLvl1BackupEnabled = serv.IsLvl1BackupEnabled;
            serverView.IsLvl2BackupEnabled = serv.IsLvl2BackupEnabled;
            serverView.IsLvl3BackupEnabled = serv.IsLvl3BackupEnabled;
            serverView.AutoRestart = serv.IsAutoRestartEnabled;
            serverView.AutoRestartCron = serv.AutoRestartCron;
            serverView.UseServerExtender = serv.UseServerExtender;
            serverView.ServerExtenderPort = serv.ServerExtenderPort;
            serverView.AutoStart = serv.IsAutoStartEnabled;
            serverView.ProcessPriority = serv.ProcessPriority;

            if (serv.AutoSaveInMinutes != null)
                serverView.AutoSaveInMinutes = serv.AutoSaveInMinutes?? -42;

            serverView.WebAdministrators = string.Join("\r\n", serv.Administrators.Select(item => item.Login).ToList());
            serverView.WebManagers = string.Join("\r\n", serv.Managers.Select(item => item.Login).ToList());
            serverView.WebUsers = string.Join("\r\n", serv.Users.Select(item => item.Login).ToList());
            if (!string.IsNullOrEmpty(serverView.SaveName))
                serverView.AsteroidAmount = Directory.GetFiles(PathHelper.GetSavePath(serv, serverView.SaveName), "*steroid???.vx2").Length;
 
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
                serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));

                List<string> WebAdminsList = serv.Administrators.Select(item => item.Login).ToList();
                List<string> WebManagersList = serv.Managers.Select(item => item.Login).ToList();
                List<string> WebUsersList = serv.Users.Select(item => item.Login).ToList();

                if ((accessLevel == AccessLevel.Manager || accessLevel == AccessLevel.Admin)
                    && model.WebAdministrators != string.Join("\r\n", WebAdminsList))
                {
                    if (!(string.IsNullOrEmpty(model.WebAdministrators) &&
                          string.IsNullOrEmpty(string.Join("\r\n", WebAdminsList))))
                    {
                        ModelState.AddModelError("AdminModified", "You can't modify the Web Administrator list");
                        return View("Details", model).Danger("You can't modify the Web Administrator list");
                    }
                }

                if (accessLevel == AccessLevel.Manager
                    && (model.Name != serv.Name
                    || model.IP != serverConfig.IP
                    || model.ServerPort != serverConfig.ServerPort
                    || model.SteamPort != serverConfig.SteamPort
                    || model.MaxPlayers != serverConfig.MaxPlayers
                    || model.MaxFloatingObjects != serverConfig.MaxFloatingObjects
                    || model.RemoveTrash != serverConfig.RemoveTrash
                    || model.ProcessPriority != serv.ProcessPriority
                    || model.ServerExtenderPort != serv.ServerExtenderPort))
                {
                    ModelState.AddModelError("ManagerModified", "You can't modify the greyed fields, bad boy !");
                    return View("Details", model).Danger("You can't modify the greyed fields, bad boy !");
                }

                if (serv.Port != model.ServerPort 
                    && !srvPrv.CheckPortAvailability(model.ServerPort))
                {
                    ModelState.AddModelError("PortUnavailable", "The server port is already in use (on " + srvPrv.GetServerByPort(model.ServerPort).Name + ")");
                    return View("Details", model).Danger("The server port is already in use (on " + srvPrv.GetServerByPort(model.ServerPort).Name + ")");
                }
                if(model.UseServerExtender
                    && serv.ServerExtenderPort != model.ServerExtenderPort
                    && !srvPrv.CheckSESEPortAvailability(model.ServerExtenderPort))
                {
                    ModelState.AddModelError("PortUnavailable", "The server extender port is already in use (on " + srvPrv.GetServerBySESEPort(model.ServerExtenderPort).Name + ")");
                    return View("Details", model).Danger("The server extender port is already in use (on " + srvPrv.GetServerBySESEPort(model.ServerExtenderPort).Name + ")");
                }


                string[] webAdminsSplitted = model.WebAdministrators != null ? Regex.Split(model.WebAdministrators, "\r\n") : new string[0];
                string[] webManagerSplitted = model.WebManagers != null ? Regex.Split(model.WebManagers, "\r\n") : new string[0];
                string[] webUsersSplitted = model.WebUsers != null ? Regex.Split(model.WebUsers, "\r\n") : new string[0];
                UserProvider usrPrv = new UserProvider(_context);
                bool errorFlag = false;
                foreach(string item in webAdminsSplitted.Where(item => !string.IsNullOrWhiteSpace(item) && !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("adm" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach(string item in webManagerSplitted.Where(item => !string.IsNullOrWhiteSpace(item) && !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("man" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }
                foreach(string item in webUsersSplitted.Where(item => !string.IsNullOrWhiteSpace(item) && !usrPrv.UserExist(item)))
                {
                    ModelState.AddModelError("usr" + item, "The user '" + item + " don't exist");
                    errorFlag = true;
                }

                if (errorFlag)
                    return View("Details", model);

                serv.Ip = model.IP;
                serv.Port = model.ServerPort;
                serv.IsPublic = model.IsPublic;

                serv.IsLvl1BackupEnabled = model.IsLvl1BackupEnabled;
                serv.IsLvl2BackupEnabled = model.IsLvl2BackupEnabled;
                serv.IsLvl3BackupEnabled = model.IsLvl3BackupEnabled;

                srvPrv.AddAdministrator(webAdminsSplitted, serv);
                srvPrv.AddManagers(webManagerSplitted, serv);
                srvPrv.AddUsers(webUsersSplitted, serv);

                serv.IsAutoRestartEnabled = model.AutoRestart;
                serv.AutoRestartCron = model.AutoRestartCron;
                serv.IsAutoStartEnabled = model.AutoStart;
                serv.AutoSaveInMinutes = model.AutoSaveInMinutes;

                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                scheduler.DeleteJob(new JobKey("AutoRestart" + serv.Id + "Job", "AutoRestart"));
                if (serv.IsAutoRestartEnabled)
                {
                    IJobDetail autoRestartJob = JobBuilder.Create<AutoRestartJob>()
                        .WithIdentity("AutoRestart" + serv.Id + "Job", "AutoRestart")
                        .UsingJobData("id", serv.Id)
                        .Build();

                    ITrigger autoRestartTrigger = TriggerBuilder.Create()
                        .WithIdentity("AutoRestart" + serv.Id + "Trigger", "AutoRestart")
                        .WithCronSchedule(model.AutoRestartCron)
                        .StartNow()
                        .Build();

                    scheduler.ScheduleJob(autoRestartJob, autoRestartTrigger);
                }

                serv.ProcessPriority = model.ProcessPriority;


                srvPrv.UpdateServer(serv);
                Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                serviceLogger.Info(serv.Name + " stopped by " + user.Login + " by save and restart in configuration");
                ServiceHelper.StopServiceAndWait(serv);

                if (model.Name != serv.Name 
                    || model.UseServerExtender != serv.UseServerExtender 
                    || model.ServerExtenderPort != serv.ServerExtenderPort)
                {
                    ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(serv));
                    if (model.Name != serv.Name)
                    {
                        string oldPath = PathHelper.GetInstancePath(serv);
                        serv.Name = model.Name;
                        srvPrv.UpdateServer(serv);
                        if (Directory.Exists(oldPath))
                            Directory.Move(oldPath, PathHelper.GetInstancePath(serv));
                    }
                    serv.UseServerExtender = model.UseServerExtender;
                    serv.ServerExtenderPort = model.ServerExtenderPort;
                    
                    srvPrv.UpdateServer(serv);

                    if (model.UseServerExtender)
                    {
                        ServiceHelper.RegisterServerExtenderService(serv);
                    }
                    else
                    {
                        ServiceHelper.RegisterService(ServiceHelper.GetServiceName(serv));
                    }
                }

                model.SaveName = serverConfig.SaveName;
                model.ScenarioType = serverConfig.ScenarioType;

                if(model.UseServerExtender)
                    model.AutoSaveInMinutes = 0;

                serverConfig.ParseIn(model);

                serverConfig.Save(serv);
                serviceLogger.Info(serv.Name + " started by " + user.Login + " by save and restart in configuration");
                ServiceHelper.StartService(serv);
                return RedirectToAction("Status", new { id = id }).Success("Server Configuration Updated");
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
                serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));

                List<string> WebAdminsList = serv.Administrators.Select(item => item.Login).ToList();
                List<string> WebManagersList = serv.Managers.Select(item => item.Login).ToList();
                List<string> WebUsersList = serv.Users.Select(item => item.Login).ToList();

                if ((accessLevel == AccessLevel.Manager || accessLevel == AccessLevel.Admin)
                    && model.WebAdministrators != string.Join("\r\n", WebAdminsList))
                {
                    if (!(string.IsNullOrEmpty(model.WebAdministrators) &&
                          string.IsNullOrEmpty(string.Join("\r\n", WebAdminsList))))
                    {
                        ModelState.AddModelError("AdminModified", "You can't modify the Web Administrator list");
                        return View("Details", model).Danger("You can't modify the Web Administrator list");
                    }
                }
                if (accessLevel == AccessLevel.Manager
                    && (model.Name != serv.Name
                    || model.IP != serverConfig.IP
                    || model.ServerPort != serverConfig.ServerPort
                    || model.SteamPort != serverConfig.SteamPort
                    || model.MaxPlayers != serverConfig.MaxPlayers
                    || model.MaxFloatingObjects != serverConfig.MaxFloatingObjects
                    || model.RemoveTrash != serverConfig.RemoveTrash
                    || model.ProcessPriority != serv.ProcessPriority
                    || model.ServerExtenderPort != serv.ServerExtenderPort))
                {
                    ModelState.AddModelError("ManagerModified", "You can't modify the greyed fields, bad boy !");
                    return View("Details", model).Danger("You can't modify the greyed fields, bad boy !");
                }

                if (serv.Port != model.ServerPort && !srvPrv.CheckPortAvailability(model.ServerPort))
                {
                    ModelState.AddModelError("PortUnavailable", "The server port is already in use (on " + srvPrv.GetServerByPort(model.ServerPort).Name + ")");
                    return View("Details", model).Danger("The server port is already in use (on " + srvPrv.GetServerByPort(model.ServerPort).Name + ")");
                }
                if(model.UseServerExtender
                    && serv.ServerExtenderPort != model.ServerExtenderPort
                    && !srvPrv.CheckSESEPortAvailability(model.ServerExtenderPort))
                {
                    ModelState.AddModelError("PortUnavailable", "The server extender port is already in use (on " + srvPrv.GetServerBySESEPort(model.ServerExtenderPort).Name + ")");
                    return View("Details", model).Danger("The server extender port is already in use (on " + srvPrv.GetServerBySESEPort(model.ServerExtenderPort).Name + ")");
                }

                string[] webAdminsSplitted = model.WebAdministrators != null ? Regex.Split(model.WebAdministrators, "\r\n") : new string[0];
                string[] webManagerSplitted = model.WebManagers != null ? Regex.Split(model.WebManagers, "\r\n") : new string[0];
                string[] webUsersSplitted = model.WebUsers != null ? Regex.Split(model.WebUsers, "\r\n") : new string[0];
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

                if ((srvPrv.GetState(serv) != ServiceState.Stopped)
                    && (model.Name != serv.Name
                    || model.UseServerExtender != serv.UseServerExtender
                    || model.ServerExtenderPort != serv.ServerExtenderPort))
                {
                    return View("Details", model).Danger("You can't change the name, activate/deactivate SE Server Extender or change it's port whan the server is running !");
                }

                serv.Ip = model.IP;
                serv.Port = model.ServerPort;
                serv.IsPublic = model.IsPublic;

                serv.IsLvl1BackupEnabled = model.IsLvl1BackupEnabled;
                serv.IsLvl2BackupEnabled = model.IsLvl2BackupEnabled;
                serv.IsLvl3BackupEnabled = model.IsLvl3BackupEnabled;

                srvPrv.AddAdministrator(webAdminsSplitted, serv);
                srvPrv.AddManagers(webManagerSplitted, serv);
                srvPrv.AddUsers(webUsersSplitted, serv);

                serv.IsAutoRestartEnabled = model.AutoRestart;
                serv.AutoRestartCron = model.AutoRestartCron;
                serv.IsAutoStartEnabled = model.AutoStart;
                serv.AutoSaveInMinutes = model.AutoSaveInMinutes;

                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                scheduler.DeleteJob(new JobKey("AutoRestart" + serv.Id + "Job", "AutoRestart"));
                if(serv.IsAutoRestartEnabled)
                {
                    IJobDetail autoRestartJob = JobBuilder.Create<AutoRestartJob>()
                        .WithIdentity("AutoRestart" + serv.Id + "Job", "AutoRestart")
                        .UsingJobData("id", serv.Id)
                        .Build();

                    ITrigger autoRestartTrigger = TriggerBuilder.Create()
                        .WithIdentity("AutoRestart" + serv.Id + "Trigger", "AutoRestart")
                        .WithCronSchedule(model.AutoRestartCron)
                        .StartNow()
                        .Build();

                    scheduler.ScheduleJob(autoRestartJob, autoRestartTrigger);
                }
                serv.ProcessPriority = model.ProcessPriority;
                srvPrv.UpdateServer(serv);

                if (srvPrv.GetState(serv) != ServiceState.Stopped)
                {
                    ServiceHelper.SetPriority(serv);
                }

                if(model.Name != serv.Name
                    || model.UseServerExtender != serv.UseServerExtender
                    || model.ServerExtenderPort != serv.ServerExtenderPort)
                {
                    ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(serv));
                    if(model.Name != serv.Name)
                    {
                        string oldPath = PathHelper.GetInstancePath(serv);
                        serv.Name = model.Name;
                        srvPrv.UpdateServer(serv);
                        if(Directory.Exists(oldPath))
                            Directory.Move(oldPath, PathHelper.GetInstancePath(serv));
                    }
                    serv.UseServerExtender = model.UseServerExtender;
                    serv.ServerExtenderPort = model.ServerExtenderPort;
                    srvPrv.UpdateServer(serv);
                    if(model.UseServerExtender)
                    {
                        ServiceHelper.RegisterServerExtenderService(serv);
                    }
                    else
                    {
                        ServiceHelper.RegisterService(ServiceHelper.GetServiceName(serv));
                    }
                }

                model.SaveName = serverConfig.SaveName;
                model.ScenarioType = serverConfig.ScenarioType;

                if (model.UseServerExtender)
                    model.AutoSaveInMinutes = 0;
                

                serverConfig.ParseIn(model);

                serverConfig.Save(serv);
                return RedirectToAction("Status", new { id = id }).Success("Server Configuration Updated");
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
