﻿using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Linq;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;

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
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            List<EntityServer> servers = srvPrv.GetServers(user);

            XMLMessage response = new XMLMessage("SRV-GSS-OK");

            foreach(EntityServer server in servers)
            {
                response.AddToContent(new XElement("Server",
                                                            new XElement("Name", server.Name),
                                                            new XElement("ID", server.Id),
                                                            new XElement("Public", server.IsPublic.ToString()),
                                                            new XElement("State", srvPrv.GetState(server).ToString()),
                                                            new XElement("AccessLevel", srvPrv.GetAccessLevel(userID, server.Id))));
            }
            return Content(response.ToString());
        }

        // GET: API/Server/GetServer/{ServerID}
        [HttpGet]
        public ActionResult GetServer(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            EntityServer server = srvPrv.GetServer(id);

            if(server == null)
                return Content(new XMLMessage(XmlResponseType.Error, "SRV-GS-UKNSRV", "The server doesn't exist").ToString());

            if(srvPrv.GetAccessLevel(userID, server.Id) == AccessLevel.None)
                return Content(new XMLMessage(XmlResponseType.Error, "SRV-GS-NOACCESS", "You don't have access to this server").ToString());

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
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            string[] serverIDsString = Request.Form["ServerIDs"].Split(';');

            List<int> serverIDs = new List<int>();

            foreach (string item in serverIDsString)
            {
                int servID = 0;
                if (!int.TryParse(item, out servID))
                {
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-STRS-INVALIDID", "The following ID is not a number : " + item).ToString());
                }
                serverIDs.Add(servID);
            }

            foreach (int item in serverIDs)
            {
                EntityServer server = srvPrv.GetServer(item);

                if(server == null)
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-STRS-UKNSRV", "The following server ID doesn't exist : " + item).ToString());

                AccessLevel accessLevel = srvPrv.GetAccessLevel(userID, server.Id);
                if (!(accessLevel != AccessLevel.None && 
                      accessLevel != AccessLevel.Guest &&
                      accessLevel != AccessLevel.User))
                {
                    return Content(new XMLMessage(XmlResponseType.Error, "SRV-STRS-NOACCESS", "You don't have the required access level on the folowing server : " + server.Name + "(" + server.Id + ")").ToString());
                }

            }


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