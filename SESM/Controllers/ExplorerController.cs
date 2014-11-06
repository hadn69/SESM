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
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    public class ExplorerController : Controller
    {
        private readonly DataContext _context = new DataContext();

        // GET: Explorer
        public ActionResult Index(int id)
        {
            ViewBag.id = id;
            return View();
        }

        private bool SecurityCheck(EntityServer server)
        {
            EntityUser user = Session["User"] as EntityUser;
            if (user == null || server == null)
                return false;

            ServerProvider srvPrv = new ServerProvider(_context);

            AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, server.Id);
            if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
            {
                return true;
            }
            return false;
        }

        public ActionResult GetDirectoryContent(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if (server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if (!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string path = Request.Form["path"];
            if (string.IsNullOrWhiteSpace(path))
                path = string.Empty;
            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));
            if (path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            if (!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The directory isn't accessible for you, bad boy !").ToString());

            if (!Directory.Exists(path))
                return Content(new XmlResponse(XmlResponseType.Error, "The directory don't exist").ToString());

            XmlResponse resp = new XmlResponse();

            List<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach (NodeInfo directory in directories)
            {
                resp.AddToContent(new XElement("Item", new XElement("Type", "Directory"),
                    new XElement("Name", directory.Name),
                    new XElement("Size", directory.Size),
                    new XElement("Timestamp", directory.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            List<NodeInfo> files = FSHelper.GetFiles(path);

            foreach (NodeInfo file in files)
            {
                resp.AddToContent(new XElement("Item", new XElement("Type", "File"),
                    new XElement("Name", file.Name),
                    new XElement("Size", file.Size),
                    new XElement("Timestamp", file.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            return Content(resp.ToString());
        }

        public ActionResult GetDirectoryDirectories(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if (server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if (!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string path = Request.Form["path"];
            if (string.IsNullOrWhiteSpace(path))
                path = string.Empty;
            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));
            if (path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            if (!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The directory isn't accessible for you, bad boy !").ToString());

            if (!Directory.Exists(path))
                return Content(new XmlResponse(XmlResponseType.Error, "The directory don't exist").ToString());

            XmlResponse resp = new XmlResponse();

            List<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach (NodeInfo directory in directories)
            {
                resp.AddToContent(new XElement("Item", new XElement("Type", "Directory"),
                    new XElement("Name", directory.Name),
                    new XElement("Size", directory.Size),
                    new XElement("Timestamp", directory.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }
            return Content(resp.ToString());
        }

        public ActionResult Delete(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if (server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if (!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string pathsFull = Request.Form["paths"];

            string[] paths = pathsFull.Split(':');

            for (int i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                    paths[i] = string.Empty;
                paths[i] = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), paths[i]));

                if (!paths[i].Contains(PathHelper.GetInstancePath(server)))
                    return
                        Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

                if (!(Directory.Exists(paths[i]) || System.IO.File.Exists(paths[i])))
                    return Content(new XmlResponse(XmlResponseType.Error, "The object don't exist").ToString());
            }
            foreach (string item in paths)
            {
                if (Directory.Exists(item))
                {
                    try
                    {
                        Directory.Delete(item, true);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (System.IO.File.Exists(item))
                {
                    try
                    {
                        System.IO.File.Delete(item);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return Content(new XmlResponse(XmlResponseType.Success, "Object(s) deleted").ToString());
        }

        public ActionResult Rename(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if (server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if (!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string sourcePath = Request.Form["sourcePath"];
            string newName = Request.Form["newName"];
            string oldname;

            sourcePath = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), sourcePath));

            if (!sourcePath.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

            if (!(Directory.Exists(sourcePath) || System.IO.File.Exists(sourcePath)))
                return Content(new XmlResponse(XmlResponseType.Error, "The object don't exist").ToString());

            if (!Regex.IsMatch(newName, "^[^:/\\*?\"<>|]*$"))
                return Content(new XmlResponse(XmlResponseType.Error, "Invalid new name, the new name of the object can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            if (Directory.Exists(sourcePath))
            {
                DirectoryInfo di = new DirectoryInfo(sourcePath);
                oldname = di.Name;
                sourcePath = di.Parent.FullName;
                if (Directory.Exists(sourcePath + "\\" + newName))
                    return Content(new XmlResponse(XmlResponseType.Error, "Invalid new name, a directory with the same name already exist").ToString());
                try
                {
                    Directory.Move(sourcePath + "\\" + oldname, sourcePath + "\\" + newName);
                    return Content(new XmlResponse(XmlResponseType.Success, "Directory renamed successfuly").ToString());
                }
                catch (Exception ex)
                {
                    return Content(new XmlResponse(XmlResponseType.Error, "Directory failed to be renamed (ex : " + ex.Message + ")").ToString());
                }
            }
            else if (System.IO.File.Exists(sourcePath))
            {
                FileInfo fi = new FileInfo(sourcePath);
                oldname = fi.Name;
                sourcePath = fi.DirectoryName;
                if (System.IO.File.Exists(sourcePath + "\\" + newName))
                    return Content(new XmlResponse(XmlResponseType.Error, "Invalid new name, a file with the same name already exist").ToString());
                try
                {
                    System.IO.File.Move(sourcePath + "\\" + oldname, sourcePath + "\\" + newName);
                    return Content(new XmlResponse(XmlResponseType.Success, "File renamed successfuly").ToString());
                }
                catch (Exception ex)
                {
                    return Content(new XmlResponse(XmlResponseType.Error, "File failed to be renamed (ex : " + ex.Message + ")").ToString());
                }
            }
            return Content(new XmlResponse(XmlResponseType.Error, "Object of the 3rd Types (please report)").ToString());
        }

        public ActionResult Download(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if (server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if (!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string pathsFull = Request.Form["paths"];

            string[] paths = pathsFull.Split(':');

            for (int i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                    paths[i] = string.Empty;
                paths[i] = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), paths[i]));

                if (!paths[i].Contains(PathHelper.GetInstancePath(server)))
                    return
                        Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

                if (!(Directory.Exists(paths[i]) || System.IO.File.Exists(paths[i])))
                    return Content(new XmlResponse(XmlResponseType.Error, "The object don't exist").ToString());
            }


            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    foreach (string item in paths)
                    {
                        if((Directory.Exists(item)))
                        {
                            zip.AddDirectory(item, new DirectoryInfo(item).Name);
                        }
                        else
                        {
                            zip.AddFile(item, string.Empty);
                        }
                    }
                    Response.Clear();
                    Response.ContentType = "application/zip";
                    string zipName = string.Empty;
                    zipName = Directory.Exists(paths[0]) ? new DirectoryInfo(paths[0]).Parent.Name : new FileInfo(paths[0]).Directory.Name;
                    if (zipName == PathHelper.GetFSDirName(server))
                    {
                        Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", server.Name + "-" +  DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));
                    }
                    else
                    {
                        Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", server.Name + "-" + zipName + "-" + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));
                    }
                    zip.Save(Response.OutputStream);
                    Response.End();
                    return RedirectToAction("Index", new { id = id });
                }
            }
            catch (Exception ex)
            {
                return Content(new XmlResponse(XmlResponseType.Error, "zip failed to be created (ex : " + ex.Message + ")").ToString());
            }
        }

        public ActionResult NewFolder(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if (server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if (!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string path = Request.Form["path"];

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));

            if (!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

            if ((Directory.Exists(path) || System.IO.File.Exists(path)))
                return Content(new XmlResponse(XmlResponseType.Error, "An object with the same name already exist").ToString());

            if (!Regex.IsMatch(PathHelper.GetLastLeaf(path), "^[^:/\\*?\"<>|]*$"))
                return Content(new XmlResponse(XmlResponseType.Error, "Invalid name, the name of the folder can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            try
            {
                Directory.CreateDirectory(path);
                return Content(new XmlResponse(XmlResponseType.Success, "Folder created").ToString());
            }
            catch (Exception ex)
            {
                return Content(new XmlResponse(XmlResponseType.Error, "An error occurend while creating the folder (exception : " + ex.Message + ")").ToString());
            }
        }

        public ActionResult NewFile(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if (server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if (!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string path = Request.Form["path"];

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));

            if (!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

            if ((Directory.Exists(path) || System.IO.File.Exists(path)))
                return Content(new XmlResponse(XmlResponseType.Error, "An object with the same name already exist").ToString());

            if (!Regex.IsMatch(PathHelper.GetLastLeaf(path), "^[^:/\\*?\"<>|]*$"))
                return Content(new XmlResponse(XmlResponseType.Error, "Invalid name, the name of the file can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            try
            {
                System.IO.File.Create(path);
                return Content(new XmlResponse(XmlResponseType.Success, "File created").ToString());
            }
            catch (Exception ex)
            {
                return Content(new XmlResponse(XmlResponseType.Error, "An error occurend while creating the file (exception : " + ex.Message + ")").ToString());
            }
        }

        public ActionResult Upload(int id, ICollection<HttpPostedFileBase> files)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if(server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if(!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string path = Request.Form["path"];
            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

            bool overwriteFiles = Request.Form["overwrite"] == "on";

            bool extractZip = Request.Form["extract"] == "on";
            
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
                return Content(new XmlResponse(XmlResponseType.Success, "File(s) uploaded").ToString());
            }
            catch(Exception ex)
            {
                return Content(new XmlResponse(XmlResponseType.Error, "An error occurend while extracting the zip (exception : " + ex.Message + ")").ToString());
            }
        }

        public ActionResult GetFileContent(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if(server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if(!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string path = Request.Form["path"];
            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

            if (!System.IO.File.Exists(path))
                return Content(new XmlResponse(XmlResponseType.Error, "File doesn't exist").ToString());

        }
    }
}