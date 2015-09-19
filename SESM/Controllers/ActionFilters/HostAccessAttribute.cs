using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;

namespace SESM.Controllers.ActionFilters
{
    public class HostAccessAttribute : ActionFilterAttribute
    {
        private string[] _permList;

        public HostAccessAttribute(params string[] permList)
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

                if(!AuthHelper.HasAccess(_permList))
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                    {
                        {"Controller", "Server"},
                        {"Action", "Index"}
                    });
                }
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