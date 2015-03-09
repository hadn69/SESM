using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NLog;
using Quartz;
using Quartz.Impl;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Server;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    [CheckLockout]
    public class ServerController : Controller
    {
        readonly DataContext _context = new DataContext();

        // GET: Server
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

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
