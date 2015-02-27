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
<<<<<<< HEAD
=======
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

            return RedirectToAction("Index", "Home").Success("Logout Successfull");
        }

        //
>>>>>>> develop
        // GET: /Account/Manage
        [HttpGet]
        public ActionResult Manage()
        {
            return View();
        }
    }
}