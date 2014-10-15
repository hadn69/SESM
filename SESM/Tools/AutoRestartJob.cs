﻿using NLog;
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
    }
}