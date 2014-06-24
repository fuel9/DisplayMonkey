using DisplayMonkey.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DisplayMonkey.Controllers
{
    public class HomeController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        public ActionResult Index()
        {
            var result = db.sp_Get_Dashboard().FirstOrDefault();
            
            return View(result);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
