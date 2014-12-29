using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Xml.Linq;
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
            EntityUser user = Session["User"] as EntityUser;
            if(user == null || !user.IsAdmin)
                return Content(new XMLMessage(XmlResponseType.Error, "SET-GSESEV-NOACCESS", "The current user don't have enough right for this action").ToString());

            string SESELocPath = SESMConfigHelper.SEDataPath + "DedicatedServer64\\SEServerExtender.exe";
            Version localVersion = new Version(0,0,0,0);
            Version remoteVersion;

            if(System.IO.File.Exists(SESELocPath))
                localVersion = AssemblyName.GetAssemblyName(SESELocPath).Version;

            string githubData = GithubHelper.GetGithubData();

            remoteVersion = GithubHelper.GetLastVersion(githubData, SESMConfigHelper.SESEDev);
            if(remoteVersion == null)
                return Content(new XMLMessage(XmlResponseType.Error, "SET-GSESEV-CONERR", "Error retrieving the github SESE Data").ToString());

            XMLMessage response = new XMLMessage("SET-GSESEV-OK");

            response.AddToContent(new XElement("Local", localVersion.ToString()));
            response.AddToContent(new XElement("Remote", remoteVersion.ToString()));
            response.AddToContent(new XElement("Diff", localVersion.CompareTo(remoteVersion)));

            return Content(response.ToString());
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