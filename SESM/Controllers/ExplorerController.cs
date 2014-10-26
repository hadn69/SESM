using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
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

        public ActionResult GetDirectoryContent(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);

            if(server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if(!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string path = Request.Form["path"];
            if(string.IsNullOrWhiteSpace(path))
                path = string.Empty;
            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));
            if(path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The directory isn't accessible for you, bad boy !").ToString());

            if(!Directory.Exists(path))
                return Content(new XmlResponse(XmlResponseType.Error, "The directory don't exist").ToString());

            XmlResponse resp = new XmlResponse();

            List<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach(NodeInfo directory in directories)
            {
                resp.AddToContent(new XElement("Item", new XElement("Type", "Directory"),
                    new XElement("Name", directory.Name),
                    new XElement("Size", directory.Size),
                    new XElement("Timestamp", directory.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            List<NodeInfo> files = FSHelper.GetFiles(path);

            foreach(NodeInfo file in files)
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

            if(server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if(!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string path = Request.Form["path"];
            if(string.IsNullOrWhiteSpace(path))
                path = string.Empty;
            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), path));
            if(path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            if(!path.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The directory isn't accessible for you, bad boy !").ToString());

            if(!Directory.Exists(path))
                return Content(new XmlResponse(XmlResponseType.Error, "The directory don't exist").ToString());

            XmlResponse resp = new XmlResponse();

            List<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach(NodeInfo directory in directories)
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

            if(server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if(!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string pathsFull = Request.Form["paths"];

            string[] paths = pathsFull.Split(':');

            for(int i = 0; i < paths.Length; i++)
            {
                if(string.IsNullOrWhiteSpace(paths[i]))
                    paths[i] = string.Empty;
                paths[i] = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), paths[i]));

                if(!paths[i].Contains(PathHelper.GetInstancePath(server)))
                    return
                        Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

                if(!(Directory.Exists(paths[i]) || System.IO.File.Exists(paths[i])))
                    return Content(new XmlResponse(XmlResponseType.Error, "The object don't exist").ToString());
            }
            foreach (string item in paths)
            {
                if(Directory.Exists(item))
                {
                    try
                    {
                        Directory.Delete(item, true);
                    }
                    catch (Exception)
                    {
                    }
                }
                if(System.IO.File.Exists(item))
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

            if(server == null)
                return Content(new XmlResponse(XmlResponseType.Error, "The server doesn't exist").ToString());

            if(!SecurityCheck(server))
                return Content(new XmlResponse(XmlResponseType.Error, "You don't have access to this server").ToString());

            string sourcePath = Request.Form["sourcePath"];
            string newName = Request.Form["newName"];
            string oldname = string.Empty;
            sourcePath = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), sourcePath));

            if(!sourcePath.Contains(PathHelper.GetInstancePath(server)))
                return Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

            if(!(Directory.Exists(sourcePath) || System.IO.File.Exists(sourcePath)))
                return Content(new XmlResponse(XmlResponseType.Error, "The object don't exist").ToString());

            if (Directory.Exists(sourcePath))
            {
                DirectoryInfo di = new DirectoryInfo(sourcePath);
                oldname = di.Name;
                sourcePath = di.Parent.FullName;
            }

            /*
            string[] paths = pathsFull.Split(':');

            for(int i = 0; i < paths.Length; i++)
            {
                if(string.IsNullOrWhiteSpace(paths[i]))
                    paths[i] = string.Empty;
                paths[i] = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), paths[i]));

                if(!paths[i].Contains(PathHelper.GetInstancePath(server)))
                    return
                        Content(new XmlResponse(XmlResponseType.Error, "The object isn't accessible for you, bad boy !").ToString());

                if(!(Directory.Exists(paths[i]) || System.IO.File.Exists(paths[i])))
                    return Content(new XmlResponse(XmlResponseType.Error, "The object don't exist").ToString());
            }*/

            return Content(new XmlResponse(XmlResponseType.Success, "Object(s) deleted").ToString());
        }
    }
}