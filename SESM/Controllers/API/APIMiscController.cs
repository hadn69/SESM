using System;
using System.Web.Mvc;
using System.Xml.Linq;
using Quartz;
using SESM.DTO;
using SESM.Tools.API;

namespace SESM.Controllers.API
{
    public class APIMiscController : Controller
    {
        // POST: API/Misc/IsCronValid
        [HttpPost]
        public ActionResult IsCronValid()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            if(user == null)
                return Content(XMLMessage.Error("MIS-ICV-NOTLOG", "No user is logged in.").ToString());

            string cron = Request.Form["Cron"];

            if(string.IsNullOrWhiteSpace(cron))
                return Content(XMLMessage.Error("MIS-ICV-MISCRON", "The Cron field must be provided").ToString());

            // ** PROCESS **
            XMLMessage response = XMLMessage.Success("MIS-ICV-OK", CronExpression.IsValidExpression(cron).ToString());

            return Content(response.ToString());
        }

        // POST: API/Misc/GetNextCronDates
        [HttpPost]
        public ActionResult GetNextCronDates()
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;

            // ** PARSING **
            if(user == null)
                return Content(XMLMessage.Error("MIS-GETDAT-NOTLOG", "No user is logged in.").ToString());

            string cron = Request.Form["Cron"];

            if(string.IsNullOrWhiteSpace(cron))
                return Content(XMLMessage.Error("MIS-GETDAT-MISCRON", "The Cron field must be provided").ToString());

            if(!CronExpression.IsValidExpression(cron))
                return Content(XMLMessage.Error("MIS-GETDAT-BADCRON", "The Cron field is invalid").ToString());

            // ** PROCESS **
            CronExpression expr = new CronExpression(cron);

            DateTimeOffset? date = DateTimeOffset.Now;
            TimeSpan offset = date.Value.Offset;

            
            XMLMessage response = new XMLMessage("MIS-GETDAT-OK");
            string sum = expr.GetExpressionSummary();
            for (int i = 0; i < 10; i++)
            {
                date = expr.GetNextValidTimeAfter(date ?? DateTimeOffset.Now);
                if (date == null)
                    break;
                date = date.Value.ToOffset(offset);

                response.AddToContent(new XElement("Date", date.Value.ToString("o")));
            }

            return Content(response.ToString());
        }
    }
}