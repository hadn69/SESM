using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Ionic.Zip;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
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
            ViewData["AccessLevel"] = srvPrv.GetAccessLevel(user.Id, serv.Id);

            string[] listDir = Directory.GetDirectories(PathHelper.GetModsPath(serv));

            List<SelectListItem> listSLI = new List<SelectListItem>();

            foreach (string item in listDir)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = PathHelper.GetLastLeaf(item);
                sli.Value = PathHelper.GetLastLeaf(item);
                listSLI.Add(sli);
            }

            string[] listfile = Directory.GetFiles(PathHelper.GetModsPath(serv));

            foreach(string item in listfile)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = PathHelper.GetLastLeaf(item);
                sli.Value = PathHelper.GetLastLeaf(item);
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
        // POST: Mod/Upload/5
        [HttpPost]
        [AdminAndAbove]
        public ActionResult Upload(int id, UploadModViewModel model)
        {
            EntityUser user = Session["User"] as EntityUser;
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
                if(!user.IsAdmin && SESMConfigHelper.BlockDll)
                    zip.RemoveEntries(zip.SelectEntries("*.dll"));
                zip.ExtractAll(path);
            }
            return RedirectToAction("Index", new {id = id}).Success("Mod Upload Successfull");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteMod")]
        public ActionResult Delete(int id, ModViewModel model)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);

            if (ModelState.IsValid && Directory.Exists(PathHelper.GetModsPath(serv, model.ModName) + @"\"))
            {
                Directory.Delete(PathHelper.GetModsPath(serv, model.ModName) + @"\", true);
                return RedirectToAction("Index", new { id = id }).Success("Mod Deleted");
            }

            if(ModelState.IsValid && System.IO.File.Exists(PathHelper.GetModsPath(serv, model.ModName)))
            {
                System.IO.File.Delete(PathHelper.GetModsPath(serv, model.ModName));
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