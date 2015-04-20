using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Ionic.Zip;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;
using SESM.Tools.Helpers;

namespace SESM.Controllers.API
{
    public class APIExplorerController : Controller
    {
        private readonly DataContext _context = new DataContext();

        // POST: API/Explorer/GetDirectoryContent
        [HttpPost]
        public ActionResult GetDirectoryContent()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);
            int serverId = -1;

            // ** PARSING / ACCESS **
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("EXP-GDC-MISID", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("EXP-GDC-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if(server == null)
                return Content(XMLMessage.Error("EXP-GDC-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("EXP-GDC-NOACCESS", "You don't have access to this server").ToString());

            string path = Request.Form["Path"];
            if(string.IsNullOrWhiteSpace(path))
                path = string.Empty;

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));
            if(path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("EXP-GDC-BADPTH", "The directory isn't accessible for you, bad boy !").ToString());

            if(!Directory.Exists(path))
                return Content(XMLMessage.Error("EXP-GDC-BADDIR", "The directory don't exist").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("EXP-GDC-OK");

            IEnumerable<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach(NodeInfo directory in directories)
            {
                response.AddToContent(new XElement("Item",
                    new XElement("Type", "Directory"),
                    new XElement("Name", directory.Name),
                    new XElement("Size", directory.Size),
                    new XElement("Date", directory.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            IEnumerable<NodeInfo> files = FSHelper.GetFiles(path);

            foreach(NodeInfo file in files)
            {
                response.AddToContent(new XElement("Item",
                    new XElement("Type", "File"),
                    new XElement("Name", file.Name),
                    new XElement("Size", file.Size),
                    new XElement("Date", file.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            return Content(response.ToString());
        }

        // POST: API/Explorer/GetDirectoryDirectories
        [HttpPost]
        public ActionResult GetDirectoryDirectories()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("EXP-GDD-MISID", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("EXP-GDD-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if(server == null)
                return Content(XMLMessage.Error("EXP-GDD-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("EXP-GDD-NOACCESS", "You don't have access to this server").ToString());

            string path = Request.Form["Path"];
            if(string.IsNullOrWhiteSpace(path))
                path = string.Empty;

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));
            if(path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("EXP-GDD-BADPTH", "The directory isn't accessible for you, bad boy !").ToString());

            if(!Directory.Exists(path))
                return Content(XMLMessage.Error("EXP-GDD-BADDIR", "The directory don't exist").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("EXP-GDD-OK");

            IEnumerable<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach(NodeInfo directory in directories)
            {
                response.AddToContent(new XElement("Item",
                    new XElement("Name", directory.Name)));
            }
            return Content(response.ToString());
        }

        // POST: API/Explorer/Delete
        [HttpPost]
        public ActionResult Delete()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("EXP-DEL-MISID", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("EXP-DEL-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if(server == null)
                return Content(XMLMessage.Error("EXP-DEL-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("EXP-DEL-NOACCESS", "You don't have access to this server").ToString());

            string[] paths = Request.Form["Paths"].Split(':');

            for(int i = 0; i < paths.Length; i++)
            {
                if(string.IsNullOrWhiteSpace(paths[i]))
                    paths[i] = string.Empty;
                paths[i] = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), paths[i]));

                if(!paths[i].Contains(PathHelper.GetInstancePath(server)))
                    return Content(XMLMessage.Error("EXP-DEL-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

                if(!(Directory.Exists(paths[i]) || System.IO.File.Exists(paths[i])))
                    return Content(XMLMessage.Error("EXP-DEL-BADEX", "The object don't exist").ToString());
            }

            // ** PROCESS **
            foreach(string item in paths)
            {
                if(Directory.Exists(item))
                {
                    try
                    {
                        Directory.Delete(item, true);
                    }
                    catch(Exception)
                    {
                    }
                }
                else if(System.IO.File.Exists(item))
                {
                    try
                    {
                        System.IO.File.Delete(item);
                    }
                    catch(Exception)
                    {
                    }
                }
            }
            return Content(XMLMessage.Success("EXP-DEL-OK", "Object(s) deleted").ToString());
        }

        // POST: API/Explorer/Rename
        [HttpPost]
        public ActionResult Rename()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("EXP-RNM-MISID", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("EXP-RNM-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if(server == null)
                return Content(XMLMessage.Error("EXP-RNM-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("EXP-RNM-NOACCESS", "You don't have access to this server").ToString());

            string path = Request.Form["Path"];
            string newName = Request.Form["NewName"];
            string oldname;

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("EXP-RNM-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

            if(!(Directory.Exists(path) || System.IO.File.Exists(path)))
                return Content(XMLMessage.Error("EXP-RNM-BADEX", "The object don't exist").ToString());

            if(!Regex.IsMatch(newName, "^[^:/\\*?\"<>|]*$"))
                return Content(XMLMessage.Error("EXP-RNM-BADNAM", "Invalid new name, the new name of the object can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            // ** PROCESS **
            if(Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                oldname = di.Name;
                path = di.Parent.FullName;
                if(Directory.Exists(path + "\\" + newName) || System.IO.File.Exists(path + "\\" + newName))
                    return Content(XMLMessage.Error("EXP-RNM-DIREX", "Invalid new name, a directory with the same name already exist").ToString());
                try
                {
                    Directory.Move(path + "\\" + oldname, path + "\\" + newName);
                }
                catch(Exception ex)
                {
                    return Content(XMLMessage.Error("EXP-RNM-DEX", "Directory failed to be renamed (ex : " + ex.Message + ")").ToString());
                }
                return Content(XMLMessage.Success("EXP-RNM-OK", "Directory renamed successfuly").ToString());
            }
            else if(System.IO.File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                oldname = fi.Name;
                path = fi.DirectoryName;
                if(Directory.Exists(path + "\\" + newName) || System.IO.File.Exists(path + "\\" + newName))
                    return Content(XMLMessage.Error("EXP-RNM-FILEX", "Invalid new name, a file with the same name already exist").ToString());
                try
                {
                    if(SESMConfigHelper.BlockDll && newName.Split('.').Last().ToLower() == "dll" && path.Contains(PathHelper.GetInstancePath(server) + @"Mods"))
                        return Content(XMLMessage.Error("EXP-RNM-BADDLL", "Invalid new name, you can't rename a file to a .dll one").ToString());

                    System.IO.File.Move(path + "\\" + oldname, path + "\\" + newName);
                    
                }
                catch(Exception ex)
                {
                    return Content(XMLMessage.Error("EXP-RNM-FEX", "File failed to be renamed (ex : " + ex.Message + ")").ToString());
                }
                return Content(XMLMessage.Success("EXP-RNM-OK", "File renamed successfuly").ToString());
            }
            return Content(XMLMessage.Error("EXP-RNM-TRD", "Object of the 3rd Types (please report this ASAP)").ToString());
        }

        // POST: API/Explorer/Download
        [HttpPost]
        public ActionResult Download()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if(server == null)
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            string[] paths = Request.Form["Paths"].Split(':');

            if (paths.Length == 1 && String.IsNullOrWhiteSpace(paths[0]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The Paths field must be provided").ToString());

            for (int i = 0; i < paths.Length; i++)
            {
                if(string.IsNullOrWhiteSpace(paths[i]))
                    paths[i] = string.Empty;
                paths[i] = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), paths[i]));

                if(!paths[i].Contains(PathHelper.GetInstancePath(server)))
                    return Content(XMLMessage.Error("XXX-XXX-XXX", "The object isn't accessible for you, bad boy !").ToString());

                if(!(Directory.Exists(paths[i]) || System.IO.File.Exists(paths[i])))
                    return Content(XMLMessage.Error("XXX-XXX-XXX", "The object don't exist").ToString());
            }

            // ** PROCESS **
            try
            {
                Response.Clear();
                if (paths.Length == 1)
                {

                    if (System.IO.File.Exists(paths[0])) // File
                    {
                        Response.ContentType = MimeMapping.GetMimeMapping(PathHelper.GetLastLeaf(paths[0]));
                        Response.AddHeader("Content-Disposition",
                            String.Format("attachment; filename={0}", PathHelper.GetLastLeaf(paths[0])));

                        Response.TransmitFile(paths[0]);
                        Response.End();
                        return null;
                    }
                    else // Directory
                    {
                        Response.ContentType = "application/zip";

                        Response.AddHeader("Content-Disposition",
                            String.Format("attachment; filename={0}", server.Name + "-" + PathHelper.GetLastLeaf(paths[0]) + "-"
                                                                      + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));
                        using (ZipFile zip = new ZipFile())
                        {
                            zip.AddDirectory(paths[0]);

                            zip.Save(Response.OutputStream);
                            Response.End();
                            return null;
                        }
                    }
                }
                else
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        foreach (string item in paths)
                        {
                            if ((Directory.Exists(item))) // Folder
                                zip.AddDirectory(item, new DirectoryInfo(item).Name);
                            else // File
                                zip.AddFile(item, string.Empty);
                        }
                        Response.ContentType = "application/zip";
                        string zipName = string.Empty;
                        zipName = Directory.Exists(paths[0])
                            ? new DirectoryInfo(paths[0]).Parent.Name
                            : new FileInfo(paths[0]).Directory.Name;
                        if (zipName == PathHelper.GetFSDirName(server))
                        {
                            Response.AddHeader("Content-Disposition",
                                String.Format("attachment; filename={0}", server.Name + "-"
                                                                          + DateTime.Now.ToString("yyyyMMddHHmm") +
                                                                          ".zip"));
                        }
                        else
                        {
                            Response.AddHeader("Content-Disposition",
                                String.Format("attachment; filename={0}", server.Name + "-" + zipName + "-"
                                                                          + DateTime.Now.ToString("yyyyMMddHHmm") +
                                                                          ".zip"));
                        }
                        zip.Save(Response.OutputStream);
                        Response.End();
                        return null;
                    }
                }
            }
            catch(Exception ex)
            {
                return Content(XMLMessage.Error("XXX-XXX-XXX", "Error Occured (ex : " + ex.Message + ")").ToString());
            }
        }

        // POST: API/Explorer/NewFolder
        [HttpPost]
        public ActionResult NewFolder()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if(server == null)
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), Request.Form["Path"]));

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The object isn't accessible for you, bad boy !").ToString());

            if((Directory.Exists(path) || System.IO.File.Exists(path)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "An object with the same name already exist").ToString());

            if(!Regex.IsMatch(PathHelper.GetLastLeaf(path), "^[^:/\\*?\"<>|]*$"))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "Invalid name, the name of the folder can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            // ** PROCESS **
            try
            {
                Directory.CreateDirectory(path);
            }
            catch(Exception ex)
            {
                return Content(XMLMessage.Error("XXX-XXX-XXX", "An error occurend while creating the folder (exception : " + ex.Message + ")").ToString());
            }
            return Content(XMLMessage.Success("XXX-XXX-XXX", "Folder created").ToString());
        }

        // POST: API/Explorer/NewFile
        [HttpPost]
        public ActionResult NewFile()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), Request.Form["Path"]));

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The object isn't accessible for you, bad boy !").ToString());

            if((Directory.Exists(path) || System.IO.File.Exists(path)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "An object with the same name already exist").ToString());

            if(!Regex.IsMatch(PathHelper.GetLastLeaf(path), "^[^:/\\*?\"<>|]*$"))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "Invalid name, the name of the file can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            // ** PROCESS **
            try
            {
                System.IO.File.Create(path);
            }
            catch(Exception ex)
            {
                return Content(XMLMessage.Error("XXX-XXX-XXX", "An error occurend while creating the file (exception : " + ex.Message + ")").ToString());
            }
            return Content(XMLMessage.Success("XXX-XXX-XXX", "File created").ToString());
        }

        // POST: API/Explorer/Upload
        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> Files)
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), Request.Form["Path"]));

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The object isn't accessible for you, bad boy !").ToString());

            bool overwriteFiles = Request.Form["Overwrite"].ToLower() == "true";
            bool extractZip = Request.Form["Extract"].ToLower() == "true";

            bool blockDll = (SESMConfigHelper.BlockDll && !user.IsAdmin);

            // ** PROCESS **
            try
            {
                foreach(HttpPostedFileBase file in Files)
                {
                    if(extractZip && file.FileName.Split('.').Last().ToLower() == "zip")
                    {
                        using(ZipFile zip = ZipFile.Read(file.InputStream))
                        {
                            if (blockDll)
                            {
                                bool testDll = false;
                                foreach (ZipEntry item in zip)
                                {
                                    if ((path + item.FileName.Replace("/",@"\")).Contains(PathHelper.GetInstancePath(server) + @"Mods")
                                        && item.FileName.Split('.').Last().ToLower() == "dll")
                                    {
                                        testDll = true;
                                        break;
                                    }
                                }
                                if (testDll)
                                    continue;
                            }

                            zip.ExtractAll(path,
                                overwriteFiles
                                    ? ExtractExistingFileAction.OverwriteSilently
                                    : ExtractExistingFileAction.Throw);
                        }
                    }
                    else
                    {
                        if(blockDll && file.FileName.Split('.').Last().ToLower() == "dll" && path.Contains(PathHelper.GetInstancePath(server) + @"Mods"))
                            continue;
                        FSHelper.SaveStream(file.InputStream, path + "\\" + file.FileName, overwriteFiles);
                    }
                }
            }
            catch(Exception ex)
            {
                return Content(XMLMessage.Error("XXX-XXX-XXX", "An error occurend while writing file (exception : " + ex.Message + ")").ToString());
            }
            return Content(XMLMessage.Success("XXX-XXX-XXX", "File(s) uploaded").ToString());
        }

        // POST: API/Explorer/GetFileContent
        [HttpPost]
        public ActionResult GetFileContent()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), Request.Form["Path"]));

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The object isn't accessible for you, bad boy !").ToString());

            if(!System.IO.File.Exists(path))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "File doesn't exist").ToString());

            if(new FileInfo(path).Length >= (512 * 1024))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "File is too big").ToString());

            // ** PROCESS **
            string content = System.IO.File.ReadAllText(path);

            XMLMessage response = new XMLMessage("XXX-XXX-XXX");
            try
            {
                response.AddToContent(new XCData(content));
                return Content(response.ToString());
            }
            catch (Exception)
            {
                return Content(XMLMessage.Error("XXX-XXX-XXX", "Failed to read file").ToString());
            }
        }

        // POST: API/Explorer/SetFileContent
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetFileContent()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), Request.Form["Path"]));

            if (!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The object isn't accessible for you, bad boy !").ToString());

            if (!System.IO.File.Exists(path))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "File doesn't exist").ToString());

            //System.Collections.Specialized.NameValueCollection qs = Request.Unvalidated.QueryString;
            string data = Request.Form["Data"];

            // ** PROCESS **
            System.IO.File.WriteAllText(path, data, new UTF8Encoding(false));

            return Content(XMLMessage.Success("XXX-XXX-XXX", "Data writed").ToString());
        }
    }
}