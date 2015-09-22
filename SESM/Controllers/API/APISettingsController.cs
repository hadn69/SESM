using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Ionic.Zip;
using NLog;
using Quartz;
using Quartz.Impl;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models;
using SESM.Tools;
using SESM.Tools.API;
using SESM.Tools.Helpers;
using SESM.Tools.Jobs;

namespace SESM.Controllers.API
{
    public class APISettingsController : Controller
    {
        private readonly DataContext _context = new DataContext();

        #region SESM

        // GET: API/Settings/GetSESMSettings        
        [HttpGet]
        [APIHostAccess("SET-GSESMS", "SETTINGS_SESM")]
        public ActionResult GetSESMSettings()
        {
            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GSESMS-OK");

            response.AddToContent(new XElement("Prefix", SESMConfigHelper.Prefix));
            response.AddToContent(new XElement("SESavePath", SESMConfigHelper.SESavePath));
            response.AddToContent(new XElement("SEDataPath", SESMConfigHelper.SEDataPath));
            response.AddToContent(new XElement("MESavePath", SESMConfigHelper.MESavePath));
            response.AddToContent(new XElement("MEDataPath", SESMConfigHelper.MEDataPath));
            response.AddToContent(new XElement("Arch", SESMConfigHelper.Arch));
            response.AddToContent(new XElement("SendLogToKeen", SESMConfigHelper.SendLogToKeen));
            response.AddToContent(new XElement("AddDateToLog", SESMConfigHelper.AddDateToLog));
            return Content(response.ToString());
        }

        // POST: API/Settings/SetSESMSettings
        [HttpPost]
        [APIHostAccess("SET-SSESMS", "SETTINGS_SESM")]
        public ActionResult SetSESMSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            string Prefix = Request.Form["Prefix"];
            if (string.IsNullOrWhiteSpace(Prefix))
                return Content(XMLMessage.Error("SRV-SSESMS-MISPRE", "The Prefix field must be provided").ToString());
            if (!Regex.IsMatch(Prefix, @"^[a-zA-Z0-9_.-]+$"))
                return Content(XMLMessage.Error("SRV-SSESMS-BADPRE", "The Prefix field must be only composed of letters, numbers, dots, dashs and underscores").ToString());

