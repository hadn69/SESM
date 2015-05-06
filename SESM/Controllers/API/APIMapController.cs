using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
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

            ServerConfigHelperBase config;

            if (server.ServerType == EnumServerType.SpaceEngineers)
                config = new SEServerConfigHelper();
            else
                config = new MEServerConfigHelper();

            config.Load(server);

            foreach (string item in Directory.GetDirectories(PathHelper.GetSavesPath(server), "*", SearchOption.TopDirectoryOnly))
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

            ServerConfigHelperBase config;

            if (server.ServerType == EnumServerType.SpaceEngineers)
                config = new SEServerConfigHelper();
            else
                config = new MEServerConfigHelper();

            config.Load(server);

            if (server.UseServerExtender)
            {
                config.AutoSaveInMinutes = Convert.ToUInt32(server.AutoSaveInMinutes);
                config.Save(server);
            }

            config.SaveName = MapDir;

            config.LoadFromSaveManager(PathHelper.GetSavePath(server, MapDir));

            if (server.UseServerExtender)
            {
                server.AutoSaveInMinutes = Convert.ToInt32(config.AutoSaveInMinutes);
                srvPrv.UpdateServer(server);
                config.AutoSaveInMinutes = 0;
            }

            config.Save(server);

            if (restartRequired)
                ServiceHelper.StartService(server);

            return Content(XMLMessage.Success("MAP-SM-OK", "The map have been selected").ToString());
        }

        // POST: API/Map/DeleteMaps
        [HttpPost]
        public ActionResult DeleteMaps()
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

            string MapDirsString = Request.Form["MapDirs"];
            if (string.IsNullOrWhiteSpace(MapDirsString))
                return Content(XMLMessage.Error("MAP-DM-MISMD", "The MapDir field must be provided").ToString());

            List<string> MapDirs = new List<string>();

            foreach (string item in MapDirsString.Split(':'))
            {
                if (!Directory.Exists(PathHelper.GetSavePath(server, item)) || item.Contains(@"\") || item.Contains("/"))
                {
                    return Content(XMLMessage.Error("MAP-DM-INVALIDPATH", "One of the map dir provided is invalid").ToString());
                }

                MapDirs.Add(item);
            }

            // ** PROCESS **

            ServerConfigHelperBase config;

            if (server.ServerType == EnumServerType.SpaceEngineers)
                config = new SEServerConfigHelper();
            else
                config = new MEServerConfigHelper();

            config.Load(server);

            foreach (string item in MapDirs)
            {
                if (config.SaveName == item)
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

                Directory.Delete(PathHelper.GetSavePath(server, item), true);
            }
            return Content(XMLMessage.Success("MAP-DM-OK", "The map(s) have been deleted").ToString());
        }

        // POST: API/Map/DownloadMaps
        [HttpPost]
        public ActionResult DownloadMaps()
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

            string MapDirsString = Request.Form["MapDirs"];
            if (string.IsNullOrWhiteSpace(MapDirsString))
                return Content(XMLMessage.Error("MAP-DM-MISMD", "The MapDir field must be provided").ToString());

            List<string> MapDirs = new List<string>();

            foreach (string item in MapDirsString.Split(':'))
            {
                if (!Directory.Exists(PathHelper.GetSavePath(server, item)) || item.Contains(@"\") || item.Contains("/"))
                {
                    return Content(XMLMessage.Error("MAP-DM-INVALIDPATH", "One of the map dir provided is invalid").ToString());
                }

                MapDirs.Add(item);
            }

            // ** PROCESS **

            ServerConfigHelperBase config;

            if (server.ServerType == EnumServerType.SpaceEngineers)
                config = new SEServerConfigHelper();
            else
                config = new MEServerConfigHelper();

            config.Load(server);

            Response.Clear();
            Response.ContentType = "application/zip";
            Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", ((MapDirs.Count == 1) ? MapDirs[0] : (server.Name + "-Maps")) + ".zip"));
            using (ZipFile zip = new ZipFile())
            {
                foreach (string item in MapDirs)
                {
                    string sourceFolderPath = PathHelper.GetSavePath(server, item) + @"\";

                    zip.AddSelectedFiles("*", sourceFolderPath, (MapDirs.Count == 1) ? String.Empty : item, true);
                    if (server.UseServerExtender && config.SaveName == item)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            zip.RemoveEntry(((MapDirs.Count == 1) ? String.Empty : item + "\\") + "Sandbox.sbc");

                            string text = System.IO.File.ReadAllText(PathHelper.GetSavePath(server, item) + @"\Sandbox.sbc", new UTF8Encoding(false));
                            text = text.Replace("<AutoSaveInMinutes>0</AutoSaveInMinutes>",
                                "<AutoSaveInMinutes>" + server.AutoSaveInMinutes + "</AutoSaveInMinutes>");

                            zip.AddEntry(((MapDirs.Count == 1) ? String.Empty : item + "\\") + "Sandbox.sbc", text, new UTF8Encoding(false));
                        }
                    }
                }
                zip.Save(Response.OutputStream);
            }
            Response.End();
            return null;
        }

        // POST: API/Map/SECreateMap
        [HttpPost]
        public ActionResult SECreateMap()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("MAP-SCM-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("MAP-SCM-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("MAP-SCM-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("MAP-SCM-NOACCESS", "You don't have access to this server").ToString());

            SESubTypeId SubTypeId;
            if (string.IsNullOrWhiteSpace(Request.Form["SubTypeId"]))
                return Content(XMLMessage.Error("MAP-SCM-MISSTI", "The SubTypeId field must be provided").ToString());
            if (!Enum.TryParse(Request.Form["SubTypeId"], out SubTypeId))
                return Content(XMLMessage.Error("MAP-SCM-MISSTI", "The SubTypeId is invalid").ToString());

            int AsteroidAmount;
            if (string.IsNullOrWhiteSpace(Request.Form["AsteroidAmount"]))
                return Content(XMLMessage.Error("MAP-SCM-MISAA", "The AsteroidAmount field must be provided").ToString());
            if (!int.TryParse(Request.Form["AsteroidAmount"], out AsteroidAmount) || AsteroidAmount < 0)
                return Content(XMLMessage.Error("MAP-SCM-MISAA", "The AsteroidAmount is invalid").ToString());

            float ProceduralDensity;
            if (string.IsNullOrWhiteSpace(Request.Form["ProceduralDensity"]))
                return Content(XMLMessage.Error("MAP-SCM-MISPD", "The ProceduralDensity field must be provided").ToString());
            if (!float.TryParse(Request.Form["ProceduralDensity"], out ProceduralDensity) || ProceduralDensity < 0 || ProceduralDensity > 1)
                return Content(XMLMessage.Error("MAP-SCM-MISPD", "The ProceduralDensity is invalid").ToString());

            int ProceduralSeed;
            if (string.IsNullOrWhiteSpace(Request.Form["ProceduralSeed"]))
                return Content(XMLMessage.Error("MAP-SCM-MISPS", "The ProceduralSeed field must be provided").ToString());
            if (!int.TryParse(Request.Form["ProceduralSeed"], out ProceduralSeed) || ProceduralDensity < 0)
                return Content(XMLMessage.Error("MAP-SCM-MISPS", "The ProceduralSeed is invalid").ToString());

            // ** PROCESS **
            ServiceState serviceState = srvPrv.GetState(server);

            if (serviceState != ServiceState.Stopped && serviceState != ServiceState.Unknow)
            {
                ServiceHelper.StopServiceAndWait(server);
                Thread.Sleep(5000);
                ServiceHelper.KillService(server);
            }

            SEServerConfigHelper config = new SEServerConfigHelper();
            config.Load(server);

            config.ScenarioType = SubTypeId;
            config.AsteroidAmount = AsteroidAmount;
            config.ProceduralDensity = ProceduralDensity;
            config.ProceduralSeed = ProceduralSeed;
            config.SaveName = string.Empty;

            config.Save(server);

            ServiceHelper.StartService(server);

            return Content(XMLMessage.Success("MAP-SCM-OK", "The map have been created, the server is (re)starting ...").ToString());
        }

        // POST: API/Map/MECreateMap
        [HttpPost]
        public ActionResult MECreateMap()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("MAP-MCM-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("MAP-MCM-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("MAP-MCM-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("MAP-MCM-NOACCESS", "You don't have access to this server").ToString());

            MESubTypeId SubTypeId;
            if (string.IsNullOrWhiteSpace(Request.Form["SubTypeId"]))
                return Content(XMLMessage.Error("MAP-MCM-MISSTI", "The SubTypeId field must be provided").ToString());
            if (!Enum.TryParse(Request.Form["SubTypeId"], out SubTypeId))
                return Content(XMLMessage.Error("MAP-MCM-MISSTI", "The SubTypeId is invalid").ToString());

            // ** PROCESS **
            ServiceState serviceState = srvPrv.GetState(server);

            if (serviceState != ServiceState.Stopped && serviceState != ServiceState.Unknow)
            {
                ServiceHelper.StopServiceAndWait(server);
                Thread.Sleep(5000);
                ServiceHelper.KillService(server);
            }

            MEServerConfigHelper config = new MEServerConfigHelper();
            config.Load(server);

            config.ScenarioType = SubTypeId;

            config.SaveName = string.Empty;

            config.Save(server);

            ServiceHelper.StartService(server);

            return Content(XMLMessage.Success("MAP-MCM-OK", "The map have been created, the server is (re)starting ...").ToString());
        }

        // POST: API/Map/UploadMap        
        [HttpPost]
        public ActionResult UploadMap(HttpPostedFileBase ZipFile)
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING **
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

            if (ZipFile == null)
                return Content(XMLMessage.Error("MAP-UPM-MISZIP", "The zipFile parameter must be provided").ToString());

            if (!Ionic.Zip.ZipFile.IsZipFile(ZipFile.InputStream, false))
                return Content(XMLMessage.Error("MAP-UPM-BADZIP", "The provided file in not a zip file").ToString());

            ZipFile.InputStream.Seek(0, SeekOrigin.Begin);

            // ** PROCESS **
            using (ZipFile zip = Ionic.Zip.ZipFile.Read(ZipFile.InputStream))
            {
                bool matchFound = false;
                bool rootZip = false;
                string rootPath = string.Empty;

                foreach (ZipEntry item in zip)
                {
                    if (item.FileName == "Sandbox.sbc")
                    {
                        if (matchFound)
                            return Content(XMLMessage.Error("MAP-UPM-MLPSBC", "Multiple Sandbox.sbc found").ToString());
                        rootZip = true;
                        matchFound = true;
                    }
                    else if (Regex.IsMatch(item.FileName, ".*/Sandbox.sbc$"))
                    {
                        if (matchFound)
                            return Content(XMLMessage.Error("MAP-UPM-MLPSBC", "Multiple Sandbox.sbc found").ToString());
                        matchFound = true;
                        rootPath = Regex.Match(item.FileName, "(.*)/Sandbox.sbc$").Groups[1].Value;
                    }
                }

                if (!matchFound)
                    return Content(XMLMessage.Error("MAP-UPM-MISSBC", "No Sandbox.sbc found").ToString());

                string dirName = string.Empty;
                using (MemoryStream ms = new MemoryStream())
                {
                    zip[rootZip ? "Sandbox.sbc" : rootPath + "/Sandbox.sbc"].Extract(ms);
                    ms.Position = 0;
                    StreamReader sr = new StreamReader(ms);
                    string sbc = sr.ReadToEnd();
                    dirName = PathHelper.SanitizeFSName(Regex.Match(sbc, @"<SessionName>(.*)<\/SessionName>").Groups[1].Value);
                }

                if (Directory.Exists(PathHelper.GetSavePath(server, dirName)) && System.IO.File.Exists(PathHelper.GetSavePath(server, dirName) + @"\Sandbox.sbc"))
                {
                    int i = 2;
                    string testDir;
                    do
                    {
                        testDir = dirName + " (" + i + ")";
                        i++;
                    } while (Directory.Exists(PathHelper.GetSavePath(server, testDir)) && System.IO.File.Exists(PathHelper.GetSavePath(server, testDir) + @"\Sandbox.sbc"));
                    dirName = testDir;
                }

                if (!Directory.Exists(PathHelper.GetSavePath(server, dirName)))
                    Directory.CreateDirectory(PathHelper.GetSavePath(server, dirName));

                if (rootZip)
                {
                    zip.ExtractAll(PathHelper.GetSavePath(server, dirName), ExtractExistingFileAction.OverwriteSilently);
                }
                else
                {
                    foreach (ZipEntry item in zip.EntriesSorted)
                    {
                        if (item.FileName.StartsWith(rootPath))
                        {
                            item.FileName = item.FileName.Substring(rootPath.Length);
                            item.Extract(PathHelper.GetSavePath(server, dirName), ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }

            }
            return Content(XMLMessage.Success("MAP-UPM-OK", "The map have been uploaded").ToString());
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