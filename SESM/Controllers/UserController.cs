using System.Web.Mvc;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;

namespace SESM.Controllers
{
    [LoggedOnly]
    [SuperAdmin]
    public class UserController : Controller
    {
        private DataContext context = new DataContext();

        [HttpGet]
        public ActionResult Index()
        {
            UserProvider usrPrv = new UserProvider(context);
            return View(usrPrv.GetUsers());
        }
        
        [HttpGet]
        public ActionResult Delete(int id)
        {
            EntityUser user = Session["User"] as EntityUser;
            if (user.Id != id)
            {
                UserProvider usrPrv = new UserProvider(context);
                EntityUser usr = usrPrv.GetUser(id);
                if (usr != null)
                    usrPrv.RemoveUser(usr);
            }
            return RedirectToAction("Index");
        }

    }
}
