using System;
using System.Collections.Generic;
using System.Threading;
using NLog;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools.Jobs
{
    [DisallowConcurrentExecution]
    public class SESEAutoUpdateJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Logger logger = LogManager.GetLogger("SESEAutoUpdateLogger");
            Run(logger, false, false);
        }

        public static ReturnEnum Run(Logger logger, bool manualFire = true, bool useLocalZip = false, bool force = false)
        {
            if(SESMConfigHelper.SESEUpdating)
                return ReturnEnum.UpdateAlreadyRunning;

            try
            {
                logger.Info("--- Starting SESE Auto-Update Process ---");
                logger.Info("Type : " + (manualFire ? "Manuel" : "Automatic"));
                logger.Info("Using Local zip : " + useLocalZip.ToString().ToUpper());
                logger.Info("Checking Versions ...");

                Version localVersion = SESEHelper.GetLocalVersion();
                logger.Info(" - Local Version : " + localVersion.ToString());

                string githubData = SESEHelper.GetGithubData();
                Version remoteVersion = SESEHelper.GetLastRemoteVersion(githubData, SESMConfigHelper.SESEAutoUpdateUseDev);
                logger.Info(" - Remote Version : " + remoteVersion.ToString());

                logger.Info("Checking SESM State ...");
                if(SESMConfigHelper.Lockdown)
                {
                    logger.Info("SESM is already updating, skiping this round, exiting ...");
                    return ReturnEnum.UpdateAlreadyRunning;
                }
                logger.Info("SESM is not currently Updating, current update can process");
                logger.Info("Initiating Lockdown mode, Starting Update Process ...");
                SESMConfigHelper.Lockdown = true;
                SESMConfigHelper.SESEUpdating = true;

                logger.Info("Waiting 30 sec for all request to end");
                Thread.Sleep(30000);

                if(useLocalZip)
                {
                    logger.Info("Using Already Avaialble Update");
                }
                else
                {
                    // Test for update
                    if(!force && localVersion.CompareTo(remoteVersion) >= 0)
                    {
                        logger.Info("No Update Available, Exiting ...");
                        return ReturnEnum.NothingToDo;
                    }
                    logger.Info("Update Available, cleaning up and downloading ...");
                    SESEHelper.CleanupUpdate();
                    string url = SESEHelper.GetLastRemoteURL(githubData, SESMConfigHelper.SESEAutoUpdateUseDev);
                    SESEHelper.DownloadUpdate(url);
                }

                logger.Info("Stopping all SESE Server : ");
                DataContext _context = new DataContext();
                ServerProvider srvPrv = new ServerProvider(_context);
                List<EntityServer> SESEServer = srvPrv.GetAllSESEServers();
                List<EntityServer> SESERunningServer = new List<EntityServer>();

                foreach(EntityServer server in SESEServer)
                {
                    logger.Info("========");
                    logger.Info("Server : " + server.Name);
                    ServiceState serverState = srvPrv.GetState(server);
                    logger.Info("        Status : " + serverState);
                    if(serverState == ServiceState.Stopped || serverState == ServiceState.Unknow)
                    {
                        logger.Info("        Server Already Stopped");
                        continue;
                    }

                    logger.Info("        Stopping server ...");
                    ServiceHelper.StopService(server);

                    SESERunningServer.Add(server);
                }
                logger.Info("Waiting for server stop (30 secs/serv max)");
                foreach(EntityServer server in SESERunningServer)
                {
                    logger.Info("========");
                    logger.Info("Server : " + server.Name);
                    logger.Info("Waiting for stopped ...");
                    ServiceHelper.WaitForStopped(server);
                }
                logger.Info("Waiting 10 secs for grace periode ...");
                Thread.Sleep(10000);
                logger.Info("Killing any remaining SESE process");
                ServiceHelper.KillAllSESEService();
                logger.Info("Waiting 10 secs for kills to finish ...");
                Thread.Sleep(10000);
                logger.Info("Extracting SESE ...");
                SESEHelper.ApplyUpdate(logger);
                logger.Info("Restarting all previously started server :");

                foreach(EntityServer server in SESERunningServer)
                {
                    logger.Info("========");
                    logger.Info("Server : " + server.Name);
                    logger.Info("        Starting server ...");
                    ServiceHelper.StartService(server);
                }

                return ReturnEnum.Success;
            }
            catch(Exception ex)
            {
                logger.Fatal("Exception : ", ex);
                return ReturnEnum.Exception;
            }
            finally
            {
                if(SESMConfigHelper.Lockdown)
                {
                    logger.Info("Lifting Lockdown");
                    SESMConfigHelper.Lockdown = false;
                    SESMConfigHelper.SESEUpdating = false;
                }
                logger.Info("--- End of SESE Auto-Update Process ---");
            }
        }

        public static JobKey GetJobKey()
        {
            return new JobKey("SESEAutoUpdate", "SESEAutoUpdate");
        }

        public static TriggerKey GetTriggerKey()
        {
            return new TriggerKey("SESEAutoUpdate", "SESEAutoUpdate");
        }
    }
}