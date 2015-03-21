using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;

namespace SESM.Controllers.API
{
    public class APIPerfMonController : Controller
    {
        private readonly DataContext _context = new DataContext();

        // POST: API/PerfMon/GetPerfData
        [HttpPost]
        public ActionResult GetPerfData()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("PM-GPD-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("PM-GPD-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("PM-GPD-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("PM-GPD-NOACCESS", "You don't have access to this server").ToString());

            XMLMessage response = new XMLMessage("PM-GPD-OK");

            XElement recent = new XElement("Recent");

            foreach (EntityPerfEntry item in server.PerfEntries.Where(x => x.CPUUsagePeak == null))
            {
                recent.Add(new XElement("PerfEntry", new XElement("Date", item.Timestamp.ToString()),
                                                     new XElement("CPU", item.CPUUsage),
                                                     new XElement("RAM", item.RamUsage)
                                                     ));
            }

            XElement compiled = new XElement("Compiled");

            foreach (EntityPerfEntry item in server.PerfEntries.Where(x => x.CPUUsagePeak != null))
            {
                recent.Add(new XElement("PerfEntry", new XElement("Date", item.Timestamp.ToString()),
                                                     new XElement("CPUAvg", item.CPUUsage),
                                                     new XElement("CPUPeak", item.CPUUsagePeak),
                                                     new XElement("CPUTrough", item.CPUUsageTrough),
                                                     new XElement("CPUQ1", item.CPUUsageQ1),
                                                     new XElement("CPUQ3", item.CPUUsageQ3),
                                                     new XElement("RAMAvg", item.RamUsage),
                                                     new XElement("RAMPeak", item.RamUsagePeak),
                                                     new XElement("RAMTrough", item.RamUsageTrough),
                                                     new XElement("RAMQ1", item.RamUsageQ1),
                                                     new XElement("RAMQ3", item.RamUsageQ3)
                                                     ));
            }

            return Content(response.ToString());
        }

        // POST: API/PerfMon/CleanPerfData
        [HttpGet]
        public ActionResult CleanPerfData()
        {
            _context.Database.ExecuteSqlCommand("truncate table SESM.dbo.EntityPerfEntries");
            return Content(XMLMessage.Success("PM-CPD-OK", "The performance datas have been cleared").ToString());
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