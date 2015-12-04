using System;
using System.Web;
using System.Web.Mvc;
using SESM.DTO;
using SESM.Tools.Helpers;
using NLog;
using SESM.Tools.API;

namespace SESM.Controllers.ActionFilters
{
    public class APICheckLockdownAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (SESMConfigHelper.Lockdown)
                try
                {

                    EntityUser user = HttpContext.Current.Session["User"] as EntityUser;

                    if (user == null || !user.IsAdmin)
                    {
                        filterContext.Result = new ContentResult
                        {
                            Content = XMLMessage.Error("LOCKDOWN", "The server is in lockdown mode, please wait and retry in a moment...").ToString()
                        };
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger exceptionLogger = LogManager.GetLogger("GenericExceptionLogger");
                    exceptionLogger.Fatal("Lockdown Exception", ex);

                    filterContext.Result = new ContentResult
                    {
                        Content = XMLMessage.Error("LOCKDOWN", "The server is in lockdown mode, please wait and retry in a moment...").ToString()
                    };
                }
        }
    }
}