using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
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
        public ActionResult GetUsers()
        {
            // ** INIT **
            UserProvider usrPrv = new UserProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("USR-GU-NOACCESS", "The current user don't have enough right for this action").ToString());

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

        // GET: API/Users/SetUser
        [HttpGet]
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
            if (!bool.TryParse(Request.Form["UserID"], out IsAdmin))
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
                User.Password = Password;

            return Content(XMLMessage.Success("USR-SU-OK", "The user have been updated successfully").ToString());
        }

        // GET: API/Users/DeleteUser
        [HttpGet]
        public ActionResult DeleteUser()
        {
            // ** INIT **
            UserProvider usrPrv = new UserProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            if (user == null || !user.IsAdmin)
                return Content(XMLMessage.Error("USR-DU-NOACCESS", "The current user don't have enough right for this action").ToString());

            int UserID;
            if (string.IsNullOrWhiteSpace(Request.Form["UserID"]))
                return Content(XMLMessage.Error("USR-DU-MISUID", "The UserID field must be provided").ToString());
            if (!int.TryParse(Request.Form["UserID"], out UserID) || !usrPrv.UserExist(UserID))
                return Content(XMLMessage.Error("USR-DU-BADUID", "The UserID field is invalid").ToString());

            // ** PROCESS **
            usrPrv.RemoveUser(usrPrv.GetUser(UserID));

            return Content(XMLMessage.Success("USR-DU-OK", "The user have been deleted successfully").ToString());
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