using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.API;

namespace SESM.Controllers.API
{
    public class APIAccessController : Controller
    {
        public DataContext CurrentContext { get; set; }

        public APIAccessController()
        {
            CurrentContext = new DataContext();
        }

        // GET: API/Access/GetHostRoles
        [HttpGet]
        [APIHostAccess("ACC-GHR", "ACCESS_HOST_READ")]
        public ActionResult GetHostRoles()
        {
            HostRoleProvider hroPrv = new HostRoleProvider(CurrentContext);

            XMLMessage response = new XMLMessage("ACC-GHR-OK");

            foreach (EntityHostRole item in hroPrv.GetHostRoles())
            {
                response.AddToContent(new XElement("HostRole", new XElement("Id", item.Id),
                                                               new XElement("Name", item.Name)));
            }

            return Content(response.ToString());
        }

        // GET: API/Access/GetHostRoleAccess
        [HttpGet]
        public ActionResult GetHostRoleAccess()
        {
            XMLMessage response = new XMLMessage("ACC-GHRA-OK");

            response.AddToContent(new XElement("ACCESS_HOST_READ", AuthHelper.HasAccess("ACCESS_HOST_READ")));
            response.AddToContent(new XElement("ACCESS_HOST_CREATE", AuthHelper.HasAccess("ACCESS_HOST_CREATE")));
            response.AddToContent(new XElement("ACCESS_HOST_DELETE", AuthHelper.HasAccess("ACCESS_HOST_DELETE")));
            response.AddToContent(new XElement("ACCESS_HOST_EDIT_NAME", AuthHelper.HasAccess("ACCESS_HOST_EDIT_NAME")));
            response.AddToContent(new XElement("ACCESS_HOST_EDIT_PERMISSION", AuthHelper.HasAccess("ACCESS_HOST_EDIT_PERMISSION")));
            response.AddToContent(new XElement("ACCESS_HOST_EDIT_USERS", AuthHelper.HasAccess("ACCESS_HOST_EDIT_USERS")));

            return Content(response.ToString());
        }

        // GET: API/Access/GetHostPermissions
        [HttpGet]
        [APIHostAccess("ACC-GHP", "ACCESS_HOST_CREATE", "ACCESS_HOST_EDIT_NAME", "ACCESS_HOST_EDIT_PERMISSION", "ACCESS_HOST_EDIT_USERS")]
        public ActionResult GetHostPermissions()
        {
            XMLMessage response = new XMLMessage("ACC-GHP-OK");

            foreach (string name in Enum.GetNames(typeof(EnumHostPerm)).OrderBy(item => item))
            {
                int value = (int)Enum.Parse(typeof(EnumHostPerm), name);

                response.AddToContent(new XElement("HostPerm", new XElement("Name", name),
                                                               new XElement("Value", value)));
            }

            return Content(response.ToString());
        }

        // POST: API/Access/GetHostRoleDetails
        [HttpPost]
        [APIHostAccess("ACC-GHRD", "ACCESS_HOST_EDIT_NAME", "ACCESS_HOST_EDIT_PERMISSION", "ACCESS_HOST_EDIT_USERS")]
        public ActionResult GetHostRoleDetails()
        {
            HostRoleProvider hroPrv = new HostRoleProvider(CurrentContext);

            int HostRoleId;
            if (string.IsNullOrWhiteSpace(Request.Form["HostRoleId"]))
                return Content(XMLMessage.Error("ACC-GHRD-MISID", "The HostRoleId field must be provided").ToString());

            if (!int.TryParse(Request.Form["HostRoleId"], out HostRoleId))
                return Content(XMLMessage.Error("ACC-GHRD-BADID", "The HostRoleId is invalid").ToString());

            EntityHostRole hostRole = hroPrv.GetHostRole(HostRoleId);

            if (hostRole == null)
                return Content(XMLMessage.Error("ACC-GHRD-UKNHR", "The HostRole doesn't exist").ToString());

            XMLMessage response = new XMLMessage("ACC-GHRD-OK");

            response.AddToContent(new XElement("Name", hostRole.Name));

            XElement perms = new XElement("Permissions");
            response.AddToContent(perms);

            foreach (EnumHostPerm item in hostRole.Permissions)
            {
                perms.Add(new XElement("Permission", (int)item));
            }

            XElement users = new XElement("Users");
            response.AddToContent(users);

            foreach (EntityUser item in hostRole.Members)
            {
                users.Add(new XElement("User", item.Id));
            }

            return Content(response.ToString());
        }

