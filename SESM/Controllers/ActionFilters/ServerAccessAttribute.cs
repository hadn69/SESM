using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;

namespace SESM.Controllers.ActionFilters
{
    public class ServerAccessAttribute : ActionFilterAttribute
    {
        private string[] _permList;

        public ServerAccessAttribute(params string[] permList)
        {
            _permList = permList;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                DataContext context = new DataContext();

                ServerProvider srvPrv = new ServerProvider(context);
                EntityUser user = HttpContext.Current.Session["User"] as EntityUser;
                int idServer = int.Parse(filterContext.ActionParameters["id"].ToString());
                EntityServer server = srvPrv.GetServer(idServer);
                if(!AuthHelper.HasAccess(filterContext.HttpContext.Session["PermSummary"] as PermSummaryContainer, server, _permList))
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                    {
                        {"Controller", "Server"},
                        {"Action", "Index"}
                    });
                }
                filterContext.Controller.ViewBag.ServerID = idServer;
                filterContext.Controller.ViewBag.Server = server;
            }
            catch (Exception ex)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    {"Controller", "Server"},
                    {"Action", "Index"}
                });
            }
        }
    }
}