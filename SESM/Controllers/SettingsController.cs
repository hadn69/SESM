using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        readonly DataContext _context = new DataContext();

        //
        // GET: Settings
        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult Index()
        {
            SettingsViewModel model = new SettingsViewModel();

            model.Prefix = SESMConfigHelper.Prefix;
            model.SESavePath = SESMConfigHelper.SESavePath;
            model.SEDataPath = SESMConfigHelper.SEDataPath;
            model.Arch = SESMConfigHelper.Arch;
            model.AddDateToLog = SESMConfigHelper.AddDateToLog;
            model.SendLogToKeen = SESMConfigHelper.SendLogToKeen;

            return View(model);
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
                    List<EntityServer> listStartedServ = srvPrv.GetAllServers().Where(item => srvPrv.GetState(item) == ServiceState.Running).ToList();

                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        ServiceHelper.StopService(ServiceHelper.GetServiceName(item));
                    }

                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        ServiceHelper.WaitForStopped(ServiceHelper.GetServiceName(item));
                    }

                    // Killing some ghost processes that might still exists
                    ServiceHelper.KillAllService();

                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(item));
                    }

                    if (model.SEDataPath != SESMConfigHelper.SEDataPath)
                    {
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
                            Directory.Move(SESMConfigHelper.SEDataPath + @"DedicatedServer\", model.SEDataPath + @"DedicatedServer\");
                        if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer64\"))
                            Directory.Move(SESMConfigHelper.SEDataPath + @"DedicatedServer64\", model.SEDataPath + @"DedicatedServer64\");
                        if (Directory.Exists(SESMConfigHelper.SEDataPath + @"SteamCMD\"))
                            Directory.Move(SESMConfigHelper.SEDataPath + @"SteamCMD\", model.SEDataPath + @"SteamCMD\");
                        if (Directory.Exists(SESMConfigHelper.SEDataPath + @"autoupdatedata\"))
                            Directory.Move(SESMConfigHelper.SEDataPath + @"autoupdatedata\", model.SEDataPath + @"autoupdatedata\");

                        SESMConfigHelper.SEDataPath = model.SEDataPath;
                    }

                    if (model.Prefix != SESMConfigHelper.Prefix)
                    {
                        foreach (EntityServer item in srvPrv.GetAllServers())
                        {
                            Directory.Move(SESMConfigHelper.SEDataPath + ServiceHelper.GetServiceName(SESMConfigHelper.Prefix, item),
                                            SESMConfigHelper.SEDataPath + ServiceHelper.GetServiceName(model.Prefix, item));
                        }
                        SESMConfigHelper.Prefix = model.Prefix;
                    }

                    if (model.SESavePath != SESMConfigHelper.SESavePath)
                    {
                        Directory.Move(SESMConfigHelper.SESavePath, model.SESavePath);
                        SESMConfigHelper.SESavePath = model.SESavePath;
                    }
                    SESMConfigHelper.Arch = model.Arch;
                    SESMConfigHelper.AddDateToLog = model.AddDateToLog;
                    SESMConfigHelper.SendLogToKeen = model.SendLogToKeen;

                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        ServiceHelper.RegisterService(ServiceHelper.GetServiceName(item));
                    }

                    foreach (EntityServer item in listStartedServ)
                    {
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
                if (!ZipFile.IsZipFile(model.ServerZip.InputStream, false))
                {
                    ModelState.AddModelError("ZipError", "Your File is not a valid zip file");
                    return View(model);
                }

                ServerProvider srvPrv = new ServerProvider(_context);

                // Getting started server list
                List<EntityServer> listStartedServ =
                    srvPrv.GetAllServers().Where(item => srvPrv.GetState(item) == ServiceState.Running).ToList();

                foreach (EntityServer item in srvPrv.GetAllServers())
                {
                    ServiceHelper.StopService(ServiceHelper.GetServiceName(item));
                }

                foreach (EntityServer item in srvPrv.GetAllServers())
                {
                    ServiceHelper.WaitForStopped(ServiceHelper.GetServiceName(item));
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
            if (!SESMConfigHelper.Diagnosis)
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
                model.DatabaseConnexion.Message = "Connection to database failed. <br/> Check your connexion string in SESM.config";
            }

            switch (SESMConfigHelper.Arch)
            {
                case ArchType.x64:
                    if (System.Environment.Is64BitOperatingSystem)
                    {
                        model.ArchMatch.State = true;
                        model.ArchMatch.Message = "You are on a 64 Bits computer and you want to run 64 Bits servers. It will work !";
                    }
                    else
                    {
                        model.ArchMatch.State = false;
                        model.ArchMatch.Message = "You are on a 32 Bits computer and you want to run 64 Bits servers. It won't work ! <br/>Please consider changing your Architecture variable to x86";
                    }
                    break;
                case ArchType.x86:
                    if (System.Environment.Is64BitOperatingSystem)
                    {
                        model.ArchMatch.State = true;
                        model.ArchMatch.Message = "You are on a 64 Bits computer and you want to run 32 Bits servers. It will work ! <br/>(but you should consider switching your arch variable to x64 for better performances)";
                    }
                    else
                    {
                        model.ArchMatch.State = true;
                        model.ArchMatch.Message = "You are on a 32 Bits computer and you want to run 32 Bits servers. It will work !";
                    }
                    break;
            }


            if (System.IO.File.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer\SpaceEngineersDedicated.exe"))
            {
                model.Binariesx86.State = true;
                model.Binariesx86.Message = "32 Bits Space Engineers bianries found at " + SESMConfigHelper.SEDataPath + @"DedicatedServer\SpaceEngineersDedicated.exe";
            }
            else
            {
                model.Binariesx86.State = false;
                model.Binariesx86.Message = "32 Bits Space Engineers bianries not found at " + SESMConfigHelper.SEDataPath + @"DedicatedServer\SpaceEngineersDedicated.exe<br/>You should try to reupload your game files or activate the auto update";
            }


            if (System.IO.File.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer64\SpaceEngineersDedicated.exe"))
            {
                model.Binariesx64.State = true;
                model.Binariesx64.Message = "64 Bits Space Engineers bianries found at " + SESMConfigHelper.SEDataPath + @"DedicatedServer64\SpaceEngineersDedicated.exe";
            }
            else
            {
                model.Binariesx64.State = false;
                model.Binariesx64.Message = "64 Bits Space Engineers bianries not found at " + SESMConfigHelper.SEDataPath + @"DedicatedServer64\SpaceEngineersDedicated.exe<br/>You should try to reupload your game files or activate the auto update";
            }


            ServiceHelper.RegisterService("SESMDiagTest");
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
                    model.ServiceDeletion.Message = "Deletion of the service \"SESMDiagTest\" failed<br/>Check if the application pool have admin rights";
                }
            }
            else
            {
                model.ServiceCreation.State = false;
                model.ServiceCreation.Message = "Creation of the service \"SESMDiagTest\" failed<br/>Check if the application pool have admin rights";

                model.ServiceDeletion.State = null;
                model.ServiceDeletion.Message = "Deletion of the service \"SESMDiagTest\" irrelevant";
            }

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
                    model.FileDeletion.Message = @"Deletion of the file C:\SESMDiagTest.bin failed <br/>Check if the application pool have admin rights";
                }


            }
            catch (Exception)
            {
                model.FileCreation.State = false;
                model.FileCreation.Message = @"Creation of the file C:\SESMDiagTest.bin failed <br/>Check if the application pool have admin rights";

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
            model.AutoUpdate = SESMConfigHelper.AutoUpdate;
            model.CronInterval = SESMConfigHelper.AUInterval;

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

            SESMConfigHelper.AutoUpdate = model.AutoUpdate;
            SESMConfigHelper.AUInterval = model.CronInterval;

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

            switch (result)
            {
                case SteamCMDHelper.SteamCMDResult.Fail_Credentials:
                    return RedirectToAction("Index").Danger("Wrong credentials, please check and try again");
                    break;
                case SteamCMDHelper.SteamCMDResult.Fail_SteamGuardMissing:
                    return RedirectToAction("Index").Danger("Steam Guard active on your account, please input the code in SteamCMD Configuration page and try again");
                    break;
                case SteamCMDHelper.SteamCMDResult.Fail_SteamGuardBadCode:
                    return RedirectToAction("Index").Danger("Wrong Steam Guard code, please input the right code in SteamCMD Configuration page and try again");
                    break;
                case SteamCMDHelper.SteamCMDResult.Fail_TooLong:
                    return RedirectToAction("Index").Warning("Update took too long. If it's the first update and you don't have a server-grade connection, please try again");
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

            }
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult SteamCMD()
        {
            SteamCMDViewModel model = new SteamCMDViewModel();
            model.UserName = SESMConfigHelper.AUUsername;
            return View(model);
        }

        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        [MultipleButton(Name = "action", Argument = "SaveSteamCMD")]
        public ActionResult SteamCMD(SteamCMDViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            SESMConfigHelper.AUUsername = model.UserName;
            if (!string.IsNullOrEmpty(model.Password))
            {
                SESMConfigHelper.AUPassword = model.Password;
            }
            Logger logger = LogManager.GetLogger("ManualUpdateLogger");
            logger.Info("----Stating SteamCMD Initialisation----");
            SteamCMDHelper.Initialise(logger, model.UserName, model.Password, model.SteamGuard);
            logger.Info("----End of SteamCMD Initialisation----");

            return RedirectToAction("Index", "Settings").Success("SteamCMD Initialized");
        }

        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        [MultipleButton(Name = "action", Argument = "FireSteamGuard")]
        public ActionResult FireSteamGuard(SteamCMDViewModel model)
        {
            if (!ModelState.IsValid)
                return View("SteamCMD", model);

            SESMConfigHelper.AUUsername = model.UserName;
            if (!string.IsNullOrEmpty(model.Password))
            {
                SESMConfigHelper.AUPassword = model.Password;
            }
            Logger logger = LogManager.GetLogger("ManualUpdateLogger");
            logger.Info("----Stating SteamCMD SteamGuard Firering----");
            SteamCMDHelper.Initialise(logger, model.UserName, model.Password, string.Empty);
            logger.Info("----End of SteamCMD SteamGuard Firering----");

            return RedirectToAction("SteamCMD", "Settings").Information("Steam Guard Fired, please check your Inbox and input the Steam Guard code below");
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        [CheckLockout]
        public ActionResult Backups()
        {
            BackupsViewModel model = new BackupsViewModel();
            model.EnableLvl1 = SESMConfigHelper.AutoBackupLvl1;
            model.EnableLvl2 = SESMConfigHelper.AutoBackupLvl2;
            model.EnableLvl3 = SESMConfigHelper.AutoBackupLvl3;

            model.NbToKeepLvl1 = SESMConfigHelper.ABNbToKeepLvl1;
            model.NbToKeepLvl2 = SESMConfigHelper.ABNbToKeepLvl2;
            model.NbToKeepLvl3 = SESMConfigHelper.ABNbToKeepLvl3;

            model.CronIntervalLvl1 = SESMConfigHelper.ABIntervalLvl1;
            model.CronIntervalLvl2 = SESMConfigHelper.ABIntervalLvl2;
            model.CronIntervalLvl3 = SESMConfigHelper.ABIntervalLvl3;

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

            SESMConfigHelper.AutoBackupLvl1 = model.EnableLvl1;
            SESMConfigHelper.AutoBackupLvl2 = model.EnableLvl2;
            SESMConfigHelper.AutoBackupLvl3 = model.EnableLvl3;

            SESMConfigHelper.ABNbToKeepLvl1 = model.NbToKeepLvl1;
            SESMConfigHelper.ABNbToKeepLvl2 = model.NbToKeepLvl2;
            SESMConfigHelper.ABNbToKeepLvl3 = model.NbToKeepLvl3;

            SESMConfigHelper.ABIntervalLvl1 = model.CronIntervalLvl1;
            SESMConfigHelper.ABIntervalLvl2 = model.CronIntervalLvl2;
            SESMConfigHelper.ABIntervalLvl3 = model.CronIntervalLvl3;

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

            return RedirectToAction("Index", "Home").Success("Auto/Manual Update Clened Up");
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