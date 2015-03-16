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
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.API;
using SESM.Tools.Helpers;
using SESM.Tools.Jobs;

namespace SESM.Controllers.API
{
    public class APIServerController : Controller
    {
        private readonly DataContext _context = new DataContext();

        #region Informations Provider

        // GET: API/Server/GetServers
        [HttpGet]
        public ActionResult GetServers()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            List<EntityServer> servers = srvPrv.GetServers(user);

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SRV-GSS-OK");

            foreach (EntityServer server in servers)
            {
                response.AddToContent(new XElement("Server", new XElement("Name", server.Name),
                                                            new XElement("ID", server.Id),
                                                            new XElement("Public", server.IsPublic.ToString()),
                                                            new XElement("State", srvPrv.GetState(server).ToString()),
                                                            new XElement("AccessLevel", srvPrv.GetAccessLevel(userID, server.Id))
                                                            ));
            }
            return Content(response.ToString());
        }

        // POST: API/Server/GetServer
        [HttpPost]
        public ActionResult GetServer()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-GS-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-GS-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-GS-UKNSRV", "The server doesn't exist").ToString());

            if (srvPrv.GetAccessLevel(userID, server.Id) == AccessLevel.None)
                return Content(XMLMessage.Error("SRV-GS-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SRV-GS-OK");

            response.AddToContent(new XElement("Name", server.Name));
            response.AddToContent(new XElement("ID", server.Id));
            response.AddToContent(new XElement("Public", server.IsPublic.ToString()));
            response.AddToContent(new XElement("State", srvPrv.GetState(server).ToString()));
            response.AddToContent(new XElement("AccessLevel", srvPrv.GetAccessLevel(userID, server.Id)));

            return Content(response.ToString());
        }

        #endregion

        #region Server CRUD

        // POST: API/Server/CreateServer
        [HttpPost]
        public ActionResult CreateServer()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            string serverName = Request.Form["ServerName"];

            if (string.IsNullOrWhiteSpace(serverName))
                return Content(XMLMessage.Error("SRV-CRS-MISNAME", "The ServerName field must be provided").ToString());

            if (!Regex.IsMatch(serverName, @"^[a-zA-Z0-9_.-]+$"))
                return Content(XMLMessage.Error("SRV-CRS-BADNAME", "The Name must be only composed of letters, numbers, dots, dashs and underscores").ToString());

            // ** PROCESS **
            EntityServer server = new EntityServer
            {
                Name = serverName,
                Ip = Default.IP,
                IsPublic = Default.IsPublic,
                Port = srvPrv.GetNextAvailablePort(),
                ServerExtenderPort = srvPrv.GetNextAvailableSESEPort()
            };
            srvPrv.CreateServer(server);

            Directory.CreateDirectory(PathHelper.GetSavesPath(server));
            Directory.CreateDirectory(PathHelper.GetInstancePath(server) + @"Mods");
            Directory.CreateDirectory(PathHelper.GetInstancePath(server) + @"Backups");

            ServerConfigHelper configHelper = new ServerConfigHelper();

            configHelper.Save(server);
            ServiceHelper.RegisterService(server);