        // POST: API/Access/SetHostRoleDetails
        [HttpPost]
        [APIHostAccess("ACC-SHRD", "ACCESS_HOST_EDIT_NAME", "ACCESS_HOST_EDIT_PERMISSION", "ACCESS_HOST_EDIT_USERS")]
        public ActionResult SetHostRoleDetails()
        {
            HostRoleProvider hroPrv = new HostRoleProvider(CurrentContext);
            UserProvider usrPrv = new UserProvider(CurrentContext);

            int HostRoleId;
            if (string.IsNullOrWhiteSpace(Request.Form["HostRoleId"]))
                return Content(XMLMessage.Error("ACC-SHRD-MISID", "The HostRoleId field must be provided").ToString());

            if (!int.TryParse(Request.Form["HostRoleId"], out HostRoleId))
                return Content(XMLMessage.Error("ACC-SHRD-BADID", "The HostRoleId is invalid").ToString());

            EntityHostRole hostRole = hroPrv.GetHostRole(HostRoleId);

            if (hostRole == null)
                return Content(XMLMessage.Error("ACC-SHRD-UKNHR", "The HostRole doesn't exist").ToString());

            string Name = Request.Form["Name"];
            string Permissions = Request.Form["Permissions"];
            string Users = Request.Form["Users"];

            if (AuthHelper.HasAccess("ACCESS_HOST_EDIT_NAME"))
            {

                if (string.IsNullOrWhiteSpace(Name))
                    return Content(XMLMessage.Error("ACC-SHRD-MISNAM", "The Name field must be provided").ToString());

                if (Name != hostRole.Name && hroPrv.HostRoleExist(Name))
                    return Content(XMLMessage.Error("ACC-SHRD-BADNAM", "The Name is already used").ToString());
            }

            List<EnumHostPerm> perms = new List<EnumHostPerm>();
            if (AuthHelper.HasAccess("ACCESS_HOST_EDIT_PERMISSION"))
            {
                foreach (string item in Permissions.Split(';'))
                {
                    if (string.IsNullOrWhiteSpace(item))
                        continue;

                    int id;
                    if (!int.TryParse(item, out id))
                        return
                            Content(
                                XMLMessage.Error("ACC-SHRD-BADPERMID", "One of the permission id isn't valid")
                                    .ToString());

                    if (!Enum.IsDefined(typeof(EnumHostPerm), id))
                        return
                            Content(XMLMessage.Error("ACC-SHRD-BADPERM", "One of the permission don't exist").ToString());

                    perms.Add((EnumHostPerm)id);
                }
            }
            List<EntityUser> users = new List<EntityUser>();
            if (AuthHelper.HasAccess("ACCESS_HOST_EDIT_USERS"))
            {
                foreach (string item in Users.Split(';'))
                {
                    if (string.IsNullOrWhiteSpace(item))
                        continue;

                    int id;
                    if (!int.TryParse(item, out id))
                        return
                            Content(XMLMessage.Error("ACC-SHRD-BADUSRID", "One of the user id isn't valid").ToString());

                    if (!usrPrv.UserExist(id))
                        return Content(XMLMessage.Error("ACC-SHRD-BADUSR", "One of the user don't exist").ToString());

                    users.Add(usrPrv.GetUser(id));
                }
            }

            if (AuthHelper.HasAccess("ACCESS_HOST_EDIT_NAME"))
                hostRole.Name = Name;

            if (AuthHelper.HasAccess("ACCESS_HOST_EDIT_PERMISSION"))
            {
                hostRole.Permissions.Clear();
                foreach (EnumHostPerm item in perms)
                {
                    hostRole.Permissions.Add(item);
                }
            }

            if (AuthHelper.HasAccess("ACCESS_HOST_EDIT_USERS"))
            {
                hostRole.Members.Clear();
                foreach (EntityUser item in users)
                {
                    hostRole.Members.Add(item);
                }
            }

            hroPrv.UpdateHostRole(hostRole);


            return Content(XMLMessage.Success("ACC-SHRD-OK", "The role was updated").ToString());
        }

