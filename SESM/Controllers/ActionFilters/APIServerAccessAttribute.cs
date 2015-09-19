using System.Web;
using System.Web.Mvc;
using SESM.Controllers.API;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.API;

namespace SESM.Controllers.ActionFilters
{
    public class APIServerAccessAttribute : ActionFilterAttribute
    {
        private string _prefix;
        private string[] _permList;

        public APIServerAccessAttribute(string prefix, params string[] permList)
        {
            _prefix = prefix;
            _permList = permList;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpRequestBase currentRequest = filterContext.HttpContext.Request;

            int serverId;
            if (string.IsNullOrWhiteSpace(currentRequest.Form["ServerID"]))
            {
                filterContext.Result = new ContentResult
                {
                    Content = XMLMessage.Error(_prefix + "-MISID", "The ServerID field must be provided").ToString()
                };
                return;
            }
            if (!int.TryParse(currentRequest.Form["ServerID"], out serverId))
            {
                filterContext.Result = new ContentResult
                {
                    Content = XMLMessage.Error(_prefix + "-BADID", "The ServerID is invalid").ToString()
                };
                return;
            }

            ServerProvider srvPrv = new ServerProvider((filterContext.Controller as IAPIController).CurrentContext);

            EntityServer server = srvPrv.GetServer(serverId);

            if (server == null)
            {
                filterContext.Result = new ContentResult
                {
                    Content = XMLMessage.Error(_prefix + "-UKNSRV", "The server doesn't exist").ToString()
                };
                return;
            }

            if (!AuthHelper.HasAccess(filterContext.HttpContext.Session["PermSummary"] as PermSummaryContainer, server, _permList))
            {
                filterContext.Result = new ContentResult
                {
                    Content = XMLMessage.Error(_prefix + "-NOACCESS", "You don't have access to this server").ToString()
                };
                return;
            }

            (filterContext.Controller as IAPIController).RequestServer = server;
        }

    }
}