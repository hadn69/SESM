using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;

namespace SESM.Controllers.API
{
    
    public class APIUsersController : Controller
    {
        private readonly DataContext _context = new DataContext();

        // GET: API/Users/GetUsers
        [HttpGet]
        [APIHostAccess("USR-GU", "USER_MANAGE")]
        public ActionResult GetUsers()
        {
            // ** INIT **
            UserProvider usrPrv = new UserProvider(_context);

            List<EntityUser> users = usrPrv.GetUsers();

            // ** PROCESS **
            XMLMessage response = new XMLMessage("USR-GU-OK");

            foreach (EntityUser item in users)
            {
                response.AddToContent(new XElement("User", new XElement("Login", item.Login),
                                                           new XElement("ID", item.Id),
                                                           new XElement("Email", item.Email),
                                                           new XElement("IsAdmin", item.IsAdmin)
                                                           ));
            }
            return Content(response.ToString());
        }

        // POST: API/Users/SetUser
        [HttpPost]
        [APIHostAccess("USR-SU", "USER_MANAGE")]
        public ActionResult SetUser()
        {
            // ** INIT **
            UserProvider usrPrv = new UserProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("USR-SU-NOACCESS", "The current user don't have enough right for this action").ToString());

            int UserID;
            if (string.IsNullOrWhiteSpace(Request.Form["UserID"]))
                return Content(XMLMessage.Error("USR-SU-MISUID", "The UserID field must be provided").ToString());
            if (!int.TryParse(Request.Form["UserID"], out UserID) || !usrPrv.UserExist(UserID))
                return Content(XMLMessage.Error("USR-SU-BADUID", "The UserID field is invalid").ToString());

            string Login = Request.Form["Login"];
            if (string.IsNullOrWhiteSpace(Login))
                return Content(XMLMessage.Error("USR-SU-MISLGN", "The Login field must be provided").ToString());

            string Email = Request.Form["Email"];
            if (string.IsNullOrWhiteSpace(Email))
                return Content(XMLMessage.Error("USR-SU-MISEML", "The Email field must be provided").ToString());
            if (!Regex.IsMatch(Email, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase))
                return Content(XMLMessage.Error("USR-SU-BADEML", "The email " + Email + " is not valid").ToString());

            bool IsAdmin;
            if (string.IsNullOrWhiteSpace(Request.Form["IsAdmin"]))
                return Content(XMLMessage.Error("USR-SU-MISIA", "The IsAdmin field must be provided").ToString());
            if (!bool.TryParse(Request.Form["IsAdmin"], out IsAdmin))
                return Content(XMLMessage.Error("USR-SU-BADIA", "The IsAdmin field is invalid").ToString());

            string Password = Request.Form["Password"];
            if (!string.IsNullOrWhiteSpace(Password) && !Regex.IsMatch(Password, "^[0-9a-f]{32}$", RegexOptions.IgnoreCase))
                return Content(XMLMessage.Error("USR-SU-BADPWD", "The Password is not a valid MD5").ToString());

            // ** PROCESS **

            EntityUser User = usrPrv.GetUser(UserID);

            User.Login = Login;
            User.Email = Email;
            User.IsAdmin = IsAdmin;
            if (!string.IsNullOrWhiteSpace(Password))
                User.Password = Password.ToLower();

            usrPrv.UpdateUser(User);

            return Content(XMLMessage.Success("USR-SU-OK", "The user have been updated successfully").ToString());
        }

        // POST: API/Users/DeleteUsers
        [HttpPost]
        [APIHostAccess("USR-DU", "USER_MANAGE")]
        public ActionResult DeleteUsers()
        {
            // ** INIT **
            UserProvider usrPrv = new UserProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("USR-DU-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PARSING **
            string[] serverIDsString = Request.Form["UserIDs"].Split(';');

            List<int> userIDs = new List<int>();

            foreach (string item in serverIDsString)
            {
                int usrID;
                if (!int.TryParse(item, out usrID))
                {
                    return Content(XMLMessage.Error("USR-DU-INVALIDID", "The following user ID is not a number : " + item).ToString());
                }
                userIDs.Add(usrID);
            }

            // ** ACCESS **
            List<EntityUser> users = new List<EntityUser>();
            foreach (int item in userIDs)
            {
                EntityUser locUser = usrPrv.GetUser(item);

                if (locUser == null)
                    return Content(XMLMessage.Error("USR-DU-UKNUSR", "The following user ID doesn't exist : " + item).ToString());

                users.Add(locUser);
            }
            
            // ** PROCESS **
            foreach (EntityUser item in users)
            {
                usrPrv.RemoveUser(item);
            }

            return Content(XMLMessage.Success("USR-DU-OK", "The user have been deleted successfully").ToString());
        }

        // POST: API/Users/CreateUser
        [HttpPost]
        [APIHostAccess("USR-CU", "USER_MANAGE")]
        public ActionResult CreateUser()
        {
            // ** INIT **
            UserProvider usrPrv = new UserProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("USR-CU-NOACCESS", "The current user don't have enough right for this action").ToString());

            // ** PARSING **
            string login = Request.Form["Login"];
            string password = Request.Form["Password"];
            string eMail = Request.Form["Email"];

            if (string.IsNullOrWhiteSpace(login))
                return Content(XMLMessage.Error("ACT-CU-MISLGN", "The Login field must be provided").ToString());

            if (string.IsNullOrWhiteSpace(password))
                return Content(XMLMessage.Error("ACT-CU-MISPWD", "The Password field must be provided").ToString());
            if (!Regex.IsMatch(password, "^[0-9a-f]{32}$", RegexOptions.IgnoreCase))
                return Content(XMLMessage.Error("ACT-CU-BADPWD", "The password " + password + " is not a valid MD5").ToString());

            if (string.IsNullOrWhiteSpace(eMail))
                return Content(XMLMessage.Error("ACT-CU-MISEML", "The Email field must be provided").ToString());
            if (!Regex.IsMatch(eMail, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase))
                return Content(XMLMessage.Error("ACT-CU-BADEML", "The email " + eMail + " is not valid").ToString());

            bool IsAdmin;
            if (string.IsNullOrWhiteSpace(Request.Form["IsAdmin"]))
                return Content(XMLMessage.Error("USR-CU-MISIA", "The IsAdmin field must be provided").ToString());
            if (!bool.TryParse(Request.Form["IsAdmin"], out IsAdmin))
                return Content(XMLMessage.Error("USR-CU-BADIA", "The IsAdmin field is invalid").ToString());

            EntityUser usr = usrPrv.GetUser(login);

            if (usr != null)
                return Content(XMLMessage.Error("USR-CU-USREXI", "User " + login + " already exist").ToString());


            // ** PROCESS **
            EntityUser newUser = new EntityUser
            {
                Login = login,
                Password = password.ToLower(),
                Email = eMail,
                IsAdmin = IsAdmin
            };

            usrPrv.AddUser(newUser);

            Session["User"] = user;
            return Content(XMLMessage.Success("USR-CU-OK", "User Created successfuly").ToString());
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