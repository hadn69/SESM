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
    public class SEAutoUpdateJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Logger logger = LogManager.GetLogger("SEAutoUpdateLogger");
            Run(logger, false, false, false);
        }

        public static ReturnEnum Run(Logger logger, bool manualFire = true, bool useLocalZip = false, bool force = false)
        {
            if (!manualFire && !SESMConfigHelper.AutoUpdateEnabled)
                return ReturnEnum.AloneJob;

            if (SESMConfigHelper.SEUpdating)
                return ReturnEnum.UpdateAlreadyRunning;

            logger.Info("--- Starting SE Auto-Update Process ---");
            logger.Info("Type : " + (manualFire ? "Manuel" : "Automatic"));
            logger.Info("Using Local zip : " + useLocalZip.ToString().ToUpper());
            logger.Info("Force : " + force.ToString().ToUpper());
            logger.Info("Checking Versions ...");

            int? localVersion = null;
            int? remoteVersion = null;
            for (int i = 0; i < 3; i++)
            {
                if (localVersion == null)
                {
                    logger.Info("Retrieving local version : ");
                    localVersion = SteamCMDHelper.GetInstalledVersion(logger);
                }

                if (remoteVersion == null)
                {
                    logger.Info("Retrieving remote version : ");
                    remoteVersion =
                        SteamCMDHelper.GetAvailableVersion(
                            !string.IsNullOrWhiteSpace(SESMConfigHelper.AutoUpdateBetaPassword), logger);
                }

                if (localVersion == null || remoteVersion == null)
                    logger.Info("Fail retrieving one of the version (try " + (i + 1) + " of 3)");
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
                SESMConfigHelper.SEUpdating = true;

                logger.Info("Update Available, downloading ...");

                logger.Info("Waiting 30 sec for all request to end");
                Thread.Sleep(30000);

                logger.Info("Stopping all SE Server : ");
                DataContext _context = new DataContext();
                ServerProvider srvPrv = new ServerProvider(_context);
                List<EntityServer> SEServer = srvPrv.GetAllServers();
                List<EntityServer> SERunningServer = new List<EntityServer>();

                foreach (EntityServer server in SEServer)
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

                    SERunningServer.Add(server);
                }
                logger.Info("Waiting for server stop (30 secs/serv max)");
                foreach (EntityServer server in SERunningServer)
                {
                    logger.Info("========");
                    logger.Info("Server : " + server.Name);
                    logger.Info("Waiting for stopped ...");
                    ServiceHelper.WaitForStopped(server);
                }

                logger.Info("Waiting 10 secs for grace periode ...");
                Thread.Sleep(10000);
                logger.Info("Killing any remaining SE process");
                ServiceHelper.KillAllServices();
                logger.Info("Waiting 10 secs for kills to finish ...");
                Thread.Sleep(10000);

                bool SESEPresent = File.Exists(SESMConfigHelper.SEDataPath + "DedicatedServer64\\SEServerExtender.exe");
                logger.Info("SESE Present (to reapply it) : " + SESEPresent.ToString().ToUpper());

                logger.Info("Cleaning SE Game Files ...");
                if (Directory.Exists(SESMConfigHelper.SEDataPath + @"Content\"))
                {
                    logger.Info("Deleting Content ...");
                    Directory.Delete(SESMConfigHelper.SEDataPath + @"Content\", true);
                }
                if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer\"))
                {
                    logger.Info("Deleting DedicatedServer ...");
                    Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer\", true);
                }
                if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer64\"))
                {
                    logger.Info("Deleting DedicatedServer64 ...");
                    Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer64\", true);
                }

                if (useLocalZip)
                {
                    logger.Info("Extracting SE Game Files ...");
                    using (ZipFile zip = ZipFile.Read(SESMConfigHelper.SEDataPath + "DedicatedServer.zip"))
                    {
                        zip.ExtractAll(SESMConfigHelper.SEDataPath);
                    }
                }
                else
                {
                    logger.Info("Updating SE Game Files ...");
                    SteamCMDHelper.Update(logger, !string.IsNullOrWhiteSpace(SESMConfigHelper.AutoUpdateBetaPassword));

                    logger.Info("Applying SE Game Files ...");
                    logger.Info("Applying Content ...");
                    FSHelper.DirectoryCopy(PathHelper.GetSyncDirPath() + @"Content\",
                        SESMConfigHelper.SEDataPath + @"Content\", true);
                    logger.Info("Applying DedicatedServer ...");
                    FSHelper.DirectoryCopy(PathHelper.GetSyncDirPath() + @"DedicatedServer\",
                        SESMConfigHelper.SEDataPath + @"DedicatedServer\", true);
                    logger.Info("Applying DedicatedServer64 ...");
                    FSHelper.DirectoryCopy(PathHelper.GetSyncDirPath() + @"DedicatedServer64\",
                        SESMConfigHelper.SEDataPath + @"DedicatedServer64\", true);
                }

                if (SESEPresent)
                {
                    logger.Info("Applying SESE ...");
                    SESEHelper.ApplyUpdate();
                }

                logger.Info("Restarting all previously started server :");

                foreach (EntityServer server in SERunningServer)
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
                SESMConfigHelper.SEUpdating = false;
                logger.Info("--- End of SE Auto-Update Process ---");
            }
        }

        public static JobKey GetJobKey()
        {
            return new JobKey("SEAutoUpdate", "SEAutoUpdate");
        }

        public static TriggerKey GetTriggerKey()
        {
            return new TriggerKey("SEAutoUpdate", "SEAutoUpdate");
        }
    }
}