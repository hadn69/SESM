using System.Web.Mvc;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.API;

namespace SESM.Controllers.API
{
    public class APISESEController : Controller
    {
        private readonly DataContext _context = new DataContext();

        // POST: API/SESE/TestWCF
        [HttpPost]
        public ActionResult TestWCF()
        {
            // ** INIT **
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityUser user = Session["User"] as EntityUser;
            int userID = user == null ? 0 : user.Id;

            // ** PARSING / ACCESS **
            int serverId = -1;
            if (string.IsNullOrWhiteSpace(Request.Form["ServerID"]))
                return Content(XMLMessage.Error("SESE-TW-MISID", "The ServerID field must be provided").ToString());

            if (!int.TryParse(Request.Form["ServerID"], out serverId))
                return Content(XMLMessage.Error("SESE-TW-BADID", "The ServerID is invalid").ToString());

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
                return Content(XMLMessage.Error("SESE-TW-UKNSRV", "The server doesn't exist").ToString());

            if (!srvPrv.IsManagerOrAbore(srvPrv.GetAccessLevel(userID, server.Id)))
                return Content(XMLMessage.Error("SESE-TW-NOACCESS", "You don't have access to this server").ToString());

            if (!server.UseServerExtender)
                return Content(XMLMessage.Error("SESE-TW-NOSESE", "SESE isn't activated on this server.").ToString());

            // ** PROCESS **
            /*
            SESEServiceHelper SESEService = new SESEServiceHelper(server.ServerExtenderPort);
            SESEService.GetPlayerList();*/
            return null;
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