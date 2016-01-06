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
using SESM.Tools.Jobs.Monitor;

namespace SESM
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Constants.SetVersion(4,3,1);

            // Resetting Run Vars
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\SESM.RunVar"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\SESM.RunVar");

            SESMConfigHelper.Prefix = SESMConfigHelper.Prefix;

            // Trying to init the registery
            SESMConfigHelper.InitializeRegistry();

            // Killing any remaining SteamCMD
            ServiceHelper.KillAllProcesses("steamcmd");
            ServiceHelper.KillAllProcesses("SteamService");
            ServiceHelper.KillAllProcesses("steamerrorreporter");

            // Registering public branch if none set
            if (string.IsNullOrWhiteSpace(SESMConfigHelper.SEAutoUpdateBranch))
                SESMConfigHelper.SEAutoUpdateBranch = "public";
            if (string.IsNullOrWhiteSpace(SESMConfigHelper.MEAutoUpdateBranch))
                SESMConfigHelper.MEAutoUpdateBranch = "public";

            // Registering routes
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            // SE Auto-Update
            if (SESMConfigHelper.SEAutoUpdateEnabled)
            {
                // Instantiating the job
                IJobDetail SEAutoUpdateJobDetail = JobBuilder.Create<SEAutoUpdateJob>()
                    .WithIdentity(SEAutoUpdateJob.GetJobKey())
                    .Build();

                ITrigger SEAutoUpdateJobTrigger = TriggerBuilder.Create()
                    .WithIdentity(SEAutoUpdateJob.GetTriggerKey())
                    .WithCronSchedule(SESMConfigHelper.SEAutoUpdateCron)
                    .Build();

                scheduler.ScheduleJob(SEAutoUpdateJobDetail, SEAutoUpdateJobTrigger);
            }

            // ME Auto-Update
            if (SESMConfigHelper.MEAutoUpdateEnabled)
            {
                // Instantiating the job
                IJobDetail MEAutoUpdateJobDetail = JobBuilder.Create<MEAutoUpdateJob>()
                    .WithIdentity(MEAutoUpdateJob.GetJobKey())
                    .Build();

                ITrigger MEAutoUpdateJobTrigger = TriggerBuilder.Create()
                    .WithIdentity(MEAutoUpdateJob.GetTriggerKey())
                    .WithCronSchedule(SESMConfigHelper.MEAutoUpdateCron)
                    .Build();

                scheduler.ScheduleJob(MEAutoUpdateJobDetail, MEAutoUpdateJobTrigger);
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
                IJobDetail collectorJob = JobBuilder.Create<CollectorJob>()
                    .WithIdentity("CollectorJob", "Monitor")
                    .Build();

                ITrigger collectorTrigger = TriggerBuilder.Create()
                    .WithIdentity("CollectorTrigger", "Monitor")
                    .WithCronSchedule("0 * * * * ?")
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(collectorJob, collectorTrigger);

                IJobDetail hourlyCrusherJob = JobBuilder.Create<HourlyCrusherJob>()
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

            ServerRoleProvider sroPrv = new ServerRoleProvider(context);
            HostRoleProvider hroPrv = new HostRoleProvider(context);

            if (sroPrv.GetServerRoles().Count == 0 && hroPrv.GetHostRoles().Count == 0)
            {
                EntityHostRole users = new EntityHostRole()
                {
                    Name = "Users",
                    PermissionsSerialized = "1"
                };

                hroPrv.AddHostRole(users);

                EntityServerRole Administrators = new EntityServerRole()
                {
                    Name = "Administrators",
                    PermissionsSerialized = "305;300;1316;1308;1317;1318;1324;1325;1321;1310;1307;" +
                                            "1319;1323;1309;1315;1305;1320;1322;1311;1313;1314;1306;" +
                                            "1300;1304;1312;1234;1212;1211;1236;1209;1235;1253;1255;" +
                                            "1226;1228;1219;1251;1246;1215;1263;1247;1248;1243;1258;" +
                                            "1245;1207;1249;1250;1242;1261;1223;1214;1217;1233;1218;" +
                                            "1205;1210;1262;1221;1220;1229;1231;1206;1225;1256;1200;" +
                                            "1208;1213;1227;1232;1241;1254;1252;1204;1238;1240;1259;" +
                                            "1260;1257;1239;1230;1244;1237;1216;1222;1224;1406;1405;" +
                                            "1401;1404;1400;1409;1402;1403;1407;1408;1410;1000;1604;" +
                                            "1602;1603;1600;1601;1605;1504;1502;1503;1500;1501;1505;" +
                                            "1700;1014;1013;1011;1012;1131;1132;1133;1130;1111;1113;" +
                                            "1114;1110;1112;1121;1122;1123;1120;1264;1265;1266"
                };

                EntityServerRole Moderators = new EntityServerRole()
                {
                    Name = "Moderators",
                    PermissionsSerialized = "1316;1317;1315;1300;1234;1235;1233;1200;1000;1600;1601;1500;1501;1014;1013;1011;1012"
                };

                sroPrv.AddServerRole(Administrators);
                sroPrv.AddServerRole(Moderators);
            }
        }
    }
}
