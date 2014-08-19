using System.Web.Mvc;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.User;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    [LoggedOnly]
    [SuperAdmin]
    [CheckLockout]
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
                if (usr == null)
                {
                    return RedirectToAction("Index").Warning("Unknow User");
                }
                string name = usr.Login;
                if (usr != null)
                    usrPrv.RemoveUser(usr);
                return RedirectToAction("Index").Success("User \"" + name + "\" deleted");
            }
            return RedirectToAction("Index").Warning("You can't delete yourself");
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
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = HashHelper.MD5Hash(model.Password);
                }
                usrPrv.UpdateUser(user);

                return RedirectToAction("Index").Success("User updated");    
            }

            return View(model);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
        
    }
}
