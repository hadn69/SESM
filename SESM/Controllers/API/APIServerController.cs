using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using System.Xml.Linq;
using NLog;
using Quartz;
using Quartz.Impl;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.API;
using SESM.Tools.Helpers;
using SESM.Tools.Jobs;

namespace SESM.Controllers.API
{
    [APICheckLockdown]
    public class APIServerController : Controller, IAPIController
    {
        public DataContext CurrentContext { get; set; }

        public EntityServer RequestServer { get; set; }

        public APIServerController()
        {
            CurrentContext = new DataContext();
        }

        #region Informations Provider
        
        // GET: API/Server/GetServers
        [HttpGet]
        public ActionResult GetServers()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);
            UserProvider usrPrv = new UserProvider(CurrentContext);

            EntityUser user = Session["User"] as EntityUser;

            int userID = user == null ? 0 : user.Id;
            EntityUser usr = usrPrv.GetUser(userID);


            List<EntityServer> servers = srvPrv.GetServers(user);
            Dictionary<EntityServer, ServiceState> serversState = srvPrv.GetState(servers);

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SRV-GSS-OK");

            foreach (EntityServer server in servers)
            {
                response.AddToContent(new XElement("Server", new XElement("Name", server.Name),
                                                             new XElement("ID", server.Id),
                                                             new XElement("Public", server.IsPublic.ToString()),
                                                             new XElement("State", serversState[server].ToString()),
                                                             new XElement("Type", server.ServerType == EnumServerType.SpaceEngineers ? server.UseServerExtender ? "SESE" : "SE" : "ME"),
                                                             new XElement("HasAnyAccess", AuthHelper.HasAnyServerAccess(server)),
                                                             new XElement("CanStart", AuthHelper.HasAccess(RequestServer, "SERVER_POWER_START")),
                                                             new XElement("CanStop", AuthHelper.HasAccess(RequestServer, "SERVER_POWER_STOP")),
                                                             new XElement("CanRestart", AuthHelper.HasAccess(RequestServer, "SERVER_POWER_RESTART")),
                                                             new XElement("CanKill", AuthHelper.HasAccess(RequestServer, "SERVER_POWER_KILL")),
                                                             new XElement("CanDelete", AuthHelper.HasAccess(RequestServer, "SERVER_DELETE"))
                                                             ));
            }
            return Content(response.ToString());
        }

        // POST: API/Server/GetServer
        [HttpPost]
        [APIServerAccess("SRV-GS", "SERVER_INFO")]
        public ActionResult GetServer()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SRV-GS-OK");

            response.AddToContent(new XElement("Name", RequestServer.Name));
            response.AddToContent(new XElement("ID", RequestServer.Id));
            response.AddToContent(new XElement("Public", RequestServer.IsPublic.ToString()));
            response.AddToContent(new XElement("State", srvPrv.GetState(RequestServer).ToString()));
            response.AddToContent(new XElement("HasAnyAccess", AuthHelper.HasAnyServerAccess(RequestServer)));
            response.AddToContent(new XElement("Type", RequestServer.ServerType == EnumServerType.SpaceEngineers ? RequestServer.UseServerExtender ? "SESE" : "SE" : "ME"));
            response.AddToContent(new XElement("CanStart", AuthHelper.HasAccess(RequestServer, "SERVER_POWER_START")));
            response.AddToContent(new XElement("CanStop", AuthHelper.HasAccess(RequestServer, "SERVER_POWER_STOP")));
            response.AddToContent(new XElement("CanRestart", AuthHelper.HasAccess(RequestServer, "SERVER_POWER_RESTART")));
            response.AddToContent(new XElement("CanKill", AuthHelper.HasAccess(RequestServer, "SERVER_POWER_KILL")));
            response.AddToContent(new XElement("CanDelete", AuthHelper.HasAccess(RequestServer, "SERVER_DELETE")));

            return Content(response.ToString());
        }

        #endregion

        #region Server CRUD

        // POST: API/Server/CreateServer
        [HttpPost]
        [APIHostAccess("SRV-CRS", "SERVER_CREATE")]
        public ActionResult CreateServer()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            string ServerName = Request.Form["ServerName"];

            if (string.IsNullOrWhiteSpace(ServerName))
                return Content(XMLMessage.Error("SRV-CRS-MISNAME", "The ServerName field must be provided").ToString());

            if (!Regex.IsMatch(ServerName, @"^[a-zA-Z0-9_.-]+$"))
                return Content(XMLMessage.Error("SRV-CRS-BADNAME", "The ServerName must be only composed of letters, numbers, dots, dashs and underscores").ToString());

            if (!srvPrv.IsNameAvaialble(ServerName))
                return Content(XMLMessage.Error("SRV-CRS-NAMEUSE", "The server " + ServerName + " Already Exist").ToString());

            string ServerType = Request.Form["ServerType"];

            if (string.IsNullOrWhiteSpace(ServerType))
                return Content(XMLMessage.Error("SRV-CRS-MISTYPE", "The ServerType field must be provided").ToString());

            if (!(ServerType == "SE" || ServerType == "ME"))
                return Content(XMLMessage.Error("SRV-CRS-BADTYPE", "The ServerType must be either SE or ME").ToString());

            EnumServerType serverTypeParsed = EnumServerType.SpaceEngineers;

            switch (ServerType)
            {
                case "SE":
                    serverTypeParsed = EnumServerType.SpaceEngineers;
                    break;
                case "ME":
                    serverTypeParsed = EnumServerType.MedievalEngineers;
                    break;
            }

            // ** PROCESS **
            EntityServer server = new EntityServer
            {
                Name = ServerName,
                Ip = SEDefault.IP,
                IsPublic = SEDefault.IsPublic,
                Port = srvPrv.GetNextAvailablePort(),
                ServerType = serverTypeParsed
            };
            srvPrv.CreateServer(server);

            Directory.CreateDirectory(PathHelper.GetSavesPath(server));
            Directory.CreateDirectory(PathHelper.GetInstancePath(server) + @"Mods");
            Directory.CreateDirectory(PathHelper.GetInstancePath(server) + @"Backups");

            ServerConfigHelperBase configHelper;

            if (server.ServerType == EnumServerType.SpaceEngineers)
                configHelper = new SEServerConfigHelper();
            else
                configHelper = new MEServerConfigHelper();

            configHelper.IP = server.Ip;
            configHelper.ServerPort = server.Port;

            configHelper.Save(server);
            ServiceHelper.RegisterService(server);

            return Content(XMLMessage.Success("SRV-CRS-OK", "The server " + ServerName + " was created").ToString());
        }

        // POST: API/Server/DeleteServers
        [HttpPost]
        public ActionResult DeleteServers()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING **
            string[] serverIDsString = Request.Form["ServerIDs"].Split(';');

            List<int> serverIDs = new List<int>();

            foreach (string item in serverIDsString)
            {
                int servID;
                if (!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-DEL-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach (int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if (server == null)
                    return Content(XMLMessage.Error("SRV-DEL-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                if (!AuthHelper.HasAccess(server, "SERVER_DELETE"))
                    return Content(XMLMessage.Error("SRV-DEL-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());

                servers.Add(server);
            }

            // ** PROCESS **
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            foreach (EntityServer item in servers)
            {
                serviceLogger.Info(item.Name + " killed by " + user.Login + " by API/Server/DeleteServers/");
                ServiceHelper.KillService(item);
                Thread.Sleep(200);
                ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(item));

                scheduler.DeleteJob(ResetPriorityJob.GetJobKey(item));
                scheduler.DeleteJob(AutoRestartJob.GetJobKey(item));
                srvPrv.RemoveServer(item);

                try
                {
                    Directory.Delete(PathHelper.GetInstancePath(item), true);
                }
                catch (Exception)
                {
                }
            }

            return Content(XMLMessage.Success("SRV-DEL-OK", "The following server(s) have been deleted : " + string.Join(", ", servers.Select(x => x.Name))).ToString());
        }

        #endregion

        #region Server Settings

        // POST: API/Server/GetSettings
        [HttpPost]
        [APIServerAccess("SRV-GSET", "SERVER_SETTINGS_GLOBAL_RD")]
        public ActionResult GetSettings()
        {
            // ** PROCESS **
            XMLMessage response = new XMLMessage("SRV-GSET-OK");

            XElement values = new XElement("Values");
            values.Add(new XElement("Name", RequestServer.Name));
            if (RequestServer.ServerType == EnumServerType.SpaceEngineers)
                values.Add(new XElement("UseServerExtender", RequestServer.UseServerExtender));
            values.Add(new XElement("Public", RequestServer.IsPublic));
            values.Add(new XElement("ProcessPriority", RequestServer.ProcessPriority.ToString()));
            values.Add(new XElement("StartupType", RequestServer.ServerStartup.ToString()));
            response.AddToContent(values);

            XElement rights = new XElement("Rights");
            rights.Add(new XElement("Name", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_NAME_WR")));
            if (RequestServer.ServerType == EnumServerType.SpaceEngineers)
                rights.Add(new XElement("UseServerExtender", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_USESESE_WR")));
            rights.Add(new XElement("Public", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_PUBLIC_WR")));
            rights.Add(new XElement("ProcessPriority", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_PROCESSPRIO_WR")));
            rights.Add(new XElement("StartupType", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_STARTUPTYPE_WR")));
            response.AddToContent(rights);

            return Content(response.ToString());
        }

        // POST: API/Server/SetSettings
        [HttpPost]
        [APIServerAccess("SRV-SSET", "SERVER_SETTINGS_GLOBAL_NAME_WR",
                                     "SERVER_SETTINGS_GLOBAL_USESESE_WR",
                                     "SERVER_SETTINGS_GLOBAL_PROCESSPRIO_WR",
                                     "SERVER_SETTINGS_GLOBAL_PUBLIC_WR",
                                     "SERVER_SETTINGS_GLOBAL_STARTUPTYPE_WR")]
        public ActionResult SetSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            // ** PARSING / ACCESS **
            bool accessName = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_NAME_WR");
            bool accessSESE = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_USESESE_WR");
            bool accessPublic = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_PUBLIC_WR");
            bool accessProcessPrio = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_PROCESSPRIO_WR");
            bool accessServerStartup = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_GLOBAL_STARTUPTYPE_WR");

            string Name;
            if (accessName)
            {
                Name = Request.Form["Name"];
                if (string.IsNullOrWhiteSpace(Name))
                    return Content(XMLMessage.Error("SRV-SSET-MISNAME", "The Name field must be provided").ToString());
                if (!Regex.IsMatch(Name, @"^[a-zA-Z0-9_.-]+$"))
                    return Content(
                            XMLMessage.Error("SRV-SSET-BADNAME",
                                "The Name field must be only composed of letters, numbers, dots, dashs and underscores")
                                .ToString());
                if (Name != RequestServer.Name && !srvPrv.IsNameAvaialble(Name))
                    return Content(XMLMessage.Error("SRV-SSET-NAMEUSE", "The server " + Name + "Already Exist").ToString());
            }
            else
                Name = RequestServer.Name;

            bool UseServerExtender;
            if (RequestServer.ServerType == EnumServerType.SpaceEngineers && accessSESE)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["UseServerExtender"]))
                    return Content(XMLMessage.Error("SRV-SSET-MISSESE", "The UseServerExtender field must be provided").ToString());
                if (!bool.TryParse(Request.Form["UseServerExtender"], out UseServerExtender))
                    return Content(XMLMessage.Error("SRV-SSET-BADSESE", "The UseServerExtender field is invalid").ToString());
            }
            else
                UseServerExtender = RequestServer.UseServerExtender;

            bool Public;
            if (accessPublic)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["Public"]))
                    return Content(XMLMessage.Error("SRV-SSET-MISPUB", "The Public field must be provided").ToString());
                if (!bool.TryParse(Request.Form["Public"], out Public))
                    return Content(XMLMessage.Error("SRV-SSET-BADPUB", "The Public field is invalid").ToString());
            }
            else
                Public = RequestServer.IsPublic;

            EnumProcessPriority ProcessPriority;
            if (accessProcessPrio)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ProcessPriority"]))
                    return Content(XMLMessage.Error("SRV-SSET-MISPP", "The ProcessPriority field must be provided").ToString());
                if (!Enum.TryParse(Request.Form["ProcessPriority"], out ProcessPriority))
                    return Content(XMLMessage.Error("SRV-SSET-BADPP", "The ProcessPriority field is invalid").ToString());
            }
            else
                ProcessPriority = RequestServer.ProcessPriority;

            EnumServerStartup ServerStartup;
            if (accessServerStartup)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["StartupType"]))
                    return Content(XMLMessage.Error("SRV-SSET-MISST", "The StartupType field must be provided").ToString());
                if (!Enum.TryParse(Request.Form["StartupType"], out ServerStartup))
                    return Content(XMLMessage.Error("SRV-SSET-BADST", "The StartupType field is invalid").ToString());
            }
            else
                ServerStartup = RequestServer.ServerStartup;

            // ** Process **
            try
            {
                ServiceState serverState = srvPrv.GetState(RequestServer);
                bool restartRequired = false;

                if ((accessName  && Name != RequestServer.Name) || (accessSESE && UseServerExtender != RequestServer.UseServerExtender) || (accessServerStartup && ServerStartup != RequestServer.ServerStartup))
                {
                    if (serverState != ServiceState.Stopped &&
                        serverState != ServiceState.Unknow)
                    {
                        ServiceHelper.StopServiceAndWait(RequestServer);
                        Thread.Sleep(5000);
                        ServiceHelper.KillService(RequestServer);
                        Thread.Sleep(1000);
                        restartRequired = true;
                    }
                    ServiceHelper.UnRegisterService(RequestServer);
                    string oldpath = PathHelper.GetInstancePath(RequestServer);
                    RequestServer.Name = Name;
                    RequestServer.UseServerExtender = UseServerExtender;
                    RequestServer.ServerStartup = ServerStartup;
                    string newpath = PathHelper.GetInstancePath(RequestServer);

                    if(oldpath != newpath)
                        Directory.Move(oldpath, newpath);

                    ServiceHelper.RegisterService(RequestServer);
                }
                if (accessPublic)
                    RequestServer.IsPublic = Public;

                if (accessProcessPrio)
                {
                    RequestServer.ProcessPriority = ProcessPriority;

                    if (serverState != ServiceState.Stopped && serverState != ServiceState.Unknow)
                        ServiceHelper.SetPriority(RequestServer);
                }


                srvPrv.UpdateServer(RequestServer);

                if (restartRequired)
                    ServiceHelper.StartService(RequestServer);

                return Content(XMLMessage.Success("SRV-SSET-OK", "The server settings have been updated").ToString());
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("SRV-SSET-EX", "Exception :" + ex).ToString());
            }
        }

        // POST: API/Server/GetJobsSettings
        [HttpPost]
        [APIServerAccess("SRV-GJS", "SERVER_SETTINGS_JOBS_RD")]
        public ActionResult GetJobsSettings()
        {
            // ** PROCESS **
            XMLMessage response = new XMLMessage("SRV-GJS-OK");

            XElement values = new XElement("Values");
            values.Add(new XElement("AutoRestart", RequestServer.IsAutoRestartEnabled));
            values.Add(new XElement("AutoRestartCron", RequestServer.AutoRestartCron));
            values.Add(new XElement("AutoStart", RequestServer.IsAutoStartEnabled));
            response.AddToContent(values);

            XElement rights = new XElement("Rights");
            rights.Add(new XElement("AutoRestart", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_JOBS_AUTORESTART_WR")));
            rights.Add(new XElement("AutoRestartCron", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_JOBS_AUTORESTARTCRON_WR")));
            rights.Add(new XElement("AutoStart", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_JOBS_AUTOSTART_WR")));
            response.AddToContent(rights);

            return Content(response.ToString());
        }

        // POST: API/Server/SetJobsSettings
        [HttpPost]
        [APIServerAccess("SRV-SJS", "SERVER_SETTINGS_JOBS_AUTORESTART_WR",
                                    "SERVER_SETTINGS_JOBS_AUTORESTARTCRON_WR",
                                    "SERVER_SETTINGS_JOBS_AUTOSTART_WR")]
        public ActionResult SetJobsSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            // ** PARSING / ACCESS **
            bool accessAutoRestart = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_JOBS_AUTORESTART_WR");
            bool accessAutoRestartCron = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_JOBS_AUTORESTARTCRON_WR");
            bool accessAutoStart = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_JOBS_AUTOSTART_WR");

            bool AutoRestart = false;
            if (accessAutoRestart)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["AutoRestart"]))
                    return Content(XMLMessage.Error("SRV-SJS-MISAR", "The AutoRestart field must be provided").ToString());
                if (!bool.TryParse(Request.Form["AutoRestart"], out AutoRestart))
                    return Content(XMLMessage.Error("SRV-SJS-BADAR", "The AutoRestart field is invalid").ToString());
            }
            else
                AutoRestart = RequestServer.IsAutoRestartEnabled;

            string AutoRestartCron;
            if (accessAutoRestartCron)
            {
                AutoRestartCron = Request.Form["AutoRestartCron"];
                if (string.IsNullOrWhiteSpace(AutoRestartCron))
                    return Content(XMLMessage.Error("SET-SJS-MISARC", "The AutoRestartCron field must be provided").ToString());
                if (!CronExpression.IsValidExpression(AutoRestartCron))
                    return Content(XMLMessage.Error("SET-SJS-BADARC", "The AutoRestartCron field is invalid").ToString());
            }
            else
                AutoRestartCron = RequestServer.AutoRestartCron;

            bool AutoStart = false;
            if (accessAutoStart)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["AutoStart"]))
                    return Content(XMLMessage.Error("SRV-SJS-MISAS", "The AutoStart field must be provided").ToString());
                if (!bool.TryParse(Request.Form["AutoStart"], out AutoStart))
                    return Content(XMLMessage.Error("SRV-SJS-BADAS", "The AutoStart field is invalid").ToString());
            }
            else
                AutoStart = RequestServer.IsAutoStartEnabled;

            // ** Process **
            try
            {
                if ((accessAutoRestart || accessAutoRestartCron) && (AutoRestart != RequestServer.IsAutoRestartEnabled || AutoRestartCron != RequestServer.AutoRestartCron))
                {
                    RequestServer.IsAutoRestartEnabled = AutoRestart;
                    RequestServer.AutoRestartCron = AutoRestartCron;

                    IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                    scheduler.DeleteJob(AutoRestartJob.GetJobKey(RequestServer));

                    if (RequestServer.IsAutoRestartEnabled)
                    {
                        // Instantiating the job
                        IJobDetail AutoRestartJobDetail = JobBuilder.Create<AutoRestartJob>()
                            .WithIdentity(AutoRestartJob.GetJobKey(RequestServer))
                            .Build();

                        ITrigger AutoRestartJobTrigger = TriggerBuilder.Create()
                            .WithIdentity(AutoRestartJob.GetTriggerKey(RequestServer))
                            .WithCronSchedule(RequestServer.AutoRestartCron)
                            .Build();

                        scheduler.ScheduleJob(AutoRestartJobDetail, AutoRestartJobTrigger);
                    }
                }

                if (accessAutoStart)
                    RequestServer.IsAutoStartEnabled = AutoStart;

                srvPrv.UpdateServer(RequestServer);

                return Content(XMLMessage.Success("SRV-SJS-OK", "The server Jobs settings have been updated").ToString());
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("SRV-SJS-EX", "Exception :" + ex).ToString());
            }
        }

        // POST: API/Server/GetBackupsSettings
        [HttpPost]
        [APIServerAccess("SRV-GBS", "SERVER_SETTINGS_BACKUPS_RD")]
        public ActionResult GetBackupsSettings()
        {
            // ** PROCESS **

            XMLMessage response = new XMLMessage("SRV-GBS-OK");

            XElement values = new XElement("Values");
            values.Add(new XElement("Lvl1BackupEnabled", RequestServer.IsLvl1BackupEnabled));
            values.Add(new XElement("Lvl1BackupInfos", "Cron : " + SESMConfigHelper.AutoBackupLvl1Cron +
                                                        " ; " + (SESMConfigHelper.AutoBackupLvl1Enabled ? "Enabled" : "Disabled") +
                                                        " ; Nb Rotating Backup : " + SESMConfigHelper.AutoBackupLvl1NbToKeep));
            values.Add(new XElement("Lvl2BackupEnabled", RequestServer.IsLvl2BackupEnabled));
            values.Add(new XElement("Lvl2BackupInfos", "Cron : " + SESMConfigHelper.AutoBackupLvl2Cron +
                                                        " ; " + (SESMConfigHelper.AutoBackupLvl2Enabled ? "Enabled" : "Disabled") +
                                                        " ; Nb Rotating Backup : " + SESMConfigHelper.AutoBackupLvl2NbToKeep));
            values.Add(new XElement("Lvl3BackupEnabled", RequestServer.IsLvl3BackupEnabled));
            values.Add(new XElement("Lvl3BackupInfos", "Cron : " + SESMConfigHelper.AutoBackupLvl3Cron +
                                                        " ; " + (SESMConfigHelper.AutoBackupLvl3Enabled ? "Enabled" : "Disabled") +
                                                        " ; Nb Rotating Backup : " + SESMConfigHelper.AutoBackupLvl3NbToKeep));
            response.AddToContent(values);

            XElement rights = new XElement("Rights");
            rights.Add(new XElement("Lvl1BackupEnabled", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_BACKUPS_LVL1_WR")));
            rights.Add(new XElement("Lvl2BackupEnabled", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_BACKUPS_LVL2_WR")));
            rights.Add(new XElement("Lvl3BackupEnabled", AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_BACKUPS_LVL3_WR")));
            response.AddToContent(rights);

            return Content(response.ToString());
        }

        // POST: API/Server/SetBackupsSettings
        [HttpPost]
        [APIServerAccess("SRV-SBS", "SERVER_SETTINGS_BACKUPS_LVL1_WR",
                                    "SERVER_SETTINGS_BACKUPS_LVL2_WR",
                                    "SERVER_SETTINGS_BACKUPS_LVL3_WR")]
        public ActionResult SetBackupsSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            // ** PARSING / ACCESS **
            bool accesslvl1 = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_BACKUPS_LVL1_WR");
            bool accesslvl2 = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_BACKUPS_LVL2_WR");
            bool accesslvl3 = AuthHelper.HasAccess(RequestServer, "SERVER_SETTINGS_BACKUPS_LVL3_WR");

            bool lvl1BackupEnabled;
            if (accesslvl1)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["Lvl1BackupEnabled"]))
                    return Content(XMLMessage.Error("SRV-SBS-MISLVL1", "The Lvl1BackupEnabled field must be provided").ToString());
                if (!bool.TryParse(Request.Form["Lvl1BackupEnabled"], out lvl1BackupEnabled))
                    return Content(XMLMessage.Error("SRV-SBS-BADLVL1", "The Lvl1BackupEnabled field is invalid").ToString());
            }
            else
                lvl1BackupEnabled = RequestServer.IsLvl1BackupEnabled;

            bool lvl2BackupEnabled;
            if (accesslvl2)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["Lvl2BackupEnabled"]))
                    return Content(XMLMessage.Error("SRV-SBS-MISLVL2", "The Lvl2BackupEnabled field must be provided").ToString());
                if (!bool.TryParse(Request.Form["Lvl2BackupEnabled"], out lvl2BackupEnabled))
                    return Content(XMLMessage.Error("SRV-SBS-BADLVL2", "The Lvl2BackupEnabled field is invalid").ToString());
            }
            else
                lvl2BackupEnabled = RequestServer.IsLvl2BackupEnabled;

            bool lvl3BackupEnabled;
            if (accesslvl3)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["Lvl3BackupEnabled"]))
                    return Content(XMLMessage.Error("SRV-SBS-MISLVL3", "The Lvl3BackupEnabled field must be provided").ToString());
                if (!bool.TryParse(Request.Form["Lvl3BackupEnabled"], out lvl3BackupEnabled))
                    return Content(XMLMessage.Error("SRV-SBS-BADLVL3", "The Lvl3BackupEnabled field is invalid").ToString());
            }
            else
                lvl3BackupEnabled = RequestServer.IsLvl3BackupEnabled;

            // ** Process **
            try
            {
                RequestServer.IsLvl1BackupEnabled = lvl1BackupEnabled;
                RequestServer.IsLvl2BackupEnabled = lvl2BackupEnabled;
                RequestServer.IsLvl3BackupEnabled = lvl3BackupEnabled;

                srvPrv.UpdateServer(RequestServer);

                return Content(XMLMessage.Success("SRV-SBS-OK", "The server backups settings have been updated").ToString());
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("SRV-SBS-EX", "Exception :" + ex).ToString());
            }
        }

        #endregion

        // POST: API/Server/GetServerRoles
        [HttpPost]
        [APIServerAccess("SRV-GSR", "ACCESS_SERVER_READ")]
        public ActionResult GetServerRoles()
        {
            ServerRoleProvider sroPrv = new ServerRoleProvider(CurrentContext);

            XMLMessage response = new XMLMessage("SRV-GSR-OK");

            foreach (EntityServerRole item in sroPrv.GetServerRoles())
            {
                response.AddToContent(new XElement("ServerRole", new XElement("Id", item.Id),
                                                                 new XElement("Name", item.Name)));
            }

            return Content(response.ToString());
        }

        // GET: API/Server/GetServerRoleAccess
        [HttpGet]
        public ActionResult GetServerRoleAccess()
        {
            XMLMessage response = new XMLMessage("ACC-GHRA-OK");

            response.AddToContent(new XElement("ACCESS_SERVER_READ", AuthHelper.HasAccess(RequestServer, "ACCESS_SERVER_READ")));
            response.AddToContent(new XElement("ACCESS_SERVER_EDIT_USERS", AuthHelper.HasAccess(RequestServer, "ACCESS_SERVER_EDIT_USERS")));

            return Content(response.ToString());
        }

        // POST: API/Server/GetServerPermissions
        [HttpPost]
        [APIServerAccess("SRV-GSP")]
        public ActionResult GetServerPermissions()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if (user == null)
                return Content(XMLMessage.Error("SRV-GSP-NOTLOG", "No user is logged in").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SRV-GSP-OK");

            foreach (string item in Enum.GetNames(typeof(EnumServerPerm)))
            {
                response.AddToContent(new XElement(item, AuthHelper.HasAccess(RequestServer, item)));
            }

            return Content(response.ToString());
        }

        // POST: API/Server/GetServerRoleDetails
        [HttpPost]
        [APIServerAccess("SRV-GSRD", "ACCESS_SERVER_EDIT_USERS")]
        public ActionResult GetServerRoleDetails()
        {
            ServerRoleProvider sroPrv = new ServerRoleProvider(CurrentContext);
            InstanceServerRoleProvider isrPrv = new InstanceServerRoleProvider(CurrentContext);

            int ServerRoleId;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerRoleId"]))
                return Content(XMLMessage.Error("SRV-GSRD-MISID", "The ServerRoleId field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerRoleId"], out ServerRoleId))
                return Content(XMLMessage.Error("SRV-GSRD-BADID", "The ServerRoleId is invalid").ToString());

            EntityServerRole serverRole = sroPrv.GetServerRole(ServerRoleId);

            if (serverRole == null)
                return Content(XMLMessage.Error("SRV-GSRD-UKNSR", "The ServerRole doesn't exist").ToString());

            EntityInstanceServerRole instanceServerRole = new EntityInstanceServerRole();

            if (isrPrv.GetInstanceServerRoles().Any(item => item.ServerRole == serverRole && item.Server == RequestServer))
            {
                instanceServerRole = isrPrv.GetInstanceServerRoles().First(item => item.ServerRole == serverRole && item.Server == RequestServer);
            }

            XMLMessage response = new XMLMessage("SRV-GSRD-OK");

            response.AddToContent(new XElement("Name", serverRole.Name));

            XElement perms = new XElement("Permissions");
            response.AddToContent(perms);

            foreach (EnumServerPerm item in serverRole.Permissions)
            {
                perms.Add(new XElement("Permission", (int)item));
            }

            XElement users = new XElement("Users");
            response.AddToContent(users);

            foreach (EntityUser item in instanceServerRole.Members)
            {
                users.Add(new XElement("User", item.Id));
            }

            return Content(response.ToString());
        }

        // POST: API/Server/SetServerRoleDetails
        [HttpPost]
        [APIServerAccess("SRV-SSRD", "ACCESS_SERVER_EDIT_USERS")]
        public ActionResult SetServerRoleDetails()
        {
            ServerRoleProvider sroPrv = new ServerRoleProvider(CurrentContext);
            InstanceServerRoleProvider isrPrv = new InstanceServerRoleProvider(CurrentContext);
            UserProvider usrPrv = new UserProvider(CurrentContext);

            int ServerRoleId;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerRoleId"]))
                return Content(XMLMessage.Error("SRV-SSRD-MISID", "The ServerRoleId field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerRoleId"], out ServerRoleId))
                return Content(XMLMessage.Error("SRV-SSRD-BADID", "The ServerRoleId is invalid").ToString());

            EntityServerRole serverRole = sroPrv.GetServerRole(ServerRoleId);

            if (serverRole == null)
                return Content(XMLMessage.Error("SRV-SSRD-UKNHR", "The ServerRole doesn't exist").ToString());

            EntityInstanceServerRole instanceServerRole = new EntityInstanceServerRole()
            {
                Server = RequestServer,
                ServerRole = serverRole
            };

            if (isrPrv.GetInstanceServerRoles().Any(item => item.ServerRole == serverRole && item.Server == RequestServer))
            {
                instanceServerRole = isrPrv.GetInstanceServerRoles().First(item => item.ServerRole == serverRole && item.Server == RequestServer);
            }
            else
            {
                isrPrv.AddInstanceServerRole(instanceServerRole);
            }

            List<EntityUser> users = new List<EntityUser>();

            string Users = Request.Form["Users"];

            foreach (string item in Users.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                int id;
                if (!int.TryParse(item, out id))
                    return
                        Content(XMLMessage.Error("SRV-SSRD-BADUSRID", "One of the user id isn't valid").ToString());

                if (!usrPrv.UserExist(id))
                    return Content(XMLMessage.Error("SRV-SSRD-BADUSR", "One of the user don't exist").ToString());

                users.Add(usrPrv.GetUser(id));
            }

            instanceServerRole.Members.Clear();

            foreach (EntityUser item in users)
            {
                instanceServerRole.Members.Add(item);
            }

            isrPrv.UpdateInstanceServerRole(instanceServerRole);

            return Content(XMLMessage.Success("SRV-SSRD-OK", "The role was updated").ToString());
        }

        // POST: API/Server/GetUsers
        [HttpPost]
        [APIServerAccess("SRV-GU", "ACCESS_SERVER_EDIT_USERS")]
        public ActionResult GetUsers()
        {
            // ** INIT **
            UserProvider usrPrv = new UserProvider(CurrentContext);

            List<EntityUser> users = usrPrv.GetUsers();

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SRV-GU-OK");

            foreach (EntityUser item in users)
            {
                response.AddToContent(new XElement("User", new XElement("Login", item.Login),
                                                           new XElement("ID", item.Id)));
            }
            return Content(response.ToString());
        }

        #region SE Server Configuration

        // POST: API/Server/SEGetConfiguration
        [HttpPost]
        [APIServerAccess("SRV-GC", "SERVER_CONFIG_SE_RD")]
        public ActionResult SEGetConfiguration()
        {
            // ** PROCESS **
            // Loading the server config
            SEServerConfigHelper serverConfig = new SEServerConfigHelper();
            serverConfig.Load(RequestServer);

            XMLMessage response = new XMLMessage("SRV-GC-OK");

            XElement values = new XElement("Values");
            values.Add(new XElement("IP", serverConfig.IP));
            values.Add(new XElement("SteamPort", serverConfig.SteamPort));
            values.Add(new XElement("ServerPort", serverConfig.ServerPort));
            values.Add(new XElement("ServerName", serverConfig.ServerName));
            values.Add(new XElement("IgnoreLastSession", serverConfig.IgnoreLastSession));
            values.Add(new XElement("PauseGameWhenEmpty", serverConfig.PauseGameWhenEmpty));
            values.Add(new XElement("EnableSpectator", serverConfig.EnableSpectator));
            values.Add(new XElement("RealisticSound", serverConfig.RealisticSound));
            values.Add(new XElement("AutoSaveInMinutes", RequestServer.AutoSaveInMinutes));
            values.Add(new XElement("InventorySizeMultiplier", serverConfig.InventorySizeMultiplier));
            values.Add(new XElement("AssemblerSpeedMultiplier", serverConfig.AssemblerSpeedMultiplier));
            values.Add(new XElement("AssemblerEfficiencyMultiplier", serverConfig.AssemblerEfficiencyMultiplier));
            values.Add(new XElement("RefinerySpeedMultiplier", serverConfig.RefinerySpeedMultiplier));
            values.Add(new XElement("GameMode", serverConfig.GameMode));
            values.Add(new XElement("EnableCopyPaste", serverConfig.EnableCopyPaste));
            values.Add(new XElement("WelderSpeedMultiplier", serverConfig.WelderSpeedMultiplier));
            values.Add(new XElement("GrinderSpeedMultiplier", serverConfig.GrinderSpeedMultiplier));
            values.Add(new XElement("HackSpeedMultiplier", serverConfig.HackSpeedMultiplier));
            values.Add(new XElement("DestructibleBlocks", serverConfig.DestructibleBlocks));
            values.Add(new XElement("MaxPlayers", serverConfig.MaxPlayers));
            values.Add(new XElement("MaxFloatingObjects", serverConfig.MaxFloatingObjects));
            values.Add(new XElement("WorldName", serverConfig.WorldName));
            values.Add(new XElement("EnvironmentHostility", serverConfig.EnvironmentHostility));
            values.Add(new XElement("WorldSizeKm", serverConfig.WorldSizeKm));
            values.Add(new XElement("PermanentDeath", serverConfig.PermanentDeath));
            values.Add(new XElement("CargoShipsEnabled", serverConfig.CargoShipsEnabled));
            values.Add(new XElement("RemoveTrash", serverConfig.RemoveTrash));
            values.Add(new XElement("ClientCanSave", serverConfig.ClientCanSave));

            XElement mods = new XElement("Mods");
            foreach (ulong mod in serverConfig.Mods)
                mods.Add(new XElement("Mod", mod));
            values.Add(mods);

            values.Add(new XElement("ViewDistance", serverConfig.ViewDistance));
            values.Add(new XElement("OnlineMode", serverConfig.OnlineMode));
            values.Add(new XElement("ResetOwnership", serverConfig.ResetOwnership));
            values.Add(new XElement("GroupID", serverConfig.GroupID));

            XElement administrators = new XElement("Administrators");
            foreach (ulong adminitrator in serverConfig.Administrators)
                administrators.Add(new XElement("Adminitrator", adminitrator));
            values.Add(administrators);

            XElement banned = new XElement("Banned");
            foreach (ulong ban in serverConfig.Banned)
                banned.Add(new XElement("Ban", ban));
            values.Add(banned);

            values.Add(new XElement("AutoHealing", serverConfig.AutoHealing));
            values.Add(new XElement("WeaponsEnabled", serverConfig.WeaponsEnabled));
            values.Add(new XElement("ShowPlayerNamesOnHud", serverConfig.ShowPlayerNamesOnHud));
            values.Add(new XElement("ThrusterDamage", serverConfig.ThrusterDamage));
            values.Add(new XElement("SpawnShipTimeMultiplier", serverConfig.SpawnShipTimeMultiplier));
            values.Add(new XElement("RespawnShipDelete", serverConfig.RespawnShipDelete));
            values.Add(new XElement("EnableToolShake", serverConfig.EnableToolShake));
            values.Add(new XElement("EnableIngameScripts", serverConfig.EnableIngameScripts));
            values.Add(new XElement("VoxelGeneratorVersion", serverConfig.VoxelGeneratorVersion));
            values.Add(new XElement("EnableOxygen", serverConfig.EnableOxygen));
            values.Add(new XElement("Enable3rdPersonView", serverConfig.Enable3rdPersonView));
            values.Add(new XElement("EnableEncounters", serverConfig.EnableEncounters));

            values.Add(new XElement("EnableFlora", serverConfig.EnableFlora));
            values.Add(new XElement("EnableStationVoxelSupport", serverConfig.EnableStationVoxelSupport));
            values.Add(new XElement("EnableSunRotation", serverConfig.EnableSunRotation));
            values.Add(new XElement("DisableRespawnShips", serverConfig.DisableRespawnShips));
            values.Add(new XElement("ScenarioEditMode", serverConfig.ScenarioEditMode));
            values.Add(new XElement("Battle", serverConfig.Battle));
            values.Add(new XElement("Scenario", serverConfig.Scenario));
            values.Add(new XElement("CanJoinRunning", serverConfig.CanJoinRunning));
            values.Add(new XElement("PhysicsIterations", serverConfig.PhysicsIterations));
            values.Add(new XElement("SunRotationIntervalMinutes", serverConfig.SunRotationIntervalMinutes));
            values.Add(new XElement("EnableJetpack", serverConfig.EnableJetpack));
            values.Add(new XElement("SpawnWithTools", serverConfig.SpawnWithTools));
            values.Add(new XElement("StartInRespawnScreen", serverConfig.StartInRespawnScreen));
            values.Add(new XElement("EnableVoxelDestruction", serverConfig.EnableVoxelDestruction));
            values.Add(new XElement("MaxDrones", serverConfig.MaxDrones));
            values.Add(new XElement("EnableDrones", serverConfig.EnableDrones));
            values.Add(new XElement("FloraDensity", serverConfig.FloraDensity));
            values.Add(new XElement("EnableCyberhounds", serverConfig.EnableCyberhounds));
            values.Add(new XElement("EnableSpiders", serverConfig.EnableSpiders));
            values.Add(new XElement("FloraDensityMultiplier", serverConfig.FloraDensityMultiplier));

            response.AddToContent(values);

            XElement rights = new XElement("Rights");
            rights.Add(new XElement("IP", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_IP_WR")));
            rights.Add(new XElement("SteamPort", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_STEAMPORT_WR")));
            rights.Add(new XElement("ServerPort", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SERVERPORT_WR")));
            rights.Add(new XElement("ServerName", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SERVERNAME_WR")));
            rights.Add(new XElement("IgnoreLastSession", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_IGNORELASTSESSION_WR")));
            rights.Add(new XElement("PauseGameWhenEmpty", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_PAUSEGAMEWHENEMPTY_WR")));
            rights.Add(new XElement("EnableSpectator", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLESPECTATOR_WR")));
            rights.Add(new XElement("RealisticSound", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_REALISTICSOUND_WR")));
            rights.Add(new XElement("AutoSaveInMinutes", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_AUTOSAVEINMINUTES_WR")));
            rights.Add(new XElement("InventorySizeMultiplier", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_INVENTORYSIZEMULTIPLIER_WR")));
            rights.Add(new XElement("AssemblerSpeedMultiplier", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ASSEMBLERSPEEDMULTIPLIER_WR")));
            rights.Add(new XElement("AssemblerEfficiencyMultiplier", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ASSEMBLEREFFICIENCYMULTIPLIER_WR")));
            rights.Add(new XElement("RefinerySpeedMultiplier", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_REFINERYSPEEDMULTIPLIER_WR")));
            rights.Add(new XElement("GameMode", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_GAMEMODE_WR")));
            rights.Add(new XElement("EnableCopyPaste", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLECOPYPASTE_WR")));
            rights.Add(new XElement("WelderSpeedMultiplier", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_WELDERSPEEDMULTIPLIER_WR")));
            rights.Add(new XElement("GrinderSpeedMultiplier", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_GRINDERSPEEDMULTIPLIER_WR")));
            rights.Add(new XElement("HackSpeedMultiplier", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_HACKSPEEDMULTIPLIER_WR")));
            rights.Add(new XElement("DestructibleBlocks", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_DESTRUCTIBLEBLOCKS_WR")));
            rights.Add(new XElement("MaxPlayers", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_MAXPLAYERS_WR")));
            rights.Add(new XElement("MaxFloatingObjects", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_MAXFLOATINGOBJECTS_WR")));
            rights.Add(new XElement("WorldName", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_WORLDNAME_WR")));
            rights.Add(new XElement("EnvironmentHostility", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENVIRONMENTHOSTILITY_WR")));
            rights.Add(new XElement("WorldSizeKm", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_WORLDSIZEKM_WR")));
            rights.Add(new XElement("PermanentDeath", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_PERMANENTDEATH_WR")));
            rights.Add(new XElement("CargoShipsEnabled", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_CARGOSHIPSENABLED_WR")));
            rights.Add(new XElement("RemoveTrash", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_REMOVETRASH_WR")));
            rights.Add(new XElement("ClientCanSave", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_CLIENTCANSAVE_WR")));
            rights.Add(new XElement("Mods", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_MODS_WR")));
            rights.Add(new XElement("ViewDistance", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_VIEWDISTANCE_WR")));
            rights.Add(new XElement("OnlineMode", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ONLINEMODE_WR")));
            rights.Add(new XElement("ResetOwnership", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_RESETOWNERSHIP_WR")));
            rights.Add(new XElement("GroupID", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_GROUPID_WR")));
            rights.Add(new XElement("Administrators", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ADMINISTRATORS_WR")));
            rights.Add(new XElement("Banned", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_BANNED_WR")));
            rights.Add(new XElement("AutoHealing", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_AUTOHEALING_WR")));
            rights.Add(new XElement("WeaponsEnabled", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_WEAPONSENABLED_WR")));
            rights.Add(new XElement("ShowPlayerNamesOnHud", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SHOWPLAYERNAMESONHUD_WR")));
            rights.Add(new XElement("ThrusterDamage", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_THRUSTERDAMAGE_WR")));
            rights.Add(new XElement("SpawnShipTimeMultiplier", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SPAWNSHIPTIMEMULTIPLIER_WR")));
            rights.Add(new XElement("RespawnShipDelete", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_RESPAWNSHIPDELETE_WR")));
            rights.Add(new XElement("EnableToolShake", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLETOOLSHAKE_WR")));
            rights.Add(new XElement("EnableIngameScripts", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEINGAMESCRIPTS_WR")));
            rights.Add(new XElement("VoxelGeneratorVersion", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_VOXELGENERATORVERSION_WR")));
            rights.Add(new XElement("EnableOxygen", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEOXYGEN_WR")));
            rights.Add(new XElement("Enable3rdPersonView", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLE3RDPERSONVIEW_WR")));
            rights.Add(new XElement("EnableEncounters", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEENCOUNTERS_WR")));

            rights.Add(new XElement("EnableFlora", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEFLORA_WR")));
            rights.Add(new XElement("EnableStationVoxelSupport", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLESTATIONVOXELSUPPORT_WR")));
            rights.Add(new XElement("EnableSunRotation", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLESUNROTATION_WR")));
            rights.Add(new XElement("DisableRespawnShips", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_DISABLERESPAWNSHIPS_WR")));
            rights.Add(new XElement("ScenarioEditMode", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SCENARIOEDITMODE_WR")));
            rights.Add(new XElement("Battle", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_BATTLE_WR")));
            rights.Add(new XElement("Scenario", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SCENARIO_WR")));
            rights.Add(new XElement("CanJoinRunning", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_CANJOINRUNNING_WR")));
            rights.Add(new XElement("PhysicsIterations", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_PHYSICSITERATIONS_WR")));
            rights.Add(new XElement("SunRotationIntervalMinutes", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SUNROTATIONINTERVALMINUTES_WR")));
            rights.Add(new XElement("EnableJetpack", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEJETPACK_WR")));
            rights.Add(new XElement("SpawnWithTools", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SPAWNWITHTOOLS_WR")));
            rights.Add(new XElement("StartInRespawnScreen", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_STARTINRESPAWNSCREEN_WR")));
            rights.Add(new XElement("EnableVoxelDestruction", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEVOXELDESTRUCTION_WR")));
            rights.Add(new XElement("MaxDrones", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_MAXDRONES_WR")));
            rights.Add(new XElement("EnableDrones", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEDRONES_WR")));
            rights.Add(new XElement("FloraDensity", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_FLORADENSITY_WR")));
            rights.Add(new XElement("EnableCyberhounds", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLECYBERHOUNDS_WR")));
            rights.Add(new XElement("EnableSpiders", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLESPIDERS_WR")));
            rights.Add(new XElement("FloraDensityMultiplier", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_FLORADENSITYMULTIPLIER_WR")));
            
            response.AddToContent(rights);

            return Content(response.ToString());
        }

        // POST: API/Server/SESetConfiguration
        [HttpPost]
        [APIServerAccess("SRV-SESC", "SERVER_CONFIG_SE_IP_WR", "SERVER_CONFIG_SE_STEAMPORT_WR", "SERVER_CONFIG_SE_SERVERPORT_WR",
                                     "SERVER_CONFIG_SE_SERVERNAME_WR", "SERVER_CONFIG_SE_IGNORELASTSESSION_WR", "SERVER_CONFIG_SE_PAUSEGAMEWHENEMPTY_WR",
                                     "SERVER_CONFIG_SE_ENABLESPECTATOR_WR", "SERVER_CONFIG_SE_REALISTICSOUND_WR", "SERVER_CONFIG_SE_AUTOSAVEINMINUTES_WR",
                                     "SERVER_CONFIG_SE_INVENTORYSIZEMULTIPLIER_WR", "SERVER_CONFIG_SE_ASSEMBLERSPEEDMULTIPLIER_WR", "SERVER_CONFIG_SE_ASSEMBLEREFFICIENCYMULTIPLIER_WR",
                                     "SERVER_CONFIG_SE_REFINERYSPEEDMULTIPLIER_WR", "SERVER_CONFIG_SE_GAMEMODE_WR", "SERVER_CONFIG_SE_ENABLECOPYPASTE_WR",
                                     "SERVER_CONFIG_SE_WELDERSPEEDMULTIPLIER_WR", "SERVER_CONFIG_SE_GRINDERSPEEDMULTIPLIER_WR", "SERVER_CONFIG_SE_HACKSPEEDMULTIPLIER_WR",
                                     "SERVER_CONFIG_SE_DESTRUCTIBLEBLOCKS_WR", "SERVER_CONFIG_SE_MAXPLAYERS_WR", "SERVER_CONFIG_SE_MAXFLOATINGOBJECTS_WR",
                                     "SERVER_CONFIG_SE_WORLDNAME_WR", "SERVER_CONFIG_SE_ENVIRONMENTHOSTILITY_WR", "SERVER_CONFIG_SE_WORLDSIZEKM_WR",
                                     "SERVER_CONFIG_SE_PERMANENTDEATH_WR", "SERVER_CONFIG_SE_CARGOSHIPSENABLED_WR", "SERVER_CONFIG_SE_REMOVETRASH_WR",
                                     "SERVER_CONFIG_SE_CLIENTCANSAVE_WR", "SERVER_CONFIG_SE_MODS_WR", "SERVER_CONFIG_SE_VIEWDISTANCE_WR",
                                     "SERVER_CONFIG_SE_ONLINEMODE_WR", "SERVER_CONFIG_SE_RESETOWNERSHIP_WR", "SERVER_CONFIG_SE_GROUPID_WR",
                                     "SERVER_CONFIG_SE_ADMINISTRATORS_WR", "SERVER_CONFIG_SE_BANNED_WR", "SERVER_CONFIG_SE_AUTOHEALING_WR",
                                     "SERVER_CONFIG_SE_WEAPONSENABLED_WR", "SERVER_CONFIG_SE_SHOWPLAYERNAMESONHUD_WR", "SERVER_CONFIG_SE_THRUSTERDAMAGE_WR",
                                     "SERVER_CONFIG_SE_SPAWNSHIPTIMEMULTIPLIER_WR", "SERVER_CONFIG_SE_RESPAWNSHIPDELETE_WR", "SERVER_CONFIG_SE_ENABLETOOLSHAKE_WR",
                                     "SERVER_CONFIG_SE_ENABLEINGAMESCRIPTS_WR", "SERVER_CONFIG_SE_VOXELGENERATORVERSION_WR", "SERVER_CONFIG_SE_ENABLEOXYGEN_WR",
                                     "SERVER_CONFIG_SE_ENABLE3RDPERSONVIEW_WR", "SERVER_CONFIG_SE_ENABLEENCOUNTERS_WR", "SERVER_CONFIG_SE_ENABLEFLORA_WR",
                                     "SERVER_CONFIG_SE_ENABLESTATIONVOXELSUPPORT_WR", "SERVER_CONFIG_SE_ENABLESUNROTATION_WR", "SERVER_CONFIG_SE_DISABLERESPAWNSHIPS_WR",
                                     "SERVER_CONFIG_SE_SCENARIOEDITMODE_WR", "SERVER_CONFIG_SE_BATTLE_WR", "SERVER_CONFIG_SE_SCENARIO_WR",
                                     "SERVER_CONFIG_SE_CANJOINRUNNING_WR", "SERVER_CONFIG_SE_PHYSICSITERATIONS_WR", "SERVER_CONFIG_SE_SUNROTATIONINTERVALMINUTES_WR",
                                     "SERVER_CONFIG_SE_ENABLEJETPACK_WR", "SERVER_CONFIG_SE_SPAWNWITHTOOLS_WR", "SERVER_CONFIG_SE_STARTINRESPAWNSCREEN_WR",
                                     "SERVER_CONFIG_SE_ENABLEVOXELDESTRUCTION_WR", "SERVER_CONFIG_SE_MAXDRONES_WR", "SERVER_CONFIG_SE_ENABLEDRONES_WR",
                                     "SERVER_CONFIG_SE_FLORADENSITY_WR", "SERVER_CONFIG_SE_ENABLECYBERHOUNDS_WR", "SERVER_CONFIG_SE_ENABLESPIDERS_WR",
                                     "SERVER_CONFIG_SE_FLORADENSITYMULTIPLIER_WR")]
        public ActionResult SESetConfiguration()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            // ** PARSING / ACCESS **

            // Loading the server config
            SEServerConfigHelper serverConfig = new SEServerConfigHelper();
            serverConfig.Load(RequestServer);

            // ==== IP ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_IP_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["IP"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISIP", "The IP field must be provided").ToString());
                if (!Regex.IsMatch(Request.Form["IP"], @"^((\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.){3}(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])$"))
                    return Content(XMLMessage.Error("SRV-SESC-BADIP", "The IP field is invalid").ToString());
                serverConfig.IP = Request.Form["IP"];
            }

            // ==== SteamPort ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_STEAMPORT_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["SteamPort"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISSTMPRT", "The SteamPort field must be provided").ToString());
                if (!int.TryParse(Request.Form["SteamPort"], out serverConfig.SteamPort) || serverConfig.SteamPort < 1 || serverConfig.SteamPort > 65535)
                    return Content(XMLMessage.Error("SRV-SESC-BADSTMPRT", "The SteamPort field is invalid").ToString());
            }

            // ==== ServerPort ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SERVERPORT_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ServerPort"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISSRVPRT", "The ServerPort field must be provided").ToString());
                if (!int.TryParse(Request.Form["ServerPort"], out serverConfig.ServerPort) || serverConfig.ServerPort < 1 || serverConfig.ServerPort > 65535)
                    return Content(XMLMessage.Error("SRV-SESC-BADSRVPRT", "The ServerPort field is invalid").ToString());
                if (!srvPrv.IsPortAvailable(serverConfig.ServerPort, RequestServer))
                    return Content(XMLMessage.Error("SRV-SESC-EXSRVPRT", "The ServerPort is already in use").ToString());

            }

            // ==== ServerName ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SERVERNAME_WR"))
            {
                if (!string.IsNullOrWhiteSpace(Request.Form["ServerName"]))
                    serverConfig.ServerName = Request.Form["ServerName"];
            }

            // ==== IgnoreLastSession ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_IGNORELASTSESSION_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["IgnoreLastSession"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISILS", "The IgnoreLastSession field must be provided").ToString());
                if (!bool.TryParse(Request.Form["IgnoreLastSession"], out serverConfig.IgnoreLastSession))
                    return Content(XMLMessage.Error("SRV-SESC-BADILS", "The IgnoreLastSession field is invalid").ToString());
            }

            // ==== PauseGameWhenEmpty ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_PAUSEGAMEWHENEMPTY_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["PauseGameWhenEmpty"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISPGWE", "The PauseGameWhenEmpty field must be provided").ToString());
                if (!bool.TryParse(Request.Form["PauseGameWhenEmpty"], out serverConfig.PauseGameWhenEmpty))
                    return Content(XMLMessage.Error("SRV-SESC-BADPGWE", "The PauseGameWhenEmpty field is invalid").ToString());
            }

            // ==== EnableSpectator ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLESPECTATOR_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableSpectator"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISES", "The EnableSpectator field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableSpectator"], out serverConfig.EnableSpectator))
                    return Content(XMLMessage.Error("SRV-SESC-BADES", "The EnableSpectator field is invalid").ToString());
            }

            // ==== RealisticSound ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_REALISTICSOUND_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["RealisticSound"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISRS", "The RealisticSound field must be provided").ToString());
                if (!bool.TryParse(Request.Form["RealisticSound"], out serverConfig.RealisticSound))
                    return Content(XMLMessage.Error("SRV-SESC-BADRS", "The RealisticSound field is invalid").ToString());
            }

            // ==== AutoSaveInMinutes ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_AUTOSAVEINMINUTES_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["AutoSaveInMinutes"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISASIM", "The AutoSaveInMinutes field must be provided").ToString());
                if (!uint.TryParse(Request.Form["AutoSaveInMinutes"], out serverConfig.AutoSaveInMinutes))
                    return Content(XMLMessage.Error("SRV-SESC-BADASIM", "The AutoSaveInMinutes field is invalid").ToString());
            }

            // ==== InventorySizeMultiplier ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_INVENTORYSIZEMULTIPLIER_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["InventorySizeMultiplier"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISISM", "The InventorySizeMultiplier field must be provided").ToString());
                if (!float.TryParse(Request.Form["InventorySizeMultiplier"], out serverConfig.InventorySizeMultiplier) || serverConfig.InventorySizeMultiplier < 1)
                    return Content(XMLMessage.Error("SRV-SESC-BADISM", "The InventorySizeMultiplier field is invalid").ToString());
            }

            // ==== AssemblerSpeedMultiplier ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ASSEMBLERSPEEDMULTIPLIER_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["AssemblerSpeedMultiplier"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISASM", "The AssemblerSpeedMultiplier field must be provided").ToString());
                if (!float.TryParse(Request.Form["AssemblerSpeedMultiplier"], out serverConfig.AssemblerSpeedMultiplier) || serverConfig.AssemblerSpeedMultiplier < 1)
                    return Content(XMLMessage.Error("SRV-SESC-BADASM", "The AssemblerSpeedMultiplier field is invalid").ToString());
            }

            // ==== AssemblerEfficiencyMultiplier ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ASSEMBLEREFFICIENCYMULTIPLIER_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["AssemblerEfficiencyMultiplier"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISAEM", "The AssemblerEfficiencyMultiplier field must be provided").ToString());
                if (!float.TryParse(Request.Form["AssemblerEfficiencyMultiplier"], out serverConfig.AssemblerEfficiencyMultiplier) || serverConfig.AssemblerEfficiencyMultiplier < 1)
                    return Content(XMLMessage.Error("SRV-SESC-BADAEM", "The AssemblerEfficiencyMultiplier field is invalid").ToString());
            }

            // ==== RefinerySpeedMultiplier ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_REFINERYSPEEDMULTIPLIER_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["RefinerySpeedMultiplier"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISRSM", "The RefinerySpeedMultiplier field must be provided").ToString());
                if (!float.TryParse(Request.Form["RefinerySpeedMultiplier"], out serverConfig.RefinerySpeedMultiplier) || serverConfig.RefinerySpeedMultiplier < 1)
                    return Content(XMLMessage.Error("SRV-SESC-BADRSM", "The RefinerySpeedMultiplier field is invalid").ToString());
            }

            // ==== GameMode ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_GAMEMODE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["GameMode"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISGM", "The GameMode field must be provided").ToString());
                if (!Enum.TryParse(Request.Form["GameMode"], out serverConfig.GameMode))
                    return Content(XMLMessage.Error("SRV-SESC-BADGM", "The GameMode field is invalid").ToString());
            }

            // ==== EnableCopyPaste ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLECOPYPASTE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableCopyPaste"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISECP", "The EnableCopyPaste field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableCopyPaste"], out serverConfig.EnableCopyPaste))
                    return Content(XMLMessage.Error("SRV-SESC-BADECP", "The EnableCopyPaste field is invalid").ToString());
            }

            // ==== WelderSpeedMultiplier ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_WELDERSPEEDMULTIPLIER_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["WelderSpeedMultiplier"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISWSM", "The WelderSpeedMultiplier field must be provided").ToString());
                if (!float.TryParse(Request.Form["WelderSpeedMultiplier"], out serverConfig.WelderSpeedMultiplier) || serverConfig.WelderSpeedMultiplier < 0)
                    return Content(XMLMessage.Error("SRV-SESC-BADWSM", "The WelderSpeedMultiplier field is invalid").ToString());
            }

            // ==== GrinderSpeedMultiplier ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_GRINDERSPEEDMULTIPLIER_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["GrinderSpeedMultiplier"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISGSM", "The GrinderSpeedMultiplier field must be provided").ToString());
                if (!float.TryParse(Request.Form["GrinderSpeedMultiplier"], out serverConfig.GrinderSpeedMultiplier) || serverConfig.GrinderSpeedMultiplier < 0)
                    return Content(XMLMessage.Error("SRV-SESC-BADGSM", "The GrinderSpeedMultiplier field is invalid").ToString());
            }

            // ==== HackSpeedMultiplier ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_HACKSPEEDMULTIPLIER_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["HackSpeedMultiplier"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISHSM", "The HackSpeedMultiplier field must be provided").ToString());
                if (!float.TryParse(Request.Form["HackSpeedMultiplier"], out serverConfig.HackSpeedMultiplier) || serverConfig.HackSpeedMultiplier < 0)
                    return Content(XMLMessage.Error("SRV-SESC-BADHSM", "The HackSpeedMultiplier field is invalid").ToString());
            }

            // ==== DestructibleBlocks ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_DESTRUCTIBLEBLOCKS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["DestructibleBlocks"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISDB", "The DestructibleBlocks field must be provided").ToString());
                if (!bool.TryParse(Request.Form["DestructibleBlocks"], out serverConfig.DestructibleBlocks))
                    return Content(XMLMessage.Error("SRV-SESC-BADDB", "The DestructibleBlocks field is invalid").ToString());
            }

            // ==== MaxPlayers ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_MAXPLAYERS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["MaxPlayers"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISMAXPL", "The MaxPlayers field must be provided").ToString());
                if (!int.TryParse(Request.Form["MaxPlayers"], out serverConfig.MaxPlayers) || serverConfig.MaxPlayers < 1)
                    return Content(XMLMessage.Error("SRV-SESC-BADMAXPL", "The MaxPlayers field is invalid").ToString());
            }

            // ==== MaxFloatingObjects ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_MAXFLOATINGOBJECTS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["MaxFloatingObjects"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISMAXFO", "The MaxFloatingObjects field must be provided").ToString());
                if (!int.TryParse(Request.Form["MaxFloatingObjects"], out serverConfig.MaxFloatingObjects) || serverConfig.MaxFloatingObjects < 1)
                    return Content(XMLMessage.Error("SRV-SESC-BADMAXFO", "The MaxFloatingObjects field is invalid").ToString());
            }

            // ==== WorldName ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_WORLDNAME_WR"))
            {
                if (!string.IsNullOrWhiteSpace(Request.Form["WorldName"]))
                    serverConfig.WorldName = Request.Form["WorldName"];
            }

            // ==== EnvironmentHostility ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENVIRONMENTHOSTILITY_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnvironmentHostility"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISEH", "The EnvironmentHostility field must be provided").ToString());
                if (!Enum.TryParse(Request.Form["EnvironmentHostility"], out serverConfig.EnvironmentHostility))
                    return Content(XMLMessage.Error("SRV-SESC-BADEH", "The EnvironmentHostility field is invalid").ToString());
            }

            // ==== WorldSizeKm ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_WORLDSIZEKM_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["WorldSizeKm"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISWSK", "The WorldSizeKm field must be provided").ToString());
                if (!int.TryParse(Request.Form["WorldSizeKm"], out serverConfig.WorldSizeKm) || serverConfig.WorldSizeKm < 0)
                    return Content(XMLMessage.Error("SRV-SESC-BADWSK", "The WorldSizeKm field is invalid").ToString());
            }

            // ==== PermanentDeath ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_PERMANENTDEATH_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["PermanentDeath"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISPD", "The PermanentDeath field must be provided").ToString());
                if (!bool.TryParse(Request.Form["PermanentDeath"], out serverConfig.PermanentDeath))
                    return Content(XMLMessage.Error("SRV-SESC-BADPD", "The PermanentDeath field is invalid").ToString());
            }

            // ==== CargoShipsEnabled ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_CARGOSHIPSENABLED_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["CargoShipsEnabled"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISCSE", "The CargoShipsEnabled field must be provided").ToString());
                if (!bool.TryParse(Request.Form["CargoShipsEnabled"], out serverConfig.CargoShipsEnabled))
                    return Content(XMLMessage.Error("SRV-SESC-BADCSE", "The CargoShipsEnabled field is invalid").ToString());
            }

            // ==== RemoveTrash ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_REMOVETRASH_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["RemoveTrash"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISRT", "The RemoveTrash field must be provided").ToString());
                if (!bool.TryParse(Request.Form["RemoveTrash"], out serverConfig.RemoveTrash))
                    return Content(XMLMessage.Error("SRV-SESC-BADRT", "The RemoveTrash field is invalid").ToString());
            }

            // ==== ClientCanSave ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_CLIENTCANSAVE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ClientCanSave"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISCCS", "The ClientCanSave field must be provided").ToString());
                if (!bool.TryParse(Request.Form["ClientCanSave"], out serverConfig.ClientCanSave))
                    return Content(XMLMessage.Error("SRV-SESC-BADCCS", "The ClientCanSave field is invalid").ToString());
            }

            // ==== Mods ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_MODS_WR"))
            {
                serverConfig.Mods.Clear();
                foreach (string mod in Request.Form["Mods"].Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(mod))
                    {
                        ulong modParsed;
                        if (!ulong.TryParse(mod, out modParsed))
                            return Content(XMLMessage.Error("SRV-SESC-BADMOD", "The Mods field is invalid").ToString());
                        serverConfig.Mods.Add(modParsed);
                    }
                }
            }

            // ==== ViewDistance ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_VIEWDISTANCE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ViewDistance"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISVD", "The ViewDistance field must be provided").ToString());
                if (!int.TryParse(Request.Form["ViewDistance"], out serverConfig.ViewDistance) ||
                    serverConfig.ViewDistance < 1)
                    return Content(XMLMessage.Error("SRV-SESC-BADVD", "The ViewDistance field is invalid").ToString());
            }

            // ==== OnlineMode ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ONLINEMODE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["OnlineMode"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISOM", "The OnlineMode field must be provided").ToString());
                if (!Enum.TryParse(Request.Form["OnlineMode"], out serverConfig.OnlineMode))
                    return Content(XMLMessage.Error("SRV-SESC-BADOM", "The OnlineMode field is invalid").ToString());
            }

            // ==== ResetOwnership ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_RESETOWNERSHIP_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ResetOwnership"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISRO", "The ResetOwnership field must be provided").ToString());
                if (!bool.TryParse(Request.Form["ResetOwnership"], out serverConfig.ResetOwnership))
                    return Content(XMLMessage.Error("SRV-SESC-BADRO", "The ResetOwnership field is invalid").ToString());
            }

            // ==== GroupID ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_GROUPID_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["GroupID"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISGRID", "The GroupID field must be provided").ToString());
                if (!ulong.TryParse(Request.Form["GroupID"], out serverConfig.GroupID))
                    return Content(XMLMessage.Error("SRV-SESC-BADGRID", "The GroupID field is invalid").ToString());
            }

            // ==== Administrators ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ADMINISTRATORS_WR"))
            {
                serverConfig.Administrators.Clear();
                foreach (string adm in Request.Form["Administrators"].Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(adm))
                    {
                        ulong admParsed;
                        if (!ulong.TryParse(adm, out admParsed))
                            return Content(XMLMessage.Error("SRV-SESC-BADADM", "The Administrators field is invalid").ToString());
                        serverConfig.Administrators.Add(admParsed);
                    }
                }
            }

            // ==== Banned ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_BANNED_WR"))
            {
                serverConfig.Banned.Clear();
                foreach (string ban in Request.Form["Banned"].Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(ban))
                    {
                        ulong banParsed;
                        if (!ulong.TryParse(ban, out banParsed))
                            return Content(XMLMessage.Error("SRV-SESC-BADBAN", "The Banned field is invalid").ToString());
                        serverConfig.Banned.Add(banParsed);
                    }
                }
            }

            // ==== AutoHealing ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_AUTOHEALING_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["AutoHealing"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISAH", "The AutoHealing field must be provided").ToString());
                if (!bool.TryParse(Request.Form["AutoHealing"], out serverConfig.AutoHealing))
                    return Content(XMLMessage.Error("SRV-SESC-BADAH", "The AutoHealing field is invalid").ToString());
            }

            // ==== WeaponsEnabled ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_WEAPONSENABLED_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["WeaponsEnabled"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISWE", "The WeaponsEnabled field must be provided").ToString());
                if (!bool.TryParse(Request.Form["WeaponsEnabled"], out serverConfig.WeaponsEnabled))
                    return Content(XMLMessage.Error("SRV-SESC-BADWE", "The WeaponsEnabled field is invalid").ToString());
            }

            // ==== ShowPlayerNamesOnHud ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SHOWPLAYERNAMESONHUD_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ShowPlayerNamesOnHud"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISSPNOH", "The ShowPlayerNamesOnHud field must be provided").ToString());
                if (!bool.TryParse(Request.Form["ShowPlayerNamesOnHud"], out serverConfig.ShowPlayerNamesOnHud))
                    return Content(XMLMessage.Error("SRV-SESC-BADSPNOH", "The ShowPlayerNamesOnHud field is invalid").ToString());
            }

            // ==== ThrusterDamage ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_THRUSTERDAMAGE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ThrusterDamage"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISTD", "The ThrusterDamage field must be provided").ToString());
                if (!bool.TryParse(Request.Form["ThrusterDamage"], out serverConfig.ThrusterDamage))
                    return Content(XMLMessage.Error("SRV-SESC-BADTD", "The ThrusterDamage field is invalid").ToString());
            }

            // ==== SpawnShipTimeMultiplier ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SPAWNSHIPTIMEMULTIPLIER_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["SpawnShipTimeMultiplier"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISSSTM", "The SpawnShipTimeMultiplier field must be provided").ToString());
                if (!float.TryParse(Request.Form["SpawnShipTimeMultiplier"], out serverConfig.SpawnShipTimeMultiplier) || serverConfig.SpawnShipTimeMultiplier < 0)
                    return Content(XMLMessage.Error("SRV-SESC-BADSSTM", "The SpawnShipTimeMultiplier field is invalid").ToString());
            }

            // ==== RespawnShipDelete ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_RESPAWNSHIPDELETE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["RespawnShipDelete"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISRSD", "The RespawnShipDelete field must be provided").ToString());
                if (!bool.TryParse(Request.Form["RespawnShipDelete"], out serverConfig.RespawnShipDelete))
                    return Content(XMLMessage.Error("SRV-SESC-BADRSD", "The RespawnShipDelete field is invalid").ToString());
            }

            // ==== EnableToolShake ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLETOOLSHAKE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableToolShake"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISETS", "The EnableToolShake field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableToolShake"], out serverConfig.EnableToolShake))
                    return Content(XMLMessage.Error("SRV-SESC-BADETS", "The EnableToolShake field is invalid").ToString());
            }

            // ==== EnableIngameScripts ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEINGAMESCRIPTS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableIngameScripts"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISETS", "The EnableIngameScripts field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableIngameScripts"], out serverConfig.EnableIngameScripts))
                    return Content(XMLMessage.Error("SRV-SESC-BADETS", "The EnableIngameScripts field is invalid").ToString());
            }

            // ==== VoxelGeneratorVersion ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_VOXELGENERATORVERSION_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["VoxelGeneratorVersion"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISVGV", "The VoxelGeneratorVersion field must be provided").ToString());
                if (!int.TryParse(Request.Form["VoxelGeneratorVersion"], out serverConfig.VoxelGeneratorVersion) ||
                    serverConfig.VoxelGeneratorVersion < 0 || serverConfig.VoxelGeneratorVersion > 2)
                    return Content(XMLMessage.Error("SRV-SESC-BADVGV", "The VoxelGeneratorVersion field is invalid").ToString());
            }

            // ==== EnableOxygen ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEOXYGEN_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableOxygen"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISEO", "The EnableOxygen field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableOxygen"], out serverConfig.EnableOxygen))
                    return Content(XMLMessage.Error("SRV-SESC-BADEO", "The EnableOxygen field is invalid").ToString());
            }

            // ==== Enable3rdPersonView ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLE3RDPERSONVIEW_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["Enable3rdPersonView"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISE3PV", "The Enable3rdPersonView field must be provided").ToString());
                if (!bool.TryParse(Request.Form["Enable3rdPersonView"], out serverConfig.Enable3rdPersonView))
                    return Content(XMLMessage.Error("SRV-SESC-BADE3PV", "The Enable3rdPersonView field is invalid").ToString());
            }

            // ==== EnableEncounters ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEENCOUNTERS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableEncounters"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISEE", "The EnableEncounters field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableEncounters"], out serverConfig.EnableEncounters))
                    return Content(XMLMessage.Error("SRV-SESC-BADEE", "The EnableEncounters field is invalid").ToString());
            }

            // ==== EnableFlora ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEFLORA_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableFlora"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISEF", "The EnableFlora field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableFlora"], out serverConfig.EnableFlora))
                    return Content(XMLMessage.Error("SRV-SESC-BADEF", "The EnableFlora field is invalid").ToString());
            }

            // ==== EnableStationVoxelSupport ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLESTATIONVOXELSUPPORT_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableStationVoxelSupport"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISESVS", "The EnableStationVoxelSupport field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableStationVoxelSupport"], out serverConfig.EnableStationVoxelSupport))
                    return Content(XMLMessage.Error("SRV-SESC-BADESVS", "The EnableStationVoxelSupport field is invalid").ToString());
            }

            // ==== EnableSunRotation ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLESUNROTATION_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableSunRotation"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISESR", "The EnableSunRotation field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableSunRotation"], out serverConfig.EnableSunRotation))
                    return Content(XMLMessage.Error("SRV-SESC-BADESR", "The EnableSunRotation field is invalid").ToString());
            }

            // ==== DisableRespawnShips ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_DISABLERESPAWNSHIPS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["DisableRespawnShips"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISDRS", "The DisableRespawnShips field must be provided").ToString());
                if (!bool.TryParse(Request.Form["DisableRespawnShips"], out serverConfig.DisableRespawnShips))
                    return Content(XMLMessage.Error("SRV-SESC-BADDRS", "The DisableRespawnShips field is invalid").ToString());
            }

            // ==== ScenarioEditMode ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SCENARIOEDITMODE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ScenarioEditMode"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISSEM", "The ScenarioEditMode field must be provided").ToString());
                if (!bool.TryParse(Request.Form["ScenarioEditMode"], out serverConfig.ScenarioEditMode))
                    return Content(XMLMessage.Error("SRV-SESC-BADSEM", "The ScenarioEditMode field is invalid").ToString());
            }

            // ==== Battle ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_BATTLE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["Battle"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISB", "The Battle field must be provided").ToString());
                if (!bool.TryParse(Request.Form["Battle"], out serverConfig.Battle))
                    return Content(XMLMessage.Error("SRV-SESC-BADB", "The Battle field is invalid").ToString());
            }

            // ==== Scenario ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SCENARIO_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["Scenario"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISS", "The Scenario field must be provided").ToString());
                if (!bool.TryParse(Request.Form["Scenario"], out serverConfig.Scenario))
                    return Content(XMLMessage.Error("SRV-SESC-BADS", "The Scenario field is invalid").ToString());
            }

            // ==== CanJoinRunning ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_CANJOINRUNNING_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["CanJoinRunning"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISCJR", "The CanJoinRunning field must be provided").ToString());
                if (!bool.TryParse(Request.Form["CanJoinRunning"], out serverConfig.CanJoinRunning))
                    return Content(XMLMessage.Error("SRV-SESC-BADCJR", "The CanJoinRunning field is invalid").ToString());
            }

            // ==== PhysicsIterations ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_PHYSICSITERATIONS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["PhysicsIterations"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISPI", "The PhysicsIterations field must be provided").ToString());
                if (!int.TryParse(Request.Form["PhysicsIterations"], out serverConfig.PhysicsIterations) || serverConfig.PhysicsIterations < 1)
                    return Content(XMLMessage.Error("SRV-SESC-BADPI", "The PhysicsIterations field is invalid").ToString());
            }

            // ==== SunRotationIntervalMinutes ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SUNROTATIONINTERVALMINUTES_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["SunRotationIntervalMinutes"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISSRIM", "The SunRotationIntervalMinutes field must be provided").ToString());
                if (!float.TryParse(Request.Form["SunRotationIntervalMinutes"], out serverConfig.SunRotationIntervalMinutes) || serverConfig.SunRotationIntervalMinutes < 0)
                    return Content(XMLMessage.Error("SRV-SESC-BADSRIM", "The SunRotationIntervalMinutes field is invalid").ToString());
            }

            // ==== EnableJetpack ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEJETPACK_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableJetpack"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISEJ", "The EnableJetpack field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableJetpack"], out serverConfig.EnableJetpack))
                    return Content(XMLMessage.Error("SRV-SESC-BADEJ", "The EnableJetpack field is invalid").ToString());
            }

            // ==== SpawnWithTools ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_SPAWNWITHTOOLS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["SpawnWithTools"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISSWT", "The SpawnWithTools field must be provided").ToString());
                if (!bool.TryParse(Request.Form["SpawnWithTools"], out serverConfig.SpawnWithTools))
                    return Content(XMLMessage.Error("SRV-SESC-BADSWT", "The SpawnWithTools field is invalid").ToString());
            }

            // ==== StartInRespawnScreen ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_STARTINRESPAWNSCREEN_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["StartInRespawnScreen"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISSIRS", "The StartInRespawnScreen field must be provided").ToString());
                if (!bool.TryParse(Request.Form["StartInRespawnScreen"], out serverConfig.StartInRespawnScreen))
                    return Content(XMLMessage.Error("SRV-SESC-BADSIRS", "The StartInRespawnScreen field is invalid").ToString());
            }

            // ==== EnableVoxelDestruction ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEVOXELDESTRUCTION_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableVoxelDestruction"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISEVD", "The EnableVoxelDestruction field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableVoxelDestruction"], out serverConfig.EnableVoxelDestruction))
                    return Content(XMLMessage.Error("SRV-SESC-BADEVD", "The EnableVoxelDestruction field is invalid").ToString());
            }

            // ==== MaxDrones ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_MAXDRONES_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["MaxDrones"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISMD", "The MaxDrones field must be provided").ToString());
                if (!int.TryParse(Request.Form["MaxDrones"], out serverConfig.MaxDrones) || serverConfig.MaxDrones < 0)
                    return Content(XMLMessage.Error("SRV-SESC-BADMD", "The MaxDrones field is invalid").ToString());
            }

            // ==== EnableDrones ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLEDRONES_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableDrones"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISED", "The EnableDrones field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableDrones"], out serverConfig.EnableDrones))
                    return Content(XMLMessage.Error("SRV-SESC-BADED", "The EnableDrones field is invalid").ToString());
            }

            // ==== FloraDensity ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_FLORADENSITY_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["FloraDensity"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISFD", "The FloraDensity field must be provided").ToString());
                if (!int.TryParse(Request.Form["FloraDensity"], out serverConfig.FloraDensity) || serverConfig.FloraDensity < 0)
                    return Content(XMLMessage.Error("SRV-SESC-BADFD", "The FloraDensity field is invalid").ToString());
            }

            // ==== EnableCyberhounds ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLECYBERHOUNDS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableCyberhounds"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISECH", "The EnableCyberhounds field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableCyberhounds"], out serverConfig.EnableCyberhounds))
                    return Content(XMLMessage.Error("SRV-SESC-BADECH", "The EnableCyberhounds field is invalid").ToString());
            }

            // ==== EnableSpiders ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_ENABLESPIDERS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableSpiders"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISECH", "The EnableSpiders field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableSpiders"], out serverConfig.EnableSpiders))
                    return Content(XMLMessage.Error("SRV-SESC-BADECH", "The EnableSpiders field is invalid").ToString());
            }

            // ==== FloraDensityMultiplier ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_FLORADENSITYMULTIPLIER_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["FloraDensityMultiplier"]))
                    return Content(XMLMessage.Error("SRV-SESC-MISFDM", "The FloraDensityMultiplier field must be provided").ToString());
                if (!float.TryParse(Request.Form["FloraDensityMultiplier"], out serverConfig.FloraDensityMultiplier) || serverConfig.FloraDensityMultiplier < 0)
                    return Content(XMLMessage.Error("SRV-SESC-BADFDM", "The FloraDensityMultiplier field is invalid").ToString());
            }

            // ** PROCESS **
            RequestServer.Ip = serverConfig.IP;
            RequestServer.Port = serverConfig.ServerPort;
            RequestServer.AutoSaveInMinutes = Convert.ToInt32(serverConfig.AutoSaveInMinutes);

            srvPrv.UpdateServer(RequestServer);

            bool restartRequired = false;
            if (srvPrv.GetState(RequestServer) != ServiceState.Stopped)
            {
                restartRequired = true;
                ServiceHelper.StopServiceAndWait(RequestServer);
            }

            if (RequestServer.UseServerExtender)
                serverConfig.AutoSaveInMinutes = 0;

            serverConfig.Save(RequestServer);

            if (restartRequired)
            {
                ServiceHelper.StartService(RequestServer);
                return Content(XMLMessage.Success("SRV-SESC-OK", "The server configuration has been updated, the server is restarting ...").ToString());
            }

            return Content(XMLMessage.Success("SRV-SESC-OK", "The server configuration has been updated").ToString());
        }

        #endregion

        #region ME Server Configuration

        // POST: API/Server/MEGetConfiguration
        [HttpPost]
        [APIServerAccess("SRV-MEGC", "SERVER_CONFIG_ME_RD")]
        public ActionResult MEGetConfiguration()
        {
            // ** PROCESS **
            // Loading the server config
            MEServerConfigHelper serverConfig = new MEServerConfigHelper();
            serverConfig.Load(RequestServer);

            XMLMessage response = new XMLMessage("SRV-MEGC-OK");

            XElement values = new XElement("Values");
            response.AddToContent(new XElement("IP", serverConfig.IP));
            response.AddToContent(new XElement("SteamPort", serverConfig.SteamPort));
            response.AddToContent(new XElement("ServerPort", serverConfig.ServerPort));
            response.AddToContent(new XElement("ServerName", serverConfig.ServerName));
            response.AddToContent(new XElement("IgnoreLastSession", serverConfig.IgnoreLastSession));
            response.AddToContent(new XElement("PauseGameWhenEmpty", serverConfig.PauseGameWhenEmpty));
            response.AddToContent(new XElement("EnableSpectator", serverConfig.EnableSpectator));
            response.AddToContent(new XElement("AutoSaveInMinutes", RequestServer.AutoSaveInMinutes));
            response.AddToContent(new XElement("GameMode", serverConfig.GameMode));
            response.AddToContent(new XElement("EnableCopyPaste", serverConfig.EnableCopyPaste));
            response.AddToContent(new XElement("MaxPlayers", serverConfig.MaxPlayers));
            response.AddToContent(new XElement("WorldName", serverConfig.WorldName));

            XElement mods = new XElement("Mods");
            foreach (ulong mod in serverConfig.Mods)
                mods.Add(new XElement("Mod", mod));
            response.AddToContent(mods);

            response.AddToContent(new XElement("OnlineMode", serverConfig.OnlineMode));
            response.AddToContent(new XElement("GroupID", serverConfig.GroupID));

            XElement administrators = new XElement("Administrators");
            foreach (ulong adminitrator in serverConfig.Administrators)
                administrators.Add(new XElement("Adminitrator", adminitrator));
            response.AddToContent(administrators);

            XElement banned = new XElement("Banned");
            foreach (ulong ban in serverConfig.Banned)
                banned.Add(new XElement("Ban", ban));
            response.AddToContent(banned);

            response.AddToContent(new XElement("ClientCanSave", serverConfig.ClientCanSave));
            response.AddToContent(new XElement("EnableStructuralSimulation", serverConfig.EnableStructuralSimulation));
            response.AddToContent(new XElement("MaxActiveFracturePieces", serverConfig.MaxActiveFracturePieces));
            response.AddToContent(new XElement("EnableBarbarians", serverConfig.EnableBarbarians));
            response.AddToContent(new XElement("MaximumBots", serverConfig.MaximumBots));
            response.AddToContent(new XElement("GameDayInRealMinutes", serverConfig.GameDayInRealMinutes));
            response.AddToContent(new XElement("DayNightRatio", serverConfig.DayNightRatio));
            response.AddToContent(new XElement("EnableAnimals", serverConfig.EnableAnimals));
            response.AddToContent(values);

            XElement rights = new XElement("Rights");
            response.AddToContent(new XElement("IP", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_IP_WR")));
            response.AddToContent(new XElement("SteamPort", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_STEAMPORT_WR")));
            response.AddToContent(new XElement("ServerPort", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_SERVERPORT_WR")));
            response.AddToContent(new XElement("ServerName", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_SERVERNAME_WR")));
            response.AddToContent(new XElement("IgnoreLastSession", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_IGNORELASTSESSION_WR")));
            response.AddToContent(new XElement("PauseGameWhenEmpty", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_PAUSEGAMEWHENEMPTY_WR")));
            response.AddToContent(new XElement("EnableSpectator", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLESPECTATOR_WR")));
            response.AddToContent(new XElement("AutoSaveInMinutes", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_AUTOSAVEINMINUTES_WR")));
            response.AddToContent(new XElement("GameMode", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_GAMEMODE_WR")));
            response.AddToContent(new XElement("EnableCopyPaste", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLECOPYPASTE_WR")));
            response.AddToContent(new XElement("MaxPlayers", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_MAXPLAYERS_WR")));
            response.AddToContent(new XElement("WorldName", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_WORLDNAME_WR")));
            response.AddToContent(new XElement("Mods", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_MODS_WR")));
            response.AddToContent(new XElement("OnlineMode", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ONLINEMODE_WR")));
            response.AddToContent(new XElement("GroupID", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_GROUPID_WR")));
            response.AddToContent(new XElement("Administrators", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ADMINISTRATORS_WR")));
            response.AddToContent(new XElement("Banned", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_BANNED_WR")));
            response.AddToContent(new XElement("ClientCanSave", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_CLIENTCANSAVE_WR")));
            response.AddToContent(new XElement("EnableStructuralSimulation", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLESTRUCTURALSIMULATION_WR")));
            response.AddToContent(new XElement("MaxActiveFracturePieces", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_MAXACTIVEFRACTUREPIECES_WR")));
            response.AddToContent(new XElement("EnableBarbarians", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLEBARBARIANS_WR")));
            response.AddToContent(new XElement("MaximumBots", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_MAXIMUMBOTS_WR")));
            response.AddToContent(new XElement("GameDayInRealMinutes", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_GAMEDAYINREALMINUTES_WR")));
            response.AddToContent(new XElement("DayNightRatio", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_DAYNIGHTRATIO_WR")));
            response.AddToContent(new XElement("EnableAnimals", AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLEANIMALS_WR")));
            response.AddToContent(rights);

            return Content(response.ToString());
        }

        // POST: API/Server/MESetConfiguration
        [HttpPost]
        [APIServerAccess("SRV-MESC", "SERVER_CONFIG_ME_IP_WR", "SERVER_CONFIG_ME_STEAMPORT_WR", "SERVER_CONFIG_ME_SERVERPORT_WR",
                                     "SERVER_CONFIG_ME_SERVERNAME_WR", "SERVER_CONFIG_ME_IGNORELASTSESSION_WR", "SERVER_CONFIG_ME_PAUSEGAMEWHENEMPTY_WR",
                                     "SERVER_CONFIG_ME_ENABLESPECTATOR_WR", "SERVER_CONFIG_ME_AUTOSAVEINMINUTES_WR", "SERVER_CONFIG_ME_GAMEMODE_WR",
                                     "SERVER_CONFIG_ME_ENABLECOPYPASTE_WR", "SERVER_CONFIG_ME_MAXPLAYERS_WR", "SERVER_CONFIG_ME_WORLDNAME_WR",
                                     "SERVER_CONFIG_ME_MODS_WR", "SERVER_CONFIG_ME_ONLINEMODE_WR", "SERVER_CONFIG_ME_GROUPID_WR",
                                     "SERVER_CONFIG_ME_ADMINISTRATORS_WR", "SERVER_CONFIG_ME_BANNED_WR", "SERVER_CONFIG_ME_CLIENTCANSAVE_WR",
                                     "SERVER_CONFIG_ME_ENABLESTRUCTURALSIMULATION_WR", "SERVER_CONFIG_ME_MAXACTIVEFRACTUREPIECES_WR", "SERVER_CONFIG_ME_ENABLEBARBARIANS_WR",
                                     "SERVER_CONFIG_ME_MAXIMUMBOTS_WR", "SERVER_CONFIG_ME_GAMEDAYINREALMINUTES_WR", "SERVER_CONFIG_ME_DAYNIGHTRATIO_WR",
                                     "SERVER_CONFIG_ME_ENABLEANIMALS_WR")]
        public ActionResult MESetConfiguration()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);


            // Loading the server config
            MEServerConfigHelper serverConfig = new MEServerConfigHelper();
            serverConfig.Load(RequestServer);


            // ==== IP ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_SE_IP_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["IP"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISIP", "The IP field must be provided").ToString());
                if (!Regex.IsMatch(Request.Form["IP"], @"^((\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.){3}(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])$"))
                    return Content(XMLMessage.Error("SRV-MESC-BADIP", "The IP field is invalid").ToString());
                serverConfig.IP = Request.Form["IP"];
            }

            // ==== SteamPort ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_STEAMPORT_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["SteamPort"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISSTMPRT", "The SteamPort field must be provided").ToString());
                if (!int.TryParse(Request.Form["SteamPort"], out serverConfig.SteamPort) || serverConfig.SteamPort < 1 || serverConfig.SteamPort > 65535)
                    return Content(XMLMessage.Error("SRV-MESC-BADSTMPRT", "The SteamPort field is invalid").ToString());
            }

            // ==== ServerPort ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_SERVERPORT_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ServerPort"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISSRVPRT", "The ServerPort field must be provided").ToString());
                if (!int.TryParse(Request.Form["ServerPort"], out serverConfig.ServerPort) || serverConfig.ServerPort < 1 || serverConfig.ServerPort > 65535)
                    return Content(XMLMessage.Error("SRV-MESC-BADSRVPRT", "The ServerPort field is invalid").ToString());
                if (!srvPrv.IsPortAvailable(serverConfig.ServerPort, RequestServer))
                    return Content(XMLMessage.Error("SRV-MESC-EXSRVPRT", "The ServerPort is already in use").ToString());
            }


            // ==== ServerName ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_SERVERNAME_WR"))
            {
                if (!string.IsNullOrWhiteSpace(Request.Form["ServerName"]))
                    serverConfig.ServerName = Request.Form["ServerName"];
            }

            // ==== IgnoreLastSession ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_IGNORELASTSESSION_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["IgnoreLastSession"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISILS", "The IgnoreLastSession field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableStructuralSimulation"], out serverConfig.IgnoreLastSession))
                    return Content(XMLMessage.Error("SRV-MESC-BADILS", "The IgnoreLastSession field is invalid").ToString());
            }

            // ==== PauseGameWhenEmpty ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_PAUSEGAMEWHENEMPTY_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["PauseGameWhenEmpty"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISPGWE", "The PauseGameWhenEmpty field must be provided").ToString());
                if (!bool.TryParse(Request.Form["PauseGameWhenEmpty"], out serverConfig.PauseGameWhenEmpty))
                    return Content(XMLMessage.Error("SRV-MESC-BADPGWE", "The PauseGameWhenEmpty field is invalid").ToString());
            }

            // ==== EnableSpectator ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLESPECTATOR_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableSpectator"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISES", "The EnableSpectator field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableSpectator"], out serverConfig.EnableSpectator))
                    return Content(XMLMessage.Error("SRV-MESC-BADES", "The EnableSpectator field is invalid").ToString());
            }

            // ==== AutoSaveInMinutes ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_AUTOSAVEINMINUTES_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["AutoSaveInMinutes"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISASIM", "The AutoSaveInMinutes field must be provided").ToString());
                if (!uint.TryParse(Request.Form["AutoSaveInMinutes"], out serverConfig.AutoSaveInMinutes))
                    return Content(XMLMessage.Error("SRV-MESC-BADASIM", "The AutoSaveInMinutes field is invalid").ToString());
            }

            // ==== GameMode ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_GAMEMODE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["GameMode"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISGM", "The GameMode field must be provided").ToString());
                if (!Enum.TryParse(Request.Form["GameMode"], out serverConfig.GameMode))
                    return Content(XMLMessage.Error("SRV-MESC-BADGM", "The GameMode field is invalid").ToString());
            }

            // ==== EnableCopyPaste ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLECOPYPASTE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableCopyPaste"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISECP", "The EnableCopyPaste field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableCopyPaste"], out serverConfig.EnableCopyPaste))
                    return Content(XMLMessage.Error("SRV-MESC-BADECP", "The EnableCopyPaste field is invalid").ToString());
            }

            // ==== MaxPlayers ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_MAXPLAYERS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["MaxPlayers"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISMAXPL", "The MaxPlayers field must be provided").ToString());
                if (!int.TryParse(Request.Form["MaxPlayers"], out serverConfig.MaxPlayers) || serverConfig.MaxPlayers < 1)
                    return Content(XMLMessage.Error("SRV-MESC-BADMAXPL", "The MaxPlayers field is invalid").ToString());
            }

            // ==== WorldName ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_WORLDNAME_WR"))
            {
                if (!string.IsNullOrWhiteSpace(Request.Form["WorldName"]))
                    serverConfig.WorldName = Request.Form["WorldName"];
            }

            // ==== Mods ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_MODS_WR"))
            {
                serverConfig.Mods.Clear();
                foreach (string mod in Request.Form["Mods"].Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(mod))
                    {
                        ulong modParsed;
                        if (!ulong.TryParse(mod, out modParsed))
                            return Content(XMLMessage.Error("SRV-MESC-BADMOD", "The Mods field is invalid").ToString());
                        serverConfig.Mods.Add(modParsed);
                    }
                }
            }

            // ==== OnlineMode ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ONLINEMODE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["OnlineMode"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISOM", "The OnlineMode field must be provided").ToString());
                if (!Enum.TryParse(Request.Form["OnlineMode"], out serverConfig.OnlineMode))
                    return Content(XMLMessage.Error("SRV-MESC-BADOM", "The OnlineMode field is invalid").ToString());
            }

            // ==== GroupID ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_GROUPID_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["GroupID"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISGRID", "The GroupID field must be provided").ToString());
                if (!ulong.TryParse(Request.Form["GroupID"], out serverConfig.GroupID))
                    return Content(XMLMessage.Error("SRV-MESC-BADGRID", "The GroupID field is invalid").ToString());
            }

            // ==== Administrators ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ADMINISTRATORS_WR"))
            {
                serverConfig.Administrators.Clear();
                foreach (string adm in Request.Form["Administrators"].Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(adm))
                    {
                        ulong admParsed;
                        if (!ulong.TryParse(adm, out admParsed))
                            return Content(XMLMessage.Error("SRV-MESC-BADADM", "The Administrators field is invalid").ToString());
                        serverConfig.Administrators.Add(admParsed);
                    }
                }
            }

            // ==== Banned ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_BANNED_WR"))
            {
                serverConfig.Banned.Clear();
                foreach (string ban in Request.Form["Banned"].Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(ban))
                    {
                        ulong banParsed;
                        if (!ulong.TryParse(ban, out banParsed))
                            return Content(XMLMessage.Error("SRV-MESC-BADBAN", "The Banned field is invalid").ToString());
                        serverConfig.Banned.Add(banParsed);
                    }
                }
            }

            // ==== ClientCanSave ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_CLIENTCANSAVE_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["ClientCanSave"]))
                    return
                        Content(XMLMessage.Error("SRV-MESC-MISCCS", "The ClientCanSave field must be provided").ToString());
                if (!bool.TryParse(Request.Form["ClientCanSave"], out serverConfig.ClientCanSave))
                    return Content(XMLMessage.Error("SRV-MESC-BADCCS", "The ClientCanSave field is invalid").ToString());
            }

            // ==== EnableStructuralSimulation ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLESTRUCTURALSIMULATION_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableStructuralSimulation"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISESS", "The EnableStructuralSimulation field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableStructuralSimulation"], out serverConfig.EnableStructuralSimulation))
                    return Content(XMLMessage.Error("SRV-MESC-BADESS", "The EnableStructuralSimulation field is invalid").ToString());
            }

            // ==== MaxActiveFracturePieces ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_MAXACTIVEFRACTUREPIECES_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["MaxActiveFracturePieces"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISMAXAFP", "The MaxActiveFracturePieces field must be provided").ToString());
                if (!uint.TryParse(Request.Form["MaxActiveFracturePieces"], out serverConfig.MaxActiveFracturePieces))
                    return Content(XMLMessage.Error("SRV-MESC-BADMAXAFP", "The MaxActiveFracturePieces field is invalid").ToString());
            }

            // ==== EnableBarbarians ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLEBARBARIANS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableBarbarians"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISEB", "The EnableBarbarians field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableBarbarians"], out serverConfig.EnableBarbarians))
                    return Content(XMLMessage.Error("SRV-MESC-BADEB", "The EnableBarbarians field is invalid").ToString());
            }

            // ==== MaximumBots ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_MAXIMUMBOTS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["MaximumBots"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISMAXB", "The MaximumBots field must be provided").ToString());
                if (!uint.TryParse(Request.Form["MaximumBots"], out serverConfig.MaximumBots))
                    return Content(XMLMessage.Error("SRV-MESC-BADMAXB", "The MaximumBots field is invalid").ToString());
            }

            // ==== GameDayInRealMinutes ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_GAMEDAYINREALMINUTES_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["GameDayInRealMinutes"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISEB", "The GameDayInRealMinutes field must be provided").ToString());
                if (!uint.TryParse(Request.Form["GameDayInRealMinutes"], out serverConfig.GameDayInRealMinutes))
                    return Content(XMLMessage.Error("SRV-MESC-BADEB", "The GameDayInRealMinutes field is invalid").ToString());
            }

            // ==== DayNightRatio ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_DAYNIGHTRATIO_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["DayNightRatio"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISDNR", "The DayNightRatio field must be provided").ToString());
                if (!double.TryParse(Request.Form["DayNightRatio"], out serverConfig.DayNightRatio))
                    return Content(XMLMessage.Error("SRV-MESC-BADDNR", "The DayNightRatio field is invalid").ToString());
            }

            // ==== EnableAnimals ====
            if (AuthHelper.HasAccess(RequestServer, "SERVER_CONFIG_ME_ENABLEANIMALS_WR"))
            {
                if (string.IsNullOrWhiteSpace(Request.Form["EnableAnimals"]))
                    return Content(XMLMessage.Error("SRV-MESC-MISEA", "The EnableAnimals field must be provided").ToString());
                if (!bool.TryParse(Request.Form["EnableAnimals"], out serverConfig.EnableAnimals))
                    return Content(XMLMessage.Error("SRV-MESC-BADEA", "The EnableAnimals field is invalid").ToString());
            }


            // ** PROCESS **
            RequestServer.Ip = serverConfig.IP;
            RequestServer.Port = serverConfig.ServerPort;
            RequestServer.AutoSaveInMinutes = Convert.ToInt32(serverConfig.AutoSaveInMinutes);

            srvPrv.UpdateServer(RequestServer);

            bool restartRequired = false;
            if (srvPrv.GetState(RequestServer) != ServiceState.Stopped)
            {
                restartRequired = true;
                ServiceHelper.StopServiceAndWait(RequestServer);
            }

            if (RequestServer.UseServerExtender)
                serverConfig.AutoSaveInMinutes = 0;

            serverConfig.Save(RequestServer);

            if (restartRequired)
            {
                ServiceHelper.StartService(RequestServer);
                return Content(XMLMessage.Success("SRV-MESC-OK", "The server configuration has been updated, the server is restarting ...").ToString());
            }

            return Content(XMLMessage.Success("SRV-MESC-OK", "The server configuration has been updated").ToString());
        }

        #endregion

        #region Power Cycle

        // POST: API/Server/StartServers
        [HttpPost]
        public ActionResult StartServers()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            string[] serverIDsString = Request.Form["ServerIDs"].Split(';');

            List<int> serverIDs = new List<int>();

            foreach (string item in serverIDsString)
            {
                int servID;
                if (!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-STRS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach (int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if (server == null)
                    return Content(XMLMessage.Error("SRV-STRS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                if (!AuthHelper.HasAccess(server, "SERVER_POWER_START"))
                    return Content(XMLMessage.Error("SRV-STRS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());

                servers.Add(server);
            }

            // ** PROCESS **
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

            foreach (EntityServer item in servers)
            {
                serviceLogger.Info(item.Name + " started by " + user.Login + " by API/Server/StartServers/");
                ServiceHelper.StartService(item);
            }

            return Content(XMLMessage.Success("SRV-STRS-OK", "The following server(s) have been started : " + string.Join(", ", servers.Select(x => x.Name))).ToString());
        }

        // POST: API/Server/StopServers
        [HttpPost]
        public ActionResult StopServers()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            string[] serverIDsString = Request.Form["ServerIDs"].Split(';');

            List<int> serverIDs = new List<int>();

            foreach (string item in serverIDsString)
            {
                int servID;
                if (!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-STPS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach (int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if (server == null)
                    return Content(XMLMessage.Error("SRV-STPS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                if (!AuthHelper.HasAccess(server, "SERVER_POWER_STOP"))
                    return Content(XMLMessage.Error("SRV-STPS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());

                servers.Add(server);
            }

            // ** PROCESS **
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

            foreach (EntityServer item in servers)
            {
                serviceLogger.Info(item.Name + " stopped by " + user.Login + " by API/Server/StopServers/");
                ServiceHelper.StopService(item);
            }

            return Content(XMLMessage.Success("SRV-STRS-OK", "The following server(s) have been stopped : " + string.Join(", ", servers.Select(x => x.Name))).ToString());
        }

        // POST: API/Server/RestartServers
        [HttpPost]
        public ActionResult RestartServers()
        {
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            bool restartOnlyStarted = (Request.Form["OnlyStarted"] ?? "False").ToLower() == "true";
            string[] serverIDsString = Request.Form["ServerIDs"].Split(';');

            List<int> serverIDs = new List<int>();

            foreach (string item in serverIDsString)
            {
                int servID;
                if (!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-RSTRS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach (int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if (server == null)
                    return Content(XMLMessage.Error("SRV-RSTRS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                if (!AuthHelper.HasAccess(server, "SERVER_POWER_RESTART"))
                    return Content(XMLMessage.Error("SRV-RSTRS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());

                servers.Add(server);
            }

            // ** PROCESS **
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            List<EntityServer> serversToRestart = new List<EntityServer>();
            foreach (EntityServer item in servers)
            {
                ServiceState serviceState = srvPrv.GetState(item);
                if (serviceState == ServiceState.Running)
                {
                    serviceLogger.Info(item.Name + " restarted (stopping part) by " + user.Login + " by API/Server/RestartServers/");
                    ServiceHelper.StopService(item);
                    if (restartOnlyStarted)
                        serversToRestart.Add(item);
                }
            }

            if (!restartOnlyStarted)
                serversToRestart = servers;

            foreach (EntityServer item in serversToRestart)
                ServiceHelper.WaitForStopped(item);

            foreach (EntityServer item in serversToRestart)
            {
                serviceLogger.Info(item.Name + " restarted (starting part) by " + user.Login + " by API/Server/RestartServers/");
                ServiceHelper.StartService(item);
            }

            return Content(XMLMessage.Success("SRV-RSTRS-OK", "The following server(s) have been restarted : "
                + string.Join(", ", serversToRestart.Select(x => x.Name))).ToString());
        }

        // POST: API/Server/KillServers
        [HttpPost]
        public ActionResult KillServers()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            string[] serverIDsString = Request.Form["ServerIDs"].Split(';');

            List<int> serverIDs = new List<int>();

            foreach (string item in serverIDsString)
            {
                int servID;
                if (!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-KILS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach (int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if (server == null)
                    return Content(XMLMessage.Error("SRV-KILS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                if (!AuthHelper.HasAccess(server, "SERVER_POWER_KILL"))
                    return Content(XMLMessage.Error("SRV-KILS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());

                servers.Add(server);
            }

            // ** PROCESS **
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

            foreach (EntityServer item in servers)
            {
                serviceLogger.Info(item.Name + " killed by " + user.Login + " by API/Server/KillServers/");
                ServiceHelper.KillService(item);
            }

            return Content(XMLMessage.Success("SRV-KILS-OK", "The following server(s) have been killed : " + string.Join(", ", servers.Select(x => x.Name))).ToString());
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CurrentContext.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}