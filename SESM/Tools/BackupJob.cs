using System;
using System.IO;
using System.Text;
using System.Threading;
using Ionic.Zip;
using NLog;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class BackupJob : IJob
    {
        public void Execute(IJobExecutionContext jobContext)
        {
            JobDataMap dataMap = jobContext.JobDetail.JobDataMap;

            int backupLvl = dataMap.GetInt("lvl");

            switch (backupLvl)
            {
                case 1:
                    if (!SESMConfigHelper.AutoBackupLvl1)
                        return;
                    break;
                case 2:
                    if (!SESMConfigHelper.AutoBackupLvl2)
                        return;
                    break;
                case 3:
                    if (!SESMConfigHelper.AutoBackupLvl3)
                        return;
                    break;
                default:
                    return;
                    break;
            }
            Logger logger = LogManager.GetLogger("Backuplvl" + backupLvl + "Logger");
            DataContext context = new DataContext();
            ServerProvider srvPrv = new ServerProvider(context);
            logger.Info("----Starting Backup lvl " + backupLvl + "----");
            foreach (EntityServer item in srvPrv.GetAllServers())
            {
                logger.Info("Checking " + item.Name);
                if (srvPrv.GetState(item) != ServiceState.Stopped)
                {
                    logger.Info("Server  " + item.Name + " is not stopped, eligible for backup");
                    bool enabled = false;
                    int nbToKeep = 0;

                    switch (backupLvl)
                    {
                        case 1:
                            if (item.IsLvl1BackupEnabled)
                                enabled = true;
                            nbToKeep = SESMConfigHelper.ABNbToKeepLvl1;
                            break;
                        case 2:
                            if (item.IsLvl2BackupEnabled)
                                enabled = true;
                            nbToKeep = SESMConfigHelper.ABNbToKeepLvl2;
                            break;
                        case 3:
                            if (item.IsLvl3BackupEnabled)
                                enabled = true;
                            nbToKeep = SESMConfigHelper.ABNbToKeepLvl3;
                            break;
                    }
                    if (enabled)
                    {
                        try
                        {
                            logger.Info("Backup lvl " + backupLvl + " selected for this server, checking for backup number");
                            if (!Directory.Exists(PathHelper.GetBackupsPath(item)))
                                Directory.CreateDirectory(PathHelper.GetBackupsPath(item));

                            string[] backupList = Directory.GetFiles(PathHelper.GetBackupsPath(item), "AutoBackupLvl" + backupLvl + "_*.zip");
                            logger.Info(backupList.Length + " Backup(s) present for this server (max : " + nbToKeep + ")");
                            Array.Sort(backupList);

                            while (nbToKeep != 0 && backupList.Length >= nbToKeep)
                            {
                                logger.Info("Deleting oldest backup");
                                File.Delete(backupList[0]);
                                backupList = Directory.GetFiles(PathHelper.GetBackupsPath(item), "AutoBackupLvl" + backupLvl + "_*.zip");
                                Array.Sort(backupList);
                            }
                            ServerConfigHelper config = new ServerConfigHelper();
                            config.LoadFromServConf(PathHelper.GetConfigurationFilePath(item));
                            if (!string.IsNullOrEmpty(config.SaveName))
                            {
                                int trycount = 1;
                                int maxtry = 3;
                                while (trycount <= maxtry)
                                {
                                    using (ZipFile zip = new ZipFile())
                                    {
                                        try
                                        {
                                            logger.Info("Trying to create backup zip (try " + trycount + " of " + maxtry + ") : " +
                                                        PathHelper.GetBackupsPath(item) + "AutoBackupLvl" + backupLvl + "_" +
                                                        DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + config.SaveName +
                                                        ".zip");
                                            zip.AddSelectedFiles("*", PathHelper.GetSavePath(item, config.SaveName),
                                                string.Empty, true);

                                            if(item.UseServerExtender)
                                            {
                                                zip.RemoveEntry("Sandbox.sbc");
                                                string text = File.ReadAllText(PathHelper.GetSavePath(item, config.SaveName) + @"\Sandbox.sbc", new UTF8Encoding(false));
                                                text = text.Replace("<AutoSaveInMinutes>0</AutoSaveInMinutes>",
                                                    "<AutoSaveInMinutes>" + item.AutoSaveInMinutes + "</AutoSaveInMinutes>");
                                                zip.AddEntry("Sandbox.sbc", text, new UTF8Encoding(false));
                                            }

                                            zip.Save(PathHelper.GetBackupsPath(item) + "AutoBackupLvl" + backupLvl + "_" +
                                                     DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + config.SaveName +
                                                     ".zip");
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Info("Caught Exception in Backup lvl " + backupLvl + " Job for server " + item.Name + ":", ex);
                                            logger.Info("Waiting 5 seconds");
                                            Thread.Sleep(5000);
                                        } 
                                    }
                                    trycount++;
                                }
                                if (trycount > maxtry)
                                    logger.Error("Failed to save backup, please report !");
                                else
                                    logger.Info("Success !");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger exceptionLogger = LogManager.GetLogger("GenericExceptionLogger");
                            exceptionLogger.Fatal("Caught Exception in Backup lvl " + backupLvl + " Job for server " + item.Name + ":", ex);
                        }
                    }
                    else
                    {
                        logger.Info("Backup lvl " + backupLvl + " is not selected for this server, moving to the next");
                    }
                }
            }
            logger.Info("----End of Backup lvl " + backupLvl + "----");
        }
    }
}