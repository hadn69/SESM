using System;
using System.Data.Entity.Migrations.Infrastructure;
using NLog;
using Quartz;
using SESM.Tools.Helpers;

namespace SESM.Tools.Jobs
{
    [DisallowConcurrentExecution]
    public class SESEAutoUpdateJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Logger logger = LogManager.GetLogger("SESEAutoUpdateLogger");
            Run(logger, false);
        }

        public ReturnEnum Run(Logger logger, bool manualFire = true)
        {
            try
            {
                logger.Info("--- Starting SESE Auto-Update Process ---");
                logger.Info("Type : " + (manualFire ? "Manuel" : "Automatic"));
                logger.Info("Checking Versions ...");

                Version localVersion = SESEHelper.GetLocalVersion();
                logger.Info(" - Local Version : " + localVersion.ToString());
                
                string githubData = SESEHelper.GetGithubData();
                Version remoteVersion = SESEHelper.GetLastRemoteVersion(githubData, SESMConfigHelper.SESEAutoUpdateUseDev);
                logger.Info(" - Remote Version : " + remoteVersion.ToString());

                // Test for update
                if (localVersion.CompareTo(remoteVersion) >= 0)
                {
                    logger.Info("No Update Available, Exiting ...");
                    return ReturnEnum.NothingToDo;
                }
                logger.Info("Update Available, Starting Update Process ...");
                logger.Info("Initiating Lockdown mode, Starting Update Process ...");


                return ReturnEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Fatal("Exception : ", ex);
                return ReturnEnum.Exception;
            }
            finally
            {
                logger.Info("--- End of SESE Auto-Update Process ---");
            }
        }

        public static JobKey GetJobKey()
        {
            return new JobKey("SESEAutoUpdate", "SESEAutoUpdate");
        }
    }
}