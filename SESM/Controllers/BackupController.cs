using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Xml;
using Ionic.Zip;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Backup;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    [LoggedOnly]
    [CheckAuth]
    [ManagerAndAbove]
    [CheckLockout]
    public class BackupController : Controller
    {
        private readonly DataContext _context = new DataContext();

        //
        // GET: Mod/5
        [HttpGet]
        public ActionResult Index(int id)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);
            if (serv == null)
            {
                return RedirectToAction("Index", "Server");
            }

            ViewData["ID"] = id;
            ViewData["AccessLevel"] = srvPrv.GetAccessLevel(user.Id, serv.Id);

            if (!Directory.Exists(PathHelper.GetBackupsPath(serv)))
                Directory.CreateDirectory(PathHelper.GetBackupsPath(serv));

            string[] listDir = Directory.GetFiles(PathHelper.GetBackupsPath(serv));

            List<SelectListItem> listSLI = new List<SelectListItem>();

            foreach (string item in listDir)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = PathHelper.GetLastDirName(item);
                sli.Value = PathHelper.GetLastDirName(item);
                listSLI.Add(sli);
            }

            ViewData["listDir"] = listSLI;

            BackupViewModel model = new BackupViewModel();


            return View(model);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteBackup")]
        public ActionResult Delete(int id, BackupViewModel model)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid && System.IO.File.Exists(PathHelper.GetBackupsPath(serv) + model.BackupName))
            {
                System.IO.File.Delete(PathHelper.GetBackupsPath(serv) + model.BackupName);
                return RedirectToAction("Index", new { id = id }).Success("Backup \"" + model.BackupName + "\" deleted successfuly");
            }

            return RedirectToAction("Index", new { id = id }).Danger("Backup \"" + model.BackupName + "\" don't exist");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DownloadBackup")]
        public ActionResult Download(int id, BackupViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            ViewData["ID"] = id;
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid)
            {
                if (System.IO.File.Exists(PathHelper.GetBackupsPath(serv) + model.BackupName))
                {

                    Response.Clear();
                    Response.ContentType = "application/zip";
                    Response.AddHeader("Content-Disposition",
                        String.Format("attachment; filename={0}", model.BackupName));

                    FileStream fs = new FileStream(PathHelper.GetBackupsPath(serv) + model.BackupName, FileMode.Open,
                        FileAccess.Read, FileShare.Read);

                    fs.CopyTo(Response.OutputStream);
                    Response.End();
                }
            }
            return RedirectToAction("Index", new {id = id});
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RestoreBackup")]
        public ActionResult Restore(int id, BackupViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            ViewData["ID"] = id;
            EntityServer serv = srvPrv.GetServer(id);

            if (!ModelState.IsValid) 
                return RedirectToAction("Status", "Server", new {id = id});

            if(!System.IO.File.Exists(PathHelper.GetBackupsPath(serv) + model.BackupName))
                return RedirectToAction("Index", new { id = id }).Danger("Backup \"" + model.BackupName + "\" don't exist");
            using (ZipFile zip = ZipFile.Read(PathHelper.GetBackupsPath(serv) + model.BackupName))
            {
                zip.ExtractAll(PathHelper.GetSavesPath(serv) + model.BackupName);
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(PathHelper.GetSavePath(serv, model.BackupName) + @"\Sandbox.sbc");
            XmlNode root = doc.DocumentElement;
            XmlNode nameNode = root.SelectSingleNode("descendant::SessionName");

            string savename = nameNode.InnerText
                .Replace("/", "")
                .Replace("\\", "")
                .Replace(":", "")
                .Replace("*", "")
                .Replace("?", "")
                .Replace("\"", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("|", "");

            bool toRestart = false;
            ServerConfigHelper config = new ServerConfigHelper();
            config.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));

            if (Directory.Exists(PathHelper.GetSavePath(serv, savename)))
            {
                    
                if (savename == config.SaveName)
                {
                    if(srvPrv.GetState(serv) != ServiceState.Stopped)
                    {
                        toRestart = true;
                        ServiceHelper.StopServiceAndWait(ServiceHelper.GetServiceName(serv));
                    }
                    config.LoadFromSave(PathHelper.GetSavePath(serv, savename));
                        
                }
                Directory.Delete(PathHelper.GetSavePath(serv, savename), true);
            }

            Directory.Move(PathHelper.GetSavesPath(serv) + model.BackupName, PathHelper.GetSavePath(serv, savename));
            config.Save(serv);

            if(toRestart)
                ServiceHelper.StartService(ServiceHelper.GetServiceName(serv));
            return RedirectToAction("Status", "Server", new { id = id }).Success("Backup \"" + model.BackupName + "\" (Map \"" + nameNode.InnerText + "\") restored");
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