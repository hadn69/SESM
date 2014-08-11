using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using SESM.DAL;
using SESM.DTO;

namespace SESM.Tools.Monitor
{
    public class HourlyCrusher :IJob
    {
        public void Execute(IJobExecutionContext jobContext)
        {
            DateTime timestamp = DateTime.Now;
            timestamp = timestamp.AddHours(-2);
            DataContext context = new DataContext();
            ServerProvider srvPrv = new ServerProvider(context);
            PerfProvider prfPrv = new PerfProvider(context);
            List<EntityServer> listServer = srvPrv.GetAllServers();
            foreach (EntityServer item in listServer)
            {
                List<EntityPerfEntry> perfEntries =
                    item.PerfEntries.Where(x => x.Timestamp <= timestamp && x.CPUUsagePeak != null)
                        .OrderBy(x => x.Timestamp)
                        .ToList();

                EntityPerfEntry perfEntry = new EntityPerfEntry();
                perfEntry.Timestamp = perfEntries[0].Timestamp;

                // CPU

                perfEntry.CPUUsage = (int)Math.Round(perfEntries.Average(x => x.CPUUsage));
                perfEntry.CPUUsagePeak = perfEntries.Max(x => x.CPUUsage);
                perfEntry.CPUUsageTrough = perfEntries.Min(x => x.CPUUsage);

                perfEntries = perfEntries.OrderBy(x => x.CPUUsage).ToList();

                perfEntry.CPUUsageQ1 = perfEntries[(int) Math.Ceiling(perfEntries.Count/4.0)].CPUUsage;
                perfEntry.CPUUsageQ3 = perfEntries[(int) Math.Ceiling(perfEntries.Count/4.0*3)].CPUUsage;

                // RAM

                perfEntry.RamUsage = (int)Math.Round(perfEntries.Average(x => x.RamUsage));
                perfEntry.RamUsagePeak = perfEntries.Max(x => x.RamUsage);
                perfEntry.RamUsageTrough = perfEntries.Min(x => x.RamUsage);

                perfEntries = perfEntries.OrderBy(x => x.RamUsage).ToList();

                perfEntry.RamUsageQ1 = perfEntries[(int) Math.Ceiling(perfEntries.Count/4.0)].RamUsage;
                perfEntry.RamUsageQ3 = perfEntries[(int) Math.Ceiling(perfEntries.Count/4.0*3)].RamUsage;


                prfPrv.RemoveEntries(perfEntries);
                item.PerfEntries.Add(perfEntry);
                srvPrv.UpdateServer(item);

            }
        }
    }
}