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
        // POST: /Account/Login
        [HttpPost]
        [GuestOnly]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserProvider usrPrv = new UserProvider(_context);
                EntityUser usr = usrPrv.GetUser(model.Login);
                if (usr != null && usr.Password == HashHelper.MD5Hash(model.Password))
                {
                    Session["User"] = usr;
                    return RedirectToAction("Index", "Server").Success("Login successful. Hello " + usr.Login);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }

            }
            return View(model);
        }

        //
        // GET: /Account/Register
        [HttpGet]
        [GuestOnly]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [GuestOnly]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password == model.RetypedPassword)
                {
                    
                    UserProvider usrPrv = new UserProvider(_context);
                    EntityUser usrTest = usrPrv.GetUser(model.Login);
                    if (usrTest == null)
                    {
                        EntityUser user = new EntityUser();
                        user.Login = model.Login;
                        user.Password = HashHelper.MD5Hash(model.Password);
                        user.Email = model.Email;

                        usrPrv.AddUser(user);

                        Session["User"] = user;
                        return RedirectToAction("Index", "Server").Success("Registration successful. Hello " + model.Login + " and welcome to SESM !");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Username already exist.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Passwords don't match.");
                }
            }
            return View(model);
        }

        //
        // GET: /Account/Logout
        [HttpGet]
        [LoggedOnly]
        public ActionResult Logout()
        {
            Session.Abandon();

            return RedirectToAction("Index", "Home").Success("Logout Successfull").Danger("test danger").Warning("test warning").Information("test info");
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