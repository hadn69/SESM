using System.Web.Mvc;

namespace SESM.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //
        // GET: /Account/Manage
        [HttpGet]
        public ActionResult Manage()
        {
            return View();
        }
    }
}