using System.Collections.Generic;
using System.Web.Mvc;
using SESM.DAL;
using SESM.DTO;

namespace SESM.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }
    }
}