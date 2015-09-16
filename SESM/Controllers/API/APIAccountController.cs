using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.API;
using SESM.Tools.Helpers;

namespace SESM.Controllers.API
{
    public class APIAccountController : Controller, IAPIController
    {
        public DataContext CurrentContext { get; set; }

        public EntityServer RequestServer { get; set; }

        public APIAccountController()
        {
            CurrentContext = new DataContext();
        }

        // GET: API/Account/GetChallenge
        [HttpGet]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult GetChallenge()
        {
            // ** PROCESS **
            string challenge = HashHelper.RandomMD5();
            Session["Challenge"] = challenge;

            return Content(XMLMessage.Success("ACT-GC-OK", challenge).ToString());
        }

        // GET: API/Account/LogOut
        [HttpGet]
        public ActionResult LogOut()
        {
            EntityUser user = Session["User"] as EntityUser;
            XMLMessage response = new XMLMessage();

            if (user == null)
                response = XMLMessage.Warning("ACT-LGO-NOTLOG", "No user is logged in");
            else
                response = XMLMessage.Success("ACT-LGO-OK", "User " + user.Login + " have been logged out");

            Session.Abandon();
            return Content(response.ToString());
        }

        // POST: API/Account/Authenticate
        [HttpPost]
        public ActionResult Authenticate()
        {
            // ** INIT **
            string challenge = (string)Session["Challenge"];
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if (user != null)
                return Content(XMLMessage.Error("ACT-ATH-ALRLOG", "A user is already logged in (" + user.Login + ")").ToString());

            // ** PARSING **
            string login = Request.Form["Login"];
            string password = Request.Form["Password"];

            if (challenge == null)
                return Content(XMLMessage.Error("ACT-ATH-EXPCHLNG", "The server challenge has expired or haven't been generated").ToString());

            if (string.IsNullOrWhiteSpace(login))
                return Content(XMLMessage.Error("ACT-ATH-MISLGN", "The Login field must be provided").ToString());

            if (string.IsNullOrWhiteSpace(password))
                return Content(XMLMessage.Error("ACT-ATH-MISPWD", "The Password field must be provided").ToString());

            UserProvider usrPrv = new UserProvider(CurrentContext);
            EntityUser usr = usrPrv.GetUser(login);

            if (usr == null)
                return Content(XMLMessage.Error("ACT-ATH-FAIL", "The provided login/password are incorrect").ToString());

            // ** PROCESS **
            // Expected value : SHA1(challenge + MD5(Password))
            if (HashHelper.SHA1Hash(challenge + usr.Password.ToLower()) != password)
                return Content(XMLMessage.Error("ACT-ATH-FAIL", "The provided login/password are incorrect").ToString());

            Session["User"] = usr;
            Session["PermSummary"] = AuthHelper.GetPermSummaries(usr);

            return Content(XMLMessage.Success("ACT-ATH-OK", "Login successful. Hello " + usr.Login).ToString());
        }

        // POST: API/Account/Register
        [HttpPost]
        public ActionResult Register()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if (user != null)
                return Content(XMLMessage.Error("ACT-REG-ALRLOG", "A user is already logged in (" + user.Login + ")").ToString());

            // ** PARSING **
            string login = Request.Form["Login"];
            string password = Request.Form["Password"];
            string eMail = Request.Form["Email"];

            if (string.IsNullOrWhiteSpace(login))
                return Content(XMLMessage.Error("ACT-REG-MISLGN", "The Login field must be provided").ToString());

            if (string.IsNullOrWhiteSpace(password))
                return Content(XMLMessage.Error("ACT-REG-MISPWD", "The Password field must be provided").ToString());
            if (!Regex.IsMatch(password, "^[0-9a-f]{32}$", RegexOptions.IgnoreCase))
                return Content(XMLMessage.Error("ACT-REG-BADPWD", "The password " + password + " is not a valid MD5").ToString());

            if (string.IsNullOrWhiteSpace(eMail))
                return Content(XMLMessage.Error("ACT-REG-MISEML", "The Email field must be provided").ToString());
            if (!Regex.IsMatch(eMail, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase))
                return Content(XMLMessage.Error("ACT-REG-BADEML", "The email " + eMail + " is not valid").ToString());