        // POST: API/Access/CreateHostRole
        [HttpPost]
        [APIHostAccess("ACC-CHR", "ACCESS_HOST_CREATE")]
        public ActionResult CreateHostRole()
        {
            UserProvider usrPrv = new UserProvider(CurrentContext);
            HostRoleProvider hroPrv = new HostRoleProvider(CurrentContext);

            // ** PARSING **
            string Name = Request.Form["Name"];
            string Permissions = Request.Form["Permissions"];
            string Users = Request.Form["Users"];

            if (string.IsNullOrWhiteSpace(Name))
                return Content(XMLMessage.Error("ACC-CHR-MISNAM", "The Name field must be provided").ToString());

            /*if (string.IsNullOrWhiteSpace(Permissions))
                return Content(XMLMessage.Error("ACC-CHR-MISPER", "The Permissions field must be provided").ToString());

            if (string.IsNullOrWhiteSpace(Users))
                return Content(XMLMessage.Error("ACC-CHR-MISUSR", "The Users field must be provided").ToString());*/

            if (hroPrv.HostRoleExist(Name))
                return Content(XMLMessage.Error("ACC-CHR-BADNAM", "The Name is already used").ToString());

            List<EnumHostPerm> perms = new List<EnumHostPerm>();

            foreach (string item in Permissions.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                int id;
                if (!int.TryParse(item, out id))
                    return Content(XMLMessage.Error("ACC-CHR-BADPERMID", "One of the permission id isn't valid").ToString());

                if (!Enum.IsDefined(typeof(EnumHostPerm), id))
                    return Content(XMLMessage.Error("ACC-CHR-BADPERM", "One of the permission don't exist").ToString());

                perms.Add((EnumHostPerm)id);
            }

            List<EntityUser> users = new List<EntityUser>();

            foreach (string item in Users.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                int id;
                if (!int.TryParse(item, out id))
                    return Content(XMLMessage.Error("ACC-CHR-BADUSRID", "One of the user id isn't valid").ToString());

                if (!usrPrv.UserExist(id))
                    return Content(XMLMessage.Error("ACC-CHR-BADUSR", "One of the user don't exist").ToString());

                users.Add(usrPrv.GetUser(id));
            }

            EntityHostRole role = new EntityHostRole();
            role.Name = Name;

            foreach (EnumHostPerm item in perms)
            {
                role.Permissions.Add(item);
            }

            foreach (EntityUser item in users)
            {
                role.Members.Add(item);
            }

            hroPrv.AddHostRole(role);


            return Content(XMLMessage.Success("ACC-CHR-OK", "The role was created").ToString());
        }

        // POST: API/Access/DeleteHostRole
        [HttpPost]
        [APIHostAccess("ACC-DHR", "ACCESS_HOST_DELETE")]
        public ActionResult DeleteHostRole()
        {
            HostRoleProvider hroPrv = new HostRoleProvider(CurrentContext);

            int HostRoleId;
            if (string.IsNullOrWhiteSpace(Request.Form["HostRoleId"]))
                return Content(XMLMessage.Error("ACC-DHR-MISID", "The HostRoleId field must be provided").ToString());

            if (!int.TryParse(Request.Form["HostRoleId"], out HostRoleId))
                return Content(XMLMessage.Error("ACC-DHR-BADID", "The HostRoleId is invalid").ToString());

            EntityHostRole hostRole = hroPrv.GetHostRole(HostRoleId);


            if (hostRole == null)
                return Content(XMLMessage.Error("ACC-DHR-UKNHR", "The HostRole doesn't exist").ToString());

            hroPrv.RemoveHostRole(hostRole);

            return Content(XMLMessage.Success("ACC-DHR-OK", "The role has been deleted").ToString());
        }




        // GET: API/Access/GetServerRoles
        [HttpGet]
        [APIHostAccess("ACC-GSR", "ACCESS_SERVER_READ")]
        public ActionResult GetServerRoles()
        {
            ServerRoleProvider sroPrv = new ServerRoleProvider(CurrentContext);

            XMLMessage response = new XMLMessage("ACC-GSR-OK");

            foreach (EntityServerRole item in sroPrv.GetServerRoles())
            {
                response.AddToContent(new XElement("ServerRole", new XElement("Id", item.Id),
                                                                 new XElement("Name", item.Name)));
            }

            return Content(response.ToString());
        }

        // GET: API/Access/GetServerRoleAccess
        [HttpGet]
        public ActionResult GetServerRoleAccess()
        {
            XMLMessage response = new XMLMessage("ACC-GHRA-OK");

            response.AddToContent(new XElement("ACCESS_SERVER_READ", AuthHelper.HasAccess("ACCESS_SERVER_READ")));
            response.AddToContent(new XElement("ACCESS_SERVER_CREATE", AuthHelper.HasAccess("ACCESS_SERVER_CREATE")));
            response.AddToContent(new XElement("ACCESS_SERVER_DELETE", AuthHelper.HasAccess("ACCESS_SERVER_DELETE")));
            response.AddToContent(new XElement("ACCESS_SERVER_EDIT_NAME", AuthHelper.HasAccess("ACCESS_SERVER_EDIT_NAME")));
            response.AddToContent(new XElement("ACCESS_SERVER_EDIT_PERMISSION", AuthHelper.HasAccess("ACCESS_SERVER_EDIT_PERMISSION")));
            response.AddToContent(new XElement("ACCESS_SERVER_EDIT_USERS", AuthHelper.HasAccess("ACCESS_SERVER_EDIT_USERS")));

            return Content(response.ToString());
        }

