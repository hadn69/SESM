using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class APIExplorer : Controller
    {
        private readonly DataContext _context = new DataContext();

        // POST: API/Explorer/GetDirectoryContent
        [HttpPost]
        public ActionResult GetDirectoryContent()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);
            int serverId = -1;

            // ** PARSING / ACCESS **
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if(server == null)
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The server doesn't exist").ToString());

            string path = Request.Form["Path"];
            if(string.IsNullOrWhiteSpace(path))
                path = string.Empty;

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));
            if(path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            if(!srvPrv.SecurityCheck(server, user))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The directory isn't accessible for you, bad boy !").ToString());

            if(!Directory.Exists(path))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The directory don't exist").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("XXX-XXX-XXX");

            IEnumerable<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach(NodeInfo directory in directories)
            {
                response.AddToContent(new XElement("Item",
                    new XElement("Type", "Directory"),
                    new XElement("Name", directory.Name),
                    new XElement("Size", directory.Size),
                    new XElement("Timestamp", directory.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            IEnumerable<NodeInfo> files = FSHelper.GetFiles(path);

            foreach(NodeInfo file in files)
            {
                response.AddToContent(new XElement("Item",
                    new XElement("Type", "File"),
                    new XElement("Name", file.Name),
                    new XElement("Size", file.Size),
                    new XElement("Timestamp", file.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            return Content(response.ToString());
        }

        // POST: API/Explorer/GetDirectoryDirectories
        [HttpPost]
        public ActionResult GetDirectoryDirectories()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
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

            string path = Request.Form["Path"];
            if(string.IsNullOrWhiteSpace(path))
                path = string.Empty;

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));
            if(path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            if(!srvPrv.SecurityCheck(server, user))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The directory isn't accessible for you, bad boy !").ToString());

            if(!Directory.Exists(path))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The directory don't exist").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("XXX-XXX-XXX");

            IEnumerable<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach(NodeInfo directory in directories)
            {
                response.AddToContent(new XElement("Item",
                    new XElement("Type", "Directory"),
                    new XElement("Name", directory.Name),
                    new XElement("Size", directory.Size),
                    new XElement("Timestamp", directory.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }
            return Content(response.ToString());
        }

        // POST: API/Explorer/Delete
        [HttpPost]
        public ActionResult Delete()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
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

            if(!srvPrv.SecurityCheck(server, user))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            string[] paths = Request.Form["Paths"].Split(':');

            for(int i = 0; i < paths.Length; i++)
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
            return Content(XMLMessage.Success("XXX-XXX-XXX", "Object(s) deleted").ToString());
        }

        // POST: API/Explorer/Rename
        [HttpPost]
        public ActionResult Rename()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
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

            if(!srvPrv.SecurityCheck(server, user))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            string path = Request.Form["Path"];
            string newName = Request.Form["NewName"];
            string oldname;

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The object isn't accessible for you, bad boy !").ToString());

            if(!(Directory.Exists(path) || System.IO.File.Exists(path)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The object don't exist").ToString());

            if(!Regex.IsMatch(newName, "^[^:/\\*?\"<>|]*$"))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "Invalid new name, the new name of the object can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            // ** PROCESS **
            if(Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                oldname = di.Name;
                path = di.Parent.FullName;
                if(Directory.Exists(path + "\\" + newName))
                    return Content(XMLMessage.Error("XXX-XXX-XXX", "Invalid new name, a directory with the same name already exist").ToString());
                try
                {
                    Directory.Move(path + "\\" + oldname, path + "\\" + newName);
                }
                catch(Exception ex)
                {
                    return Content(XMLMessage.Error("XXX-XXX-XXX", "Directory failed to be renamed (ex : " + ex.Message + ")").ToString());
                }
                return Content(XMLMessage.Success("XXX-XXX-XXX", "Directory renamed successfuly").ToString());
            }
            else if(System.IO.File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                oldname = fi.Name;
                path = fi.DirectoryName;
                if(System.IO.File.Exists(path + "\\" + newName))
                    return Content(XMLMessage.Error("XXX-XXX-XXX", "Invalid new name, a file with the same name already exist").ToString());
                try
                {
                    System.IO.File.Move(path + "\\" + oldname, path + "\\" + newName);
                    
                }
                catch(Exception ex)
                {
                    return Content(XMLMessage.Error("XXX-XXX-XXX", "File failed to be renamed (ex : " + ex.Message + ")").ToString());
                }
                return Content(XMLMessage.Success("XXX-XXX-XXX", "File renamed successfuly").ToString());
            }
            return Content(XMLMessage.Error("XXX-XXX-XXX", "Object of the 3rd Types (please report this ASAP)").ToString());
        }

        // POST: API/Explorer/Download
        [HttpPost]
        public ActionResult Download()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
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

            if(!srvPrv.SecurityCheck(server, user))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "You don't have access to this server").ToString());

            string[] paths = Request.Form["Paths"].Split(':');

            for(int i = 0; i < paths.Length; i++)
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
                using(ZipFile zip = new ZipFile())
                {
                    foreach(string item in paths)
                    {
                        if((Directory.Exists(item)))
                            zip.AddDirectory(item, new DirectoryInfo(item).Name);
                        else
                            zip.AddFile(item, string.Empty);
                    }
                    Response.Clear();
                    Response.ContentType = "application/zip";
                    string zipName = string.Empty;
                    zipName = Directory.Exists(paths[0]) ? new DirectoryInfo(paths[0]).Parent.Name : new FileInfo(paths[0]).Directory.Name;
                    if(zipName == PathHelper.GetFSDirName(server))
                    {
                        Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", server.Name + "-" 
                            + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));
                    }
                    else
                    {
                        Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", server.Name + "-" + zipName + "-"
                            + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));
                    }
                    zip.Save(Response.OutputStream);
                    Response.End();
                    return null;
                }
            }
            catch(Exception ex)
            {
                return Content(XMLMessage.Error("XXX-XXX-XXX", "zip failed to be created (ex : " + ex.Message + ")").ToString());
            }
        }

        // POST: API/Explorer/NewFolder
        [HttpPost]
        public ActionResult NewFolder()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
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

            if(!srvPrv.SecurityCheck(server, user))
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
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

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
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), Request.Form["Path"]));

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The object isn't accessible for you, bad boy !").ToString());

            bool overwriteFiles = Request.Form["overwrite"] == "True";
            bool extractZip = Request.Form["extract"] == "True";

            // ** PROCESS **
            try
            {
                foreach(HttpPostedFileBase file in files)
                {
                    if(extractZip && file.FileName.Split('.').Last().ToLower() == "zip")
                    {
                        using(ZipFile zip = ZipFile.Read(file.InputStream))
                        {
                            zip.ExtractAll(path,
                                overwriteFiles
                                    ? ExtractExistingFileAction.OverwriteSilently
                                    : ExtractExistingFileAction.Throw);
                        }
                    }
                    else
                    {
                        FSHelper.SaveStream(file.InputStream, path + "\\" + file.FileName, overwriteFiles);
                    }
                }
            }
            catch(Exception ex)
            {
                return Content(XMLMessage.Error("XXX-XXX-XXX", "An error occurend while extracting the zip (exception : " + ex.Message + ")").ToString());
            }
            return Content(XMLMessage.Success("XXX-XXX-XXX", "File(s) uploaded").ToString());
        }

        // POST: API/Explorer/GetFileContent
        [HttpPost]
        public ActionResult GetFileContent()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;
            ServerProvider srvPrv = new ServerProvider(_context);

            // ** PARSING / ACCESS **
            int serverId = -1;
            if(string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID field must be provided").ToString());

            if(!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("XXX-XXX-XXX", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

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

            response.AddToContent(new XCData(content));
            return Content(response.ToString());
        }
    }
}