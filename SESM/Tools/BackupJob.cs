using System;
using System.IO;
using Ionic.Zip;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class BackupJob :IJob
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

            DataContext context = new DataContext();
            ServerProvider srvPrv = new ServerProvider(context);

            foreach (EntityServer item in srvPrv.GetAllServers())
            {
                if (srvPrv.GetState(item) != ServiceState.Stopped)
                {
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
                        if (!Directory.Exists(PathHelper.GetBackupsPath(item)))
                            Directory.CreateDirectory(PathHelper.GetBackupsPath(item));

                        string[] backupList = Directory.GetFiles(PathHelper.GetBackupsPath(item), "AutoBackupLvl" + backupLvl + "_*.zip");

                        Array.Sort(backupList);

                        while (nbToKeep != 0 && backupList.Length >= nbToKeep)
                        {
                            File.Delete(backupList[0]);
                            backupList = Directory.GetFiles(PathHelper.GetBackupsPath(item), "AutoBackupLvl" + backupLvl + "_*.zip");
                            Array.Sort(backupList);
                        }
                        ServerConfigHelper config = new ServerConfigHelper();
                        config.Load(PathHelper.GetConfigurationFilePath(item));
                        if(!string.IsNullOrEmpty(config.SaveName))
                            using (ZipFile zip = new ZipFile())
                            {
                                zip.AddSelectedFiles("*", PathHelper.GetSavePath(item, config.SaveName), string.Empty, true);
                                zip.Save(PathHelper.GetBackupsPath(item) + "AutoBackupLvl" + backupLvl + "_"+ DateTime.Now.ToString("yyyyMMddHHmmss")  + ".zip");
                            }
                    }
                }
            }
        }
    }
}