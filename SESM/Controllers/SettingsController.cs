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
        public ActionResult SE()
        {

            return View();
        }

        [HttpGet]
        [SuperAdmin]
        public ActionResult SESE()
        {

            return View();
        }

       
   /*
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
        */

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
                IJobDetail BackupJob = JobBuilder.Create<AutoBackupJob>()
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
                IJobDetail BackupJob = JobBuilder.Create<AutoBackupJob>()
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
                IJobDetail BackupJob = JobBuilder.Create<AutoBackupJob>()
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
        public ActionResult CleanPerf() // TODO : Webservice call
        {
            _context.Database.ExecuteSqlCommand("truncate table SESM.dbo.EntityPerfEntries");
            return RedirectToAction("Index", "Home");
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
