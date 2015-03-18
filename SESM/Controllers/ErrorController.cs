using System.Web.Mvc;
using SESM.Tools.API;

namespace SESM.Controllers
{
    public class ErrorController : Controller
    {
        [ActionName("404")]
        public ActionResult Error404(string url)
        {
            Response.StatusCode = 404;
            return View();
        }

        [ActionName("404API")]
        public ActionResult Error404API(string url)
        {
            Response.StatusCode = 404;
            XMLMessage response = XMLMessage.Error("ERROR404", "Error 404 - Not Found");

            return Content(response.ToString());
        }
    }
}