using System.Web.Mvc;
using SESM.Controllers.ActionFilters;
using SESM.DAL;

namespace SESM.Controllers
{
    [CheckLockout]
    public class ServerController : Controller
    {
        readonly DataContext _context = new DataContext();

        // GET: Servers
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        // GET: 1/Dashboard
        [HttpGet]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Dashboard(int id)
        {
            return View();
        }

        // GET: 1/Maps
        [HttpGet]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Maps(int id)
        {
            return View();
        }

        // GET: 1/Configuration
        [HttpGet]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Configuration(int id)
        {
            return View();
        }

        // GET: 1/Explorer
        [HttpGet]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Explorer(int id)
        { 
            return View();
        }

        // GET: 1/Settings
        [HttpGet]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Settings(int id)
        {
            return View();
        }

        // GET: 1/Monitor
        [HttpGet]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult Monitor(int id)
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
