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
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;
using SESM.Tools.Helpers;
using SESM.Tools.Jobs;

namespace SESM.Controllers.API
{
    public class APISettingsController : Controller
    {
        private readonly DataContext _context = new DataContext();

        #region SE

        // GET: API/Settings/GetSEStatus        
        [HttpGet]
        public ActionResult GetSEStatus()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** ACCESS **
            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-GSES-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GSES-OK");

            response.AddToContent(new XElement("UpdateRunning", SESMConfigHelper.SEUpdating.ToString()));
            response.AddToContent(new XElement("NbServer", srvPrv.GetAllServers().Count));
            return Content(response.ToString());
        }

        // GET: API/Settings/GetSEVersion        
        [HttpGet]
        public ActionResult GetSEVersion()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-GSEV-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PROCESS **
            int localVersion = SteamCMDHelper.GetInstalledVersion();

            int remoteVersion = SteamCMDHelper.GetAvailableVersion(!string.IsNullOrWhiteSpace(SESMConfigHelper.AutoUpdateBetaPassword));
            
            XMLMessage response = new XMLMessage("SET-GSEV-OK");

            response.AddToContent(new XElement("Local", localVersion.ToString()));
            response.AddToContent(new XElement("Remote", remoteVersion.ToString()));
            response.AddToContent(new XElement("Diff", localVersion.CompareTo(remoteVersion)));

