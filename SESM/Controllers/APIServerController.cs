using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using NLog;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    public class APIServerController : Controller
    {
        private readonly DataContext _context = new DataContext();

        private bool SecurityCheck(EntityServer server)
        {
            EntityUser user = Session["User"] as EntityUser;
            if(user == null || server == null)
                return false;

            ServerProvider srvPrv = new ServerProvider(_context);

            AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, server.Id);
            if(accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
            {
                return true;
            }
            return false;
        }

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

        // GET: API/Server/GetServer/{ServerID}
        [HttpGet]
        public ActionResult GetServer(int id)
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            EntityServer server = srvPrv.GetServer(id);

            // ** ACCESS **
            if(server == null)
                return Content(new XMLMessage(XmlResponseType.Error, "SRV-GS-UKNSRV", "The server doesn't exist").ToString());

            if(srvPrv.GetAccessLevel(userID, server.Id) == AccessLevel.None)
                return Content(new XMLMessage(XmlResponseType.Error, "SRV-GS-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("SRV-GS-OK");

            response.AddToContent(new XElement("Name", server.Name));
            response.AddToContent(new XElement("ID", server.Id));
            response.AddToContent(new XElement("Public", server.IsPublic.ToString()));
            response.AddToContent(new XElement("State", srvPrv.GetState(server).ToString()));
            response.AddToContent(new XElement("AccessLevel", srvPrv.GetAccessLevel(userID, server.Id)));

            return Content(response.ToString());
        }

        // GET: API/Server/StartServers/
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
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-STRS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach(int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if(server == null)
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-STRS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if(accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-STRS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
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

            return Content(new XMLMessage(XmlResponseType.Success, "SRV-STRS-OK", "The following server(s) have been started : " + string.Join(", ", servers.Select(x => x.Name))).ToString());
        }

        // GET: API/Server/StopServers/
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
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-STPS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach(int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if(server == null)
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-STPS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if(accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-STPS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
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

            return Content(new XMLMessage(XmlResponseType.Success, "SRV-STRS-OK", "The following server(s) have been stopped : " + string.Join(", ", servers.Select(x => x.Name))).ToString());
        }

        // GET: API/Server/RestartServers/
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
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-RSTRS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            // ** ACCESS **
            List<EntityServer> servers = new List<EntityServer>();
            foreach(int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if(server == null)
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-RSTRS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if(accessLevel == AccessLevel.None
                    || accessLevel == AccessLevel.Guest
                    || accessLevel == AccessLevel.User)
                {
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-RSTRS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + " (" + server.Id + ")").ToString());
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
                    serviceLogger.Info(item.Name + " stopped by " + user.Login + " by API/Server/RestartServers/");
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

            return Content(new XMLMessage(XmlResponseType.Success, "SRV-RSTRS-OK", "The following server(s) have been restarted : "
                + string.Join(", ", serversToRestart.Select(x => x.Name))).ToString());
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