using System.Linq;
using System.Web.Mvc;
using SESM.DAL;

namespace SESM.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }
	}
}