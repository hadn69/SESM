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

        //
        // GET: Server
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: Server/Configuration
        [HttpGet]
        [CheckAuth]
        public ActionResult Configuration(int id)
        {
            return View();
        }

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
            //serverView = serverConfig.ParseOut(serverView);
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
                serverView.AutoSaveInMinutes = serv.AutoSaveInMinutes ?? -42;

            serverView.WebAdministrators = string.Join("\r\n", serv.Administrators.Select(item => item.Login).ToList());
            serverView.WebManagers = string.Join("\r\n", serv.Managers.Select(item => item.Login).ToList());
            serverView.WebUsers = string.Join("\r\n", serv.Users.Select(item => item.Login).ToList());
            if (!string.IsNullOrEmpty(serverView.SaveName) && Directory.Exists(PathHelper.GetSavePath(serv, serverView.SaveName)))
                serverView.AsteroidAmount = Directory.GetFiles(PathHelper.GetSavePath(serv, serverView.SaveName), "*steroid???.vx2").Length;

            return View(serverView);
        }
        /*
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
        */
        /*
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

                if((!(srvPrv.GetState(serv) == ServiceState.Stopped || srvPrv.GetState(serv) == ServiceState.Unknow))
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
        */
        //
        // GET: Server/HourlyStats/5
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
            ViewData["ID"] = id;
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
