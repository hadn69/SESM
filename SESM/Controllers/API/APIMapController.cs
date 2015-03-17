using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;
using SESM.Tools.Helpers;

namespace SESM.Controllers.API
{
    public class APIMapController : Controller
    {
        private readonly DataContext _context = new DataContext();

        // GET: API/Map/GetMaps
        [HttpPost]
        public ActionResult GetMaps()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("MAP-GM-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("MAP-GM-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("MAP-GM-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("MAP-GM-NOACCESS", "You don't have access to this server").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("MAP-GM-OK");

            foreach (string item in Directory.GetDirectories(PathHelper.GetSavesPath(server),"*",SearchOption.TopDirectoryOnly))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(item);

                string mapName;
                try
                {
                    mapName = Regex.Match(System.IO.File.ReadAllText(item + @"\Sandbox.sbc"), @"<SessionName>(.*)<\/SessionName>").Groups[1].Value;
                }
                catch (Exception)
                {
                    mapName = "Unavailable";
                }

                NodeInfo dirSizeInfo = FSHelper.GetDirSizeInfo(item);


                response.AddToContent(new XElement("Map", new XElement("DirName", dirInfo.Name),
                                                          new XElement("MapName", mapName),
                                                          new XElement("Date", dirSizeInfo.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss")),
                                                          new XElement("Size", dirSizeInfo.Size.ToString())
                                                          ));

            }

            return Content(response.ToString());
        }

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