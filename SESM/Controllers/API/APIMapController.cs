using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using System.Xml.Linq;
using Ionic.Zip;
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

            ServerConfigHelper config = new ServerConfigHelper();
            config.Load(server);

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
                                                          new XElement("Selected", dirInfo.Name == config.SaveName),
                                                          new XElement("MapName", mapName),
                                                          new XElement("Date", dirSizeInfo.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss")),
                                                          new XElement("Size", dirSizeInfo.Size.ToString())
                                                          ));

            }

            return Content(response.ToString());
        }

        // GET: API/Map/SelectMap
        [HttpPost]
        public ActionResult SelectMap()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("MAP-SM-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("MAP-SM-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("MAP-SM-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("MAP-SM-NOACCESS", "You don't have access to this server").ToString());

            string MapDir = Request.Form["MapDir"];
            if (string.IsNullOrWhiteSpace(MapDir))
                return Content(XMLMessage.Error("SRV-SM-MISMD", "The MapDir field must be provided").ToString());
            if (!Directory.Exists(PathHelper.GetSavePath(server, MapDir)) || MapDir.Contains(@"\") || MapDir.Contains("/"))
                return Content(XMLMessage.Error("SRV-SM-BADMD", "The map " + MapDir + " don't exist").ToString());

            // ** PROCESS **
            ServiceState serviceState = srvPrv.GetState(server);

            bool restartRequired = false;
            if (serviceState != ServiceState.Stopped && serviceState != ServiceState.Unknow)
            {
                restartRequired = true;
                ServiceHelper.StopServiceAndWait(server);
                Thread.Sleep(5000);
                ServiceHelper.KillService(server);
            }

            ServerConfigHelper config = new ServerConfigHelper();
            config.Load(server);

            if (server.UseServerExtender)
            {
                config.AutoSaveInMinutes = server.AutoSaveInMinutes ?? 5;
                config.Save(server);
            }

            config.SaveName = MapDir;

            config.LoadFromSaveManager(PathHelper.GetSavePath(server, MapDir));

            if (server.UseServerExtender)
            {
                server.AutoSaveInMinutes = config.AutoSaveInMinutes;
                srvPrv.UpdateServer(server);
                config.AutoSaveInMinutes = 0;
            }

            config.Save(server);

            if(restartRequired)
                ServiceHelper.StartService(server);

            return Content(XMLMessage.Success("SRV-SM-OK", "The map have been selected").ToString());
        }

        // GET: API/Map/DeleteMap
        [HttpPost]
        public ActionResult DeleteMap()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("MAP-DM-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("MAP-DM-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("MAP-DM-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("MAP-DM-NOACCESS", "You don't have access to this server").ToString());

            string MapDir = Request.Form["MapDir"];
            if (string.IsNullOrWhiteSpace(MapDir))
                return Content(XMLMessage.Error("SRV-DM-MISMD", "The MapDir field must be provided").ToString());
            if (!Directory.Exists(PathHelper.GetSavePath(server, MapDir)) || MapDir.Contains(@"\") || MapDir.Contains("/"))
                return Content(XMLMessage.Error("SRV-DM-BADMD", "The map " + MapDir + " don't exist").ToString());

            // ** PROCESS **

            ServerConfigHelper config = new ServerConfigHelper();
            config.Load(server);
            
            if (config.SaveName == MapDir)
            {
                ServiceState serviceState = srvPrv.GetState(server);

                if (serviceState != ServiceState.Stopped && serviceState != ServiceState.Unknow)
                {
                    ServiceHelper.StopServiceAndWait(server);
                    Thread.Sleep(5000);
                    ServiceHelper.KillService(server);
                }

                config.SaveName = string.Empty;
                config.Save(server);
            }

            Directory.Delete(PathHelper.GetSavePath(server, MapDir), true);

            return Content(XMLMessage.Success("SRV-DM-OK", "The map have been deleted").ToString());
        }

        // GET: API/Map/DownloadMap
        [HttpPost]
        public ActionResult DownloadMap()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("MAP-DWM-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("MAP-DWM-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("MAP-DWM-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("MAP-DWM-NOACCESS", "You don't have access to this server").ToString());

            string MapDir = Request.Form["MapDir"];
            if (string.IsNullOrWhiteSpace(MapDir))
                return Content(XMLMessage.Error("SRV-DWM-MISMD", "The MapDir field must be provided").ToString());
            if (!Directory.Exists(PathHelper.GetSavePath(server, MapDir)) || MapDir.Contains(@"\") || MapDir.Contains("/"))
                return Content(XMLMessage.Error("SRV-DWM-BADMD", "The map " + MapDir + " don't exist").ToString());

            // ** PROCESS **

            string sourceFolderPath = PathHelper.GetSavePath(server, MapDir) + @"\";
            if (Directory.Exists(sourceFolderPath))
            {
                Response.Clear();
                Response.ContentType = "application/zip";
                Response.AddHeader("Content-Disposition",
                    String.Format("attachment; filename={0}", MapDir + ".zip"));

                using (ZipFile zip = new ZipFile())
                {
                    zip.AddSelectedFiles("*", sourceFolderPath, string.Empty, true);
                    if (server.UseServerExtender)
                    {
                        ServerConfigHelper config = new ServerConfigHelper();
                        config.Load(server);

                        if (config.SaveName == MapDir)
                        {
                            zip.RemoveEntry("Sandbox.sbc");
                            string text = System.IO.File.ReadAllText(sourceFolderPath + "Sandbox.sbc",
                                new UTF8Encoding(false));
                            text = text.Replace("<AutoSaveInMinutes>0</AutoSaveInMinutes>",
                                "<AutoSaveInMinutes>" + server.AutoSaveInMinutes + "</AutoSaveInMinutes>");
                            zip.AddEntry("Sandbox.sbc", text, new UTF8Encoding(false));
                        }
                    }

                    zip.Save(Response.OutputStream);
                }
                Response.End();
            }
            return null;
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