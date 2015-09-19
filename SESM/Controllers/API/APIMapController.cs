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
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models;
using SESM.Tools.API;
using SESM.Tools.Helpers;

namespace SESM.Controllers.API
{
    public class APIMapController : Controller, IAPIController
    {
        public DataContext CurrentContext { get; set; }

        public EntityServer RequestServer { get; set; }

        public APIMapController()
        {
            CurrentContext = new DataContext();
        }

        // GET: API/Map/GetMaps
        [HttpPost]
        [APIServerAccess("MAP-GM", "SERVER_MAP_SE_LIST", "SERVER_MAP_ME_LIST")]
        public ActionResult GetMaps()
        {
            // ** PROCESS **
            XMLMessage response = new XMLMessage("MAP-GM-OK");

            ServerConfigHelperBase config;

            if (RequestServer.ServerType == EnumServerType.SpaceEngineers)
                config = new SEServerConfigHelper();
            else
                config = new MEServerConfigHelper();

            config.Load(RequestServer);

            foreach (string item in Directory.GetDirectories(PathHelper.GetSavesPath(RequestServer), "*", SearchOption.TopDirectoryOnly))
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
        [APIServerAccess("MAP-SM", "SERVER_MAP_SE_SELECT", "SERVER_MAP_ME_SELECT")]
        public ActionResult SelectMap()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            string MapDir = Request.Form["MapDir"];
            if (string.IsNullOrWhiteSpace(MapDir))
                return Content(XMLMessage.Error("SRV-SM-MISMD", "The MapDir field must be provided").ToString());
            if (!Directory.Exists(PathHelper.GetSavePath(RequestServer, MapDir)) || MapDir.Contains(@"\") || MapDir.Contains("/"))
                return Content(XMLMessage.Error("SRV-SM-BADMD", "The map " + MapDir + " don't exist").ToString());

            // ** PROCESS **
            ServiceState serviceState = srvPrv.GetState(RequestServer);

            bool restartRequired = false;
            if (serviceState != ServiceState.Stopped && serviceState != ServiceState.Unknow)
            {
                restartRequired = true;
                ServiceHelper.StopServiceAndWait(RequestServer);
                Thread.Sleep(5000);
                ServiceHelper.KillService(RequestServer);
            }

            ServerConfigHelperBase config;

            if (RequestServer.ServerType == EnumServerType.SpaceEngineers)
                config = new SEServerConfigHelper();
            else
                config = new MEServerConfigHelper();

            config.Load(RequestServer);

            if (RequestServer.UseServerExtender)
            {
                config.AutoSaveInMinutes = Convert.ToUInt32(RequestServer.AutoSaveInMinutes);
                config.Save(RequestServer);
            }

            config.SaveName = MapDir;

            config.LoadFromSaveManager(PathHelper.GetSavePath(RequestServer, MapDir));

            if (RequestServer.UseServerExtender)
            {
                RequestServer.AutoSaveInMinutes = Convert.ToInt32(config.AutoSaveInMinutes);
                srvPrv.UpdateServer(RequestServer);
                config.AutoSaveInMinutes = 0;
            }

            config.Save(RequestServer);

            if (restartRequired)
                ServiceHelper.StartService(RequestServer);

            return Content(XMLMessage.Success("MAP-SM-OK", "The map have been selected").ToString());
        }

        // POST: API/Map/DeleteMaps
        [HttpPost]
        [APIServerAccess("MAP-DM", "SERVER_MAP_SE_DELETE", "SERVER_MAP_ME_DELETE")]
        public ActionResult DeleteMaps()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            // ** PARSING / ACCESS **
            string MapDirsString = Request.Form["MapDirs"];
            if (string.IsNullOrWhiteSpace(MapDirsString))
                return Content(XMLMessage.Error("MAP-DM-MISMD", "The MapDir field must be provided").ToString());

            List<string> MapDirs = new List<string>();

            foreach (string item in MapDirsString.Split(':'))
            {
                if (!Directory.Exists(PathHelper.GetSavePath(RequestServer, item)) || item.Contains(@"\") || item.Contains("/"))
                {
                    return Content(XMLMessage.Error("MAP-DM-INVALIDPATH", "One of the map dir provided is invalid").ToString());
                }

                MapDirs.Add(item);
            }

            // ** PROCESS **

            ServerConfigHelperBase config;

            if (RequestServer.ServerType == EnumServerType.SpaceEngineers)
                config = new SEServerConfigHelper();
            else
                config = new MEServerConfigHelper();

            config.Load(RequestServer);

            foreach (string item in MapDirs)
            {
                if (config.SaveName == item)
                {
                    ServiceState serviceState = srvPrv.GetState(RequestServer);

                    if (serviceState != ServiceState.Stopped && serviceState != ServiceState.Unknow)
                    {
                        ServiceHelper.StopServiceAndWait(RequestServer);
                        Thread.Sleep(5000);
                        ServiceHelper.KillService(RequestServer);
                    }

                    config.SaveName = string.Empty;
                    config.Save(RequestServer);
                }

                Directory.Delete(PathHelper.GetSavePath(RequestServer, item), true);
            }
            return Content(XMLMessage.Success("MAP-DM-OK", "The map(s) have been deleted").ToString());
        }

        // POST: API/Map/DownloadMaps
        [HttpPost]
        [APIServerAccess("MAP-DWM", "SERVER_MAP_SE_DOWNLOAD", "SERVER_MAP_ME_DOWNLOAD")]
        public ActionResult DownloadMaps()
        {
            // ** PARSING / ACCESS **
            string MapDirsString = Request.Form["MapDirs"];
            if (string.IsNullOrWhiteSpace(MapDirsString))
                return Content(XMLMessage.Error("MAP-DWM-MISMD", "The MapDir field must be provided").ToString());

            List<string> MapDirs = new List<string>();

            foreach (string item in MapDirsString.Split(':'))
            {
                if (!Directory.Exists(PathHelper.GetSavePath(RequestServer, item)) || item.Contains(@"\") || item.Contains("/"))
                {
                    return Content(XMLMessage.Error("MAP-DWM-INVALIDPATH", "One of the map dir provided is invalid").ToString());
                }

                MapDirs.Add(item);
            }

            // ** PROCESS **

            ServerConfigHelperBase config;

            if (RequestServer.ServerType == EnumServerType.SpaceEngineers)
                config = new SEServerConfigHelper();
            else
                config = new MEServerConfigHelper();

            config.Load(RequestServer);

            Response.Clear();
            Response.ContentType = "application/zip";
            Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", ((MapDirs.Count == 1) ? MapDirs[0] : (RequestServer.Name + "-Maps")) + ".zip"));
            using (ZipFile zip = new ZipFile())
            {
                foreach (string item in MapDirs)
                {
                    string sourceFolderPath = PathHelper.GetSavePath(RequestServer, item) + @"\";

                    zip.AddSelectedFiles("*", sourceFolderPath, (MapDirs.Count == 1) ? String.Empty : item, true);
                    if (RequestServer.UseServerExtender && config.SaveName == item)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            zip.RemoveEntry(((MapDirs.Count == 1) ? String.Empty : item + "\\") + "Sandbox.sbc");

                            string text = System.IO.File.ReadAllText(PathHelper.GetSavePath(RequestServer, item) + @"\Sandbox.sbc", new UTF8Encoding(false));
                            text = text.Replace("<AutoSaveInMinutes>0</AutoSaveInMinutes>",
                                "<AutoSaveInMinutes>" + RequestServer.AutoSaveInMinutes + "</AutoSaveInMinutes>");

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
        [APIServerAccess("MAP-SCM", "SERVER_MAP_SE_CREATE")]
        public ActionResult SECreateMap()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            // ** PARSING / ACCESS **
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
            ServiceState serviceState = srvPrv.GetState(RequestServer);

            if (serviceState != ServiceState.Stopped && serviceState != ServiceState.Unknow)
            {
                ServiceHelper.StopServiceAndWait(RequestServer);
                Thread.Sleep(5000);
                ServiceHelper.KillService(RequestServer);
            }

            SEServerConfigHelper config = new SEServerConfigHelper();
            config.Load(RequestServer);

            config.ScenarioType = SubTypeId;
            config.AsteroidAmount = AsteroidAmount;
            config.ProceduralDensity = ProceduralDensity;
            config.ProceduralSeed = ProceduralSeed;
            config.SaveName = string.Empty;

            config.Save(RequestServer);

            ServiceHelper.StartService(RequestServer);

            return Content(XMLMessage.Success("MAP-SCM-OK", "The map have been created, the server is (re)starting ...").ToString());
        }

        // POST: API/Map/MECreateMap
        [HttpPost]
        [APIServerAccess("MAP-MCM", "SERVER_MAP_ME_CREATE")]
        public ActionResult MECreateMap()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(CurrentContext);

            // ** PARSING / ACCESS **
            MESubTypeId SubTypeId;
            if (string.IsNullOrWhiteSpace(Request.Form["SubTypeId"]))
                return Content(XMLMessage.Error("MAP-MCM-MISSTI", "The SubTypeId field must be provided").ToString());
            if (!Enum.TryParse(Request.Form["SubTypeId"], out SubTypeId))
                return Content(XMLMessage.Error("MAP-MCM-MISSTI", "The SubTypeId is invalid").ToString());

            // ** PROCESS **
            ServiceState serviceState = srvPrv.GetState(RequestServer);

            if (serviceState != ServiceState.Stopped && serviceState != ServiceState.Unknow)
            {
                ServiceHelper.StopServiceAndWait(RequestServer);
                Thread.Sleep(5000);
                ServiceHelper.KillService(RequestServer);
            }

            MEServerConfigHelper config = new MEServerConfigHelper();
            config.Load(RequestServer);

            config.ScenarioType = SubTypeId;

            config.SaveName = string.Empty;

            config.Save(RequestServer);

            ServiceHelper.StartService(RequestServer);

            return Content(XMLMessage.Success("MAP-MCM-OK", "The map have been created, the server is (re)starting ...").ToString());
        }

        // POST: API/Map/UploadMap        
        [HttpPost]
        [APIServerAccess("MAP-UPM", "SERVER_MAP_SE_UPLOAD", "SERVER_MAP_ME_UPLOAD")]
        public ActionResult UploadMap(HttpPostedFileBase ZipFile)
        {
            // ** PARSING **
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

                if (Directory.Exists(PathHelper.GetSavePath(RequestServer, dirName)) && System.IO.File.Exists(PathHelper.GetSavePath(RequestServer, dirName) + @"\Sandbox.sbc"))
                {
                    int i = 2;
                    string testDir;
                    do
                    {
                        testDir = dirName + " (" + i + ")";
                        i++;
                    } while (Directory.Exists(PathHelper.GetSavePath(RequestServer, testDir)) && System.IO.File.Exists(PathHelper.GetSavePath(RequestServer, testDir) + @"\Sandbox.sbc"));
                    dirName = testDir;
                }

                if (!Directory.Exists(PathHelper.GetSavePath(RequestServer, dirName)))
                    Directory.CreateDirectory(PathHelper.GetSavePath(RequestServer, dirName));

                if (rootZip)
                {
                    zip.ExtractAll(PathHelper.GetSavePath(RequestServer, dirName), ExtractExistingFileAction.OverwriteSilently);
                }
                else
                {
                    foreach (ZipEntry item in zip.EntriesSorted)
                    {
                        if (item.FileName.StartsWith(rootPath))
                        {
                            item.FileName = item.FileName.Substring(rootPath.Length);
                            item.Extract(PathHelper.GetSavePath(RequestServer, dirName), ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }

            }
            return Content(XMLMessage.Success("MAP-UPM-OK", "The map have been uploaded").ToString());
        }

    }
}