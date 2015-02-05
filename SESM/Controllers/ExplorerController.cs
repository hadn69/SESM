using System.Web.Mvc;
using SESM.Controllers.ActionFilters;

namespace SESM.Controllers
{
    public class ExplorerController : Controller
    {
        [CheckAuth]
        public ActionResult Browse(int id)
        {
            return View();
        }
    }
}