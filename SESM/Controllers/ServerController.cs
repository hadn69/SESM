using System.Web.Mvc;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;

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
        public ActionResult Dashboard(int id)
        {
            return View();
        }

        // GET: 1/Maps
        [HttpGet]
        [CheckAuth]
        public ActionResult Maps(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityServer serv = srvPrv.GetServer(id);

            if(serv.ServerType == EnumServerType.SpaceEngineers)
                return View("SEMaps");
            else
                return View("MEMaps");
        }

        // GET: 1/Configuration
        [HttpGet]
        [CheckAuth]
        public ActionResult Configuration(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityServer serv = srvPrv.GetServer(id);

            if (serv.ServerType == EnumServerType.SpaceEngineers)
                return View("SEConfiguration");
            else
                return View("MEConfiguration");
        }

        // GET: 1/Explorer
        [HttpGet]
        [CheckAuth]
        public ActionResult Explorer(int id)
        { 
            return View();
        }

        // GET: 1/Settings
        [HttpGet]
        [CheckAuth]
        public ActionResult Settings(int id)
        {
            return View();
        }

        // GET: 1/Monitor
        [HttpGet]
        [CheckAuth]
        public ActionResult Monitor(int id)
        {
            return View();
        }

        // GET: 1/Access
        [HttpGet]
        [CheckAuth]
        public ActionResult Access(int id)
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