            return Content(XMLMessage.Success("SRV-CRS-OK", "The server " + serverName + " was created").ToString());
        }

        // POST: API/Server/DeleteServers
        [HttpPost]
        public ActionResult DeleteServers()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

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

                if (!user.IsAdmin)
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
        public ActionResult GetSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-GSET-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-GSET-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-GSET-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-GSET-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **

            XMLMessage response = new XMLMessage("SRV-GSET-OK");

            response.AddToContent(new XElement("Name", server.Name));
            response.AddToContent(new XElement("Public", server.IsPublic));
            response.AddToContent(new XElement("ProcessPriority", server.ProcessPriority.ToString()));

            return Content(response.ToString());
        }

        // POST: API/Server/SetSettings
        [HttpPost]
        public ActionResult SetSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-SSET-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-SSET-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-SSET-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsAdminOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-SSET-NOACCESS", "You don't have access to this server").ToString());

            string serverName = Request.Form["Name"];
            if (string.IsNullOrWhiteSpace(serverName))
                return Content(XMLMessage.Error("SRV-SSET-MISNAME", "The Name field must be provided").ToString());
            if (!Regex.IsMatch(serverName, @"^[a-zA-Z0-9_.-]+$"))
                return Content(XMLMessage.Error("SRV-SSET-BADNAME", "The Name field must be only composed of letters, numbers, dots, dashs and underscores").ToString());
            if (serverName != server.Name && !srvPrv.IsNameAvaialble(serverName))
                return Content(XMLMessage.Error("SRV-SSET-NAMEUSE", "The server " + serverName + "Already Exist").ToString());

            bool serverPublic;
            if (string.IsNullOrWhiteSpace(Request.Form["Public"]))
                return Content(XMLMessage.Error("SRV-SSET-MISPUB", "The Public field must be provided").ToString());
            if (!bool.TryParse(Request.Form["Public"], out serverPublic))
                return Content(XMLMessage.Error("SRV-SSET-BADPUB", "The Public field is invalid").ToString());

            EnumProcessPriority serverProcessPriority;
            if (string.IsNullOrWhiteSpace(Request.Form["ProcessPriority"]))
                return Content(XMLMessage.Error("SRV-SSET-MISPP", "The ProcessPriority field must be provided").ToString());
            if (!Enum.TryParse(Request.Form["ProcessPriority"], out serverProcessPriority))
                return Content(XMLMessage.Error("SRV-SSET-BADPP", "The ProcessPriority field is invalid").ToString());

            // ** Process **
            try
            {
                ServiceState serverState = srvPrv.GetState(server);
                bool restartRequired = false;

                if (serverName != server.Name)
                {
                    if (serverState != ServiceState.Stopped &&
                        serverState != ServiceState.Unknow)
                    {
                        ServiceHelper.StopServiceAndWait(server);
                        Thread.Sleep(5000);
                        ServiceHelper.KillService(server);
                        Thread.Sleep(1000);
                        restartRequired = true;
                    }
                    ServiceHelper.UnRegisterService(server);
                    string oldpath = PathHelper.GetInstancePath(server);
                    server.Name = serverName;
                    string newpath = PathHelper.GetInstancePath(server);

                    Directory.Move(oldpath, newpath);

                    if (server.UseServerExtender)
                        ServiceHelper.RegisterServerExtenderService(server);
                    else
                        ServiceHelper.RegisterService(server);
                }
                server.IsPublic = serverPublic;
                server.ProcessPriority = serverProcessPriority;

                if (serverState != ServiceState.Stopped && serverState != ServiceState.Unknow)
                    ServiceHelper.SetPriority(server);

                srvPrv.UpdateServer(server);
                if (restartRequired)
                    ServiceHelper.StartService(server);

                return Content(XMLMessage.Success("SRV-SSET-OK", "The server settings have been updated").ToString());
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("SRV-SSET-EX", "Exception :" + ex).ToString());
            }
        }

        // POST: API/Server/GetSESESettings
        [HttpPost]
        public ActionResult GetSESESettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-GSESES-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-GSESES-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-GSESES-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-GSESES-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **

            XMLMessage response = new XMLMessage("SRV-GSESES-OK");

            response.AddToContent(new XElement("UseServerExtender", server.UseServerExtender));
            response.AddToContent(new XElement("ServerExtenderPort", server.ServerExtenderPort));

            return Content(response.ToString());
        }

        // POST: API/Server/SetSESESettings
        [HttpPost]
        public ActionResult SetSESESettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-SSESES-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-SSESES-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-SSESES-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-SSESES-NOACCESS", "You don't have access to this server").ToString());

            bool useServerExtender;
            if (string.IsNullOrWhiteSpace(Request.Form["UseServerExtender"]))
                return Content(XMLMessage.Error("SRV-SSESES-MISUSE", "The UseServerExtender field must be provided").ToString());
            if (!bool.TryParse(Request.Form["UseServerExtender"], out useServerExtender))
                return Content(XMLMessage.Error("SRV-SSESES-BADUSE", "The UseServerExtender field is invalid").ToString());

            int serverExtenderPort;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerExtenderPort"]))
                return Content(XMLMessage.Error("SRV-SSESES-MISSEP", "The ServerExtenderPort field must be provided").ToString());
            if (!int.TryParse(Request.Form["ServerExtenderPort"], out serverExtenderPort) || serverExtenderPort < 1 || serverExtenderPort > 65535)
                return Content(XMLMessage.Error("SRV-SSESES-BADSEP", "The ServerExtenderPort field is invalid").ToString());
            if (serverExtenderPort != server.ServerExtenderPort && !srvPrv.IsPortAvailable(serverExtenderPort, server))
                return Content(XMLMessage.Error("SRV-SSESES-EXSEP", "The ServerExtenderPort is already in use by another server").ToString());

            // ** Process **
            try
            {
                ServiceState serverState = srvPrv.GetState(server);
                bool restartRequired = false;

                if (serverState != ServiceState.Stopped &&
                    serverState != ServiceState.Unknow)
                {
                    ServiceHelper.StopServiceAndWait(server);
                    Thread.Sleep(5000);
                    ServiceHelper.KillService(server);
                    Thread.Sleep(1000);
                    restartRequired = true;
                }

                if (useServerExtender != server.UseServerExtender)
                {
                    server.UseServerExtender = useServerExtender;
                    ServiceHelper.UnRegisterService(server);
                    if (server.UseServerExtender)
                        ServiceHelper.RegisterServerExtenderService(server);
                    else
                        ServiceHelper.RegisterService(server);
                }

                server.ServerExtenderPort = serverExtenderPort;

                srvPrv.UpdateServer(server);
                if (restartRequired)
                    ServiceHelper.StartService(server);

                return Content(XMLMessage.Success("SRV-SSESES-OK", "The server SESE settings have been updated").ToString());
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("SRV-SSESES-EX", "Exception :" + ex).ToString());
            }
        }

        // POST: API/Server/GetJobsSettings
        [HttpPost]
        public ActionResult GetJobsSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-GJS-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-GJS-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-GJS-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-GJS-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **

            XMLMessage response = new XMLMessage("SRV-GJS-OK");

            response.AddToContent(new XElement("AutoRestart", server.IsAutoRestartEnabled));
            response.AddToContent(new XElement("AutoRestartCron", server.AutoRestartCron));
            response.AddToContent(new XElement("AutoStart", server.IsAutoStartEnabled));

            return Content(response.ToString());
        }

        // POST: API/Server/SetJobsSettings
        [HttpPost]
        public ActionResult SetJobsSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-SJS-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-SJS-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-SJS-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-SJS-NOACCESS", "You don't have access to this server").ToString());

            bool autoRestart;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoRestart"]))
                return Content(XMLMessage.Error("SRV-SJS-MISAR", "The AutoRestart field must be provided").ToString());
            if (!bool.TryParse(Request.Form["AutoRestart"], out autoRestart))
                return Content(XMLMessage.Error("SRV-SJS-BADAR", "The AutoRestart field is invalid").ToString());

            string autoRestartCron = Request.Form["AutoRestartCron"];
            if (string.IsNullOrWhiteSpace(autoRestartCron))
                return Content(XMLMessage.Error("SET-SJS-MISARC", "The AutoRestartCron field must be provided").ToString());
            if (!CronExpression.IsValidExpression(autoRestartCron))
                return Content(XMLMessage.Error("SET-SJS-BADARC", "The AutoRestartCron field is invalid").ToString());

            bool autoStart;
            if (string.IsNullOrWhiteSpace(Request.Form["AutoStart"]))
                return Content(XMLMessage.Error("SRV-SJS-MISAS", "The AutoStart field must be provided").ToString());
            if (!bool.TryParse(Request.Form["AutoStart"], out autoStart))
                return Content(XMLMessage.Error("SRV-SJS-BADAS", "The AutoStart field is invalid").ToString());

            // ** Process **
            try
            {
                if (autoRestart != server.IsAutoRestartEnabled || server.AutoRestartCron != autoRestartCron)
                {
                    server.IsAutoRestartEnabled = autoRestart;
                    server.AutoRestartCron = autoRestartCron;

                    IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                    scheduler.DeleteJob(SEAutoUpdateJob.GetJobKey());

                    if (server.IsAutoRestartEnabled)
                    {
                        // Instantiating the job
                        IJobDetail AutoRestartJobDetail = JobBuilder.Create<AutoRestartJob>()
                            .WithIdentity(AutoRestartJob.GetJobKey(server))
                            .Build();

                        ITrigger AutoRestartJobTrigger = TriggerBuilder.Create()
                            .WithIdentity(AutoRestartJob.GetTriggerKey(server))
                            .WithCronSchedule(autoRestartCron)
                            .Build();

                        scheduler.ScheduleJob(AutoRestartJobDetail, AutoRestartJobTrigger);
                    }
                }

                server.IsAutoStartEnabled = autoStart;

                srvPrv.UpdateServer(server);

                return Content(XMLMessage.Success("SRV-SSESESET-OK", "The server Jobs settings have been updated").ToString());
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("SRV-SSESESET-EX", "Exception :" + ex).ToString());
            }
        }

        // POST: API/Server/GetBackupsSettings
        [HttpPost]
        public ActionResult GetBackupsSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-GBS-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-GBS-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-GBS-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-GBS-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **

            XMLMessage response = new XMLMessage("SRV-GBS-OK");

            response.AddToContent(new XElement("Lvl1BackupEnabled", server.IsLvl1BackupEnabled));
            response.AddToContent(new XElement("Lvl1BackupInfos", "Cron : " + SESMConfigHelper.AutoBackupLvl1Cron +
                                                                " ; " + (SESMConfigHelper.AutoBackupLvl1Enabled ? "Enabled" : "Disabled") +
                                                                " ; Nb Rotating Backup : " + SESMConfigHelper.AutoBackupLvl1NbToKeep));
            response.AddToContent(new XElement("Lvl2BackupEnabled", server.IsLvl2BackupEnabled));
            response.AddToContent(new XElement("Lvl2BackupInfos", "Cron : " + SESMConfigHelper.AutoBackupLvl2Cron +
                                                                " ; " + (SESMConfigHelper.AutoBackupLvl2Enabled ? "Enabled" : "Disabled") +
                                                                " ; Nb Rotating Backup : " + SESMConfigHelper.AutoBackupLvl2NbToKeep));
            response.AddToContent(new XElement("Lvl3BackupEnabled", server.IsLvl3BackupEnabled));
            response.AddToContent(new XElement("Lvl3BackupInfos", "Cron : " + SESMConfigHelper.AutoBackupLvl3Cron +
                                                                " ; " + (SESMConfigHelper.AutoBackupLvl3Enabled ? "Enabled" : "Disabled") +
                                                                " ; Nb Rotating Backup : " + SESMConfigHelper.AutoBackupLvl3NbToKeep));

            return Content(response.ToString());
        }

        // POST: API/Server/SetBackupsSettings
        [HttpPost]
        public ActionResult SetBackupsSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-SBS-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-SBS-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-SBS-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-SBS-NOACCESS", "You don't have access to this server").ToString());

            bool lvl1BackupEnabled;
            if (string.IsNullOrWhiteSpace(Request.Form["Lvl1BackupEnabled"]))
                return Content(XMLMessage.Error("SRV-SBS-LVL1", "The Lvl1BackupEnabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["Lvl1BackupEnabled"], out lvl1BackupEnabled))
                return Content(XMLMessage.Error("SRV-SBS-LVL1", "The Lvl1BackupEnabled field is invalid").ToString());

            bool lvl2BackupEnabled;
            if (string.IsNullOrWhiteSpace(Request.Form["Lvl2BackupEnabled"]))
                return Content(XMLMessage.Error("SRV-SBS-LVL2", "The Lvl2BackupEnabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["Lvl2BackupEnabled"], out lvl2BackupEnabled))
                return Content(XMLMessage.Error("SRV-SBS-LVL2", "The Lvl2BackupEnabled field is invalid").ToString());

            bool lvl3BackupEnabled;
            if (string.IsNullOrWhiteSpace(Request.Form["Lvl3BackupEnabled"]))
                return Content(XMLMessage.Error("SRV-SBS-LVL3", "The Lvl3BackupEnabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["Lvl3BackupEnabled"], out lvl3BackupEnabled))
                return Content(XMLMessage.Error("SRV-SBS-LVL3", "The Lvl3BackupEnabled field is invalid").ToString());

            // ** Process **
            try
            {
                server.IsLvl1BackupEnabled = lvl1BackupEnabled;
                server.IsLvl2BackupEnabled = lvl2BackupEnabled;
                server.IsLvl3BackupEnabled = lvl3BackupEnabled;

                srvPrv.UpdateServer(server);

                return Content(XMLMessage.Success("SRV-SBS-OK", "The server backups settings have been updated").ToString());
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("SRV-SBS-EX", "Exception :" + ex).ToString());
            }
        }

        // POST: API/Server/GetAccessSettings
        [HttpPost]
        public ActionResult GetAccessSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-GAS-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-GAS-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-GAS-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-GAS-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **

            XMLMessage response = new XMLMessage("SRV-GAS-OK");

            XElement webAdministrators = new XElement("WebAdministrators");
            foreach (EntityUser wadmin in server.Administrators)
                webAdministrators.Add(new XElement("WebAdministrator", wadmin.Login));
            response.AddToContent(webAdministrators);

            XElement webManagers = new XElement("WebManagers");
            foreach (EntityUser wmanager in server.Managers)
                webManagers.Add(new XElement("WebManager", wmanager.Login));
            response.AddToContent(webManagers);

            XElement webUsers = new XElement("WebUsers");
            foreach (EntityUser wuser in server.Users)
                webUsers.Add(new XElement("WebUser", wuser.Login));
            response.AddToContent(webUsers);

            return Content(response.ToString());
        }

        // POST: API/Server/SetAccessSettings
        [HttpPost]
        public ActionResult SetAccessSettings()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);
            UserProvider usrPrv = new UserProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-SAS-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-SAS-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-SAS-UKNSRV", "The server doesn't exist").ToString());

            AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
            if (!srvPrv.IsManagerOrAbore(accessLevel))
                return Content(XMLMessage.Error("SRV-SAS-NOACCESS", "You don't have access to this server").ToString());

            string webAdministratorsStr = Request.Form["WebAdministrators"];
            if (accessLevel == AccessLevel.Manager && !string.IsNullOrWhiteSpace(webAdministratorsStr))
                return Content(XMLMessage.Error("SRV-SAS-NOADMACC", "You don't sufficient priviledges to edit the administrators list").ToString());

            string webManagersStr = Request.Form["WebManagers"];
            string webUsersStr = Request.Form["WebUsers"];

            // ** Process **
            try
            {
                if(accessLevel != AccessLevel.Manager)
                    srvPrv.AddAdministrator(webAdministratorsStr.Split(';'),server);
                srvPrv.AddManagers(webManagersStr.Split(';'), server);
                srvPrv.AddUsers(webUsersStr.Split(';'), server);

                srvPrv.UpdateServer(server);

                return Content(XMLMessage.Success("SRV-SAS-OK", "The server Access settings have been updated").ToString());
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("SRV-SAS-EX", "Exception :" + ex).ToString());
            }
        }

        #endregion

        #region Server Configuration

        // POST: API/Server/GetConfiguration
        [HttpPost]
        public ActionResult GetConfiguration()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-GC-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-GC-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-GC-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SRV-GC-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **
            // Loading the server config
            ServerConfigHelper serverConfig = new ServerConfigHelper();
            serverConfig.Load(server);

            XMLMessage response = new XMLMessage("SRV-GC-OK");

            response.AddToContent(new XElement("IP", serverConfig.IP));
            response.AddToContent(new XElement("SteamPort", serverConfig.SteamPort));
            response.AddToContent(new XElement("ServerPort", serverConfig.ServerPort));
            response.AddToContent(new XElement("ServerName", serverConfig.ServerName));
            response.AddToContent(new XElement("IgnoreLastSession", serverConfig.IgnoreLastSession));
            response.AddToContent(new XElement("PauseGameWhenEmpty", serverConfig.PauseGameWhenEmpty));
            response.AddToContent(new XElement("EnableSpectator", serverConfig.EnableSpectator));
            response.AddToContent(new XElement("RealisticSound", serverConfig.RealisticSound));
            response.AddToContent(new XElement("AutoSaveInMinutes", serverConfig.AutoSaveInMinutes));
            response.AddToContent(new XElement("InventorySizeMultiplier", serverConfig.InventorySizeMultiplier));
            response.AddToContent(new XElement("AssemblerSpeedMultiplier", serverConfig.AssemblerSpeedMultiplier));
            response.AddToContent(new XElement("AssemblerEfficiencyMultiplier", serverConfig.AssemblerEfficiencyMultiplier));
            response.AddToContent(new XElement("RefinerySpeedMultiplier", serverConfig.RefinerySpeedMultiplier));
            response.AddToContent(new XElement("GameMode", serverConfig.GameMode));
            response.AddToContent(new XElement("EnableCopyPaste", serverConfig.EnableCopyPaste));
            response.AddToContent(new XElement("WelderSpeedMultiplier", serverConfig.WelderSpeedMultiplier));
            response.AddToContent(new XElement("GrinderSpeedMultiplier", serverConfig.GrinderSpeedMultiplier));
            response.AddToContent(new XElement("HackSpeedMultiplier", serverConfig.HackSpeedMultiplier));
            response.AddToContent(new XElement("DestructibleBlocks", serverConfig.DestructibleBlocks));
            response.AddToContent(new XElement("MaxPlayers", serverConfig.MaxPlayers));
            response.AddToContent(new XElement("MaxFloatingObjects", serverConfig.MaxFloatingObjects));
            response.AddToContent(new XElement("WorldName", serverConfig.WorldName));
            response.AddToContent(new XElement("EnvironmentHostility", serverConfig.EnvironmentHostility));
            response.AddToContent(new XElement("WorldSizeKm", serverConfig.WorldSizeKm));
            response.AddToContent(new XElement("PermanentDeath", serverConfig.PermanentDeath));
            response.AddToContent(new XElement("CargoShipsEnabled", serverConfig.CargoShipsEnabled));
            response.AddToContent(new XElement("RemoveTrash", serverConfig.RemoveTrash));
            response.AddToContent(new XElement("ClientCanSave", serverConfig.ClientCanSave));

            XElement mods = new XElement("Mods");
            foreach (ulong mod in serverConfig.Mods)
                mods.Add(new XElement("Mod", mod));
            response.AddToContent(mods);

            response.AddToContent(new XElement("ViewDistance", serverConfig.ViewDistance));
            response.AddToContent(new XElement("OnlineMode", serverConfig.OnlineMode));
            response.AddToContent(new XElement("ResetOwnership", serverConfig.ResetOwnership));

            XElement administrators = new XElement("Administrators");
            foreach (ulong adminitrator in serverConfig.Administrators)
                administrators.Add(new XElement("Adminitrator", adminitrator));
            response.AddToContent(administrators);

            XElement banned = new XElement("Banned");
            foreach (ulong ban in serverConfig.Banned)
                banned.Add(new XElement("Ban", ban));
            response.AddToContent(banned);

            response.AddToContent(new XElement("AutoHealing", serverConfig.AutoHealing));
            response.AddToContent(new XElement("WeaponsEnabled", serverConfig.WeaponsEnabled));
            response.AddToContent(new XElement("ShowPlayerNamesOnHud", serverConfig.ShowPlayerNamesOnHud));
            response.AddToContent(new XElement("ThrusterDamage", serverConfig.ThrusterDamage));
            response.AddToContent(new XElement("SpawnShipTimeMultiplier", serverConfig.SpawnShipTimeMultiplier));
            response.AddToContent(new XElement("RespawnShipDelete", serverConfig.RespawnShipDelete));
            response.AddToContent(new XElement("EnableToolShake", serverConfig.EnableToolShake));
            response.AddToContent(new XElement("EnableIngameScripts", serverConfig.EnableIngameScripts));

            return Content(response.ToString());
        }

        // POST: API/Server/GetConfigurationRights
        [HttpPost]
        public ActionResult GetConfigurationRights()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-GCR-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-GCR-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-GCR-UKNSRV", "The server doesn't exist").ToString());

            AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
            if (!srvPrv.IsManagerOrAbore(accessLevel))
                return Content(XMLMessage.Error("SRV-GCR-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **
            bool isAdmin = accessLevel != AccessLevel.Manager;

            XMLMessage response = new XMLMessage("SRV-GCR-OK");

            response.AddToContent(new XElement("IP", isAdmin));
            response.AddToContent(new XElement("SteamPort", isAdmin));
            response.AddToContent(new XElement("ServerPort", isAdmin));
            response.AddToContent(new XElement("ServerName", true));
            response.AddToContent(new XElement("IgnoreLastSession", true));
            response.AddToContent(new XElement("PauseGameWhenEmpty", true));
            response.AddToContent(new XElement("EnableSpectator", true));
            response.AddToContent(new XElement("RealisticSound", true));
            response.AddToContent(new XElement("AutoSaveInMinutes", true));
            response.AddToContent(new XElement("InventorySizeMultiplier", true));
            response.AddToContent(new XElement("AssemblerSpeedMultiplier", true));
            response.AddToContent(new XElement("AssemblerEfficiencyMultiplier", true));
            response.AddToContent(new XElement("RefinerySpeedMultiplier", true));
            response.AddToContent(new XElement("GameMode", true));
            response.AddToContent(new XElement("EnableCopyPaste", true));
            response.AddToContent(new XElement("WelderSpeedMultiplier", true));
            response.AddToContent(new XElement("GrinderSpeedMultiplier", true));
            response.AddToContent(new XElement("HackSpeedMultiplier", true));
            response.AddToContent(new XElement("DestructibleBlocks", true));
            response.AddToContent(new XElement("MaxPlayers", isAdmin));
            response.AddToContent(new XElement("MaxFloatingObjects", isAdmin));
            response.AddToContent(new XElement("WorldName", true));
            response.AddToContent(new XElement("EnvironmentHostility", true));
            response.AddToContent(new XElement("WorldSizeKm", true));
            response.AddToContent(new XElement("PermanentDeath", true));
            response.AddToContent(new XElement("CargoShipsEnabled", true));
            response.AddToContent(new XElement("RemoveTrash", isAdmin));
            response.AddToContent(new XElement("ClientCanSave", true));
            response.AddToContent(new XElement("Mods", true));
            response.AddToContent(new XElement("ViewDistance", true));
            response.AddToContent(new XElement("OnlineMode", true));
            response.AddToContent(new XElement("ResetOwnership", true));
            response.AddToContent(new XElement("Administrators", true));
            response.AddToContent(new XElement("Banned", true));
            response.AddToContent(new XElement("AutoHealing", true));
            response.AddToContent(new XElement("WeaponsEnabled", true));
            response.AddToContent(new XElement("ShowPlayerNamesOnHud", true));
            response.AddToContent(new XElement("ThrusterDamage", true));
            response.AddToContent(new XElement("SpawnShipTimeMultiplier", true));
            response.AddToContent(new XElement("RespawnShipDelete", true));
            response.AddToContent(new XElement("EnableToolShake", true));
            response.AddToContent(new XElement("EnableIngameScripts", true));

            return Content(response.ToString());
        }

        // POST: API/Server/SetConfiguration
        [HttpPost]
        public ActionResult SetConfiguration()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-SC-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-SC-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SRV-SC-UKNSRV", "The server doesn't exist").ToString());

            AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
            if (!srvPrv.IsManagerOrAbore(accessLevel))
                return Content(XMLMessage.Error("SRV-SC-NOACCESS", "You don't have access to this server").ToString());

            bool isAdmin = accessLevel != AccessLevel.Manager;

            // Loading the server config
            ServerConfigHelper serverConfig = new ServerConfigHelper();
            serverConfig.Load(server);

            if (isAdmin)
            {
                // ==== IP ====
                if (string.IsNullOrWhiteSpace(Request.Form["IP"]))
                    return Content(XMLMessage.Error("SRV-SC-MISIP", "The IP field must be provided").ToString());
                if (!Regex.IsMatch(Request.Form["IP"], @"^((\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.){3}(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])$"))
                    return Content(XMLMessage.Error("SRV-SC-BADIP", "The IP field is invalid").ToString());
                serverConfig.IP = Request.Form["IP"];

                // ==== SteamPort ====
                if (string.IsNullOrWhiteSpace(Request.Form["SteamPort"]))
                    return Content(XMLMessage.Error("SRV-SC-MISSTMPRT", "The SteamPort field must be provided").ToString());
                if (!int.TryParse(Request.Form["SteamPort"], out serverConfig.SteamPort) || serverConfig.SteamPort < 1 || serverConfig.SteamPort > 65535)
                    return Content(XMLMessage.Error("SRV-SC-BADSTMPRT", "The SteamPort field is invalid").ToString());

                // ==== ServerPort ====
                if (string.IsNullOrWhiteSpace(Request.Form["ServerPort"]))
                    return Content(XMLMessage.Error("SRV-SC-MISSRVPRT", "The ServerPort field must be provided").ToString());
                if (!int.TryParse(Request.Form["ServerPort"], out serverConfig.ServerPort) || serverConfig.ServerPort < 1 || serverConfig.ServerPort > 65535)
                    return Content(XMLMessage.Error("SRV-SC-BADSRVPRT", "The ServerPort field is invalid").ToString());
                if (!srvPrv.IsPortAvailable(serverConfig.ServerPort, server))
                    return Content(XMLMessage.Error("SRV-SC-EXSRVPRT", "The ServerPort is already in use").ToString());

                // ==== MaxPlayers ====
                if (string.IsNullOrWhiteSpace(Request.Form["MaxPlayers"]))
                    return Content(XMLMessage.Error("SRV-SC-MISMAXPL", "The MaxPlayers field must be provided").ToString());
                if (!int.TryParse(Request.Form["MaxPlayers"], out serverConfig.MaxPlayers) || serverConfig.MaxPlayers < 1)
                    return Content(XMLMessage.Error("SRV-SC-BADMAXPL", "The MaxPlayers field is invalid").ToString());

                // ==== MaxFloatingObjects ====
                if (string.IsNullOrWhiteSpace(Request.Form["MaxFloatingObjects"]))
                    return Content(XMLMessage.Error("SRV-SC-MISMAXFO", "The MaxFloatingObjects field must be provided").ToString());
                if (!int.TryParse(Request.Form["MaxFloatingObjects"], out serverConfig.MaxFloatingObjects) || serverConfig.MaxFloatingObjects < 1)
                    return Content(XMLMessage.Error("SRV-SC-BADMAXFO", "The MaxFloatingObjects field is invalid").ToString());

                // ==== RemoveTrash ====
                if (string.IsNullOrWhiteSpace(Request.Form["RemoveTrash"]))
                    return Content(XMLMessage.Error("SRV-SC-MISRT", "The RemoveTrash field must be provided").ToString());
                if (!bool.TryParse(Request.Form["RemoveTrash"], out serverConfig.RemoveTrash))
                    return Content(XMLMessage.Error("SRV-SC-BADRT", "The RemoveTrash field is invalid").ToString());
            }

            // ==== ServerName ====
            if (!string.IsNullOrWhiteSpace(Request.Form["ServerName"]))
                serverConfig.ServerName = Request.Form["ServerName"];

            // ==== IgnoreLastSession ====
            if (string.IsNullOrWhiteSpace(Request.Form["IgnoreLastSession"]))
                return Content(XMLMessage.Error("SRV-SC-MISILS", "The IgnoreLastSession field must be provided").ToString());
            if (!bool.TryParse(Request.Form["IgnoreLastSession"], out serverConfig.IgnoreLastSession))
                return Content(XMLMessage.Error("SRV-SC-BADILS", "The IgnoreLastSession field is invalid").ToString());

            // ==== PauseGameWhenEmpty ====
            if (string.IsNullOrWhiteSpace(Request.Form["PauseGameWhenEmpty"]))
                return Content(XMLMessage.Error("SRV-SC-MISPGWE", "The PauseGameWhenEmpty field must be provided").ToString());
            if (!bool.TryParse(Request.Form["PauseGameWhenEmpty"], out serverConfig.PauseGameWhenEmpty))
                return Content(XMLMessage.Error("SRV-SC-BADPGWE", "The PauseGameWhenEmpty field is invalid").ToString());

            // ==== EnableSpectator ====
            if (string.IsNullOrWhiteSpace(Request.Form["EnableSpectator"]))
                return Content(XMLMessage.Error("SRV-SC-MISES", "The EnableSpectator field must be provided").ToString());
            if (!bool.TryParse(Request.Form["EnableSpectator"], out serverConfig.EnableSpectator))
                return Content(XMLMessage.Error("SRV-SC-BADES", "The EnableSpectator field is invalid").ToString());

            // ==== RealisticSound ====
            if (string.IsNullOrWhiteSpace(Request.Form["RealisticSound"]))
                return Content(XMLMessage.Error("SRV-SC-MISRS", "The RealisticSound field must be provided").ToString());
            if (!bool.TryParse(Request.Form["RealisticSound"], out serverConfig.RealisticSound))
                return Content(XMLMessage.Error("SRV-SC-BADRS", "The RealisticSound field is invalid").ToString());

            // ==== AutoSaveInMinutes ====
            if (string.IsNullOrWhiteSpace(Request.Form["AutoSaveInMinutes"]))
                return Content(XMLMessage.Error("SRV-SC-MISASIM", "The AutoSaveInMinutes field must be provided").ToString());
            if (!int.TryParse(Request.Form["AutoSaveInMinutes"], out serverConfig.AutoSaveInMinutes) || serverConfig.AutoSaveInMinutes < 0)
                return Content(XMLMessage.Error("SRV-SC-BADASIM", "The AutoSaveInMinutes field is invalid").ToString());

            // ==== InventorySizeMultiplier ====
            if (string.IsNullOrWhiteSpace(Request.Form["InventorySizeMultiplier"]))
                return Content(XMLMessage.Error("SRV-SC-MISISM", "The InventorySizeMultiplier field must be provided").ToString());
            if (!int.TryParse(Request.Form["InventorySizeMultiplier"], out serverConfig.InventorySizeMultiplier) || serverConfig.InventorySizeMultiplier < 1)
                return Content(XMLMessage.Error("SRV-SC-BADISM", "The InventorySizeMultiplier field is invalid").ToString());

            // ==== AssemblerSpeedMultiplier ====
            if (string.IsNullOrWhiteSpace(Request.Form["AssemblerSpeedMultiplier"]))
                return Content(XMLMessage.Error("SRV-SC-MISASM", "The AssemblerSpeedMultiplier field must be provided").ToString());
            if (!int.TryParse(Request.Form["AssemblerSpeedMultiplier"], out serverConfig.AssemblerSpeedMultiplier) || serverConfig.AssemblerSpeedMultiplier < 1)
                return Content(XMLMessage.Error("SRV-SC-BADASM", "The AssemblerSpeedMultiplier field is invalid").ToString());

            // ==== AssemblerEfficiencyMultiplier ====
            if (string.IsNullOrWhiteSpace(Request.Form["AssemblerEfficiencyMultiplier"]))
                return Content(XMLMessage.Error("SRV-SC-MISAEM", "The AssemblerEfficiencyMultiplier field must be provided").ToString());
            if (!int.TryParse(Request.Form["AssemblerEfficiencyMultiplier"], out serverConfig.AssemblerEfficiencyMultiplier) || serverConfig.AssemblerEfficiencyMultiplier < 1)
                return Content(XMLMessage.Error("SRV-SC-BADAEM", "The AssemblerEfficiencyMultiplier field is invalid").ToString());

            // ==== RefinerySpeedMultiplier ====
            if (string.IsNullOrWhiteSpace(Request.Form["RefinerySpeedMultiplier"]))
                return Content(XMLMessage.Error("SRV-SC-MISRSM", "The RefinerySpeedMultiplier field must be provided").ToString());
            if (!int.TryParse(Request.Form["RefinerySpeedMultiplier"], out serverConfig.RefinerySpeedMultiplier) || serverConfig.RefinerySpeedMultiplier < 1)
                return Content(XMLMessage.Error("SRV-SC-BADRSM", "The RefinerySpeedMultiplier field is invalid").ToString());

            // ==== GameMode ====
            if (string.IsNullOrWhiteSpace(Request.Form["GameMode"]))
                return Content(XMLMessage.Error("SRV-SC-MISGM", "The GameMode field must be provided").ToString());
            if (!Enum.TryParse(Request.Form["RefinerySpeedMultiplier"], out serverConfig.GameMode))
                return Content(XMLMessage.Error("SRV-SC-BADGM", "The GameMode field is invalid").ToString());

            // ==== EnableCopyPaste ====
            if (string.IsNullOrWhiteSpace(Request.Form["EnableCopyPaste"]))
                return Content(XMLMessage.Error("SRV-SC-MISECP", "The EnableCopyPaste field must be provided").ToString());
            if (!bool.TryParse(Request.Form["EnableCopyPaste"], out serverConfig.EnableCopyPaste))
                return Content(XMLMessage.Error("SRV-SC-BADECP", "The EnableCopyPaste field is invalid").ToString());

            // ==== WelderSpeedMultiplier ====
            if (string.IsNullOrWhiteSpace(Request.Form["WelderSpeedMultiplier"]))
                return Content(XMLMessage.Error("SRV-SC-MISWSM", "The WelderSpeedMultiplier field must be provided").ToString());
            if (!double.TryParse(Request.Form["WelderSpeedMultiplier"], out serverConfig.WelderSpeedMultiplier) || serverConfig.WelderSpeedMultiplier < 0)
                return Content(XMLMessage.Error("SRV-SC-BADWSM", "The WelderSpeedMultiplier field is invalid").ToString());

            // ==== GrinderSpeedMultiplier ====
            if (string.IsNullOrWhiteSpace(Request.Form["GrinderSpeedMultiplier"]))
                return Content(XMLMessage.Error("SRV-SC-MISGSM", "The GrinderSpeedMultiplier field must be provided").ToString());
            if (!double.TryParse(Request.Form["GrinderSpeedMultiplier"], out serverConfig.GrinderSpeedMultiplier) || serverConfig.GrinderSpeedMultiplier < 0)
                return Content(XMLMessage.Error("SRV-SC-BADGSM", "The GrinderSpeedMultiplier field is invalid").ToString());

            // ==== HackSpeedMultiplier ====
            if (string.IsNullOrWhiteSpace(Request.Form["HackSpeedMultiplier"]))
                return Content(XMLMessage.Error("SRV-SC-MISHSM", "The HackSpeedMultiplier field must be provided").ToString());
            if (!double.TryParse(Request.Form["HackSpeedMultiplier"], out serverConfig.HackSpeedMultiplier) || serverConfig.HackSpeedMultiplier < 0)
                return Content(XMLMessage.Error("SRV-SC-BADHSM", "The HackSpeedMultiplier field is invalid").ToString());

            // ==== DestructibleBlocks ====
            if (string.IsNullOrWhiteSpace(Request.Form["DestructibleBlocks"]))
                return Content(XMLMessage.Error("SRV-SC-MISDB", "The DestructibleBlocks field must be provided").ToString());
            if (!bool.TryParse(Request.Form["DestructibleBlocks"], out serverConfig.DestructibleBlocks))
                return Content(XMLMessage.Error("SRV-SC-BADDB", "The DestructibleBlocks field is invalid").ToString());

            // ==== WorldName ====
            if (!string.IsNullOrWhiteSpace(Request.Form["WorldName"]))
                serverConfig.WorldName = Request.Form["WorldName"];

            // ==== EnvironmentHostility ====
            if (string.IsNullOrWhiteSpace(Request.Form["EnvironmentHostility"]))
                return Content(XMLMessage.Error("SRV-SC-MISEH", "The EnvironmentHostility field must be provided").ToString());
            if (!Enum.TryParse(Request.Form["EnvironmentHostility"], out serverConfig.EnvironmentHostility))
                return Content(XMLMessage.Error("SRV-SC-BADEH", "The EnvironmentHostility field is invalid").ToString());

            // ==== WorldSizeKm ====
            if (string.IsNullOrWhiteSpace(Request.Form["WorldSizeKm"]))
                return Content(XMLMessage.Error("SRV-SC-MISWSK", "The WorldSizeKm field must be provided").ToString());
            if (!int.TryParse(Request.Form["WorldSizeKm"], out serverConfig.WorldSizeKm) || serverConfig.WorldSizeKm < 0)
                return Content(XMLMessage.Error("SRV-SC-BADWSK", "The WorldSizeKm field is invalid").ToString());

            // ==== PermanentDeath ====
            if (string.IsNullOrWhiteSpace(Request.Form["PermanentDeath"]))
                return Content(XMLMessage.Error("SRV-SC-MISPD", "The PermanentDeath field must be provided").ToString());
            if (!bool.TryParse(Request.Form["PermanentDeath"], out serverConfig.PermanentDeath))
                return Content(XMLMessage.Error("SRV-SC-BADPD", "The PermanentDeath field is invalid").ToString());

            // ==== CargoShipsEnabled ====
            if (string.IsNullOrWhiteSpace(Request.Form["CargoShipsEnabled"]))
                return Content(XMLMessage.Error("SRV-SC-MISCSE", "The CargoShipsEnabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["CargoShipsEnabled"], out serverConfig.CargoShipsEnabled))
                return Content(XMLMessage.Error("SRV-SC-BADCSE", "The CargoShipsEnabled field is invalid").ToString());

            // ==== ClientCanSave ====
            if (string.IsNullOrWhiteSpace(Request.Form["ClientCanSave"]))
                return Content(XMLMessage.Error("SRV-SC-MISCCS", "The ClientCanSave field must be provided").ToString());
            if (!bool.TryParse(Request.Form["ClientCanSave"], out serverConfig.CargoShipsEnabled))
                return Content(XMLMessage.Error("SRV-SC-BADCCS", "The ClientCanSave field is invalid").ToString());

            // ==== Mods ====
            serverConfig.Mods.Clear();
            foreach (string mod in Request.Form["Mods"].Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(mod))
                {
                    ulong modParsed;
                    if (!ulong.TryParse(mod, out modParsed))
                        return Content(XMLMessage.Error("SRV-SC-BADMOD", "The Mods field is invalid").ToString());
                    serverConfig.Mods.Add(modParsed);
                }
            }

            // ==== ViewDistance ====
            if (string.IsNullOrWhiteSpace(Request.Form["ViewDistance"]))
                return Content(XMLMessage.Error("SRV-SC-MISVD", "The ViewDistance field must be provided").ToString());
            if (!int.TryParse(Request.Form["ViewDistance"], out serverConfig.ViewDistance) || serverConfig.ViewDistance < 1)
                return Content(XMLMessage.Error("SRV-SC-BADVD", "The ViewDistance field is invalid").ToString());

            // ==== OnlineMode ====
            if (string.IsNullOrWhiteSpace(Request.Form["OnlineMode"]))
                return Content(XMLMessage.Error("SRV-SC-MISOM", "The OnlineMode field must be provided").ToString());
            if (!Enum.TryParse(Request.Form["OnlineMode"], out serverConfig.OnlineMode))
                return Content(XMLMessage.Error("SRV-SC-BADOM", "The OnlineMode field is invalid").ToString());

            // ==== ResetOwnership ====
            if (string.IsNullOrWhiteSpace(Request.Form["ResetOwnership"]))
                return Content(XMLMessage.Error("SRV-SC-MISRO", "The ResetOwnership field must be provided").ToString());
            if (!bool.TryParse(Request.Form["ResetOwnership"], out serverConfig.ResetOwnership))
                return Content(XMLMessage.Error("SRV-SC-BADRO", "The ResetOwnership field is invalid").ToString());

            // ==== Administrators ====
            serverConfig.Administrators.Clear();
            foreach (string adm in Request.Form["Administrators"].Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(adm))
                {
                    ulong admParsed;
                    if (!ulong.TryParse(adm, out admParsed))
                        return
                            Content(XMLMessage.Error("SRV-SC-BADADM", "The Administrators field is invalid").ToString());
                    serverConfig.Administrators.Add(admParsed);
                }
            }

            // ==== Banned ====
            serverConfig.Banned.Clear();
            foreach (string ban in Request.Form["Banned"].Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(ban))
                {
                    ulong banParsed;
                    if (!ulong.TryParse(ban, out banParsed))
                        return Content(XMLMessage.Error("SRV-SC-BADBAN", "The Banned field is invalid").ToString());
                    serverConfig.Banned.Add(banParsed);
                }
            }

            // ==== AutoHealing ====
            if (string.IsNullOrWhiteSpace(Request.Form["AutoHealing"]))
                return Content(XMLMessage.Error("SRV-SC-MISAH", "The AutoHealing field must be provided").ToString());
            if (!bool.TryParse(Request.Form["AutoHealing"], out serverConfig.AutoHealing))
                return Content(XMLMessage.Error("SRV-SC-BADAH", "The AutoHealing field is invalid").ToString());

            // ==== WeaponsEnabled ====
            if (string.IsNullOrWhiteSpace(Request.Form["WeaponsEnabled"]))
                return Content(XMLMessage.Error("SRV-SC-MISWE", "The WeaponsEnabled field must be provided").ToString());
            if (!bool.TryParse(Request.Form["WeaponsEnabled"], out serverConfig.WeaponsEnabled))
                return Content(XMLMessage.Error("SRV-SC-BADWE", "The WeaponsEnabled field is invalid").ToString());

            // ==== ShowPlayerNamesOnHud ====
            if (string.IsNullOrWhiteSpace(Request.Form["ShowPlayerNamesOnHud"]))
                return Content(XMLMessage.Error("SRV-SC-MISSPNOH", "The ShowPlayerNamesOnHud field must be provided").ToString());
            if (!bool.TryParse(Request.Form["ShowPlayerNamesOnHud"], out serverConfig.ShowPlayerNamesOnHud))
                return Content(XMLMessage.Error("SRV-SC-BADSPNOH", "The ShowPlayerNamesOnHud field is invalid").ToString());

            // ==== ThrusterDamage ====
            if (string.IsNullOrWhiteSpace(Request.Form["ThrusterDamage"]))
                return Content(XMLMessage.Error("SRV-SC-MISTD", "The ThrusterDamage field must be provided").ToString());
            if (!bool.TryParse(Request.Form["ThrusterDamage"], out serverConfig.ThrusterDamage))
                return Content(XMLMessage.Error("SRV-SC-BADTD", "The ThrusterDamage field is invalid").ToString());

            // ==== SpawnShipTimeMultiplier ====
            if (string.IsNullOrWhiteSpace(Request.Form["SpawnShipTimeMultiplier"]))
                return Content(XMLMessage.Error("SRV-SC-MISSSTM", "The SpawnShipTimeMultiplier field must be provided").ToString());
            if (!int.TryParse(Request.Form["SpawnShipTimeMultiplier"], out serverConfig.SpawnShipTimeMultiplier) || serverConfig.SpawnShipTimeMultiplier < 0)
                return Content(XMLMessage.Error("SRV-SC-BADSSTM", "The SpawnShipTimeMultiplier field is invalid").ToString());

            // ==== RespawnShipDelete ====
            if (string.IsNullOrWhiteSpace(Request.Form["RespawnShipDelete"]))
                return Content(XMLMessage.Error("SRV-SC-MISRSD", "The RespawnShipDelete field must be provided").ToString());
            if (!bool.TryParse(Request.Form["RespawnShipDelete"], out serverConfig.RespawnShipDelete))
                return Content(XMLMessage.Error("SRV-SC-BADRSD", "The RespawnShipDelete field is invalid").ToString());

            // ==== EnableToolShake ====
            if (string.IsNullOrWhiteSpace(Request.Form["EnableToolShake"]))
                return Content(XMLMessage.Error("SRV-SC-MISETS", "The EnableToolShake field must be provided").ToString());
            if (!bool.TryParse(Request.Form["EnableToolShake"], out serverConfig.EnableToolShake))
                return Content(XMLMessage.Error("SRV-SC-BADETS", "The EnableToolShake field is invalid").ToString());

            // ==== EnableIngameScripts ====
            if (string.IsNullOrWhiteSpace(Request.Form["EnableIngameScripts"]))
                return Content(XMLMessage.Error("SRV-SC-MISETS", "The EnableIngameScripts field must be provided").ToString());
            if (!bool.TryParse(Request.Form["EnableIngameScripts"], out serverConfig.EnableIngameScripts))
                return Content(XMLMessage.Error("SRV-SC-BADETS", "The EnableIngameScripts field is invalid").ToString());

            // ** PROCESS **
            server.Ip = serverConfig.IP;
            server.Port = serverConfig.ServerPort;
            server.AutoSaveInMinutes = serverConfig.AutoSaveInMinutes;

            srvPrv.UpdateServer(server);

            bool restartRequired = false;
            if (srvPrv.GetState(server) != ServiceState.Stopped)
            {
                restartRequired = true;
                ServiceHelper.StopServiceAndWait(server);
            }

            if (server.UseServerExtender)
                serverConfig.AutoSaveInMinutes = 0;

            serverConfig.Save(server);

            if (restartRequired)
            {
                ServiceHelper.StartService(server);
                return Content(XMLMessage.Success("SRV-SC-OK", "The server configuration has been updated, the server is restarting ...").ToString());
            }

            return Content(XMLMessage.Success("SRV-SC-OK", "The server configuration has been updated").ToString());
        }

        #endregion

        #region Power Cycle

        // POST: API/Server/StartServers
        [HttpPost]
        public ActionResult StartServers()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

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

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if (accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(XMLMessage.Error("SRV-STRS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
                }
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
            ServerProvider srvPrv = new ServerProvider(_context);

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

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if (accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(XMLMessage.Error("SRV-STPS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
                }
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
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

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

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if (accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(XMLMessage.Error("SRV-RSTRS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
                }
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
                    serviceLogger.Info(item.Name + " restarted by " + user.Login + " by API/Server/RestartServers/");
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
                serviceLogger.Info(item.Name + " started by " + user.Login + " by API/Server/RestartServers/");
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
            ServerProvider srvPrv = new ServerProvider(_context);

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

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if (accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(XMLMessage.Error("SRV-KILS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
                }
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
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}