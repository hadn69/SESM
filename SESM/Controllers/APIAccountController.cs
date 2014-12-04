using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    public class APIAccountController : Controller
    {
        private readonly DataContext _context = new DataContext();

        // API/Account/GetChallenge
        public ActionResult GetChallenge()
        {
            // ** PROCESS **
            string challenge = HashHelper.RandomMD5();
            Session["Challenge"] = challenge;

            return Content(new XMLMessage(XmlResponseType.Success, "ACT-GC-OK", challenge).ToString());
        }

        [HttpGet]
        // API/Account/LogOut
        public ActionResult LogOut()
        {
            EntityUser user = Session["User"] as EntityUser;
            XMLMessage returnMessage = new XMLMessage();

            if(user == null)
                returnMessage = new XMLMessage(XmlResponseType.Warning, "ACT-LGO-NOTLOG", "No user is logged in");
            else
                returnMessage = new XMLMessage(XmlResponseType.Success, "ACT-LGO-OK", "User " + user.Login + " have been logged out");


            Session.Abandon();
            return Content(returnMessage.ToString());
        }

        // API/Account/Authenticate
        public ActionResult Authenticate()
        {
            // ** INIT **
            string challenge = (string)Session["Challenge"];
            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            if(user != null)
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-ATH-ALRLOG", "A user is already logged in (" + user.Login + ")").ToString());

            if(challenge == null)
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-ATH-EXPCHLNG", "The server challenge has expired or haven't been generated").ToString());

            string login = Request.Form["Login"];
            string password = Request.Form["Password"];

            if(string.IsNullOrWhiteSpace(login))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-ATH-MISLGN", "The Login field must be provided").ToString());

            if(string.IsNullOrWhiteSpace(password))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-ATH-MISPWD", "The Password field must be provided").ToString());

            UserProvider usrPrv = new UserProvider(_context);
            EntityUser usr = usrPrv.GetUser(login);

            if(usr == null)
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-ATH-FAIL", "The provided login/password are incorrect").ToString());

            // ** PROCESS **
            // Expected value : SHA1(challenge + MD5(Password))

            if(HashHelper.SHA1Hash(challenge + usr.Password.ToLower()) != password)
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-ATH-FAIL", "The provided login/password are incorrect").ToString());

            Session["User"] = usr;
            return Content(new XMLMessage(XmlResponseType.Success, "ACT-ATH-OK", "Login successful. Hello " + usr.Login).ToString());
        }

        [HttpPost]
        // API/Account/Register
        public ActionResult Register()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            if(user != null)
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-REG-ALRLOG", "A user is already logged in (" + user.Login + ")").ToString());

            string login = Request.Form["Login"];
            string password = Request.Form["Password"];
            string eMail = Request.Form["Email"];

            if(string.IsNullOrWhiteSpace(login))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-REG-MISLGN", "The Login field must be provided").ToString());

            if(string.IsNullOrWhiteSpace(password))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-REG-MISPWD", "The Password field must be provided").ToString());

            if(string.IsNullOrWhiteSpace(eMail))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-REG-MISEML", "The EMail field must be provided").ToString());

            UserProvider usrPrv = new UserProvider(_context);
            EntityUser usr = usrPrv.GetUser(login);

            if(usr != null)
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-REG-USREXI", "User " + login + " already exist").ToString());

            if(!Regex.IsMatch(password, "^[0-9a-f]{32}$", RegexOptions.IgnoreCase))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-REG-BADPWD", "The password " + password + " is not a valid MD5").ToString());

            if(!Regex.IsMatch(eMail, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-REG-BADEML", "The email " + eMail + " is not valid").ToString());

            // ** PROCESS **
            EntityUser newUser = new EntityUser();
            newUser.Login = login;
            newUser.Password = password.ToUpper();
            newUser.Email = eMail;

            usrPrv.AddUser(newUser);

            Session["User"] = user;
            return Content(new XMLMessage(XmlResponseType.Success, "ACT-REG-OK", "Register successful. Hello " + user.Login).ToString());
        }

        // API/Account/GetDetails
        public ActionResult GetDetails()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** PROCESS **
            if(user == null)
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-GTD-NOTLOG", "No user is logged in").ToString());

            XMLMessage response = new XMLMessage("ACT-GTD-OK");

            response.AddToContent(new XElement("Login", user.Login));
            response.AddToContent(new XElement("Email", user.Email));

            return Content(response.ToString());
        }

        // API/Account/SetDetails
        [HttpPost]
        public ActionResult SetDetails()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            if(user == null)
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-STD-NOTLOG", "No user is logged in").ToString());

            string login = Request.Form["Login"];
            string eMail = Request.Form["Email"];
            string oldPassword = Request.Form["OldPassword"];
            string newPassword = Request.Form["NewPassword"];

            if(string.IsNullOrWhiteSpace(login))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-STD-MISLGN", "The Login field must be provided").ToString());

            if(string.IsNullOrWhiteSpace(eMail))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-STD-MISEML", "The EMail field must be provided").ToString());

            if(string.IsNullOrWhiteSpace(oldPassword) != string.IsNullOrWhiteSpace(newPassword))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-STD-MISPWD", "One of OldPassword or NewPassword has been provided but not the other").ToString());

            UserProvider usrPrv = new UserProvider(_context);

            if(user.Login != login && usrPrv.UserExist(login))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-STD-USREXI", "User " + login + " already exist").ToString());

            if(!string.IsNullOrWhiteSpace(oldPassword))
            {
                if(!Regex.IsMatch(oldPassword, "^[0-9a-f]{32}$", RegexOptions.IgnoreCase))
                    return Content(new XMLMessage(XmlResponseType.Error, "ACT-STD-BADOPWD","The old password " + oldPassword + " is not a valid MD5").ToString());

                if(!Regex.IsMatch(newPassword, "^[0-9a-f]{32}$", RegexOptions.IgnoreCase))
                    return Content(new XMLMessage(XmlResponseType.Error, "ACT-STD-BADNPWD","The new password " + newPassword + " is not a valid MD5").ToString());

                if(user.Password.ToUpper() != oldPassword.ToUpper())
                    return Content(new XMLMessage(XmlResponseType.Error, "ACT-STD-OPWDMIS", "The provided old password mismatch with the stored one").ToString());
            }

            if(!Regex.IsMatch(eMail, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-STD-BADEML", "The email " + eMail + " is not valid").ToString());

            // ** PROCESS **

            user = usrPrv.GetUser(user.Id);

            user.Email = eMail;
            user.Login = login;
            if (!string.IsNullOrWhiteSpace(oldPassword))
                user.Password = newPassword.ToUpper();
            usrPrv.UpdateUser(user);
            Session["User"] = user;
            return Content(new XMLMessage(XmlResponseType.Success, "ACT-STD-OK", "User " + login + " updated").ToString());
        }
    }
}