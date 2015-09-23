using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;

namespace SESM.Controllers.API
{
    public class APIPerfMonController : Controller, IAPIController
    {
        public DataContext CurrentContext { get; set; }

        public EntityServer RequestServer { get; set; }

        public APIPerfMonController()
        {
            CurrentContext = new DataContext();
        }

        // POST: API/PerfMon/GetPerfData
        [HttpPost]
        [APIServerAccess("PM-GPD", "SERVER_PERF_READ")]
        public ActionResult GetPerfData()
        {
            // ** PARSING / ACCESS **
            XMLMessage response = new XMLMessage("PM-GPD-OK");

            XElement recent = new XElement("Recent");

            foreach (EntityPerfEntry item in RequestServer.PerfEntries.Where(x => x.CPUUsagePeak == null))
            {
                recent.Add(new XElement("PerfEntry", new XElement("Date", item.Timestamp.ToString("o")),
                                                     new XElement("CPU", item.CPUUsage),
                                                     new XElement("RAM", item.RamUsage)
                                                     ));
            }

            XElement compiled = new XElement("Compiled");

            foreach (EntityPerfEntry item in RequestServer.PerfEntries.Where(x => x.CPUUsagePeak != null))
            {
                compiled.Add(new XElement("PerfEntry", new XElement("Date", item.Timestamp.ToString("o")),
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

            response.AddToContent(recent);
            response.AddToContent(compiled);

            return Content(response.ToString());
        }

        // GET: API/PerfMon/CleanPerfData
        [HttpGet]
        [APIHostAccess("PM-CPD", "PERF_CLEANUP")]
        public ActionResult CleanPerfData()
        {
            CurrentContext.Database.ExecuteSqlCommand("truncate table dbo.EntityPerfEntries");
            return Content(XMLMessage.Success("PM-CPD-OK", "The performance datas have been cleared").ToString());
        }
    }
}