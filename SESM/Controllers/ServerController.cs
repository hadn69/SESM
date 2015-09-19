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
        [ServerAccess("SERVER_INFO")]
        public ActionResult Dashboard(int id)
        {
            return View();
        }

        // GET: 1/Maps
        [HttpGet]
        [ServerAccess("SERVER_MAP_SE_LIST", "SERVER_MAP_ME_LIST")]
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
        [ServerAccess("SERVER_CONFIG_SE_RD", "SERVER_CONFIG_ME_RD")]
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
        [ServerAccess("SERVER_EXPLORER_LIST")]
        public ActionResult Explorer(int id)
        { 
            return View();
        }

        // GET: 1/Settings
        [HttpGet]
        [ServerAccess("SERVER_SETTINGS_GLOBAL_RD", "SERVER_SETTINGS_JOBS_RD", "SERVER_SETTINGS_BACKUPS_RD")]
        public ActionResult Settings(int id)
        {
            return View();
        }

        // GET: 1/Monitor
        [HttpGet]
        [ServerAccess("SERVER_PERF_READ")]
        public ActionResult Monitor(int id)
        {
            return View();
        }

        // GET: 1/Access
        [HttpGet]
        [ServerAccess("ACCESS_SERVER_READ")]
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
