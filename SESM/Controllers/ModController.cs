using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
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
            ViewData["AccessLevel"] = srvPrv.GetAccessLevel(user.Id, serv.Id);

            string[] listDir = Directory.GetDirectories(PathHelper.GetModsPath(serv));

            List<SelectListItem> listSLI = new List<SelectListItem>();

            foreach (string item in listDir)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = PathHelper.GetLastDirName(item);
                sli.Value = PathHelper.GetLastDirName(item);
                listSLI.Add(sli);
            }

            ViewData["listDir"] = listSLI;

            ModViewModel model = new ModViewModel();


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
            return RedirectToAction("Index", new {id = id});
        }

        //
        // POST: Map/Save/5
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteMod")]
        public ActionResult Delete(int id, MapViewModel model)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid && Directory.Exists(PathHelper.GetModsPath(serv, model.MapName) + @"\"))
            {
                Directory.Delete(PathHelper.GetModsPath(serv, model.MapName) + @"\", true);
            }

            return RedirectToAction("Index", new { id = id });
        }
    }
}