        // GET: API/Access/GetServerPermissions
        [HttpGet]
        [APIHostAccess("ACC-GSP", "ACCESS_SERVER_CREATE", "ACCESS_SERVER_EDIT_NAME", "ACCESS_SERVER_EDIT_PERMISSION")]
        public ActionResult GetServerPermissions()
        {
            XMLMessage response = new XMLMessage("ACC-GSP-OK");

            foreach (string name in Enum.GetNames(typeof(EnumServerPerm)).OrderBy(item => item))
            {
                int value = (int)Enum.Parse(typeof(EnumServerPerm), name);

                response.AddToContent(new XElement("ServerPerm", new XElement("Name", name),
                                                                 new XElement("Value", value)));
            }

            return Content(response.ToString());
        }

        // POST: API/Access/GetServerRoleDetails
        [HttpPost]
        [APIHostAccess("ACC-GSRD", "ACCESS_SERVER_EDIT_NAME", "ACCESS_SERVER_EDIT_PERMISSION")]
        public ActionResult GetServerRoleDetails()
        {
            ServerRoleProvider sroPrv = new ServerRoleProvider(CurrentContext);

            int ServerRoleId;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerRoleId"]))
                return Content(XMLMessage.Error("ACC-GSRD-MISID", "The ServerRoleId field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerRoleId"], out ServerRoleId))
                return Content(XMLMessage.Error("ACC-GSRD-BADID", "The ServerRoleId is invalid").ToString());

            EntityServerRole serverRole = sroPrv.GetServerRole(ServerRoleId);

            if (serverRole == null)
                return Content(XMLMessage.Error("ACC-GSRD-UKNSR", "The ServerRole doesn't exist").ToString());

            XMLMessage response = new XMLMessage("ACC-GSRD-OK");

            response.AddToContent(new XElement("Name", serverRole.Name));

            XElement perms = new XElement("Permissions");
            response.AddToContent(perms);

            foreach (EnumServerPerm item in serverRole.Permissions)
            {
                perms.Add(new XElement("Permission", (int)item));
            }

            return Content(response.ToString());
        }

        // POST: API/Access/SetServerRoleDetails
        [HttpPost]
        [APIHostAccess("ACC-SSRD", "ACCESS_SERVER_EDIT_NAME", "ACCESS_SERVER_EDIT_PERMISSION")]
        public ActionResult SetServerRoleDetails()
        {
            ServerRoleProvider sroPrv = new ServerRoleProvider(CurrentContext);

            int ServerRoleId;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerRoleId"]))
                return Content(XMLMessage.Error("ACC-SSRD-MISID", "The ServerRoleId field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerRoleId"], out ServerRoleId))
                return Content(XMLMessage.Error("ACC-SSRD-BADID", "The ServerRoleId is invalid").ToString());

            EntityServerRole serverRole = sroPrv.GetServerRole(ServerRoleId);

            if (serverRole == null)
                return Content(XMLMessage.Error("ACC-SSRD-UKNHR", "The ServerRole doesn't exist").ToString());

            string Name = Request.Form["Name"];
            string Permissions = Request.Form["Permissions"];

            if (AuthHelper.HasAccess("ACCESS_SERVER_EDIT_NAME"))
            {

                if (string.IsNullOrWhiteSpace(Name))
                    return Content(XMLMessage.Error("ACC-SSRD-MISNAM", "The Name field must be provided").ToString());

                if (Name != serverRole.Name && sroPrv.ServerRoleExist(Name))
                    return Content(XMLMessage.Error("ACC-SSRD-BADNAM", "The Name is already used").ToString());
            }

            List<EnumServerPerm> perms = new List<EnumServerPerm>();
            if (AuthHelper.HasAccess("ACCESS_SERVER_EDIT_PERMISSION"))
            {
                foreach (string item in Permissions.Split(';'))
                {
                    if (string.IsNullOrWhiteSpace(item))
                        continue;

                    int id;
                    if (!int.TryParse(item, out id))
                        return Content(XMLMessage.Error("ACC-SSRD-BADPERMID", "One of the permission id isn't valid").ToString());

                    if (!Enum.IsDefined(typeof(EnumServerPerm), id))
                        return Content(XMLMessage.Error("ACC-SSRD-BADPERM", "One of the permission don't exist").ToString());

                    perms.Add((EnumServerPerm)id);
                }
            }
            