            UserProvider usrPrv = new UserProvider(CurrentContext);
            EntityUser usr = usrPrv.GetUser(login);

            if (usr != null)
                return Content(XMLMessage.Error("ACT-REG-USREXI", "User " + login + " already exist").ToString());


            // ** PROCESS **
            EntityUser newUser = new EntityUser();
            newUser.Login = login;
            newUser.Password = password.ToLower();
            newUser.Email = eMail;

            usrPrv.AddUser(newUser);

            Session["User"] = newUser;
            return Content(XMLMessage.Success("ACT-REG-OK", "Register successful. Hello " + newUser.Login).ToString());
        }

        // GET: API/Account/GetDetails
        [HttpGet]
        public ActionResult GetDetails()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if (user == null)
                return Content(XMLMessage.Error("ACT-GTD-NOTLOG", "No user is logged in").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("ACT-GTD-OK");

            response.AddToContent(new XElement("Login", user.Login));
            response.AddToContent(new XElement("Email", user.Email));
            response.AddToContent(new XElement("IsSuperAdmin", user.IsAdmin));

            return Content(response.ToString());
        }

        // GET: API/Account/GetHostPerms
        [HttpGet]
        public ActionResult GetHostPerms()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if (user == null)
                return Content(XMLMessage.Error("ACT-GHP-NOTLOG", "No user is logged in").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("ACT-GHP-OK");

            foreach (string item in Enum.GetNames(typeof(EnumHostPerm)))
            {
                response.AddToContent(new XElement(item, AuthHelper.HasAccess(item)));
            }
            
            return Content(response.ToString());
        }

        // POST: API/Account/SetDetails
        [HttpPost]
        public ActionResult SetDetails()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** ACCESS **
            if (user == null)
                return Content(XMLMessage.Error("ACT-STD-NOTLOG", "No user is logged in").ToString());

            // ** PARSING **
            string login = Request.Form["Login"];
            if (string.IsNullOrWhiteSpace(login))
                return Content(XMLMessage.Error("ACT-STD-MISLGN", "The Login field must be provided").ToString());

            string eMail = Request.Form["Email"];
            if (string.IsNullOrWhiteSpace(eMail))
                return Content(XMLMessage.Error("ACT-STD-MISEML", "The EMail field must be provided").ToString());

            string oldPassword = Request.Form["OldPassword"];
            string newPassword = Request.Form["NewPassword"];
            if (string.IsNullOrWhiteSpace(oldPassword) != string.IsNullOrWhiteSpace(newPassword))
                return Content(XMLMessage.Error("ACT-STD-MISPWD", "One of OldPassword or NewPassword has been provided but not the other").ToString());

            UserProvider usrPrv = new UserProvider(CurrentContext);

            if (user.Login != login && usrPrv.UserExist(login))
                return Content(XMLMessage.Error("ACT-STD-USREXI", "User " + login + " already exist").ToString());

            if (!string.IsNullOrWhiteSpace(oldPassword))
            {
                if (!Regex.IsMatch(oldPassword, "^[0-9a-f]{32}$", RegexOptions.IgnoreCase))
                    return Content(XMLMessage.Error("ACT-STD-BADOPWD", "The old password " + oldPassword + " is not a valid MD5").ToString());

                if (!Regex.IsMatch(newPassword, "^[0-9a-f]{32}$", RegexOptions.IgnoreCase))
                    return Content(XMLMessage.Error("ACT-STD-BADNPWD", "The new password " + newPassword + " is not a valid MD5").ToString());

                if (user.Password.ToUpper() != oldPassword.ToUpper())
                    return Content(XMLMessage.Error("ACT-STD-OPWDMIS", "The provided old password mismatch with the stored one").ToString());
            }

            if (!Regex.IsMatch(eMail, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase))
                return Content(XMLMessage.Error("ACT-STD-BADEML", "The email " + eMail + " is not valid").ToString());

            // ** PROCESS **

            user = usrPrv.GetUser(user.Id);

            user.Email = eMail;
            user.Login = login;
            if (!string.IsNullOrWhiteSpace(oldPassword))
                user.Password = newPassword.ToUpper();
            usrPrv.UpdateUser(user);
            Session["User"] = user;
            return Content(XMLMessage.Success("ACT-STD-OK", "User " + login + " updated").ToString());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CurrentContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}