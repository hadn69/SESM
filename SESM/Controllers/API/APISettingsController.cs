using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Ionic.Zip;
using NLog;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;
using SESM.Tools.Helpers;

namespace SESM.Controllers.API
{
    public class APISettingsController : Controller
    {
        private readonly DataContext _context = new DataContext();

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
            // TODO
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

            // Instatiating the job



            return Content(XMLMessage.Success("SET-GSESES-OK", "The SESE Settings has been updated").ToString());
        }

        // POST: API/Settings/UploadSESE        
        [HttpPost]
        public ActionResult UploadSESE(HttpPostedFileBase zipFile)
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING **
            if(zipFile == null)
                return Content(XMLMessage.Error("SET-UPSESE-MISZIP", "The zipFile parameter must be provided").ToString());

            if(!ZipFile.IsZipFile(zipFile.InputStream, false))
                return Content(XMLMessage.Error("SET-UPSESE-BADZIP", "The provided file in not a zip file").ToString());

            zipFile.InputStream.Seek(0, SeekOrigin.Begin);
            using(ZipFile zip = ZipFile.Read(zipFile.InputStream))
            {
                if(!zip.ContainsEntry("SEServerExtender.exe"))
                    return Content(XMLMessage.Error("SET-UPSESE-NOTSESE", "The provided zip don't contain SESE").ToString());

                // ** ACCESS **
                if(user == null || !user.IsAdmin)
                    return Content(XMLMessage.Error("SET-UPSESE-NOACCESS","The current user don't have enough right for this action").ToString());

                // ** PROCESS **
                Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

                List<EntityServer> serverList =
                    srvPrv.GetAllSESEServers().Where(item => srvPrv.GetState(item) != ServiceState.Stopped).ToList();

                foreach(EntityServer item in serverList)
                {
                    serviceLogger.Info(item.Name + " stopped by " + user.Login + " by API/Settings/UploadSESE");
                    ServiceHelper.StopService(item);
                }

                foreach(EntityServer item in serverList)
                {
                    ServiceHelper.WaitForStopped(item);
                }

                Thread.Sleep(2000);
                ServiceHelper.KillAllSESEService();

                try
                {
                    zip.ExtractAll(SESMConfigHelper.SEDataPath + "DedicatedServer64/",ExtractExistingFileAction.OverwriteSilently);
                }
                catch (Exception)
                {
                    return Content(XMLMessage.Error("SET-UPSESE-FAILEXTR", "An error occurred while extracting the zip file").ToString());
                }

                foreach(EntityServer item in serverList)
                {
                    serviceLogger.Info(item.Name + " started by " + user.Login + " by API/Settings/UploadSESE");
                    ServiceHelper.StartService(item);
                }

                return Content(XMLMessage.Success("SET-UPSESE-OK", "The following server(s) have been restarted : " + string.Join(", ", serverList.Select(x => x.Name))).ToString());
            }
        }


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