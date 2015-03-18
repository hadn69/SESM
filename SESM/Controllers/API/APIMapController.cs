using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using System.Xml.Linq;
using Ionic.Zip;
using SESM.DAL;
using SESM.DTO;
using SESM.Models;
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

        // POST: API/Map/SelectMap
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

            return Content(XMLMessage.Success("MAP-SM-OK", "The map have been selected").ToString());
        }

        // POST: API/Map/DeleteMap
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

            return Content(XMLMessage.Success("MAP-DM-OK", "The map have been deleted").ToString());
        }

        // POST: API/Map/DownloadMap
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

        // POST: API/Map/CreateMap
        [HttpPost]
        public ActionResult CreateMap()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("MAP-CM-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("MAP-CM-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("MAP-CM-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("MAP-CM-NOACCESS", "You don't have access to this server").ToString());
            
            SubTypeId SubTypeId;
            if (string.IsNullOrWhiteSpace(Request.Form["SubTypeId"]))
                return Content(XMLMessage.Error("MAP-CM-MISSTI", "The SubTypeId field must be provided").ToString());
            if (!Enum.TryParse(Request.Form["SubTypeId"], out SubTypeId))
                return Content(XMLMessage.Error("MAP-CM-MISSTI", "The SubTypeId is invalid").ToString());

            int AsteroidAmount;
            if (string.IsNullOrWhiteSpace(Request.Form["AsteroidAmount"]))
                return Content(XMLMessage.Error("MAP-CM-MISAA", "The AsteroidAmount field must be provided").ToString());
            if (!int.TryParse(Request.Form["AsteroidAmount"], out AsteroidAmount) || AsteroidAmount < 0)
                return Content(XMLMessage.Error("MAP-CM-MISAA", "The AsteroidAmount is invalid").ToString());

            float ProceduralDensity;
            if (string.IsNullOrWhiteSpace(Request.Form["ProceduralDensity"]))
                return Content(XMLMessage.Error("MAP-CM-MISPD", "The ProceduralDensity field must be provided").ToString());
            if (!float.TryParse(Request.Form["ProceduralDensity"], out ProceduralDensity) || ProceduralDensity < 0 || ProceduralDensity > 1)
                return Content(XMLMessage.Error("MAP-CM-MISPD", "The ProceduralDensity is invalid").ToString());

            int ProceduralSeed;
            if (string.IsNullOrWhiteSpace(Request.Form["ProceduralSeed"]))
                return Content(XMLMessage.Error("MAP-CM-MISPS", "The ProceduralSeed field must be provided").ToString());
            if (!int.TryParse(Request.Form["ProceduralSeed"], out ProceduralSeed) || ProceduralDensity < 0)
                return Content(XMLMessage.Error("MAP-CM-MISPS", "The ProceduralSeed is invalid").ToString());

            // ** PROCESS **
            ServiceState serviceState = srvPrv.GetState(server);

            if (serviceState != ServiceState.Stopped && serviceState != ServiceState.Unknow)
            {
                ServiceHelper.StopServiceAndWait(server);
                Thread.Sleep(5000);
                ServiceHelper.KillService(server);
            }

            ServerConfigHelper config = new ServerConfigHelper();
            config.Load(server);

            config.ScenarioType = SubTypeId;
            config.AsteroidAmount = AsteroidAmount;
            config.ProceduralDensity = ProceduralDensity;
            config.ProceduralSeed = ProceduralSeed;
            config.SaveName = string.Empty;

            config.Save(server);

            ServiceHelper.StartService(server);

            return Content(XMLMessage.Success("MAP-CM-OK", "The map have been created, the server is (re)starting ...").ToString());
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