using System.Web.Mvc;

namespace SESM.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Home/
        public ActionResult Versions()
        {
            return View();
        }
    }
}