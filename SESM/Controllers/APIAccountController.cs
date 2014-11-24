using System.Collections.Generic;
using System.Net.NetworkInformation;
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

        [HttpPost]
        // API/Account/LogOut
        public ActionResult LogOut()
        {
            EntityUser user = Session["User"] as EntityUser;
            XMLMessage returnMessage = new XMLMessage();

            if (user == null)
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

            // ** PARSING **
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

            // ** PARSING **

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

            if(!Regex.IsMatch(password, "^[0-9a-f]{32}$",RegexOptions.IgnoreCase))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-REG-BADPWD", "The password " + password + " is not a valid MD5").ToString());

            if(!Regex.IsMatch(eMail, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase))
                return Content(new XMLMessage(XmlResponseType.Error, "ACT-REG-BADEML", "The email " + eMail + " is not valid").ToString());

            // ** PROCESS **
            EntityUser user = new EntityUser();
            user.Login = login;
            user.Password = password.ToUpper();
            user.Email = eMail;

            usrPrv.AddUser(user);

            Session["User"] = user;
            return Content(new XMLMessage(XmlResponseType.Success, "ACT-REG-OK", "Register successful. Hello " + user.Login).ToString());
        }
    }
}