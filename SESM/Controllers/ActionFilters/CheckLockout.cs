using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Controllers.ActionFilters
{
    public class CheckLockout : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if(SESMConfigHelper.Lockdown)
                try
                {
                
                    EntityUser user = HttpContext.Current.Session["User"] as EntityUser;

                    if (user == null || !user.IsAdmin)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                        {
                            {"Controller", "Home"},
                            {"Action", "Index"}
                        });
                        return;
                    }
                }
                catch (Exception)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                    {
                        {"Controller", "Home"},
                        {"Action", "Index"}
                    });
                }
        }
    }
}