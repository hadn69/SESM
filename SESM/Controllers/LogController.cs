using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Ionic.Zip;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Log;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    [LoggedOnly]
    [CheckAuth]
    [CheckLockout]
    public class LogController : Controller
    {
        readonly DataContext _context = new DataContext();

        //
        // GET: Log/5
        [HttpGet]
        [ManagerAndAbove]
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

            string[] listLog = Directory.GetFiles(PathHelper.GetInstancePath(serv),"*.log");

            List<SelectListItem> listSLI = new List<SelectListItem>();

            foreach (string item in listLog)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = System.IO.File.GetLastWriteTime(item).ToString("dd/MM/yy HH:mm:ss") + " " + PathHelper.GetLastLeaf(item);
                sli.Value = PathHelper.GetLastLeaf(item);
                listSLI.Add(sli);
            }

            ViewData["listLog"] = listSLI;

            LogViewModel model = new LogViewModel();

            return View(model);
        }

        //
        // POST: Log/View/5
        [HttpPost]
        [ManagerAndAbove]
        [MultipleButton(Name = "action", Argument = "ViewLog")]
        public ActionResult View(int id, LogViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            ViewData["ID"] = id;
            EntityServer serv = srvPrv.GetServer(id);
            string path = PathHelper.GetInstancePath(serv) + model.LogName;
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", new {id = id});
            }
            if (System.IO.File.Exists(path))
            {
                ViewData["LogName"] = model.LogName;
                FileStream logFileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader logFileReader = new StreamReader(logFileStream);
                List<string> logList = new List<string>();
                while (!logFileReader.EndOfStream)
                {
                    logList.Add(logFileReader.ReadLine());
                }

                logFileReader.Close();
                logFileStream.Close();
                ViewData["logEntries"] = logList.ToArray();
            }
            else
                ViewData["logEntries"] = new string[0];
            return View("View");
        }

        //
        // POST: Log/DeleteLog
        [HttpPost]
        [ManagerAndAbove]
        [MultipleButton(Name = "action", Argument = "DeleteLog")]
        public ActionResult Delete(int id, LogViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid && System.IO.File.Exists(PathHelper.GetInstancePath(serv) + model.LogName))
            {
                System.IO.File.Delete(PathHelper.GetInstancePath(serv) + model.LogName);
                return RedirectToAction("Index", new { id = id }).Success("Log \"" + model.LogName + "\" deleted");
            }

            return RedirectToAction("Index", new { id = id }).Success("Log \"" + model.LogName + "\" don't exist");
        }

        //
        // POST: Log/DownloadLog/5
        [HttpPost]
        [ManagerAndAbove]
        [MultipleButton(Name = "action", Argument = "DownloadLog")]
        public ActionResult Download(int id, LogViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            ViewData["ID"] = id;
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid && System.IO.File.Exists(PathHelper.GetInstancePath(serv) + model.LogName))
            {
                Response.Clear();
                Response.ContentType = "application/zip";
                Response.AddHeader("Content-Disposition",
                    String.Format("attachment; filename={0}", model.LogName + ".zip"));

                using (ZipFile zip = new ZipFile())
                {
                    zip.AddFile(PathHelper.GetInstancePath(serv) + model.LogName,"/");
                    zip.Save(Response.OutputStream);
                }
                Response.End();
            }
            return RedirectToAction("Index", new { id = id });
        }

        //
        // POST: Log/DownloadLog/5
        [HttpGet]
        [ManagerAndAbove]
        public ActionResult DownloadAll(int id)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            ViewData["ID"] = id;
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid)
            {
                Response.Clear();
                Response.ContentType = "application/zip";
                Response.AddHeader("Content-Disposition",
                    String.Format("attachment; filename={0}", serv.Name + "_Logs.zip"));

                using (ZipFile zip = new ZipFile())
                {
                    zip.AddFiles(Directory.GetFiles(PathHelper.GetInstancePath(serv), "*.log"),"/");
                    zip.Save(Response.OutputStream);
                }
                Response.End();
            }
            return RedirectToAction("Index", new { id = id });
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