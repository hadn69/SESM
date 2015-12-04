using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class AutoStartJob : IJob
    {
        public void Execute(IJobExecutionContext jobContext)
        {
            try
            {
                if (!SESMConfigHelper.Lockdown)
                {
                    DataContext context = new DataContext();
                    ServerProvider srvPrv = new ServerProvider(context);

                    List<EntityServer> listServ = srvPrv.GetAllServers()
                        .Where(x => x.IsAutoStartEnabled && srvPrv.GetState(x) != ServiceState.Running)
                        .ToList();
                    Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                    foreach (EntityServer server in listServ)
                    {
                        serviceLogger.Info(server.Name + " started by autostart");
                        ServiceHelper.StartService(server);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger exceptionLogger = LogManager.GetLogger("GenericExceptionLogger");
                exceptionLogger.Fatal("Caught Exception in AutoStart Job", ex);
            }
        }

        public static JobKey GetJobKey()
        {
            return new JobKey("AutoStart", "AutoStart");
        }

        public static TriggerKey GetTriggerKey()
        {
            return new TriggerKey("AutoStart", "AutoStart");
        }
    }
}