            string SESavePath = Request.Form["SESavePath"];
            if (string.IsNullOrWhiteSpace(SESavePath))
                return Content(XMLMessage.Error("SRV-SSESMS-MISSSP", "The SESavePath field must be provided").ToString());
            if (!SESavePath.EndsWith(@"\"))
                return Content(XMLMessage.Error("SRV-SSESMS-BADSSP", "The SESavePath field must end with \\").ToString());

            string SEDataPath = Request.Form["SEDataPath"];
            if (string.IsNullOrWhiteSpace(SEDataPath))
                return Content(XMLMessage.Error("SRV-SSESMS-MISSDP", "The SEDataPath field must be provided").ToString());
            if (!SEDataPath.EndsWith(@"\"))
                return Content(XMLMessage.Error("SRV-SSESMS-BADSDP", "The SEDataPath field must end with \\").ToString());

            string MESavePath = Request.Form["MESavePath"];
            if (string.IsNullOrWhiteSpace(MESavePath))
                return Content(XMLMessage.Error("SRV-SSESMS-MISMSP", "The MESavePath field must be provided").ToString());
            if (!SESavePath.EndsWith(@"\"))
                return Content(XMLMessage.Error("SRV-SSESMS-BADMSP", "The MESavePath field must end with \\").ToString());

            string MEDataPath = Request.Form["MEDataPath"];
            if (string.IsNullOrWhiteSpace(MEDataPath))
                return Content(XMLMessage.Error("SRV-SSESMS-MISMDP", "The MEDataPath field must be provided").ToString());
            if (!SEDataPath.EndsWith(@"\"))
                return Content(XMLMessage.Error("SRV-SSESMS-BADMDP", "The MEDataPath field must end with \\").ToString());

            ArchType Arch;
            if (string.IsNullOrWhiteSpace(Request.Form["Arch"]))
                return Content(XMLMessage.Error("SRV-SSESMS-MISAR", "The Arch field must be provided").ToString());
            if (!Enum.TryParse(Request.Form["Arch"], out Arch))
                return Content(XMLMessage.Error("SRV-SSESMS-BADAR", "The Arch field is invalid").ToString());

            bool SendLogToKeen;
            if (string.IsNullOrWhiteSpace(Request.Form["SendLogToKeen"]))
                return Content(XMLMessage.Error("SRV-SSESMS-MISSLTK", "The SendLogToKeen field must be provided").ToString());
            if (!bool.TryParse(Request.Form["SendLogToKeen"], out SendLogToKeen))
                return Content(XMLMessage.Error("SRV-SSESMS-BADSLTK", "The SendLogToKeen field is invalid").ToString());

            bool AddDateToLog;
            if (string.IsNullOrWhiteSpace(Request.Form["AddDateToLog"]))
                return Content(XMLMessage.Error("SRV-SSESMS-MISADTL", "The AddDateToLog field must be provided").ToString());
            if (!bool.TryParse(Request.Form["AddDateToLog"], out AddDateToLog))
                return Content(XMLMessage.Error("SRV-SSESMS-BADADTL", "The AddDateToLog field is invalid").ToString());

            // ** Process **
            try
            {
                SESMConfigHelper.Lockdown = true;
                List<EntityServer> SEServer = srvPrv.GetAllServers();
                List<EntityServer> SERunningServer = new List<EntityServer>();

                foreach (EntityServer server in SEServer)
                {
                    ServiceState serverState = srvPrv.GetState(server);
                    if (serverState == ServiceState.Stopped || serverState == ServiceState.Unknow)
                    {
                        continue;
                    }
                    ServiceHelper.StopService(server);

                    SERunningServer.Add(server);
                }
                foreach (EntityServer server in SERunningServer)
                {
                    ServiceHelper.WaitForStopped(server);
                }

                Thread.Sleep(10000);
                ServiceHelper.KillAllServices();
                Thread.Sleep(10000);

                foreach (EntityServer server in SEServer)
                {
                    ServiceHelper.UnRegisterService(server);
                }

                if (Prefix != SESMConfigHelper.Prefix)
                {
                    foreach (EntityServer server in SEServer)
                    {
                        Directory.Move(PathHelper.GetInstancePath(server), PathHelper.GetInstancePath(Prefix, server));
                    }
                }

                if (SESavePath != SESMConfigHelper.SESavePath)
                {
                    Directory.Move(SESMConfigHelper.SESavePath, SESavePath);
                }

                if (SEDataPath != SESMConfigHelper.SEDataPath)
                {
                    Directory.Move(SESMConfigHelper.SEDataPath, SEDataPath);
                }

                if (MESavePath != SESMConfigHelper.MESavePath)
                {
                    Directory.Move(SESMConfigHelper.MESavePath, MESavePath);
                }

                if (MEDataPath != SESMConfigHelper.MEDataPath)
                {
                    Directory.Move(SESMConfigHelper.MEDataPath, MEDataPath);
                }

                SESMConfigHelper.Prefix = Prefix;
                SESMConfigHelper.SESavePath = SESavePath;
                SESMConfigHelper.SEDataPath = SEDataPath;
                SESMConfigHelper.MESavePath = MESavePath;
                SESMConfigHelper.MEDataPath = MEDataPath;
                SESMConfigHelper.Arch = Arch;
                SESMConfigHelper.SendLogToKeen = SendLogToKeen;
                SESMConfigHelper.AddDateToLog = AddDateToLog;

                foreach (EntityServer server in SEServer)
                {
                    ServiceHelper.RegisterService(server);
                }

                foreach (EntityServer server in SERunningServer)
                {
                    ServiceHelper.StartService(server);
                }

                return Content(XMLMessage.Success("SRV-SSESMS-OK", "The SESM settings have been updated").ToString());
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("SRV-SSESMS-EX", "Exception :" + ex).ToString());
            }
            finally
            {
                SESMConfigHelper.Lockdown = false;
            }
        }

        // GET: API/Settings/GetBackupsSettings        
        [HttpGet]
        [APIHostAccess("SET-GBS", "SETTINGS_SESM")]
        public ActionResult GetBackupsSettings()
        {
            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GBS-OK");

            response.AddToContent(new XElement("AutoBackupLvl1Enabled", SESMConfigHelper.AutoBackupLvl1Enabled));
            response.AddToContent(new XElement("AutoBackupLvl1Cron", SESMConfigHelper.AutoBackupLvl1Cron));
            response.AddToContent(new XElement("AutoBackupLvl1NbToKeep", SESMConfigHelper.AutoBackupLvl1NbToKeep));

            response.AddToContent(new XElement("AutoBackupLvl2Enabled", SESMConfigHelper.AutoBackupLvl2Enabled));
            response.AddToContent(new XElement("AutoBackupLvl2Cron", SESMConfigHelper.AutoBackupLvl2Cron));
            response.AddToContent(new XElement("AutoBackupLvl2NbToKeep", SESMConfigHelper.AutoBackupLvl2NbToKeep));

            response.AddToContent(new XElement("AutoBackupLvl3Enabled", SESMConfigHelper.AutoBackupLvl3Enabled));
            response.AddToContent(new XElement("AutoBackupLvl3Cron", SESMConfigHelper.AutoBackupLvl3Cron));
            response.AddToContent(new XElement("AutoBackupLvl3NbToKeep", SESMConfigHelper.AutoBackupLvl3NbToKeep));

            return Content(response.ToString());
        }

        // POST: API/Settings/SetBackupsSettings        
        [HttpPost]
        [APIHostAccess("SET-SBS", "SETTINGS_SESM")]
        public ActionResult SetBackupsSettings()
        {
            bool AutoBackupLvl1Enabled;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoBackupLvl1Enabled"]))
                return Content(XMLMessage.Error("SRV-SBS-MISL1E", "The AutoBackupLvl1Enabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["AutoBackupLvl1Enabled"], out AutoBackupLvl1Enabled))
                return Content(XMLMessage.Error("SRV-SBS-MISL1E", "The AutoBackupLvl1Enabled is invalid").ToString());

            string AutoBackupLvl1Cron = Request.Form["AutoBackupLvl1Cron"];
            if (string.IsNullOrWhiteSpace(AutoBackupLvl1Cron))
                return Content(XMLMessage.Error("SRV-SBS-MISL1C", "The AutoBackupLvl1Cron field must be provided").ToString());
            if (!CronExpression.IsValidExpression(AutoBackupLvl1Cron))
                return Content(XMLMessage.Error("SRV-SBS-BADL1C", "The AutoBackupLvl1Cron field is invalid").ToString());

            int AutoBackupLvl1NbToKeep;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoBackupLvl1NbToKeep"]))
                return Content(XMLMessage.Error("SRV-SBS-MISL1N", "The AutoBackupLvl1NbToKeep field must be provided").ToString());
            if (!int.TryParse(Request.Form["AutoBackupLvl1NbToKeep"], out AutoBackupLvl1NbToKeep) || AutoBackupLvl1NbToKeep < 1)
                return Content(XMLMessage.Error("SRV-SBS-MISL1N", "The AutoBackupLvl1NbToKeep is invalid").ToString());

            bool AutoBackupLvl2Enabled;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoBackupLvl2Enabled"]))
                return Content(XMLMessage.Error("SRV-SBS-MISL2E", "The AutoBackupLvl2Enabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["AutoBackupLvl2Enabled"], out AutoBackupLvl2Enabled))
                return Content(XMLMessage.Error("SRV-SBS-MISL2E", "The AutoBackupLvl2Enabled is invalid").ToString());

            string AutoBackupLvl2Cron = Request.Form["AutoBackupLvl2Cron"];
            if (string.IsNullOrWhiteSpace(AutoBackupLvl2Cron))
                return Content(XMLMessage.Error("SRV-SBS-MISL2C", "The AutoBackupLvl2Cron field must be provided").ToString());
            if (!CronExpression.IsValidExpression(AutoBackupLvl2Cron))
                return Content(XMLMessage.Error("SRV-SBS-BADL2C", "The AutoBackupLvl2Cron field is invalid").ToString());

            int AutoBackupLvl2NbToKeep;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoBackupLvl2NbToKeep"]))
                return Content(XMLMessage.Error("SRV-SBS-MISL2N", "The AutoBackupLvl2NbToKeep field must be provided").ToString());
            if (!int.TryParse(Request.Form["AutoBackupLvl2NbToKeep"], out AutoBackupLvl2NbToKeep) || AutoBackupLvl2NbToKeep < 1)
                return Content(XMLMessage.Error("SRV-SBS-MISL2N", "The AutoBackupLvl2NbToKeep is invalid").ToString());

            bool AutoBackupLvl3Enabled;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoBackupLvl3Enabled"]))
                return Content(XMLMessage.Error("SRV-SBS-MISL3E", "The AutoBackupLvl3Enabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["AutoBackupLvl3Enabled"], out AutoBackupLvl3Enabled))
                return Content(XMLMessage.Error("SRV-SBS-MISL3E", "The AutoBackupLvl3Enabled is invalid").ToString());

            string AutoBackupLvl3Cron = Request.Form["AutoBackupLvl3Cron"];
            if (string.IsNullOrWhiteSpace(AutoBackupLvl3Cron))
                return Content(XMLMessage.Error("SRV-SBS-MISL3C", "The AutoBackupLvl3Cron field must be provided").ToString());
            if (!CronExpression.IsValidExpression(AutoBackupLvl3Cron))
                return Content(XMLMessage.Error("SRV-SBS-BADL3C", "The AutoBackupLvl3Cron field is invalid").ToString());

            int AutoBackupLvl3NbToKeep;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoBackupLvl3NbToKeep"]))
                return Content(XMLMessage.Error("SRV-SBS-MISL3N", "The AutoBackupLvl3NbToKeep field must be provided").ToString());
            if (!int.TryParse(Request.Form["AutoBackupLvl3NbToKeep"], out AutoBackupLvl3NbToKeep) || AutoBackupLvl3NbToKeep < 1)
                return Content(XMLMessage.Error("SRV-SBS-MISL3N", "The AutoBackupLvl3NbToKeep is invalid").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-SBS-OK");

            SESMConfigHelper.AutoBackupLvl1Enabled = AutoBackupLvl1Enabled;
            SESMConfigHelper.AutoBackupLvl1Cron = AutoBackupLvl1Cron;
            SESMConfigHelper.AutoBackupLvl1NbToKeep = AutoBackupLvl1NbToKeep;

            SESMConfigHelper.AutoBackupLvl2Enabled = AutoBackupLvl2Enabled;
            SESMConfigHelper.AutoBackupLvl2Cron = AutoBackupLvl2Cron;
            SESMConfigHelper.AutoBackupLvl2NbToKeep = AutoBackupLvl2NbToKeep;

            SESMConfigHelper.AutoBackupLvl3Enabled = AutoBackupLvl3Enabled;
            SESMConfigHelper.AutoBackupLvl3Cron = AutoBackupLvl3Cron;
            SESMConfigHelper.AutoBackupLvl3NbToKeep = AutoBackupLvl3NbToKeep;

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.DeleteJob(AutoBackupJob.GetJobKey(1));
            scheduler.DeleteJob(AutoBackupJob.GetJobKey(2));
            scheduler.DeleteJob(AutoBackupJob.GetJobKey(3));


            if (SESMConfigHelper.AutoBackupLvl1Enabled)
            {
                IJobDetail BackupJob = JobBuilder.Create<AutoBackupJob>()
                    .WithIdentity(AutoBackupJob.GetJobKey(1))
                    .UsingJobData("lvl", 1)
                    .Build();

                ITrigger BackupTrigger = TriggerBuilder.Create()
                    .WithIdentity(AutoBackupJob.GetTriggerKey(1))
                    .WithCronSchedule(SESMConfigHelper.AutoBackupLvl1Cron)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(BackupJob, BackupTrigger);
            }

            if (SESMConfigHelper.AutoBackupLvl2Enabled)
            {
                IJobDetail BackupJob = JobBuilder.Create<AutoBackupJob>()
                    .WithIdentity(AutoBackupJob.GetJobKey(2))
                    .UsingJobData("lvl", 2)
                    .Build();

                ITrigger BackupTrigger = TriggerBuilder.Create()
                    .WithIdentity(AutoBackupJob.GetTriggerKey(2))
                    .WithCronSchedule(SESMConfigHelper.AutoBackupLvl2Cron)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(BackupJob, BackupTrigger);
            }

            if (SESMConfigHelper.AutoBackupLvl3Enabled)
            {
                IJobDetail BackupJob = JobBuilder.Create<AutoBackupJob>()
                    .WithIdentity(AutoBackupJob.GetJobKey(3))
                    .UsingJobData("lvl", 3)
                    .Build();

                ITrigger BackupTrigger = TriggerBuilder.Create()
                    .WithIdentity(AutoBackupJob.GetTriggerKey(3))
                    .WithCronSchedule(SESMConfigHelper.AutoBackupLvl3Cron)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(BackupJob, BackupTrigger);
            }

            return Content(response.ToString());
        }

        #endregion

        #region SE

        // GET: API/Settings/GetSEStatus        
        [HttpGet]
        [APIHostAccess("SET-GSES", "SETTINGS_SE")]
        public ActionResult GetSEStatus()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GSES-OK");

            response.AddToContent(new XElement("UpdateRunning", SESMConfigHelper.SEUpdating));
            response.AddToContent(new XElement("NbServer", srvPrv.GetAllSEServers().Count));
            return Content(response.ToString());
        }

        // GET: API/Settings/GetSEVersion        
        [HttpGet]
        [APIHostAccess("SET-GSEV", "SETTINGS_SE")]
        public ActionResult GetSEVersion()
        {
            // ** PROCESS **
            Logger logger = LogManager.GetLogger("SEGetVersionLogger");
            int? localVersion = null;

            int? remoteVersion = null;

            for (int i = 0; i < 5; i++)
            {
                if (localVersion == null)
                {
                    logger.Info("Retrieving local version : ");
                    localVersion = SteamCMDHelper.GetSEInstalledVersion(logger);
                }

                if (remoteVersion == null)
                {
                    logger.Info("Retrieving remote version : ");
                    remoteVersion =
                        SteamCMDHelper.GetSEAvailableVersion(
                            !string.IsNullOrWhiteSpace(SESMConfigHelper.SEAutoUpdateBetaPassword), logger);
                }

                if (localVersion == null || remoteVersion == null)
                {
                    logger.Info("Fail retrieving one of the version (try " + (i + 1) + " of 5), waiting and retrying ...");
                    Thread.Sleep(2000);
                }
                else
                    break;

            }

            if (localVersion == null || remoteVersion == null)
            {
                return Content(XMLMessage.Error("SET-GSEV-FAIL", "Fail retieving one of the version. See SEGetVersion log for more info").ToString());
            }

            XMLMessage response = new XMLMessage("SET-GSEV-OK");

            response.AddToContent(new XElement("Local", localVersion.ToString()));
            response.AddToContent(new XElement("Remote", remoteVersion.ToString()));
            response.AddToContent(new XElement("Diff", localVersion - remoteVersion));

            return Content(response.ToString());
        }

        // GET: API/Settings/GetSESettings        
        [HttpGet]
        [APIHostAccess("SET-GSESE", "SETTINGS_SE")]
        public ActionResult GetSESettings()
        {
            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GSESE-OK");

            response.AddToContent(new XElement("AutoUpdateEnabled", SESMConfigHelper.SEAutoUpdateEnabled));
            response.AddToContent(new XElement("AutoUpdateCron", SESMConfigHelper.SEAutoUpdateCron));
            response.AddToContent(new XElement("AutoUpdateBetaPassword", SESMConfigHelper.SEAutoUpdateBetaPassword));

            return Content(response.ToString());
        }

        // POST: API/Settings/SetSESettings        
        [HttpPost]
        [APIHostAccess("SET-SSES", "SETTINGS_SE")]
        public ActionResult SetSESettings()
        {
            // ** PARSING **
            bool autoUpdateEnabled;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoUpdateEnabled"]))
                return Content(XMLMessage.Error("SET-SSES-MISAUE", "The AutoUpdateEnabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["AutoUpdateEnabled"], out autoUpdateEnabled))
                return Content(XMLMessage.Error("SET-SSES-BADAUE", "The AutoUpdateEnabled field must be equal to \"True\" or \"False\"").ToString());


            string autoUpdateCron = Request.Form["AutoUpdateCron"];
            if (string.IsNullOrWhiteSpace(Request.Form["AutoUpdateCron"]))
                return Content(XMLMessage.Error("SET-SSES-MISAUC", "The AutoUpdateCron field must be provided").ToString());
            if (!CronExpression.IsValidExpression(autoUpdateCron))
                return Content(XMLMessage.Error("SET-SSES-BADAUC", "The AutoUpdateCron field is invalid").ToString());

            string autoUpdateBetaPassword = Request.Form["AutoUpdateBetaPassword"];

            // ** PROCESS **
            SESMConfigHelper.SEAutoUpdateEnabled = autoUpdateEnabled;
            SESMConfigHelper.SEAutoUpdateCron = autoUpdateCron;
            SESMConfigHelper.SEAutoUpdateBetaPassword = autoUpdateBetaPassword;

            // Deleting the Job
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.DeleteJob(SEAutoUpdateJob.GetJobKey());

            if (SESMConfigHelper.SEAutoUpdateEnabled)
            {
                // Instantiating the job
                IJobDetail SEAutoUpdateJobDetail = JobBuilder.Create<SEAutoUpdateJob>()
                    .WithIdentity(SEAutoUpdateJob.GetJobKey())
                    .Build();

                ITrigger SEAutoUpdateJobTrigger = TriggerBuilder.Create()
                    .WithIdentity(SEAutoUpdateJob.GetTriggerKey())
                    .WithCronSchedule(SESMConfigHelper.SEAutoUpdateCron)
                    .Build();

                scheduler.ScheduleJob(SEAutoUpdateJobDetail, SEAutoUpdateJobTrigger);
            }

            return Content(XMLMessage.Success("SET-SSES-OK", "The SE Settings has been updated").ToString());
        }

        // POST: API/Settings/UploadSE        
        [HttpPost]
        [APIHostAccess("SET-UPSE", "SETTINGS_SE")]
        public ActionResult UploadSE(HttpPostedFileBase ZipFile)
        {
            // ** PARSING **
            if (ZipFile == null)
                return Content(XMLMessage.Error("SET-UPSE-MISZIP", "The zipFile parameter must be provided").ToString());

            if (!Ionic.Zip.ZipFile.IsZipFile(ZipFile.InputStream, false))
                return Content(XMLMessage.Error("SET-UPSE-BADZIP", "The provided file in not a zip file").ToString());

            ZipFile.InputStream.Seek(0, SeekOrigin.Begin);

            ZipFile.SaveAs(SESMConfigHelper.SEDataPath + "DedicatedServer.zip");

            Logger logger = LogManager.GetLogger("SEManualUpdateLogger");
            ReturnEnum result = SEAutoUpdateJob.Run(logger, true, true, false);

            if (result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPSE-NOK", "An error occured : " + result.ToString()).ToString());


            return Content(XMLMessage.Success("SET-UPSE-OK", "SE Game Files applied").ToString());
        }

        // POST: API/Settings/UpdateSE        
        [HttpPost]
        [APIHostAccess("SET-UPDSE", "SETTINGS_SE")]
        public ActionResult UpdateSE()
        {
            // ** PARSING **
            bool force = false;
            if (!string.IsNullOrWhiteSpace(Request.Form["Force"]))
                if (!bool.TryParse(Request.Form["Force"], out force))
                    return Content(XMLMessage.Error("SET-UPDSE-BADFRC", "The value provided in the Force field is not valid").ToString());

            // ** PROCESS **
            Logger logger = LogManager.GetLogger("SEManualUpdateLogger");
            ReturnEnum result = SEAutoUpdateJob.Run(logger, true, false, force);

            if (result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPDSE-NOK", "An error occured : " + result.ToString()).ToString());

            return Content(XMLMessage.Success("SET-UPDSE-OK", "SE Game Files applied").ToString());
        }

        #endregion

        #region ME

        // GET: API/Settings/GetMEStatus        
        [HttpGet]
        [APIHostAccess("SET-GMES", "SETTINGS_ME")]
        public ActionResult GetMEStatus()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);
           
            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GMES-OK");

            response.AddToContent(new XElement("UpdateRunning", SESMConfigHelper.MEUpdating));
            response.AddToContent(new XElement("NbServer", srvPrv.GetAllMEServers().Count));
            return Content(response.ToString());
        }

        // GET: API/Settings/GetMEVersion        
        [HttpGet]
        [APIHostAccess("SET-GMEV", "SETTINGS_ME")]
        public ActionResult GetMEVersion()
        {
            // ** PROCESS **
            Logger logger = LogManager.GetLogger("MEGetVersionLogger");
            int? localVersion = null;

            int? remoteVersion = null;

            for (int i = 0; i < 5; i++)
            {
                if (localVersion == null)
                {
                    logger.Info("Retrieving local version : ");
                    localVersion = SteamCMDHelper.GetMEInstalledVersion(logger);
                }

                if (remoteVersion == null)
                {
                    logger.Info("Retrieving remote version : ");
                    remoteVersion =
                        SteamCMDHelper.GetMEAvailableVersion(
                            !string.IsNullOrWhiteSpace(SESMConfigHelper.MEAutoUpdateBetaPassword), logger);
                }

                if (localVersion == null || remoteVersion == null)
                {
                    logger.Info("Fail retrieving one of the version (try " + (i + 1) + " of 5), waiting and retrying ...");
                    Thread.Sleep(2000);
                }
                else
                    break;

            }

            if (localVersion == null || remoteVersion == null)
            {
                return Content(XMLMessage.Error("SET-GMEV-FAIL", "Fail retieving one of the version. See MEGetVersion log for more info").ToString());
            }

            XMLMessage response = new XMLMessage("SET-GMEV-OK");

            response.AddToContent(new XElement("Local", localVersion.ToString()));
            response.AddToContent(new XElement("Remote", remoteVersion.ToString()));
            response.AddToContent(new XElement("Diff", localVersion - remoteVersion));

            return Content(response.ToString());
        }

        // GET: API/Settings/GetMESettings        
        [HttpGet]
        [APIHostAccess("SET-GMESE", "SETTINGS_ME")]
        public ActionResult GetMESettings()
        {
            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GMESE-OK");

            response.AddToContent(new XElement("AutoUpdateEnabled", SESMConfigHelper.MEAutoUpdateEnabled));
            response.AddToContent(new XElement("AutoUpdateCron", SESMConfigHelper.MEAutoUpdateCron));
            response.AddToContent(new XElement("AutoUpdateBetaPassword", SESMConfigHelper.MEAutoUpdateBetaPassword));

            return Content(response.ToString());
        }

        // POST: API/Settings/SetMESettings        
        [HttpPost]
        [APIHostAccess("SET-SMES", "SETTINGS_ME")]
        public ActionResult SetMESettings()
        {
            // ** PARSING **
            bool autoUpdateEnabled;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoUpdateEnabled"]))
                return Content(XMLMessage.Error("SET-SMES-MISAUE", "The AutoUpdateEnabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["AutoUpdateEnabled"], out autoUpdateEnabled))
                return Content(XMLMessage.Error("SET-SMES-BADAUE", "The AutoUpdateEnabled field must be equal to \"True\" or \"False\"").ToString());


            string autoUpdateCron = Request.Form["AutoUpdateCron"];
            if (string.IsNullOrWhiteSpace(Request.Form["AutoUpdateCron"]))
                return Content(XMLMessage.Error("SET-SMES-MISAUC", "The AutoUpdateCron field must be provided").ToString());
            if (!CronExpression.IsValidExpression(autoUpdateCron))
                return Content(XMLMessage.Error("SET-SMES-BADAUC", "The AutoUpdateCron field is invalid").ToString());

            string autoUpdateBetaPassword = Request.Form["AutoUpdateBetaPassword"];

            // ** PROCESS **
            SESMConfigHelper.MEAutoUpdateEnabled = autoUpdateEnabled;
            SESMConfigHelper.MEAutoUpdateCron = autoUpdateCron;
            SESMConfigHelper.MEAutoUpdateBetaPassword = autoUpdateBetaPassword;

            // Deleting the Job
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.DeleteJob(MEAutoUpdateJob.GetJobKey());

            if (SESMConfigHelper.MEAutoUpdateEnabled)
            {
                // Instantiating the job
                IJobDetail MEAutoUpdateJobDetail = JobBuilder.Create<MEAutoUpdateJob>()
                    .WithIdentity(MEAutoUpdateJob.GetJobKey())
                    .Build();

                ITrigger MEAutoUpdateJobTrigger = TriggerBuilder.Create()
                    .WithIdentity(MEAutoUpdateJob.GetTriggerKey())
                    .WithCronSchedule(SESMConfigHelper.MEAutoUpdateCron)
                    .Build();

                scheduler.ScheduleJob(MEAutoUpdateJobDetail, MEAutoUpdateJobTrigger);
            }

            return Content(XMLMessage.Success("SET-SMES-OK", "The ME Settings has been updated").ToString());
        }

        // POST: API/Settings/UploadME        
        [HttpPost]
        [APIHostAccess("SET-UPME", "SETTINGS_ME")]
        public ActionResult UploadME(HttpPostedFileBase ZipFile)
        {
            // ** PARSING **
            if (ZipFile == null)
                return Content(XMLMessage.Error("SET-UPME-MISZIP", "The zipFile parameter must be provided").ToString());

            if (!Ionic.Zip.ZipFile.IsZipFile(ZipFile.InputStream, false))
                return Content(XMLMessage.Error("SET-UPME-BADZIP", "The provided file in not a zip file").ToString());

            ZipFile.InputStream.Seek(0, SeekOrigin.Begin);

            ZipFile.SaveAs(SESMConfigHelper.MEDataPath + "DedicatedServer.zip");

            Logger logger = LogManager.GetLogger("MEManualUpdateLogger");
            ReturnEnum result = MEAutoUpdateJob.Run(logger, true, true, false);

            if (result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPME-NOK", "An error occured : " + result.ToString()).ToString());


            return Content(XMLMessage.Success("SET-UPME-OK", "ME Game Files applied").ToString());
        }

        // POST: API/Settings/UpdateME        
        [HttpPost]
        [APIHostAccess("SET-UPDME", "SETTINGS_ME")]
        public ActionResult UpdateME()
        {
            // ** PARSING **
            bool force = false;
            if (!string.IsNullOrWhiteSpace(Request.Form["Force"]))
                if (!bool.TryParse(Request.Form["Force"], out force))
                    return Content(XMLMessage.Error("SET-UPDME-BADFRC", "The value provided in the Force field is not valid").ToString());

            // ** PROCESS **
            Logger logger = LogManager.GetLogger("MEManualUpdateLogger");
            ReturnEnum result = MEAutoUpdateJob.Run(logger, true, false, force);

            if (result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPDME-NOK", "An error occured : " + result.ToString()).ToString());

            return Content(XMLMessage.Success("SET-UPDME-OK", "ME Game Files applied").ToString());
        }

        #endregion

        #region SESE

        // GET: API/Settings/GetSESEStatus        
        [HttpGet]
        [APIHostAccess("SET-GSESES", "SETTINGS_SESE")]
        public ActionResult GetSESEStatus()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GSESES-OK");

            response.AddToContent(new XElement("UpdateRunning", SESMConfigHelper.SESEUpdating.ToString()));
            response.AddToContent(new XElement("NbServer", srvPrv.GetAllSESEServers().Count));
            return Content(response.ToString());
        }

        // GET: API/Settings/GetSESEVersion        
        [HttpGet]
        [APIHostAccess("SET-GSESEV", "SETTINGS_SESE")]
        public ActionResult GetSESEVersion()
        {
            // ** PROCESS **
            Version localVersion = SESEHelper.GetLocalVersion();

            Version remoteVersion = SESEHelper.GetLastRemoteVersion(SESMConfigHelper.SESEAutoUpdateUseDev);
            if (remoteVersion == null)
                return Content(XMLMessage.Error("SET-GSESEV-CONERR", "Error retrieving the github SESE Data").ToString());

            XMLMessage response = new XMLMessage("SET-GSESEV-OK");

            response.AddToContent(new XElement("Local", localVersion.ToString()));
            response.AddToContent(new XElement("Remote", remoteVersion.ToString()));
            response.AddToContent(new XElement("Diff", localVersion.CompareTo(remoteVersion)));

            return Content(response.ToString());
        }

        // GET: API/Settings/GetSESESettings        
        [HttpGet]
        [APIHostAccess("SET-GSESESE", "SETTINGS_SESE")]
        public ActionResult GetSESESettings()
        {
            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GSESESE-OK");

            response.AddToContent(new XElement("GithubURL", SESMConfigHelper.SESEUpdateURL));
            response.AddToContent(new XElement("Dev", SESMConfigHelper.SESEAutoUpdateUseDev.ToString()));
            response.AddToContent(new XElement("AutoUpdateEnabled", SESMConfigHelper.SESEAutoUpdateEnabled.ToString()));
            response.AddToContent(new XElement("AutoUpdateCron", SESMConfigHelper.SESEAutoUpdateCron));

            return Content(response.ToString());
        }

        // POST: API/Settings/SetSESESettings        
        [HttpPost]
        [APIHostAccess("SET-SSESES", "SETTINGS_SESE")]
        public ActionResult SetSESESettings()
        {
            // ** PARSING **
            string githubURL = Request.Form["GithubURL"];
            if (string.IsNullOrWhiteSpace(githubURL))
                return Content(XMLMessage.Error("SET-SSESES-MISGIT", "The Github URL must be provided").ToString());

            if (!Regex.IsMatch(githubURL, @"^https?:\/\/api\.github\.com\/repos\/.*$", RegexOptions.IgnoreCase))
                return Content(XMLMessage.Error("SET-SSESES-BADGIT", "The Github URL is invalid").ToString());


            bool dev = true;
            if (string.IsNullOrWhiteSpace(Request.Form["Dev"]))
                return Content(XMLMessage.Error("SET-SSESES-MISDEV", "The Dev field must be provided").ToString());

            if (!bool.TryParse(Request.Form["Dev"], out dev))
                return Content(XMLMessage.Error("SET-SSESES-BADDEV", "The Dev field must be equalt to \"True\" or \"False\"").ToString());


            bool autoUpdateEnabled = false;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoUpdateEnabled"]))
                return Content(XMLMessage.Error("SET-SSESES-MISAUE", "The AutoUpdateEnabled field must be provided").ToString());

            if (!bool.TryParse(Request.Form["AutoUpdateEnabled"], out autoUpdateEnabled))
                return Content(XMLMessage.Error("SET-SSESES-BADAUE", "The AutoUpdateEnabled field must be equalt to \"True\" or \"False\"").ToString());


            string autoUpdateCron = Request.Form["AutoUpdateCron"];
            if (string.IsNullOrWhiteSpace(Request.Form["AutoUpdateCron"]))
                return Content(XMLMessage.Error("SET-SSESES-MISAUC", "The AutoUpdateCron field must be provided").ToString());

            if (!CronExpression.IsValidExpression(autoUpdateCron))
                return Content(XMLMessage.Error("SET-SSESES-BADAUC", "The AutoUpdateCron field is invalid").ToString());

            // ** PROCESS **
            SESMConfigHelper.SESEUpdateURL = githubURL;
            SESMConfigHelper.SESEAutoUpdateUseDev = dev;
            SESMConfigHelper.SESEAutoUpdateEnabled = autoUpdateEnabled;
            SESMConfigHelper.SESEAutoUpdateCron = autoUpdateCron;

            // Deleting the Job
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.DeleteJob(SESEAutoUpdateJob.GetJobKey());

            if (SESMConfigHelper.SESEAutoUpdateEnabled)
            {
                // Instantiating the job
                IJobDetail SESEAutoUpdateJobDetail = JobBuilder.Create<SESEAutoUpdateJob>()
                    .WithIdentity(SESEAutoUpdateJob.GetJobKey())
                    .Build();

                ITrigger SESEAutoUpdateJobTrigger = TriggerBuilder.Create()
                    .WithIdentity(SESEAutoUpdateJob.GetTriggerKey())
                    .WithCronSchedule(SESMConfigHelper.SESEAutoUpdateCron)
                    .Build();

                scheduler.ScheduleJob(SESEAutoUpdateJobDetail, SESEAutoUpdateJobTrigger);
            }

            return Content(XMLMessage.Success("SET-GSESES-OK", "The SESE Settings has been updated").ToString());
        }

        // POST: API/Settings/UploadSESE        
        [HttpPost]
        [APIHostAccess("SET-UPSESE", "SETTINGS_SESE")]
        public ActionResult UploadSESE(HttpPostedFileBase ZipFile)
        {
            // ** PARSING **
            if (ZipFile == null)
                return Content(XMLMessage.Error("SET-UPSESE-MISZIP", "The zipFile parameter must be provided").ToString());

            if (!Ionic.Zip.ZipFile.IsZipFile(ZipFile.InputStream, false))
                return Content(XMLMessage.Error("SET-UPSESE-BADZIP", "The provided file in not a zip file").ToString());

            ZipFile.InputStream.Seek(0, SeekOrigin.Begin);
            using (ZipFile zip = Ionic.Zip.ZipFile.Read(ZipFile.InputStream))
            {
                if (!zip.ContainsEntry("SEServerExtender.exe"))
                    return
                        Content(XMLMessage.Error("SET-UPSESE-NOTSESE", "The provided zip don't contain SESE").ToString());
            }
            SESEHelper.CleanupUpdate();

            string zipName = ZipFile.FileName;
            if (!(zipName.StartsWith("SEServerExtender") && zipName.EndsWith(".zip")))
                zipName = "SEServerExtender_Uploaded.zip";

            ZipFile.SaveAs(SESMConfigHelper.SEDataPath + zipName);

            Logger logger = LogManager.GetLogger("SESEManualUpdateLogger");
            ReturnEnum result = SESEAutoUpdateJob.Run(logger, true, true);

            if (result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPSESE-NOK", "An error occured : " + result.ToString()).ToString());


            return Content(XMLMessage.Success("SET-UPSESE-OK", "SESE Update applied").ToString());
        }

        // POST: API/Settings/UpdateSESE        
        [HttpPost]
        [APIHostAccess("SET-UPDSESE", "SETTINGS_SESE")]
        public ActionResult UpdateSESE()
        {
            // ** PARSING **
            bool force = false;
            if (!string.IsNullOrWhiteSpace(Request.Form["Force"]))
                if (!bool.TryParse(Request.Form["Force"], out force))
                    return Content(XMLMessage.Error("SET-UPDSESE-NOACCESS", "The value provided in the Force field is not valid").ToString());

            // ** PROCESS **
            Logger logger = LogManager.GetLogger("SESEManualUpdateLogger");
            ReturnEnum result = SESEAutoUpdateJob.Run(logger, true, false, force);

            if (result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPDSESE-NOK", "An error occured : " + result.ToString()).ToString());

            return Content(XMLMessage.Success("SET-UPDSESE-OK", "SESE Update applied").ToString());
        }

        // GET: API/Settings/DeleteSESE        
        [HttpGet]
        [APIHostAccess("SET-DELSESE", "SETTINGS_SESE")]
        public ActionResult DeleteSESE()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PROCESS **
            List<EntityServer> serverList = srvPrv.GetAllSESEServers();

            foreach (EntityServer server in serverList)
            {
                ServiceHelper.StopService(server);
            }

            foreach (EntityServer server in serverList)
            {
                ServiceHelper.WaitForStopped(server);
            }
            Thread.Sleep(10000);
            ServiceHelper.KillAllSESEServices();
            Thread.Sleep(2000);
            if (System.IO.File.Exists(SESMConfigHelper.SEDataPath + "DedicatedServer64\\SEServerExtender.exe"))
                System.IO.File.Delete(SESMConfigHelper.SEDataPath + "DedicatedServer64\\SEServerExtender.exe");

            return Content(XMLMessage.Success("SET-DELSESE-OK", "SESE deleted").ToString());
        }

        #endregion

        // GET: API/Settings/GetCachedVersions        
        [HttpGet]
        public ActionResult GetCachedVersions()
        {

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GCV-OK");

            response.AddToContent(new XElement("SELocalVersion", VersionCache.SELocalVersion));
            response.AddToContent(new XElement("SERemoteVersion", VersionCache.SERemoteVersion));
            response.AddToContent(new XElement("MELocalVersion", VersionCache.MELocalVersion));
            response.AddToContent(new XElement("MERemoteVersion", VersionCache.MERemoteVersion));
            response.AddToContent(new XElement("SESELocalVersion", VersionCache.SESELocalVersion));
            response.AddToContent(new XElement("SESERemoteVersion", VersionCache.SESERemoteVersion));
            response.AddToContent(new XElement("SESMLocalVersion", Constants.GetVersion()));

            return Content(response.ToString());
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