using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NLog;
using Quartz;
using Quartz.Impl;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Server;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    [CheckLockout]
    public class ServerController : Controller
    {
        readonly DataContext _context = new DataContext();

        // GET: Server
        [HttpGet]
        public ActionResult Index()
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

        // GET: 1/Maps
        [HttpGet]
        [CheckAuth]
        public ActionResult Maps(int id)
        {
            return View();
        }




        // GET: Server/HourlyStats/5
        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult HourlyStats(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);
            ViewData["ID"] = id;
            List<EntityPerfEntry> perfEntries = serv.PerfEntries.Where(x => x.Timestamp >= DateTime.Now.AddHours(-2)).OrderBy(x => x.Timestamp).ToList();
            ViewData["perfEntries"] = perfEntries;
            return View();
        }

        [HttpGet]
        [LoggedOnly]
        [CheckAuth]
        [ManagerAndAbove]
        public ActionResult GlobalStats(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer serv = srvPrv.GetServer(id);
            ViewData["ID"] = id;
            List<EntityPerfEntry> perfEntries = serv.PerfEntries.Where(x => x.CPUUsagePeak != null).OrderBy(x => x.Timestamp).ToList();
            ViewData["perfEntries"] = perfEntries;
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
