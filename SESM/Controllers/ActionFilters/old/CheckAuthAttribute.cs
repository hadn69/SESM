using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SESM.DAL;
using SESM.DTO;

namespace SESM.Controllers.ActionFilters
{
    public class CheckAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                DataContext context = new DataContext();

                ServerProvider srvPrv = new ServerProvider(context);
                EntityUser user = HttpContext.Current.Session["User"] as EntityUser;
                int idServer = int.Parse(filterContext.ActionParameters["id"].ToString());
                EntityServer server = srvPrv.GetServer(idServer);
                if((user == null && !server.IsPublic) || (user != null && !srvPrv.CheckAccess(user.Id, idServer)))
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