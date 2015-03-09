using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Ionic.Zip;
using NLog;
using Quartz;
using Quartz.Impl;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Settings;
using SESM.Tools;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{

    public class SettingsController : Controller
    {
        private readonly DataContext _context = new DataContext();

        [HttpGet]
        [SuperAdmin]
        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        [SuperAdmin]
        public ActionResult SESE()
        {

            return View();
        }

       
        //
        // POST: Settings
        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult Index(SettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                EntityUser user = Session["User"] as EntityUser;
                ServerProvider srvPrv = new ServerProvider(_context);
                bool flag = false;
                if (model.SEDataPath.Substring(model.SEDataPath.Length - 1, 1) != @"\")
                {
                    flag = true;
                    ModelState.AddModelError("DataPath", @"The server path must end with \");
                }
                if (model.SESavePath.Substring(model.SESavePath.Length - 1, 1) != @"\")
                {
                    flag = true;
                    ModelState.AddModelError("SavePath", @"The save path must end with \");
                }

                if (flag)
                    return View(model);

                if (model.Prefix != SESMConfigHelper.Prefix
                    || model.SEDataPath != SESMConfigHelper.SEDataPath
                    || model.SESavePath != SESMConfigHelper.SESavePath
                    || model.Arch != SESMConfigHelper.Arch
                    || model.AddDateToLog != SESMConfigHelper.AddDateToLog
                    || model.SendLogToKeen != SESMConfigHelper.SendLogToKeen)
                {
                    // Getting started server list
                    List<EntityServer> listStartedServ =
                        srvPrv.GetAllServers().Where(item => srvPrv.GetState(item) == ServiceState.Running).ToList();
                    Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        serviceLogger.Info(item.Name + " stopped by " + user.Login + " to Update Global Settings");
                        ServiceHelper.StopService(item);
                    }

                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        ServiceHelper.WaitForStopped(item);
                    }

                    // Killing some ghost processes that might still exists
                    ServiceHelper.KillAllService();

                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(item));
                    }
                    
                    if (model.SEDataPath != SESMConfigHelper.SEDataPath)
                    {
                        /*
                        if (!Directory.Exists(model.SEDataPath))
                            Directory.CreateDirectory(model.SEDataPath);
                        if (Directory.Exists(model.SEDataPath + @"Content\"))
                            Directory.Delete(model.SEDataPath + @"Content\", true);
                        if (Directory.Exists(model.SEDataPath + @"DedicatedServer\"))
                            Directory.Delete(model.SEDataPath + @"DedicatedServer\", true);
                        if (Directory.Exists(model.SEDataPath + @"DedicatedServer64\"))
                            Directory.Delete(model.SEDataPath + @"DedicatedServer64\", true);
                        if (Directory.Exists(model.SEDataPath + @"SteamCMD\"))
                            Directory.Delete(model.SEDataPath + @"SteamCMD\", true);
                        if (Directory.Exists(model.SEDataPath + @"autoupdatedata\"))
                            Directory.Delete(model.SEDataPath + @"autoupdatedata\", true);



                        if (Directory.Exists(SESMConfigHelper.SEDataPath + @"Content\"))
                            Directory.Move(SESMConfigHelper.SEDataPath + @"Content\", model.SEDataPath + @"Content\");
                        if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer\"))
                            Directory.Move(SESMConfigHelper.SEDataPath + @"DedicatedServer\",
                                model.SEDataPath + @"DedicatedServer\");
                        if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer64\"))
                            Directory.Move(SESMConfigHelper.SEDataPath + @"DedicatedServer64\",
                                model.SEDataPath + @"DedicatedServer64\");
                        if (Directory.Exists(SESMConfigHelper.SEDataPath + @"SteamCMD\"))
                            Directory.Move(SESMConfigHelper.SEDataPath + @"SteamCMD\", model.SEDataPath + @"SteamCMD\");
                        if (Directory.Exists(SESMConfigHelper.SEDataPath + @"autoupdatedata\"))
                            Directory.Move(SESMConfigHelper.SEDataPath + @"autoupdatedata\",
                                model.SEDataPath + @"autoupdatedata\");
                        */
                        SESMConfigHelper.SEDataPath = model.SEDataPath;
                    }

                    if (model.Prefix != SESMConfigHelper.Prefix)
                    {
                        foreach (EntityServer item in srvPrv.GetAllServers())
                        {
                            Directory.Move(
                                SESMConfigHelper.SEDataPath +
                                ServiceHelper.GetServiceName(SESMConfigHelper.Prefix, item),
                                SESMConfigHelper.SEDataPath + ServiceHelper.GetServiceName(model.Prefix, item));
                        }
                        SESMConfigHelper.Prefix = model.Prefix;
                    }

                    if (model.SESavePath != SESMConfigHelper.SESavePath)
                    {
                        //Directory.Move(SESMConfigHelper.SESavePath, model.SESavePath);
                        SESMConfigHelper.SESavePath = model.SESavePath;
                    }
                    SESMConfigHelper.Arch = model.Arch;
                    SESMConfigHelper.AddDateToLog = model.AddDateToLog;
                    SESMConfigHelper.SendLogToKeen = model.SendLogToKeen;

                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        ServiceHelper.RegisterService(item);
                    }

                    foreach (EntityServer item in listStartedServ)
                    {
                        serviceLogger.Info(item.Name + " started by " + user.Login + " to Update Global Settings");
                        ServiceHelper.StartService(item);
                    }
                    RedirectToAction("Index", "Server");
                }
            }
            return View(model);
        }

        //
        // GET: Settings/UploadBin
        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult UploadBin()
        {
            return View();
        }

        //
        // POST: Settings/UploadBin
        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult UploadBin(UploadBinViewModel model)
        {
            if (ModelState.IsValid)
            {
                EntityUser user = Session["User"] as EntityUser;
                if (!ZipFile.IsZipFile(model.ServerZip.InputStream, false))
                {
                    ModelState.AddModelError("ZipError", "Your File is not a valid zip file");
                    return View(model);
                }

                ServerProvider srvPrv = new ServerProvider(_context);

                // Getting started server list
                List<EntityServer> listStartedServ =
                    srvPrv.GetAllServers().Where(item => srvPrv.GetState(item) == ServiceState.Running).ToList();
                Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                foreach (EntityServer item in srvPrv.GetAllServers())
                {
                    serviceLogger.Info(item.Name + " stopped by " + user.Login + " to Update Binaries");
                    ServiceHelper.StopService(item);
                }

                foreach (EntityServer item in srvPrv.GetAllServers())
                {
                    ServiceHelper.WaitForStopped(item);
                }

                // Killing some ghost processes that might still exists
                ServiceHelper.KillAllService();

                model.ServerZip.InputStream.Seek(0, SeekOrigin.Begin);

                using (ZipFile zip = ZipFile.Read(model.ServerZip.InputStream))
                {
                    if (!Directory.Exists(SESMConfigHelper.SEDataPath))
                        Directory.CreateDirectory(SESMConfigHelper.SEDataPath);
                    if (Directory.Exists(SESMConfigHelper.SEDataPath + @"Content\"))
                        Directory.Delete(SESMConfigHelper.SEDataPath + @"Content\", true);
                    if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer\"))
                        Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer\", true);
                    if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer64\"))
                        Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer64\", true);
                    zip.ExtractAll(SESMConfigHelper.SEDataPath);
                }

                foreach (EntityServer item in listStartedServ)
                {
                    serviceLogger.Info(item.Name + " started by " + user.Login + " to Update Binaries");
                    ServiceHelper.StartService(item);
                }

                return RedirectToAction("Index", "Server");
            }
            return View(model);
        }

        //
        // GET: Settings/Diagnosis
        [HttpGet]
        public ActionResult Diagnosis()
        {
            if (!SESMConfigHelper.DiagnosisEnabled)
            {
                return RedirectToAction("Index", "Home");
            }
            DiagnosisViewModel model = new DiagnosisViewModel();


            if (_context.Database.Exists())
            {
                model.DatabaseConnexion.State = true;
                model.DatabaseConnexion.Message = "Connection to database successful";
            }
            else
            {
                model.DatabaseConnexion.State = false;
                model.DatabaseConnexion.Message =
                    "Connection to database failed. <br/> Check your connexion string in SESM.config";
            }

            switch (SESMConfigHelper.Arch)
            {
                case ArchType.x64:
                    if (Environment.Is64BitOperatingSystem)
                    {
                        model.ArchMatch.State = true;
                        model.ArchMatch.Message =
                            "You are on a 64 Bits computer and you want to run 64 Bits servers. It will work !";
                    }
                    else
                    {
                        model.ArchMatch.State = false;
                        model.ArchMatch.Message =
                            "You are on a 32 Bits computer and you want to run 64 Bits servers. It won't work ! <br/>Please consider changing your Architecture variable to x86";
                    }
                    break;
                case ArchType.x86:
                    if (Environment.Is64BitOperatingSystem)
                    {
                        model.ArchMatch.State = true;
                        model.ArchMatch.Message =
                            "You are on a 64 Bits computer and you want to run 32 Bits servers. It will work ! <br/>(but you should consider switching your arch variable to x64 for better performances)";
                    }
                    else
                    {
                        model.ArchMatch.State = true;
                        model.ArchMatch.Message =
                            "You are on a 32 Bits computer and you want to run 32 Bits servers. It will work !";
                    }
                    break;
            }


            if (System.IO.File.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer\SpaceEngineersDedicated.exe"))
            {
                model.Binariesx86.State = true;
                model.Binariesx86.Message = "32 Bits Space Engineers bianries found at " + SESMConfigHelper.SEDataPath +
                                            @"DedicatedServer\SpaceEngineersDedicated.exe";
            }
            else
            {
                model.Binariesx86.State = false;
                model.Binariesx86.Message = "32 Bits Space Engineers bianries not found at " +
                                            SESMConfigHelper.SEDataPath +
                                            @"DedicatedServer\SpaceEngineersDedicated.exe<br/>You should try to reupload your game files or activate the auto update";
            }


            if (System.IO.File.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer64\SpaceEngineersDedicated.exe"))
            {
                model.Binariesx64.State = true;
                model.Binariesx64.Message = "64 Bits Space Engineers bianries found at " + SESMConfigHelper.SEDataPath +
                                            @"DedicatedServer64\SpaceEngineersDedicated.exe";
            }
            else
            {
                model.Binariesx64.State = false;
                model.Binariesx64.Message = "64 Bits Space Engineers bianries not found at " +
                                            SESMConfigHelper.SEDataPath +
                                            @"DedicatedServer64\SpaceEngineersDedicated.exe<br/>You should try to reupload your game files or activate the auto update";
            }


            /*ServiceHelper.RegisterService("SESMDiagTest");
            if (ServiceHelper.DoesServiceExist("SESMDiagTest"))
            {
                model.ServiceCreation.State = true;
                model.ServiceCreation.Message = "Creation of the service \"SESMDiagTest\" successful";

                ServiceHelper.UnRegisterService("SESMDiagTest");
                if (!ServiceHelper.DoesServiceExist("SESMDiagTest"))
                {
                    model.ServiceDeletion.State = true;
                    model.ServiceDeletion.Message = "Deletion of the service \"SESMDiagTest\" successful";
                }
                else
                {
                    model.ServiceDeletion.State = false;
                    model.ServiceDeletion.Message =
                        "Deletion of the service \"SESMDiagTest\" failed<br/>Check if the application pool have admin rights";
                }
            }
            else
            {
                model.ServiceCreation.State = false;
                model.ServiceCreation.Message =
                    "Creation of the service \"SESMDiagTest\" failed<br/>Check if the application pool have admin rights";

                model.ServiceDeletion.State = null;
                model.ServiceDeletion.Message = "Deletion of the service \"SESMDiagTest\" irrelevant";
            }
            */
            try
            {
                FileStream stream = System.IO.File.Create(@"C:\SESMDiagTest.bin");
                stream.Close();
                stream.Dispose();
                model.FileCreation.State = true;
                model.FileCreation.Message = @"Creation of the file C:\SESMDiagTest.bin successful";

                try
                {
                    System.IO.File.Delete(@"\testDiag.bin");
                    model.FileDeletion.State = true;
                    model.FileDeletion.Message = @"Deletion of the file C:\SESMDiagTest.bin successful";
                }
                catch (Exception)
                {
                    model.FileDeletion.State = false;
                    model.FileDeletion.Message =
                        @"Deletion of the file C:\SESMDiagTest.bin failed <br/>Check if the application pool have admin rights";
                }


            }
            catch (Exception)
            {
                model.FileCreation.State = false;
                model.FileCreation.Message =
                    @"Creation of the file C:\SESMDiagTest.bin failed <br/>Check if the application pool have admin rights";

                model.FileDeletion.State = null;
                model.FileDeletion.Message = @"Deletion of the file C:\SESMDiagTest.bin irrelevant";
            }

            UserProvider usrPrv = new UserProvider(_context);

            if (model.DatabaseConnexion.State == true)
            {
                model.SuperAdmin.State = false;
                model.SuperAdmin.Message = @"No super administrator found";

                foreach (EntityUser item in usrPrv.GetUsers())
                {
                    if (item.IsAdmin)
                    {
                        model.SuperAdmin.State = true;
                        model.SuperAdmin.Message = @"At least 1 super administrator found";
                    }
                }
            }
            else
            {
                model.SuperAdmin.State = null;
                model.SuperAdmin.Message = @"Super administrator search irrelevant";
            }
            return View(model);
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult AutoUpdate()
        {
            AutoUpdateViewModel model = new AutoUpdateViewModel();
            model.AutoUpdate = SESMConfigHelper.AutoUpdateEnabled;
            model.CronInterval = SESMConfigHelper.AutoUpdateCron;

            return View(model);
        }

        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult AutoUpdate(AutoUpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            SESMConfigHelper.AutoUpdateEnabled = model.AutoUpdate;
            SESMConfigHelper.AutoUpdateCron = model.CronInterval;

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

            scheduler.DeleteJob(new JobKey("AutoUpdateJob", "AutoUpdate"));

            if (model.AutoUpdate)
            {
                IJobDetail autoUpdateJob = JobBuilder.Create<AutoUpdate>()
                    .WithIdentity("AutoUpdateJob", "AutoUpdate")
                    .Build();

                ITrigger autoUpdateTrigger = TriggerBuilder.Create()
                    .WithIdentity("AutoUpdateTrigger", "AutoUpdate")
                    .WithCronSchedule(model.CronInterval)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(autoUpdateJob, autoUpdateTrigger);
            }

            return RedirectToAction("Index", "Settings").Success("Auto-Update Parameters Updated");
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult ManualUpdate()
        {
            Logger logger = LogManager.GetLogger("ManualUpdateLogger");

            logger.Info("----Starting ManualUpdate----");
            SteamCMDHelper.SteamCMDResult result = SteamCMDHelper.Update(logger, 300);
            logger.Info("----End of ManualUpdate----");
            /*
            switch (result)
            {
                case SteamCMDHelper.SteamCMDResult.Fail_Credentials:
                    return RedirectToAction("Index").Danger("Wrong credentials, please check and try again");
                    break;
                case SteamCMDHelper.SteamCMDResult.Fail_SteamGuardMissing:
                    return
                        RedirectToAction("Index")
                            .Danger(
                                "Steam Guard active on your account, please input the code in SteamCMD Configuration page and try again");
                    break;
                case SteamCMDHelper.SteamCMDResult.Fail_SteamGuardBadCode:
                    return
                        RedirectToAction("Index")
                            .Danger(
                                "Wrong Steam Guard code, please input the right code in SteamCMD Configuration page and try again");
                    break;
                case SteamCMDHelper.SteamCMDResult.Fail_TooLong:
                    return
                        RedirectToAction("Index")
                            .Warning(
                                "Update took too long. If it's the first update and you don't have a server-grade connection, please try again");
                    break;
                case SteamCMDHelper.SteamCMDResult.Success_NothingToDo:
                    return RedirectToAction("Index").Success("There are no updates available :-(");
                    break;
                case SteamCMDHelper.SteamCMDResult.Success_UpdateInstalled:
                    return RedirectToAction("Index").Success("Manual Update Successful");
                    break;
                default:
                    return RedirectToAction("Index").Danger("Manual Update : Unknow Error");
                    break;

            }*/
            return null;
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult SteamCMD()
        {
            SteamCMDViewModel model = new SteamCMDViewModel();
            return View(model);
        }

 

        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult Backups(BackupsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.DeleteJob(new JobKey("BackupLvl1Job", "Backups"));
            scheduler.DeleteJob(new JobKey("BackupLvl2Job", "Backups"));
            scheduler.DeleteJob(new JobKey("BackupLvl3Job", "Backups"));
            if (model.EnableLvl1)
            {
                IJobDetail BackupJob = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupLvl1Job", "Backups")
                    .UsingJobData("lvl", 1)
                    .Build();

                ITrigger BackupTrigger = TriggerBuilder.Create()
                    .WithIdentity("BackupLvl1Trigger", "Backups")
                    .WithCronSchedule(model.CronIntervalLvl1)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(BackupJob, BackupTrigger);
            }

            if (model.EnableLvl2)
            {
                IJobDetail BackupJob = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupLvl2Job", "Backups")
                    .UsingJobData("lvl", 2)
                    .Build();

                ITrigger BackupTrigger = TriggerBuilder.Create()
                    .WithIdentity("BackupLvl2Trigger", "Backups")
                    .WithCronSchedule(model.CronIntervalLvl2)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(BackupJob, BackupTrigger);
            }

            if (model.EnableLvl3)
            {
                IJobDetail BackupJob = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupLvl3Job", "Backups")
                    .UsingJobData("lvl", 3)
                    .Build();

                ITrigger BackupTrigger = TriggerBuilder.Create()
                    .WithIdentity("BackupLvl3Trigger", "Backups")
                    .WithCronSchedule(model.CronIntervalLvl3)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(BackupJob, BackupTrigger);
            }

            return RedirectToAction("Index", "Server");
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult CleanSteamCMD()
        {
            if (Directory.Exists(SESMConfigHelper.SEDataPath + @"\SteamCMD\"))
                Directory.Delete(SESMConfigHelper.SEDataPath + @"\SteamCMD\", true);

            if (Directory.Exists(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\"))
                Directory.Delete(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\", true);

            return RedirectToAction("Index", "Home").Success("Auto/Manual Update Cleaned Up");
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult CleanPerf()
        {
            _context.Database.ExecuteSqlCommand("truncate table SESM.dbo.EntityPerfEntries");
            return RedirectToAction("Index", "Home").Success("Perf Data Cleaned Up");
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult SESEManualUpdate()
        {
            Logger logger = LogManager.GetLogger("ManualUpdateLogger");

            logger.Info("----Starting SESEManualUpdate----");

            string SESEUrl = ""; //SESEHelper.UpdateIsAvailable();
            if(!string.IsNullOrEmpty(SESEUrl))
            {
                logger.Info("SE Server Extender Update detected");
                logger.Info("URL : " + SESEUrl);
                logger.Info("Cleaning up SESE Zip");
                SESEHelper.CleanupUpdate();
                logger.Info("Downloading New SESE Update Zip");
                SESEHelper.DownloadUpdate(SESEUrl);
                logger.Info("Initiating lockdown mode ...");
                SESMConfigHelper.Lockdown = true;
                logger.Info("Waiting 30 secs for all requests to end ...");
                Thread.Sleep(30000);
                DataContext context = new DataContext();
                ServerProvider srvPrv = new ServerProvider(context);
                List<EntityServer> listStartedServ = srvPrv.GetAllServers().Where(item => item.UseServerExtender && srvPrv.GetState(item) == ServiceState.Running).ToList();

                logger.Info("Stopping SESE running server ...");
                Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                foreach(EntityServer item in listStartedServ)
                {
                    logger.Info("Sending stop order to " + item.Name);
                    serviceLogger.Info(item.Name + " stopped by SESEManualUpdate");
                    ServiceHelper.StopService(item);
                }
                foreach(EntityServer item in listStartedServ)
                {
                    logger.Info("Waiting for stop of " + item.Name);
                    ServiceHelper.WaitForStopped(item);
                }
                logger.Info("Killing ghosts processes");
                // Killing some ghost processes that might still exists
                ServiceHelper.KillAllSESEService();

                logger.Info("Applying SESE Files");
                SESEHelper.ApplyUpdate();

                logger.Info("SESE Update finished, lifting lockdown");
                SESMConfigHelper.Lockdown = false;

                foreach(EntityServer item in listStartedServ)
                {
                    logger.Info("Restarting " + item.Name);
                    serviceLogger.Info(item.Name + " stopped by SESEManualUpdate");
                    ServiceHelper.StartService(item);
                }

                logger.Info("----End of SESEManualUpdate----");
                return RedirectToAction("Index").Success("SESE Update applyed");
            }
            else
            {
                logger.Info("No SE Server Extender Update detected");
                logger.Info("----End of SESEManualUpdate----");
                return RedirectToAction("Index").Warning("No SESE Update detected");
            }
        }

        public ActionResult SESEManualUpdateForce()
        {
            Logger logger = LogManager.GetLogger("ManualUpdateLogger");

            logger.Info("----Starting SESEManualUpdateForce----");
            logger.Info("Cleaning up SESE Zip");
            SESEHelper.CleanupUpdate();
            string SESEUrl = "";//SESEHelper.UpdateIsAvailable();
            if(!string.IsNullOrEmpty(SESEUrl))
            {
                logger.Info("SE Server Extender Update detected");
                logger.Info("URL : " + SESEUrl);

                logger.Info("Downloading New SESE Update Zip");
                SESEHelper.DownloadUpdate(SESEUrl);
                logger.Info("Initiating lockdown mode ...");
                SESMConfigHelper.Lockdown = true;
                logger.Info("Waiting 30 secs for all requests to end ...");
                Thread.Sleep(30000);
                DataContext context = new DataContext();
                ServerProvider srvPrv = new ServerProvider(context);
                List<EntityServer> listStartedServ = srvPrv.GetAllServers().Where(item => item.UseServerExtender && srvPrv.GetState(item) == ServiceState.Running).ToList();

                logger.Info("Stopping SESE running server ...");
                Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                foreach(EntityServer item in listStartedServ)
                {
                    logger.Info("Sending stop order to " + item.Name);
                    serviceLogger.Info(item.Name + " stopped by SESEManualUpdate");
                    ServiceHelper.StopService(item);
                }
                foreach(EntityServer item in listStartedServ)
                {
                    logger.Info("Waiting for stop of " + item.Name);
                    ServiceHelper.WaitForStopped(item);
                }
                logger.Info("Killing ghosts processes");
                // Killing some ghost processes that might still exists
                ServiceHelper.KillAllSESEService();

                logger.Info("Applying SESE Files");
                SESEHelper.ApplyUpdate();

                logger.Info("SESE Update finished, lifting lockdown");
                SESMConfigHelper.Lockdown = false;

                foreach(EntityServer item in listStartedServ)
                {
                    logger.Info("Restarting " + item.Name);
                    serviceLogger.Info(item.Name + " stopped by SESEManualUpdate");
                    ServiceHelper.StartService(item);
                }

                logger.Info("----End of SESEManualUpdateForce----");
                return RedirectToAction("Index").Success("SESE Update applyed");
            }
            else
            {
                logger.Info("No SE Server Extender Update detected");
                logger.Info("----End of SESEManualUpdateForce----");
                return RedirectToAction("Index").Warning("No SESE Update detected");
            }
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult HourlyStats()
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            List<EntityServer> serv = srvPrv.GetAllServers();
            Dictionary<string, List<EntityPerfEntry>> perfEntries = serv.ToDictionary(server => server.Name, server => server.PerfEntries.Where(x => x.Timestamp >= DateTime.Now.AddHours(-2)).OrderBy(x => x.Timestamp).ToList());

            ViewData["perfEntries"] = perfEntries;
            return View();
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult GlobalStats()
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            List<EntityServer> serv = srvPrv.GetAllServers();
            Dictionary<string, List<EntityPerfEntry>> perfEntries = serv.ToDictionary(server => server.Name, server => server.PerfEntries.Where(x => x.CPUUsagePeak != null).OrderBy(x => x.Timestamp).ToList());

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
