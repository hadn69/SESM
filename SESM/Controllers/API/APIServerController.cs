using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Xml.Linq;
using NLog;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;
using SESM.Tools.Helpers;

namespace SESM.Controllers.API
{
    public class APIServerController : Controller
    {
        private readonly DataContext _context = new DataContext();

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

            foreach(EntityServer server in servers)
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
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SRV-GS-MISID", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SRV-GS-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if(server == null)
                return Content(XMLMessage.Error("SRV-GS-UKNSRV", "The server doesn't exist").ToString());

            if(srvPrv.GetAccessLevel(userID, server.Id) == AccessLevel.None)
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

        // POST: API/Server/StartServers/
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

            foreach(string item in serverIDsString)
            {
                int servID;
                if(!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-STRS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach(int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if(server == null)
                    return Content(XMLMessage.Error("SRV-STRS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if(accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(XMLMessage.Error("SRV-STRS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
                }
                servers.Add(server);
            }

            // ** PROCESS **
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

            foreach(EntityServer item in servers)
            {
                serviceLogger.Info(item.Name + " started by " + user.Login + " by API/Server/StartServers/");
                ServiceHelper.StartService(item);
            }

            return Content(XMLMessage.Success("SRV-STRS-OK", "The following server(s) have been started : " + string.Join(", ", servers.Select(x => x.Name))).ToString());
        }

        // POST: API/Server/StopServers/
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

            foreach(string item in serverIDsString)
            {
                int servID;
                if(!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-STPS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach(int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if(server == null)
                    return Content(XMLMessage.Error("SRV-STPS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if(accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(XMLMessage.Error("SRV-STPS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
                }
                servers.Add(server);
            }

            // ** PROCESS **
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

            foreach(EntityServer item in servers)
            {
                serviceLogger.Info(item.Name + " stopped by " + user.Login + " by API/Server/StopServers/");
                ServiceHelper.StopService(item);
            }

            return Content(XMLMessage.Success("SRV-STRS-OK", "The following server(s) have been stopped : " + string.Join(", ", servers.Select(x => x.Name))).ToString());
        }

        // POST: API/Server/RestartServers/
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

            foreach(string item in serverIDsString)
            {
                int servID;
                if(!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-RSTRS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach(int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if(server == null)
                    return Content(XMLMessage.Error("SRV-RSTRS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if(accessLevel == AccessLevel.None
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
            foreach(EntityServer item in servers)
            {
                ServiceState serviceState = srvPrv.GetState(item);
                if(serviceState == ServiceState.Running)
                {
                    serviceLogger.Info(item.Name + " restarted by " + user.Login + " by API/Server/RestartServers/");
                    ServiceHelper.StopService(item);
                    if(restartOnlyStarted)
                        serversToRestart.Add(item);
                }
            }

            if(!restartOnlyStarted)
                serversToRestart = servers;

            foreach(EntityServer item in serversToRestart)
                ServiceHelper.WaitForStopped(item);

            foreach(EntityServer item in serversToRestart)
            {
                serviceLogger.Info(item.Name + " started by " + user.Login + " by API/Server/RestartServers/");
                ServiceHelper.StartService(item);
            }

            return Content(XMLMessage.Success("SRV-RSTRS-OK", "The following server(s) have been restarted : "
                + string.Join(", ", serversToRestart.Select(x => x.Name))).ToString());
        }

        // POST: API/Server/KillServers/
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

            foreach(string item in serverIDsString)
            {
                int servID;
                if(!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-KILS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach(int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if(server == null)
                    return Content(XMLMessage.Error("SRV-KILS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if(accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(XMLMessage.Error("SRV-KILS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
                }
                servers.Add(server);
            }

            // ** PROCESS **
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

            foreach(EntityServer item in servers)
            {
                serviceLogger.Info(item.Name + " killed by " + user.Login + " by API/Server/KillServers/");
                ServiceHelper.KillService(item);
            }

            return Content(XMLMessage.Success("SRV-KILS-OK", "The following server(s) have been killed : " + string.Join(", ", servers.Select(x => x.Name))).ToString());
        }

        // POST: API/Server/DeleteServers/
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

            foreach(string item in serverIDsString)
            {
                int servID;
                if(!int.TryParse(item, out servID))
                {
                    return Content(XMLMessage.Error("SRV-DEL-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach(int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if(server == null)
                    return Content(XMLMessage.Error("SRV-DEL-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                if(!user.IsAdmin)
                    return Content(XMLMessage.Error("SRV-DEL-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());

                servers.Add(server);
            }

            // ** PROCESS **
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");

            foreach(EntityServer item in servers)
            {
                serviceLogger.Info(item.Name + " killed by " + user.Login + " by API/Server/DeleteServers/");
                ServiceHelper.KillService(item);
                Thread.Sleep(200);
                ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(item));
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

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}