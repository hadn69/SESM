using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using Ionic.Zip;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Map;
using SESM.Models.Views.Mod;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    [LoggedOnly]
    [CheckAuth]
    [CheckLockout]
    public class ModController : Controller
    {
        private readonly DataContext _context = new DataContext();

        //
        // GET: Mod/5
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

            /*string[] listDir = Directory.GetDirectories(PathHelper.GetModsPath(serv));

            List<SelectListItem> listSLI = new List<SelectListItem>();

            foreach (string item in listDir)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = PathHelper.GetLastLeaf(item);
                sli.Value = PathHelper.GetLastLeaf(item);
                listSLI.Add(sli);
            }

            ViewData["listDir"] = listSLI;

            ModViewModelOld model = new ModViewModelOld();
            */

            ModViewModel model = new ModViewModel();
            ServerConfigHelper serverConfig = new ServerConfigHelper();
            serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));

            foreach (ulong mod in serverConfig.Mods)
            {
                model.ModName.Add(mod.ToString());
            }

            return View(model);
        }

        // GET: Mod/GetWorkshopInfo/12345678
        [HttpGet]
        public ActionResult GetWorkshopInfo(int id)
        {
            string item = Request.QueryString["item"];
            string data = new System.Net.WebClient().DownloadString("http://steamcommunity.com/sharedfiles/filedetails/?id=" + item);

            Match matchTitle = Regex.Match(data, @"<div class=""workshopItemTitle"">(.*)</div>", RegexOptions.IgnoreCase);
            Match matchURL = Regex.Match(data, @"(http:.*)' \);""><img id=""previewImageMain""", RegexOptions.IgnoreCase);

            XElement response = new XElement("WorkshopInfo",
                                    new XAttribute("ModName", matchTitle.Groups[1]),
                                    new XAttribute("ThumbnailURL", matchURL.Groups[1]));

            return Content(response.ToString());
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
        // POST: Mod/Upload/5
        [HttpPost]
        [AdminAndAbove]
        public ActionResult Upload(int id, UploadModViewModel model)
        {
            if (!ZipFile.IsZipFile(model.ModZip.InputStream, false))
            {
                ModelState.AddModelError("ZipError", "Your File is not a valid zip file");
                return View(model);
            }
            model.ModZip.InputStream.Seek(0, SeekOrigin.Begin);
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            string path = PathHelper.GetModsPath(serv);

            using (ZipFile zip = ZipFile.Read(model.ModZip.InputStream))
            {
                Directory.CreateDirectory(path);
                zip.ExtractAll(path);
            }
            return RedirectToAction("Index", new {id = id}).Success("Mod Upload Successfull");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteMod")]
        public ActionResult Delete(int id, MapViewModel model)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid && Directory.Exists(PathHelper.GetModsPath(serv, model.MapName) + @"\"))
            {
                Directory.Delete(PathHelper.GetModsPath(serv, model.MapName) + @"\", true);
                return RedirectToAction("Index", new { id = id }).Success("Mod Deleted");
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