            return Content(response.ToString());
        }

        // GET: API/Settings/GetSESettings        
        [HttpGet]
        public ActionResult GetSESettings()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-GSES-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GSES-OK");

            response.AddToContent(new XElement("AutoUpdateEnabled", SESMConfigHelper.AutoUpdateEnabled));
            response.AddToContent(new XElement("AutoUpdateCron", SESMConfigHelper.AutoUpdateCron));
            response.AddToContent(new XElement("AutoUpdateBetaPassword", SESMConfigHelper.AutoUpdateBetaPassword));

            return Content(response.ToString());
        }

        // POST: API/Settings/SetSESettings        
        [HttpPost]
        public ActionResult SetSESettings()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-SSES-NOACCESS", "The current user don't have enough right for this action").ToString());

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
            SESMConfigHelper.AutoUpdateEnabled = autoUpdateEnabled;
            SESMConfigHelper.AutoUpdateCron = autoUpdateCron;
            SESMConfigHelper.AutoUpdateBetaPassword = autoUpdateBetaPassword;

            // Deleting the Job
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.DeleteJob(SEAutoUpdateJob.GetJobKey());

            if (SESMConfigHelper.AutoUpdateEnabled)
            {
                // Instantiating the job
                IJobDetail SEAutoUpdateJobDetail = JobBuilder.Create<SESEAutoUpdateJob>()
                    .WithIdentity(SEAutoUpdateJob.GetJobKey())
                    .Build();

                ITrigger SEAutoUpdateJobTrigger = TriggerBuilder.Create()
                    .WithIdentity(SEAutoUpdateJob.GetTriggerKey())
                    .WithCronSchedule(SESMConfigHelper.SESEAutoUpdateCron)
                    .Build();

                scheduler.ScheduleJob(SEAutoUpdateJobDetail, SEAutoUpdateJobTrigger);
            }

            return Content(XMLMessage.Success("SET-SSES-OK", "The SESE Settings has been updated").ToString());
        }

        // POST: API/Settings/UploadSE        
        [HttpPost]
        public ActionResult UploadSE(HttpPostedFileBase ZipFile)
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            if (ZipFile == null)
                return Content(XMLMessage.Error("SET-UPSE-MISZIP", "The zipFile parameter must be provided").ToString());

            if (!Ionic.Zip.ZipFile.IsZipFile(ZipFile.InputStream, false))
                return Content(XMLMessage.Error("SET-UPSE-BADZIP", "The provided file in not a zip file").ToString());

            ZipFile.InputStream.Seek(0, SeekOrigin.Begin);

            // ** ACCESS **
            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-UPSE-NOACCESS", "The current user don't have enough right for this action").ToString());

            SESEHelper.CleanupUpdate();

            ZipFile.SaveAs(SESMConfigHelper.SEDataPath + "DedicatedServer.zip");

            Logger logger = LogManager.GetLogger("SEManualUpdateLogger");
            ReturnEnum result = SEAutoUpdateJob.Run(logger, true, true, false);

            if (result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPSE-NOK", "An error occured : " + result.ToString()).ToString());


            return Content(XMLMessage.Success("SET-UPSE-OK", "SE Game Files applied").ToString());
        }

        // POST: API/Settings/UpdateSE        
        [HttpPost]
        public ActionResult UpdateSE()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            bool force = false;
            if (!string.IsNullOrWhiteSpace(Request.Form["Force"]))
                if (!bool.TryParse(Request.Form["Force"], out force))
                    return Content(XMLMessage.Error("SET-UPDSE-BADFRC", "The value provided in the Force field is not valid").ToString());

            // ** ACCESS **
            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-UPDSE-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PROCESS **
            Logger logger = LogManager.GetLogger("SEManualUpdateLogger");
            ReturnEnum result = SEAutoUpdateJob.Run(logger, true, false, force);

            if (result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPDSE-NOK", "An error occured : " + result.ToString()).ToString());

            return Content(XMLMessage.Success("SET-UPDSE-OK", "SE Game Files applied").ToString());
        }

        #endregion

        #region SESE

        // GET: API/Settings/GetSESEStatus        
        [HttpGet]
        public ActionResult GetSESEStatus()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** ACCESS **
            if(user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-GSESES-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GSESES-OK");

            response.AddToContent(new XElement("UpdateRunning", SESMConfigHelper.SESEUpdating.ToString()));
            response.AddToContent(new XElement("NbServer", srvPrv.GetAllSESEServers().Count));
            return Content(response.ToString());
        }

        // GET: API/Settings/GetSESEVersion        
        [HttpGet]
        public ActionResult GetSESEVersion()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if(user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-GSESEV-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PROCESS **
            Version localVersion = SESEHelper.GetLocalVersion();

            Version remoteVersion = SESEHelper.GetLastRemoteVersion(SESMConfigHelper.SESEAutoUpdateUseDev);
            if(remoteVersion == null)
                return Content(XMLMessage.Error("SET-GSESEV-CONERR", "Error retrieving the github SESE Data").ToString());

            XMLMessage response = new XMLMessage("SET-GSESEV-OK");

            response.AddToContent(new XElement("Local", localVersion.ToString()));
            response.AddToContent(new XElement("Remote", remoteVersion.ToString()));
            response.AddToContent(new XElement("Diff", localVersion.CompareTo(remoteVersion)));

            return Content(response.ToString());
        }

        // GET: API/Settings/GetSESESettings        
        [HttpGet]
        public ActionResult GetSESESettings()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if(user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-GSESES-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SET-GSESES-OK");

            response.AddToContent(new XElement("GithubURL", SESMConfigHelper.SESEUpdateURL));
            response.AddToContent(new XElement("Dev", SESMConfigHelper.SESEAutoUpdateUseDev.ToString()));
            response.AddToContent(new XElement("AutoUpdateEnabled", SESMConfigHelper.AutoUpdateEnabled.ToString()));
            response.AddToContent(new XElement("AutoUpdateCron", SESMConfigHelper.SESEAutoUpdateCron));

            return Content(response.ToString());
        }

        // POST: API/Settings/SetSESESettings        
        [HttpPost]
        public ActionResult SetSESESettings()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if(user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-SSESES-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PARSING **
            string githubURL = Request.Form["GithubURL"];
            if(string.IsNullOrWhiteSpace(githubURL))
                return Content(XMLMessage.Error("SET-SSESES-MISGIT", "The Github URL must be provided").ToString());

            if(!Regex.IsMatch(githubURL, @"^https?:\/\/api\.github\.com\/repos\/.*$", RegexOptions.IgnoreCase))
                return Content(XMLMessage.Error("SET-SSESES-BADGIT", "The Github URL is invalid").ToString());


            bool dev = true;
            if(string.IsNullOrWhiteSpace(Request.Form["Dev"]))
                return Content(XMLMessage.Error("SET-SSESES-MISDEV", "The Dev field must be provided").ToString());

            if(!bool.TryParse(Request.Form["Dev"], out dev))
                return Content(XMLMessage.Error("SET-SSESES-BADDEV", "The Dev field must be equalt to \"True\" or \"False\"").ToString());


            bool autoUpdateEnabled = false;
            if(string.IsNullOrWhiteSpace(Request.Form["AutoUpdateEnabled"]))
                return Content(XMLMessage.Error("SET-SSESES-MISAUE", "The AutoUpdateEnabled field must be provided").ToString());

            if(!bool.TryParse(Request.Form["AutoUpdateEnabled"], out autoUpdateEnabled))
                return Content(XMLMessage.Error("SET-SSESES-BADAUE", "The AutoUpdateEnabled field must be equalt to \"True\" or \"False\"").ToString());


            string autoUpdateCron = Request.Form["AutoUpdateCron"];
            if(string.IsNullOrWhiteSpace(Request.Form["AutoUpdateCron"]))
                return Content(XMLMessage.Error("SET-SSESES-MISAUC", "The AutoUpdateCron field must be provided").ToString());

            if(!CronExpression.IsValidExpression(autoUpdateCron))
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
        public ActionResult UploadSESE(HttpPostedFileBase ZipFile)
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            if(ZipFile == null)
                return Content(XMLMessage.Error("SET-UPSESE-MISZIP", "The zipFile parameter must be provided").ToString());

            if(!Ionic.Zip.ZipFile.IsZipFile(ZipFile.InputStream, false))
                return Content(XMLMessage.Error("SET-UPSESE-BADZIP", "The provided file in not a zip file").ToString());

            ZipFile.InputStream.Seek(0, SeekOrigin.Begin);
            using (ZipFile zip = Ionic.Zip.ZipFile.Read(ZipFile.InputStream))
            {
                if (!zip.ContainsEntry("SEServerExtender.exe"))
                    return
                        Content(XMLMessage.Error("SET-UPSESE-NOTSESE", "The provided zip don't contain SESE").ToString());
            }
            // ** ACCESS **
            if(user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-UPSESE-NOACCESS","The current user don't have enough right for this action").ToString());

            SESEHelper.CleanupUpdate();

            string zipName = ZipFile.FileName;
            if (!(zipName.StartsWith("SEServerExtender") && zipName.EndsWith(".zip")))
                zipName = "SEServerExtender_Uploaded.zip";

            ZipFile.SaveAs(SESMConfigHelper.SEDataPath + zipName);

            Logger logger = LogManager.GetLogger("SESEManualUpdateLogger");
            ReturnEnum result = SESEAutoUpdateJob.Run(logger, true, true);

            if(result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPSESE-NOK", "An error occured : " + result.ToString()).ToString());
                

            return Content(XMLMessage.Success("SET-UPSESE-OK", "SESE Update applied").ToString());
        }

        // POST: API/Settings/UpdateSESE        
        [HttpPost]
        public ActionResult UpdateSESE()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            bool force = false;
            if (!string.IsNullOrWhiteSpace(Request.Form["Force"]))
                if(!bool.TryParse(Request.Form["Force"], out force))
                    return Content(XMLMessage.Error("SET-UPDSESE-NOACCESS", "The value provided in the Force field is not valid").ToString());

            // ** ACCESS **
            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-UPDSESE-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PROCESS **
            Logger logger = LogManager.GetLogger("SESEManualUpdateLogger");
            ReturnEnum result = SESEAutoUpdateJob.Run(logger, true, false, force);
            
            if(result != ReturnEnum.Success)
                return Content(XMLMessage.Warning("SET-UPDSESE-NOK", "An error occured : " + result.ToString()).ToString());

            return Content(XMLMessage.Success("SET-UPDSESE-OK", "SESE Update applied").ToString());
        }

        // GET: API/Settings/DeleteSESE        
        [HttpGet]
        public ActionResult DeleteSESE()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** ACCESS **
            if(user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("SET-DELSESE-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PROCESS **
            List<EntityServer> serverList = srvPrv.GetAllSESEServers();

            foreach (EntityServer server in serverList)
            {
                ServiceHelper.StopService(server);
            }

            foreach(EntityServer server in serverList)
            {
                ServiceHelper.WaitForStopped(server);
            }
            Thread.Sleep(10000);
            ServiceHelper.KillAllSESEServices();
            Thread.Sleep(2000);
            if(System.IO.File.Exists(SESMConfigHelper.SEDataPath + "DedicatedServer64\\SEServerExtender.exe"))
                System.IO.File.Delete(SESMConfigHelper.SEDataPath + "DedicatedServer64\\SEServerExtender.exe");

            return Content(XMLMessage.Success("SET-DELSESE-OK", "SESE deleted").ToString());
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}