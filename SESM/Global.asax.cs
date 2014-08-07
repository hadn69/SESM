using System.Web.Mvc;
using System.Web.Routing;
using Quartz;
using Quartz.Impl;
using SESM.Tools;
using SESM.Tools.Helpers;
using SESM.Tools.Monitor;

namespace SESM
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            
            IJobDetail collectorJob = JobBuilder.Create<Collector>()
                .WithIdentity("CollectorJob", "Monitor")
                .Build();

            ITrigger collectorTrigger = TriggerBuilder.Create()
                .WithIdentity("CollectorTrigger", "Monitor")
                .WithCronSchedule("0 * * * * ?")
                .StartNow()
                .Build();

            scheduler.ScheduleJob(collectorJob, collectorTrigger);

            IJobDetail hourlyCrusherJob = JobBuilder.Create<HourlyCrusher>()
                .WithIdentity("HourlyCrusherJob", "Monitor")
                .Build();

            ITrigger hourlyCrusherTrigger = TriggerBuilder.Create()
                .WithIdentity("HourlyCrusherTrigger", "Monitor")
                .WithCronSchedule("0 0 * * * ?")
                .StartNow()
                .Build();
            
            scheduler.ScheduleJob(hourlyCrusherJob, hourlyCrusherTrigger);
            
            IJobDetail autoUpdateJob = JobBuilder.Create<AutoUpdate>()
                .WithIdentity("AutoUpdateJob", "AutoUpdate")
                .Build();

            ITrigger autoUpdateTrigger = TriggerBuilder.Create()
                .WithIdentity("AutoUpdateTrigger", "AutoUpdate")
                .WithCronSchedule(SESMConfigHelper.AUInterval)
                .StartNow()
                .Build();

            scheduler.ScheduleJob(autoUpdateJob, autoUpdateTrigger);

            if (SESMConfigHelper.AutoBackupLvl1)
            {
                IJobDetail BackupJob = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupLvl1Job", "Backups")
                    .UsingJobData("lvl", 1)
                    .Build();

                ITrigger BackupTrigger = TriggerBuilder.Create()
                    .WithIdentity("BackupLvl1Trigger", "Backups")
                    .WithCronSchedule(SESMConfigHelper.ABIntervalLvl1)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(BackupJob, BackupTrigger);
            }

            if (SESMConfigHelper.AutoBackupLvl2)
            {
                IJobDetail BackupJob = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupLvl2Job", "Backups")
                    .UsingJobData("lvl", 2)
                    .Build();

                ITrigger BackupTrigger = TriggerBuilder.Create()
                    .WithIdentity("BackupLvl2Trigger", "Backups")
                    .WithCronSchedule(SESMConfigHelper.ABIntervalLvl2)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(BackupJob, BackupTrigger);
            }

            if (SESMConfigHelper.AutoBackupLvl3)
            {
                IJobDetail BackupJob = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupLvl3Job", "Backups")
                    .UsingJobData("lvl", 3)
                    .Build();

                ITrigger BackupTrigger = TriggerBuilder.Create()
                    .WithIdentity("BackupLvl3Trigger", "Backups")
                    .WithCronSchedule(SESMConfigHelper.ABIntervalLvl3)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(BackupJob, BackupTrigger);
            }
        }
    }
}
