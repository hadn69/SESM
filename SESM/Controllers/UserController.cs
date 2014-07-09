using System.Web.Mvc;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.View.User;
using SESM.Tools;

namespace SESM.Controllers
{
    [LoggedOnly]
    [SuperAdmin]
    public class UserController : Controller
    {
        private readonly DataContext _context = new DataContext();

        [HttpGet]
        public ActionResult Index()
        {
            UserProvider usrPrv = new UserProvider(_context);
            return View(usrPrv.GetUsers());
        }
        
        [HttpGet]
        public ActionResult Delete(int id)
        {
            EntityUser user = Session["User"] as EntityUser;
            if (user.Id != id)
            {
                UserProvider usrPrv = new UserProvider(_context);
                EntityUser usr = usrPrv.GetUser(id);
                if (usr != null)
                    usrPrv.RemoveUser(usr);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            UserProvider usrPrv = new UserProvider(_context);

            EntityUser user = usrPrv.GetUser(id);
            UserViewModel model = new UserViewModel();
            if (user == null)
                return RedirectToAction("Index");
            model.Email = user.Email;
            model.Login = user.Login;
            model.Password = "";
            model.IsAdmin = user.IsAdmin;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserProvider usrPrv = new UserProvider(_context);

                EntityUser user = usrPrv.GetUser(id);
                user.Email = model.Email;
                user.Login = model.Login;
                user.IsAdmin = model.IsAdmin;
                if (!(model.Password == null || model.Password == string.Empty))
                {
                    user.Password = HashHelper.MD5Hash(model.Password);
                }
                usrPrv.UpdateUser(user);

                return RedirectToAction("Index");    
            }

            return View(model);
        }
    }
}
