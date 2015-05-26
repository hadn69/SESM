using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SESM.Controllers.ActionFilters
{
    public class LoggedOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["User"] == null)
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