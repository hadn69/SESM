using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using Ionic.Zip;
using NLog;
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
    [CheckLockout]
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
            ViewData["ServerState"] = srvPrv.GetState(serv);

            string[] listDir = Directory.GetDirectories(PathHelper.GetSavesPath(serv));

            List<SelectListItem> listSLI = new List<SelectListItem>();

            foreach (string item in listDir)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = PathHelper.GetLastLeaf(item);
                sli.Value = PathHelper.GetLastLeaf(item);
                listSLI.Add(sli);
            }

            ViewData["listDir"] = listSLI;

            MapViewModel model = new MapViewModel();

            ServerConfigHelper serverConfig = new ServerConfigHelper();
            serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));
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
                if(srvPrv.GetState(serv) != ServiceState.Stopped)
                    return RedirectToAction("Index", new { id = id });
                if (serv.UseServerExtender)
                {
                    ServerConfigHelper oldConfig = new ServerConfigHelper();
                    oldConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));
                    oldConfig.LoadFromSave(PathHelper.GetSavePath(serv, oldConfig.SaveName) + @"\Sandbox.sbc");
                    oldConfig.AutoSaveInMinutes = serv.AutoSaveInMinutes??5;
                    oldConfig.Save(serv);
                }

                ServerConfigHelper serverConfig = new ServerConfigHelper();
                serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));
                serverConfig.LoadFromSave(PathHelper.GetSavePath(serv, serverConfig.SaveName) + @"\Sandbox.sbc");
                serverConfig.SaveName = model.MapName;
                serverConfig.LoadFromSaveManager(PathHelper.GetSavePath(serv, model.MapName) + @"\Sandbox.sbc");
                serv.AutoSaveInMinutes = serverConfig.AutoSaveInMinutes;
                srvPrv.UpdateServer(serv);
                if (serv.UseServerExtender)
                {
                    serverConfig.AutoSaveInMinutes = 0;
                }
                serverConfig.Save(serv);
                return RedirectToAction("Status", "Server", new { id = id }).Success("Map Selected");
            }

            return RedirectToAction("Index", new { id = id });
        }

        //
        // POST: Map/Save/5
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveRestartMap")]
        public ActionResult SaveRestart(int id, MapViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);


            if (ModelState.IsValid)
            {
                Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

                serviceLogger.Info(serv.Name + " stopped by " + user.Login + " by select and restart map button");
                ServiceHelper.StopServiceAndWait(serv);

                if(serv.UseServerExtender)
                {
                    ServerConfigHelper oldConfig = new ServerConfigHelper();
                    oldConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));
                    oldConfig.LoadFromSave(PathHelper.GetSavePath(serv, oldConfig.SaveName) + @"\Sandbox.sbc");
                    oldConfig.AutoSaveInMinutes = serv.AutoSaveInMinutes ?? 5;
                    oldConfig.Save(serv);
                }

                ServerConfigHelper serverConfig = new ServerConfigHelper();
                serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));
                serverConfig.LoadFromSave(PathHelper.GetSavePath(serv, serverConfig.SaveName) + @"\Sandbox.sbc");
                serverConfig.SaveName = model.MapName;
                serverConfig.LoadFromSaveManager(PathHelper.GetSavePath(serv, model.MapName) + @"\Sandbox.sbc");
                serv.AutoSaveInMinutes = serverConfig.AutoSaveInMinutes;
                srvPrv.UpdateServer(serv);
                if(serv.UseServerExtender)
                {
                    serverConfig.AutoSaveInMinutes = 0;
                }
                serverConfig.Save(serv);
                
                serviceLogger.Info(serv.Name + " started by " + user.Login + " by select and restart map button");
                ServiceHelper.StartService(serv);
                return RedirectToAction("Status", "Server", new { id = id }).Success("Map Selected, server is restarting"); ;
            }

            return RedirectToAction("Index", new { id = id });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RenameMap")]
        public ActionResult Rename(int id, MapViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid)
            {
                ViewData["ID"] = id;
                RenameMapViewModel renameModel = new RenameMapViewModel();
                if (!Directory.Exists(PathHelper.GetSavePath(serv, model.MapName)))
                    return RedirectToAction("Index", new {id = id});
                renameModel.CurrentMapName = model.MapName;
                renameModel.NewMapName = model.MapName;
                return View("Rename", renameModel);
            }

            return RedirectToAction("Index", new { id = id });
        }

        
        [HttpPost]
        public ActionResult Rename(int id, RenameMapViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid)
            {
                if (!Directory.Exists(PathHelper.GetSavePath(serv, model.CurrentMapName)))
                    return RedirectToAction("Index", new { id = id }).Danger("Map \"" + model.CurrentMapName + "\" don't exist"); ;
                if(model.CurrentMapName == model.NewMapName) // Nothing to do
                    return RedirectToAction("Index", new { id = id });

                ServerConfigHelper serverConfig = new ServerConfigHelper();
                serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));
                bool toRestart = false;
                Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                if (model.CurrentMapName == serverConfig.SaveName)
                {
               
                    if(srvPrv.GetState(serv) == ServiceState.Running)
                    {
                        serviceLogger.Info(serv.Name + " stopped by " + user.Login + " for map renaming");
                        ServiceHelper.StopServiceAndWait(serv);
                        toRestart = true;
                    }

                }
                XmlDocument doc = new XmlDocument();
                doc.Load(PathHelper.GetSavePath(serv, model.CurrentMapName) + @"\Sandbox.sbc");
                XmlNode root = doc.DocumentElement;
                XmlNode nameNode = root.SelectSingleNode("descendant::SessionName");
                nameNode.InnerText = model.NewMapName;
                doc.Save(PathHelper.GetSavePath(serv, model.CurrentMapName) + @"\Sandbox.sbc");

                Directory.Move(PathHelper.GetSavePath(serv, model.CurrentMapName), PathHelper.GetSavePath(serv, model.NewMapName));
                if (model.CurrentMapName == serverConfig.SaveName)
                {
                    serverConfig.SaveName = model.NewMapName;
                    serverConfig.LoadFromSave(PathHelper.GetSavePath(serv, serverConfig.SaveName));
                    serverConfig.Save(serv);
                }

                if (toRestart)
                {
                    serviceLogger.Info(serv.Name + " started by " + user.Login + " for map renaming");
                    ServiceHelper.StartService(serv);
                }
                return RedirectToAction("Index", new { id = id }).Success("Map Renamed");
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
                return RedirectToAction("Index", new { id = id }).Success("Map deleted");
            }
            // TODO : better message
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
                Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                serviceLogger.Info(serv.Name + " stopped by " + user.Login + " for new map");
                ServiceHelper.StopServiceAndWait(serv);
                ServerConfigHelper serverConfig = new ServerConfigHelper();
                serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));
                serverConfig.ScenarioType = model.MapType;
                serverConfig.AsteroidAmount = model.AsteroidAmount;
                serverConfig.SaveName = string.Empty;
                serv.AutoSaveInMinutes = serverConfig.AutoSaveInMinutes;
                srvPrv.UpdateServer(serv);
                if(serv.UseServerExtender)
                {
                    serverConfig.AutoSaveInMinutes = 0;
                }
                serverConfig.Save(serv);
                serviceLogger.Info(serv.Name + " stopped by " + user.Login + " for new map");
                ServiceHelper.StartService(serv);
                return RedirectToAction("Status","Server",  new { id = id }).Success("Server Starting, map in creation ...");
            }

            return View(model);
        }

        //
        // GET: Map/Upload/5
        [HttpGet]
        public ActionResult Upload(int id)
        {
            ViewData["ID"] = id;
            return View();
        }

        //
        // POST: Map/Upload/5
        [HttpPost]
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
            return RedirectToAction("Index", new { id = id }).Success("Map Upload Successful");
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