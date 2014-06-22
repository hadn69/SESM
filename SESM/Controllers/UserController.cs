using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SESM.DAL;
using SESM.DTO;

namespace SESM.Controllers
{
    public class UserController : Controller
    {
        private DataContext context = new DataContext();

        // GET: User
        public ActionResult Index()
        {
            UserProvider usrPrv = new UserProvider(context);
            return View(usrPrv.GetUsers());
        }

    }
}
