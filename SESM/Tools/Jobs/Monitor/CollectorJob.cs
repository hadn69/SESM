using System;
using System.Collections.Generic;
using NLog;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools.Jobs.Monitor
{
    public class CollectorJob : IJob
    {
        public void Execute(IJobExecutionContext jobContext)
        {
            try
            {
                DateTime timestamp = DateTime.Now;
                DataContext context = new DataContext();
                ServerProvider srvPrv = new ServerProvider(context);
                List<EntityServer> listServer = srvPrv.GetAllServers();
                foreach (EntityServer item in listServer)
                {
                    ServiceHelper.Ressources? ressources = ServiceHelper.GetCurrentRessourceUsage(ServiceHelper.GetServiceName(item));
                    if (ressources != null)
                    {
                        EntityPerfEntry perfEntry = new EntityPerfEntry();
                        perfEntry.Timestamp = timestamp;
                        perfEntry.CPUUsage = ressources.Value.CPU;
                        perfEntry.RamUsage = ressources.Value.Memory;

                        item.PerfEntries.Add(perfEntry);
                        srvPrv.UpdateServer(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger exceptionLogger = LogManager.GetLogger("GenericExceptionLogger");
                exceptionLogger.Fatal("Caught Exception in Collector Job", ex);
            }
        }
    }
}