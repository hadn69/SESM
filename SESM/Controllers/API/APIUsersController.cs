using System.Web.Mvc;
using SESM.DAL;

namespace SESM.Controllers.API
{
    public class APIUsersController : Controller
    {
        private readonly DataContext _context = new DataContext();



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}