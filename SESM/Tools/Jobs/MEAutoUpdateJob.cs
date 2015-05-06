using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Ionic.Zip;
using NLog;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools.Jobs
{
    [DisallowConcurrentExecution]
    public class MEAutoUpdateJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Logger logger = LogManager.GetLogger("SEAutoUpdateLogger");
            Run(logger, false, false, false);
        }

        public static ReturnEnum Run(Logger logger, bool manualFire = true, bool useLocalZip = false, bool force = false)
        {
            if (!manualFire && !SESMConfigHelper.MEAutoUpdateEnabled)
                return ReturnEnum.AloneJob;

            if (SESMConfigHelper.MEUpdating)
                return ReturnEnum.UpdateAlreadyRunning;

            logger.Info("--- Starting ME Auto-Update Process ---");
            logger.Info("Type : " + (manualFire ? "Manuel" : "Automatic"));
            logger.Info("Using Local zip : " + useLocalZip.ToString().ToUpper());
            logger.Info("Force : " + force.ToString().ToUpper());
            logger.Info("Checking Versions ...");

            int? localVersion = null;
            int? remoteVersion = null;
            for (int i = 0; i < 5; i++)
            {
                if (localVersion == null)
                {
                    logger.Info("Retrieving local version : ");
                    localVersion = SteamCMDHelper.GetMEInstalledVersion(logger);
                }

                if (remoteVersion == null)
                {
                    logger.Info("Retrieving remote version : ");
                    remoteVersion =
                        SteamCMDHelper.GetMEAvailableVersion(
                            !string.IsNullOrWhiteSpace(SESMConfigHelper.MEAutoUpdateBetaPassword), logger);
                }

                if (localVersion == null || remoteVersion == null)
                {
                    logger.Info("Fail retrieving one of the version (try " + (i + 1) + " of 5), waiting and retrying ...");
                    Thread.Sleep(2000);
                }
                else
                    break;

            }
            if (localVersion == null || remoteVersion == null)
            {
                logger.Info("Fail retrieving one of the version (too much try), steam CMD problem");
                return ReturnEnum.Error;
            }

            logger.Info(" - Local Version : " + localVersion);
            logger.Info(" - Remote Version : " + remoteVersion);

            // Test for update
            if (!useLocalZip && !force && localVersion >= remoteVersion)
            {
                logger.Info("No Update Available, Exiting ...");
                return ReturnEnum.NothingToDo;
            }

            logger.Info("Checking SESM State ...");
            if (SESMConfigHelper.Lockdown)
            {
                logger.Info("SESM is already updating, skiping this round, exiting ...");
                return ReturnEnum.UpdateAlreadyRunning;
            }
            try
            {
                logger.Info("SESM is not currently Updating, current update can process");
                logger.Info("Initiating Lockdown mode, Starting Update Process ...");
                SESMConfigHelper.Lockdown = true;
                SESMConfigHelper.MEUpdating = true;

                logger.Info("Update Available, downloading ...");

                logger.Info("Waiting 30 sec for all request to end");
                Thread.Sleep(30000);

                logger.Info("Stopping all ME Server : ");
                DataContext _context = new DataContext();
                ServerProvider srvPrv = new ServerProvider(_context);
                List<EntityServer> MEServer = srvPrv.GetAllMEServers();
                List<EntityServer> MERunningServer = new List<EntityServer>();

                foreach (EntityServer server in MEServer)
                {
                    logger.Info("========");
                    logger.Info("Server : " + server.Name);
                    ServiceState serverState = srvPrv.GetState(server);
                    logger.Info("        Status : " + serverState);
                    if (serverState == ServiceState.Stopped || serverState == ServiceState.Unknow)
                    {
                        logger.Info("        Server Already Stopped");
                        continue;
                    }

                    logger.Info("        Stopping server ...");
                    ServiceHelper.StopService(server);

                    MERunningServer.Add(server);
                }
                logger.Info("Waiting for server stop (30 secs/serv max)");
                foreach (EntityServer server in MERunningServer)
                {
                    logger.Info("========");
                    logger.Info("Server : " + server.Name);
                    logger.Info("Waiting for stopped ...");
                    ServiceHelper.WaitForStopped(server);
                }

                logger.Info("Waiting 10 secs for grace periode ...");
                Thread.Sleep(10000);
                logger.Info("Killing any remaining ME process");
                ServiceHelper.KillAllMEServices();
                logger.Info("Waiting 10 secs for kills to finish ...");
                Thread.Sleep(10000);

                logger.Info("Cleaning SE Game Files ...");
                if (Directory.Exists(SESMConfigHelper.MEDataPath + @"Content\"))
                {
                    logger.Info("Deleting Content ...");
                    Directory.Delete(SESMConfigHelper.MEDataPath + @"Content\", true);
                }
                if (Directory.Exists(SESMConfigHelper.MEDataPath + @"DedicatedServer\"))
                {
                    logger.Info("Deleting DedicatedServer ...");
                    Directory.Delete(SESMConfigHelper.MEDataPath + @"DedicatedServer\", true);
                }
                if (Directory.Exists(SESMConfigHelper.MEDataPath + @"DedicatedServer64\"))
                {
                    logger.Info("Deleting DedicatedServer64 ...");
                    Directory.Delete(SESMConfigHelper.MEDataPath + @"DedicatedServer64\", true);
                }

                if (useLocalZip)
                {
                    logger.Info("Extracting ME Game Files ...");
                    using (ZipFile zip = ZipFile.Read(SESMConfigHelper.MEDataPath + "DedicatedServer.zip"))
                    {
                        zip.ExtractAll(SESMConfigHelper.MEDataPath);
                    }
                }
                else
                {
                    logger.Info("Updating ME Game Files ...");
                    SteamCMDHelper.UpdateME(logger, !string.IsNullOrWhiteSpace(SESMConfigHelper.MEAutoUpdateBetaPassword));

                    logger.Info("Applying ME Game Files ...");
                    logger.Info("Applying Content ...");
                    FSHelper.DirectoryCopy(PathHelper.GetMESyncDirPath() + @"Content\",
                        SESMConfigHelper.MEDataPath + @"Content\", true);
                    logger.Info("Applying DedicatedServer ...");
                    FSHelper.DirectoryCopy(PathHelper.GetMESyncDirPath() + @"DedicatedServer\",
                        SESMConfigHelper.MEDataPath + @"DedicatedServer\", true);
                    logger.Info("Applying DedicatedServer64 ...");
                    FSHelper.DirectoryCopy(PathHelper.GetMESyncDirPath() + @"DedicatedServer64\",
                        SESMConfigHelper.MEDataPath + @"DedicatedServer64\", true);
                }

                logger.Info("Restarting all previously started server :");

                foreach (EntityServer server in MERunningServer)
                {
                    logger.Info("========");
                    logger.Info("Server : " + server.Name);
                    logger.Info("        Starting server ...");
                    ServiceHelper.StartService(server);
                }

                return ReturnEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Fatal("Exception : ", ex);
                return ReturnEnum.Exception;
            }
            finally
            {
                logger.Info("Lifting Lockdown");
                SESMConfigHelper.Lockdown = false;
                SESMConfigHelper.MEUpdating = false;
                logger.Info("--- End of ME Auto-Update Process ---");
            }
        }

        public static JobKey GetJobKey()
        {
            return new JobKey("MEAutoUpdate", "MEAutoUpdate");
        }

        public static TriggerKey GetTriggerKey()
        {
            return new TriggerKey("MEAutoUpdate", "MEAutoUpdate");
        }
    }
}