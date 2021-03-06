﻿using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Quartz;
using SESM.DAL;
using SESM.DTO;

namespace SESM.Tools.Jobs.Monitor
{
    public class HourlyCrusherJob : IJob
    {
        private static Logger _logger = LogManager.GetLogger("HourlyCrusherLogger");
        public void Execute(IJobExecutionContext jobContext)
        {
            try
            {
                _logger.Info("----Starting HourlyCrusherJob----");
                DateTime timestamp = DateTime.Now;
                timestamp = timestamp.AddHours(-1);
                _logger.Info("Getting untreated perf entries prior to " + timestamp.ToString("G"));
                DataContext context = new DataContext();
                ServerProvider srvPrv = new ServerProvider(context);
                PerfProvider prfPrv = new PerfProvider(context);
                List<EntityServer> listServer = srvPrv.GetAllServers();
                foreach (EntityServer item in listServer)
                {
                    _logger.Info("Getting perf entries of " + item.Name);

                    _logger.Info(item.PerfEntries.Count + " total perf entries");

                    List<EntityPerfEntry> perfEntries =
                        item.PerfEntries.Where(x => x.Timestamp <= timestamp && x.CPUUsagePeak == null)
                            .OrderBy(x => x.Timestamp)
                            .ToList();

                    _logger.Info(perfEntries.Count + " to process");
                    if (perfEntries.Count == 0)
                    {
                        _logger.Info("No entry to process, skipping to next server");
                        continue;
                    }

                    _logger.Debug("Summary Results : ");
                    EntityPerfEntry perfEntry = new EntityPerfEntry();
                    perfEntry.Timestamp = perfEntries[0].Timestamp;
                    _logger.Debug("TimeStamp : " + perfEntry.Timestamp.ToString("G"));

                    // CPU
                    _logger.Debug("==CPU==");
                    perfEntry.CPUUsage = (int)Math.Round(perfEntries.Average(x => x.CPUUsage));
                    perfEntry.CPUUsagePeak = perfEntries.Max(x => x.CPUUsage);
                    perfEntry.CPUUsageTrough = perfEntries.Min(x => x.CPUUsage);
                    _logger.Debug("  CPUUsage : " + perfEntry.CPUUsage);
                    _logger.Debug("  CPUUsagePeak : " + perfEntry.CPUUsagePeak);
                    _logger.Debug("  CPUUsageTrough : " + perfEntry.CPUUsageTrough);

                    perfEntries = perfEntries.OrderBy(x => x.CPUUsage).ToList();

                    perfEntry.CPUUsageQ1 = perfEntries[(int)Math.Ceiling(perfEntries.Count / 4.0)].CPUUsage;
                    perfEntry.CPUUsageQ3 = perfEntries[(int)Math.Ceiling(perfEntries.Count / 4.0 * 3)].CPUUsage;
                    _logger.Debug("  CPUUsageQ1 : " + perfEntry.CPUUsageQ1);
                    _logger.Debug("  CPUUsageQ3 : " + perfEntry.CPUUsageQ3);

                    // RAM
                    _logger.Debug("==RAM==");
                    perfEntry.RamUsage = (int)Math.Round(perfEntries.Average(x => x.RamUsage));
                    perfEntry.RamUsagePeak = perfEntries.Max(x => x.RamUsage);
                    perfEntry.RamUsageTrough = perfEntries.Min(x => x.RamUsage);
                    _logger.Debug("  RamUsage : " + perfEntry.RamUsage);
                    _logger.Debug("  RamUsagePeak : " + perfEntry.RamUsagePeak);
                    _logger.Debug("  RamUsageTrough : " + perfEntry.RamUsageTrough);

                    perfEntries = perfEntries.OrderBy(x => x.RamUsage).ToList();

                    perfEntry.RamUsageQ1 = perfEntries[(int)Math.Ceiling(perfEntries.Count / 4.0)].RamUsage;
                    perfEntry.RamUsageQ3 = perfEntries[(int)Math.Ceiling(perfEntries.Count / 4.0 * 3)].RamUsage;
                    _logger.Debug("  RamUsageQ1 : " + perfEntry.RamUsageQ1);
                    _logger.Debug("  RamUsageQ3 : " + perfEntry.RamUsageQ3);

                    _logger.Info("Deleting the perf entries");
                    prfPrv.RemoveEntries(perfEntries);

                    prfPrv.RemoveEntries(item.PerfEntries.Where(x => x.Timestamp <= timestamp.AddDays(-14))
                        .ToList());

                    _logger.Info("Adding the summary perf entry");
                    item.PerfEntries.Add(perfEntry);
                    srvPrv.UpdateServer(item);

                }
                _logger.Info("----End of HourlyCrusherJob----");
            }
            catch (Exception ex)
            {
                Logger exceptionLogger = LogManager.GetLogger("GenericExceptionLogger");
                exceptionLogger.Fatal("Caught Exception in Hourly Crusher Job :", ex);
            }
        }
    }
}