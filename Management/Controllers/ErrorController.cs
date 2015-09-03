/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DisplayMonkey.Controllers
{
    public class ErrorController : BaseController
    {
        public ActionResult Index()
        {
            return View("Error");
        }

        public ActionResult NotFound()
        {
            return View();
        }
    }

    public class MissingItem
    {
        public MissingItem(int id)
        {
            this.id = id;
            this.resource = HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
        }
        public MissingItem(int id, string resource)
        {
            this.id = id;
            this.resource = resource;
        }
        public int id { get; private set; }
        public string resource { get; private set; }
    }
}