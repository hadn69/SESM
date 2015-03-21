using System.Web.Mvc;
using SESM.Controllers.ActionFilters;
using SESM.DAL;

namespace SESM.Controllers
{

    public class SettingsController : Controller
    {
        private readonly DataContext _context = new DataContext();

        [HttpGet]
        [SuperAdmin]
        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        [SuperAdmin]
        public ActionResult SE()
        {

            return View();
        }

        [HttpGet]
        [SuperAdmin]
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
