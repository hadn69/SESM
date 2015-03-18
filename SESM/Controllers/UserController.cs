using System.Web.Mvc;
using SESM.Controllers.ActionFilters;

namespace SESM.Controllers
{
    [SuperAdmin]
    public class UserController : Controller
    {
        // GET : /Users/
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }

}
