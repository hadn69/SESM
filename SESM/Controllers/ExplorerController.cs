using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Xml.Linq;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    public class ExplorerController : Controller
    {
        private readonly DataContext _context = new DataContext();

        // GET: Explorer
        public ActionResult Index()
        {
            return View();
        }

        [CheckAuth]
        public ActionResult GetDirectoryContent(int id)
        {
            ServerProvider srvPrv = new ServerProvider(_context);
            EntityServer server = srvPrv.GetServer(id);
            XElement xmlResponse = new XElement("DirectoryContent");
            XDocument xdoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                xmlResponse
            );
            
            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(server), Request.QueryString["path"]));
            if (path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            if (!path.Contains(PathHelper.GetInstancePath(server)))
            {
                xmlResponse.Add(new XElement("Type", "Error"));
                xmlResponse.Add(new XElement("Message", "The directory isn't accessible for you, bad boy !"));
                return Content(xdoc.Declaration + xdoc.ToString());
            } 

            if (!Directory.Exists(path))
            {
                xmlResponse.Add(new XElement("Type", "Error"));
                xmlResponse.Add(new XElement("Message", "The directory don't exist"));
                return Content(xdoc.Declaration + xdoc.ToString());
            }

            xmlResponse.Add(new XElement("Type", "Success"));
            XElement data = new XElement("Data");
            xmlResponse.Add(data);

            List<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach (NodeInfo directory in directories)
            {
                data.Add(new XElement("Item",new XElement("Type","Directory"), 
                    new XElement("Name", directory.Name),
                    new XElement("Size", directory.Size),
                    new XElement("Timestamp", directory.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            List<NodeInfo> files = FSHelper.GetFiles(path);

            foreach (NodeInfo file in files)
            {
                data.Add(new XElement("Item", new XElement("Type", "File"),
                    new XElement("Name", file.Name),
                    new XElement("Size", file.Size),
                    new XElement("Timestamp", file.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            return Content(xdoc.Declaration + xdoc.ToString());
        }
    }
}