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

                if (!srvPrv.CheckAccess(user.Id, int.Parse(filterContext.ActionParameters["id"].ToString())))
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                    {
                        {"Controller", "Server"},
                        {"Action", "Index"}
                    });
                }
            }
            catch (Exception)
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