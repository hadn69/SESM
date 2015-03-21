using System;
using System.Collections.Generic;
using System.Linq;
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
            return View();
        }

        // GET: 1/Configuration
        [HttpGet]
        [CheckAuth]
        public ActionResult Configuration(int id)
        {
            return View();
        }

        [HttpGet]
        [CheckAuth]
        // GET: 1/Explorer
        public ActionResult Explorer(int id)
        { 
            return View();
        }

        [HttpGet]
        [CheckAuth]
        // GET: 1/Settings
        public ActionResult Settings(int id)
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
