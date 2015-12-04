using System.Web.Mvc;
using SESM.Controllers.ActionFilters;
using SESM.DAL;

namespace SESM.Controllers
{
    [CheckLockdown]
    public class SettingsController : Controller
    {
        private readonly DataContext _context = new DataContext();

        [HttpGet]
        [HostAccess("SETTINGS_SESM")]
        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        [HostAccess("SETTINGS_SE")]
        public ActionResult SE()
        {

            return View();
        }

        [HttpGet]
        [HostAccess("SETTINGS_ME")]
        public ActionResult ME()
        {

            return View();
        }

        [HttpGet]
        [HostAccess("SETTINGS_SESE")]
        public ActionResult SESE()
        {

            return View();
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
