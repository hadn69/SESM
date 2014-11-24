using System.Web.Mvc;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Account;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    public class AccountController : Controller
    {
        readonly DataContext _context = new DataContext();

        //
        // GET: /Account/Login
        [HttpGet]
        [GuestOnly]
        public ActionResult Login()
        {
            
            return View();
        }

        //
        // GET: /Account/Logout
        [HttpGet]
        [LoggedOnly]
        public ActionResult Logout()
        {
            Session.Abandon();

            return RedirectToAction("Index", "Home").Success("Logout Successfull");
        }

        //
        // GET: /Account/Manage
        [HttpGet]
        [LoggedOnly]
        public ActionResult Manage()
        {
            EntityUser sUser = Session["User"] as EntityUser;
            UserProvider usrPrv = new UserProvider(_context);
            EntityUser user = usrPrv.GetUser(sUser.Id);

            ManageViewModel model = new ManageViewModel();
            model.Email = user.Email;
            return View(model);
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [LoggedOnly]
        public ActionResult Manage(ManageViewModel model)
        {
            if (ModelState.IsValid)
            {

                EntityUser sUser = Session["User"] as EntityUser;
                UserProvider usrPrv = new UserProvider(_context);
                EntityUser user = usrPrv.GetUser(sUser.Id);

                if (model.OldPassword != null)
                {
                    if (HashHelper.MD5Hash(model.OldPassword) != user.Password)
                    {
                        ModelState.AddModelError("Old Password Mismatch", "Old Password Mismatch");
                        return View(model);
                    }
                    if (model.NewPassword != model.RetypedPassword)
                    {
                        ModelState.AddModelError("Password Mismatch", "Password Mismatch");
                        return View(model);
                    }
                    user.Password = HashHelper.MD5Hash(model.NewPassword);


                }
                user.Email = model.Email;
                usrPrv.UpdateUser(user);
                return View(model).Success("Account Updated");
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