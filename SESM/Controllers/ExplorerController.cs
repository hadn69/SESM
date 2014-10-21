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
            string path = Request.QueryString["path"];

            path = PathHelper.GetInstancePath(server) + path; 

            if (!Directory.Exists(path))
            {
                xmlResponse.Add(new XElement("Type", "Error"));
                xmlResponse.Add(new XElement("Message", "The directory don't exist"));
                return Content(xmlResponse.ToString());
            }
            xmlResponse.Add(new XElement("Type", "Success"));
            XElement data = new XElement("Content");
            xmlResponse.Add(data);
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                data.Add(new XElement("Item",new XElement("Type","Directory"), new XElement("Name", PathHelper.GetLastLeaf(directory))));
            }

            string[] files = Directory.GetFiles(path);
            foreach(string directory in directories)
            {
                data.Add(new XElement("Item", new XElement("Type", "Directory"), new XElement("Name", PathHelper.GetLastLeaf(directory))));
            }
              
            return Content(xmlResponse.ToString());
        }
    }
}