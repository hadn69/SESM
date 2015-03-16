using System;
using NLog;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class AutoRestartJob : IJob
    {
        public void Execute(IJobExecutionContext jobContext)
        {
            try
            {
                if (!SESMConfigHelper.Lockdown)
                {
                    JobDataMap dataMap = jobContext.JobDetail.JobDataMap;
                    int serverId = dataMap.GetInt("id");

                    DataContext context = new DataContext();
                    ServerProvider srvPrv = new ServerProvider(context);

                    EntityServer server = srvPrv.GetServer(serverId);
                    Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
                    serviceLogger.Info(server.Name + " restarted by autorestart");
                    ServiceHelper.RestartService(server);
                }
            }
            catch (Exception ex)
            {
                Logger exceptionLogger = LogManager.GetLogger("GenericExceptionLogger");
                exceptionLogger.Fatal("Caught Exception in AutoRestart Job", ex);
            }
        }

        public static JobKey GetJobKey(EntityServer server)
        {
            return new JobKey("AutoRestart" + server.Id, "AutoRestart");
        }

        public static TriggerKey GetTriggerKey(EntityServer server)
        {
            return new TriggerKey("AutoRestart" + server.Id, "AutoRestart");
        }
    }
}