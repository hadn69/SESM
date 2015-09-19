using System.Web.Mvc;
using SESM.Tools;
using SESM.Tools.API;

namespace SESM.Controllers.ActionFilters
{
    public class APIHostAccessAttribute : ActionFilterAttribute
    {
        private string _prefix;
        private string[] _permList;

        public APIHostAccessAttribute(string prefix, params string[] permList)
        {
            _prefix = prefix;
            _permList = permList;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!AuthHelper.HasAccess(filterContext.HttpContext.Session["PermSummary"] as PermSummaryContainer, null, _permList))
            {
                filterContext.Result = new ContentResult
                {
                    Content = XMLMessage.Error(_prefix + "-NOACCESS", "You don't have access to this action").ToString()
                };
                return;
            }
        }

    }
}