using System.Web.Mvc;
using System.Web.Routing;
using Quartz;
using Quartz.Impl;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.Helpers;
using SESM.Tools.Monitor;

namespace SESM
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Constants.SetVersion(2,6,0);

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();


            if (SESMConfigHelper.PerfMonitor)
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
            if (SESMConfigHelper.AutoUpdate)
            {
                IJobDetail autoUpdateJob = JobBuilder.Create<AutoUpdate>()
                    .WithIdentity("AutoUpdateJob", "AutoUpdate")
                    .Build();

                ITrigger autoUpdateTrigger = TriggerBuilder.Create()
                    .WithIdentity("AutoUpdateTrigger", "AutoUpdate")
                    .WithCronSchedule(SESMConfigHelper.AUInterval)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(autoUpdateJob, autoUpdateTrigger);
            }
            if (SESMConfigHelper.AutoBackupLvl1)
            {
                IJobDetail backupJob = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupLvl1Job", "Backups")
                    .UsingJobData("lvl", 1)
                    .Build();

                ITrigger backupTrigger = TriggerBuilder.Create()
                    .WithIdentity("BackupLvl1Trigger", "Backups")
                    .WithCronSchedule(SESMConfigHelper.ABIntervalLvl1)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(backupJob, backupTrigger);
            }

            if (SESMConfigHelper.AutoBackupLvl2)
            {
                IJobDetail backupJob = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupLvl2Job", "Backups")
                    .UsingJobData("lvl", 2)
                    .Build();

                ITrigger backupTrigger = TriggerBuilder.Create()
                    .WithIdentity("BackupLvl2Trigger", "Backups")
                    .WithCronSchedule(SESMConfigHelper.ABIntervalLvl2)
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(backupJob, backupTrigger);
            }

            if (SESMConfigHelper.AutoBackupLvl3)
            {
                IJobDetail backupJob = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupLvl3Job", "Backups")
                    .UsingJobData("lvl", 3)
                    .Build();

                ITrigger backupTrigger = TriggerBuilder.Create()
                    .WithIdentity("BackupLvl3Trigger", "Backups")
                    .WithCronSchedule(SESMConfigHelper.ABIntervalLvl3)
                    .StartNow()
                    .Build();
                
                scheduler.ScheduleJob(backupJob, backupTrigger);
            }

            DataContext context = new DataContext();

            ServerProvider srvPrv = new ServerProvider(context);

            foreach (EntityServer item in srvPrv.GetAllServers())
            {
                if (item.IsAutoRestartEnabled)
                {
                    IJobDetail autoRestartJob = JobBuilder.Create<AutoRestartJob>()
                        .WithIdentity("AutoRestart" + item.Id + "Job", "AutoRestart")
                        .UsingJobData("id", item.Id)
                        .Build();

                    ITrigger autoRestartTrigger = TriggerBuilder.Create()
                        .WithIdentity("AutoRestart" + item.Id + "Trigger", "AutoRestart")
                        .WithCronSchedule(SESMConfigHelper.ABIntervalLvl3)
                        .StartNow()
                        .Build();

                    scheduler.ScheduleJob(autoRestartJob, autoRestartTrigger);
                }
            }

            IJobDetail autoStartJob = JobBuilder.Create<AutoStartJob>()
                        .WithIdentity("AutoStartJob", "AutoStart")
                        .Build();

            ITrigger autoStartTrigger = TriggerBuilder.Create()
                .WithIdentity("AutoStartTrigger", "AutoStart")
                .WithCronSchedule("0 0/15 * * * ?")
                .StartNow()
                .Build();

            scheduler.ScheduleJob(autoStartJob, autoStartTrigger);

            /*
            // Font for the signature generator
            GraphHelper.privateFontCollection = new PrivateFontCollection();

            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-Black.ttf");
            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-BlackItalic.ttf");
            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-Bold.ttf");
            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-BoldItalic.ttf");
            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-Hairline.ttf");
            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-HairlineItalic.ttf");
            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-Italic.ttf");
            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-Light.ttf");
            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-LightItalic.ttf");
            GraphHelper.privateFontCollection.AddFontFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\font\Lato-Regular.ttf");*/
        }
    }
}
