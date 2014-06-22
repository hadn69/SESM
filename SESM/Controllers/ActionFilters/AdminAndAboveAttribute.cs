using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SESM.DAL;
using SESM.DTO;

namespace SESM.Controllers.ActionFilters
{
    public class AdminAndAboveAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                DataContext context = new DataContext();

                ServerProvider srvPrv = new ServerProvider(context);
                EntityUser user = HttpContext.Current.Session["User"] as EntityUser;
                AccessLevel accessLevel = srvPrv.GetAccessLevel(user.Id, int.Parse(filterContext.ActionParameters["id"].ToString()));
                if (accessLevel == AccessLevel.Guest || accessLevel == AccessLevel.User || accessLevel == AccessLevel.Manager)
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