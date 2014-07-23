using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Ionic.Zip;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Map;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    [LoggedOnly]
    [CheckAuth]
    [ManagerAndAbove]
    public class MapController : Controller
    {
        readonly DataContext _context = new DataContext();

        //
        // GET: Map/5
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
            

            string[] listDir = Directory.GetDirectories(PathHelper.GetSavesPath(serv));

            List<SelectListItem> listSLI = new List<SelectListItem>();

            foreach (string item in listDir)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = PathHelper.GetLastDirName(item);
                sli.Value = PathHelper.GetLastDirName(item);
                listSLI.Add(sli);
            }

            ViewData["listDir"] = listSLI;

            MapViewModel model = new MapViewModel();

            ServerConfigHelper serverConfig = new ServerConfigHelper();
            serverConfig.Load(PathHelper.GetConfigurationFilePath(serv));
            model.MapName = serverConfig.SaveName;

            return View(model);
        }

        //
        // POST: Map/Save/5
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveMap")]
        public ActionResult Save(int id, MapViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);


            if (ModelState.IsValid)
            {
                ServiceHelper.StopServiceAndWait(ServiceHelper.GetServiceName(serv));
                ServerConfigHelper serverConfig = new ServerConfigHelper();
                serverConfig.Load(PathHelper.GetConfigurationFilePath(serv));
                serverConfig.SaveName = model.MapName;
                serverConfig.Save(serv);
                ServiceHelper.StartService(ServiceHelper.GetServiceName(serv));
                return RedirectToAction("Status", "Server", new { id = id });
            }

            return RedirectToAction("Index", new { id = id });
        }

        //
        // POST: Map/Save/5
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteMap")]
        public ActionResult Delete(int id, MapViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid && Directory.Exists(PathHelper.GetSavePath(serv, model.MapName) + @"\"))
            {
                Directory.Delete(PathHelper.GetSavePath(serv, model.MapName) + @"\", true);
            }

            return RedirectToAction("Index", new { id = id });
        }

        //
        // POST: Map/Save/5
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DownloadMap")]
        public ActionResult Download(int id, MapViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            ViewData["ID"] = id;
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid)
            {
                string sourceFolderPath = PathHelper.GetSavePath(serv, model.MapName) + @"\";
                if (Directory.Exists(sourceFolderPath))
                {

                    Response.Clear();
                    Response.ContentType = "application/zip";
                    Response.AddHeader("Content-Disposition",
                        String.Format("attachment; filename={0}", model.MapName + ".zip"));

                    using (ZipFile zip = new ZipFile())
                    {
                        zip.AddSelectedFiles("*", sourceFolderPath, string.Empty, true);
                        zip.Save(Response.OutputStream);
                    }
                    Response.End();
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        //
        // GET: Map/New/5
        [HttpGet]
        public ActionResult New(int id)
        {
            ViewData["ID"] = id;
            return View(new NewMapViewModel());
        }

        //
        // POST: Map/New/5
        [HttpPost]
        public ActionResult New(int id, NewMapViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            ViewData["ID"] = id;
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid)
            {
                ServiceHelper.StopServiceAndWait(ServiceHelper.GetServiceName(serv));
                ServerConfigHelper serverConfig = new ServerConfigHelper();
                serverConfig.Load(PathHelper.GetConfigurationFilePath(serv));
                serverConfig.ScenarioType = model.MapType;
                serverConfig.AsteroidAmount = model.AsteroidAmount;
                serverConfig.SaveName = string.Empty;
                serverConfig.Save(serv);
                ServiceHelper.StartService(ServiceHelper.GetServiceName(serv));
                return RedirectToAction("Status","Server",  new { id = id });
            }

            return View(model);
        }

        //
        // GET: Map/Upload/5
        [HttpGet]
        [AdminAndAbove]
        public ActionResult Upload(int id)
        {
            ViewData["ID"] = id;
            return View();
        }

        //
        // POST: Map/Upload/5
        [HttpPost]
        [AdminAndAbove]
        public ActionResult Upload(int id, UploadMapViewModel model)
        {
            if (!ZipFile.IsZipFile(model.SaveZip.InputStream, false))
            {
                ModelState.AddModelError("ZipError", "Your File is not a valid zip file");
                return View(model);
            }
            model.SaveZip.InputStream.Seek(0, SeekOrigin.Begin);
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            string[] listDir = Directory.GetDirectories(PathHelper.GetSavesPath(serv));

            if (listDir.Any(item => item == model.SaveName))
            {
                ModelState.AddModelError("nameAlreadExist", "A save with this name already exist");
                return View(model);
            }
            string path = PathHelper.GetSavesPath(serv) + model.SaveName + @"\";

            using (ZipFile zip = ZipFile.Read(model.SaveZip.InputStream))
            {
                if (!zip.ContainsEntry("SANDBOX_0_0_0_.sbs"))
                {
                    ModelState.AddModelError("InvalidSave", "Your save zip is invalid");
                    return View(model);
                }
                Directory.CreateDirectory(path);
                zip.ExtractAll(path);
            }
            return RedirectToAction("Index", new { id = id });
        }
    }
}