            if (AuthHelper.HasAccess("ACCESS_SERVER_EDIT_NAME"))
                serverRole.Name = Name;

            if (AuthHelper.HasAccess("ACCESS_SERVER_EDIT_PERMISSION"))
            {
                serverRole.Permissions.Clear();
                foreach (EnumServerPerm item in perms)
                {
                    serverRole.Permissions.Add(item);
                }
            }

            sroPrv.UpdateServerRole(serverRole);


            return Content(XMLMessage.Success("ACC-SSRD-OK", "The role was updated").ToString());
        }

        // POST: API/Access/CreateServerRole
        [HttpPost]
        [APIHostAccess("ACC-CSR", "ACCESS_SERVER_CREATE")]
        public ActionResult CreateServerRole()
        {
            ServerRoleProvider sroPrv = new ServerRoleProvider(CurrentContext);

            // ** PARSING **
            string Name = Request.Form["Name"];
            string Permissions = Request.Form["Permissions"];

            if (string.IsNullOrWhiteSpace(Name))
                return Content(XMLMessage.Error("ACC-CSR-MISNAM", "The Name field must be provided").ToString());

            if (sroPrv.ServerRoleExist(Name))
                return Content(XMLMessage.Error("ACC-CSR-BADNAM", "The Name is already used").ToString());

            List<EnumServerPerm> perms = new List<EnumServerPerm>();

            foreach (string item in Permissions.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                int id;
                if (!int.TryParse(item, out id))
                    return Content(XMLMessage.Error("ACC-CSR-BADPERMID", "One of the permission id isn't valid").ToString());

                if (!Enum.IsDefined(typeof(EnumServerPerm), id))
                    return Content(XMLMessage.Error("ACC-CSR-BADPERM", "One of the permission don't exist").ToString());

                perms.Add((EnumServerPerm)id);
            }

            EntityServerRole role = new EntityServerRole();
            role.Name = Name;

            foreach (EnumServerPerm item in perms)
            {
                role.Permissions.Add(item);
            }

            sroPrv.AddServerRole(role);


            return Content(XMLMessage.Success("ACC-CHR-OK", "The role was created").ToString());
        }

        // POST: API/Access/DeleteServerRole
        [HttpPost]
        [APIHostAccess("ACC-DSR", "ACCESS_SERVER_DELETE")]
        public ActionResult DeleteServerRole()
        {
            ServerRoleProvider sroPrv = new ServerRoleProvider(CurrentContext);

            int ServerRoleId;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerRoleId"]))
                return Content(XMLMessage.Error("ACC-DSR-MISID", "The ServerRoleId field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerRoleId"], out ServerRoleId))
                return Content(XMLMessage.Error("ACC-DSR-BADID", "The ServerRoleId is invalid").ToString());

            EntityServerRole serverRole = sroPrv.GetServerRole(ServerRoleId);


            if (serverRole == null)
                return Content(XMLMessage.Error("ACC-DSR-UKNHR", "The ServerRole doesn't exist").ToString());

            sroPrv.RemoveServerRole(serverRole);

            return Content(XMLMessage.Success("ACC-DSR-OK", "The role has been deleted").ToString());
        }

        // GET: API/Access/GetUsers
        [HttpGet]
        [APIHostAccess("ACC-GU", "ACCESS_HOST_CREATE",
                                 "ACCESS_HOST_EDIT_NAME",
                                 "ACCESS_HOST_EDIT_PERMISSION",
                                 "ACCESS_HOST_EDIT_USERS",
                                 "ACCESS_SERVER_CREATE",
                                 "ACCESS_SERVER_EDIT_NAME",
                                 "ACCESS_SERVER_EDIT_PERMISSION")]
        public ActionResult GetUsers()
        {
            // ** INIT **
            UserProvider usrPrv = new UserProvider(CurrentContext);

            List<EntityUser> users = usrPrv.GetUsers();

            // ** PROCESS **
            XMLMessage response = new XMLMessage("ACC-GU-OK");

            foreach (EntityUser item in users)
            {
                response.AddToContent(new XElement("User", new XElement("Login", item.Login),
                                                           new XElement("ID", item.Id)));
            }
            return Content(response.ToString());
        }

    }
}