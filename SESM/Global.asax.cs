using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Quartz;
using Quartz.Impl;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.Helpers;
using SESM.Tools.Jobs;
using SESM.Tools.Monitor;

namespace SESM
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Constants.SetVersion(3,1,0);
            // Resetting Run Vars
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\SESM.RunVar"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\SESM.RunVar");

            // Registering routes
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            // Auto-Update
            if (SESMConfigHelper.AutoUpdateEnabled)
            {
                // Instantiating the job
                IJobDetail SEAutoUpdateJobDetail = JobBuilder.Create<SEAutoUpdateJob>()
                    .WithIdentity(SEAutoUpdateJob.GetJobKey())
                    .Build();

                ITrigger SEAutoUpdateJobTrigger = TriggerBuilder.Create()
                    .WithIdentity(SEAutoUpdateJob.GetTriggerKey())
                    .WithCronSchedule(SESMConfigHelper.AutoUpdateCron)
                    .Build();

                scheduler.ScheduleJob(SEAutoUpdateJobDetail, SEAutoUpdateJobTrigger);
            }

            // SESE Auto-update
            if (SESMConfigHelper.SESEAutoUpdateEnabled)
            {
                // Instantiating the job
                IJobDetail SESEAutoUpdateJobDetail = JobBuilder.Create<SESEAutoUpdateJob>()
                    .WithIdentity(SESEAutoUpdateJob.GetJobKey())
                    .Build();

                ITrigger SESEAutoUpdateJobTrigger = TriggerBuilder.Create()
                    .WithIdentity(SESEAutoUpdateJob.GetTriggerKey())
                    .WithCronSchedule(SESMConfigHelper.SESEAutoUpdateCron)
                    .Build();

                scheduler.ScheduleJob(SESEAutoUpdateJobDetail, SESEAutoUpdateJobTrigger);
            }

            // Perf Monitor
            if (SESMConfigHelper.PerfMonitorEnabled)
            {
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
            }

            // Auto Backups
            if (SESMConfigHelper.AutoBackupLvl1Enabled)
            {
                IJobDetail backupJob = JobBuilder.Create<AutoBackupJob>()
                    .WithIdentity(AutoBackupJob.GetJobKey(1))
                    .UsingJobData("lvl", 1)
                    .Build();

                ITrigger backupTrigger = TriggerBuilder.Create()
                    .WithIdentity(AutoBackupJob.GetTriggerKey(1))
                    .WithCronSchedule(SESMConfigHelper.AutoBackupLvl1Cron)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(backupJob, backupTrigger);
            }

            if (SESMConfigHelper.AutoBackupLvl2Enabled)
            {
                IJobDetail backupJob = JobBuilder.Create<AutoBackupJob>()
                    .WithIdentity(AutoBackupJob.GetJobKey(2))
                    .UsingJobData("lvl", 2)
                    .Build();

                ITrigger backupTrigger = TriggerBuilder.Create()
                    .WithIdentity(AutoBackupJob.GetTriggerKey(2))
                    .WithCronSchedule(SESMConfigHelper.AutoBackupLvl2Cron)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(backupJob, backupTrigger);
            }

            if (SESMConfigHelper.AutoBackupLvl3Enabled)
            {
                IJobDetail backupJob = JobBuilder.Create<AutoBackupJob>()
                    .WithIdentity(AutoBackupJob.GetJobKey(3))
                    .UsingJobData("lvl", 3)
                    .Build();

                ITrigger backupTrigger = TriggerBuilder.Create()
                    .WithIdentity(AutoBackupJob.GetTriggerKey(3))
                    .WithCronSchedule(SESMConfigHelper.AutoBackupLvl3Cron)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(backupJob, backupTrigger);
            }


            // Auto Restart
            DataContext context = new DataContext();
            ServerProvider srvPrv = new ServerProvider(context);

            foreach (EntityServer item in srvPrv.GetAllServers())
            {
                if (item.IsAutoRestartEnabled)
                {
                    try
                    {

                        IJobDetail autoRestartJob = JobBuilder.Create<AutoRestartJob>()
                            .WithIdentity(AutoRestartJob.GetJobKey(item))
                            .UsingJobData("id", item.Id)
                            .Build();

                        ITrigger autoRestartTrigger = TriggerBuilder.Create()
                            .WithIdentity(AutoRestartJob.GetTriggerKey(item))
                            .WithCronSchedule(item.AutoRestartCron)
                            .StartNow()
                            .Build();

                        scheduler.ScheduleJob(autoRestartJob, autoRestartTrigger);

                    }
                    catch (Exception)
                    {
                    }
                }
            }

            IJobDetail autoStartJob = JobBuilder.Create<AutoStartJob>()
                        .WithIdentity(AutoStartJob.GetJobKey())
                        .Build();

            ITrigger autoStartTrigger = TriggerBuilder.Create()
                .WithIdentity(AutoStartJob.GetTriggerKey())
                .WithCronSchedule("0 0/15 * * * ?")
                .StartNow()
                .Build();

            scheduler.ScheduleJob(autoStartJob, autoStartTrigger);
        }
    }
}
