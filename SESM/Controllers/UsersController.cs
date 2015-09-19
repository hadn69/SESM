using System.Web.Mvc;
using SESM.Controllers.ActionFilters;

namespace SESM.Controllers
{
    [HostAccess("USER_MANAGE")]
    public class UsersController : Controller
    {
        // GET : /Users/
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }

}
