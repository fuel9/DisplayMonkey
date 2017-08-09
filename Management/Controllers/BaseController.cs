/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using DisplayMonkey.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DisplayMonkey.Controllers
{
    public partial class BaseController : Controller
    {
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string culture = null, uiCulture = null;

            // attempt to read the culture cookie from Request
            HttpCookie cultureCookie = Request.Cookies["accept-language"];
            if (cultureCookie != null)
            {
                string lang = cultureCookie.Value;
                if (CultureHelpers.IsValid(lang))
                    culture = lang;
                uiCulture = CultureHelpers.GetSupported(lang);
            }

            // fallback to browser preferred culture
            if ((culture == null || uiCulture == null) && 
                Request.UserLanguages != null && 
                Request.UserLanguages.Length > 0)
            {
                foreach (string lang in Request.UserLanguages)
                {
                    if (culture == null && CultureHelpers.IsValid(lang))
                        culture = lang;
                    
                    if (uiCulture == null)
                        uiCulture = CultureHelpers.GetSupported(lang);
                    
                    if (culture != null && uiCulture != null)
                        break;
                }
            }

            // set current thread culture if any
            CultureHelpers.SetCurrentCulture(culture);       // locale
            CultureHelpers.SetCurrentUICulture(uiCulture);   // resource manager

            // execute controller
            return base.BeginExecuteCore(callback, state);
        }

        // NOT USED CURRENTLY
        public ActionResult SetCulture(string culture)
        {
            // Validate input
            culture = CultureHelpers.GetSupported(culture);

            // Save culture in a cookie
            HttpCookie cookie = Request.Cookies["accept-language"];
            if (cookie != null)
                cookie.Value = culture;   // update cookie value
            else
                cookie = new HttpCookie("accept-language")
                {
                    Value = culture,
                    Expires = DateTime.Now.AddYears(1),
                };

            Response.Cookies.Add(cookie);
            
            return RedirectToAction("Index");
        }

        private RedirectResult RestoreReferrer()
        {
            try
            {
                if (TempData[HtmlExtensions.CameFrom] != null)
                {
                    return null;
                }

                string referrer = Request.Form[HtmlExtensions.CameFrom];

                if (!string.IsNullOrWhiteSpace(referrer))
                {
                    return new RedirectResult(referrer);
                }
            }

            catch { }

            return null;
        }

        /// <summary>
        /// This will carry over referrer URL between chained POST actions
        /// Use this in POST actions prior to redirecting to any further CRUD action
        /// </summary>
        protected void PushReferrer()
        {
            if (TempData[HtmlExtensions.CameFrom] == null)
            {
                TempData[HtmlExtensions.CameFrom] = Request.Form[HtmlExtensions.CameFrom];
            }
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result is RedirectToRouteResult)
            {
                RedirectResult referrer = RestoreReferrer();
                if (referrer != null)
                {
                    filterContext.Result = referrer;
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }

    public static class BaseControllerExtensions
    {
        public static void FillTemplatesSelectList(
            this BaseController _controller, 
            DisplayMonkeyEntities _db, 
            FrameTypes _frameType, 
            object _selected = null
            )
        {
            var query = _db.Templates
                .Where(t => t.FrameType == _frameType)
                .Select(t => new
                {
                    TemplateId = t.TemplateId,
                    Name = t.Name
                })
                .OrderBy(t => t.Name)
                .ToList()
                ;

            _controller.ViewBag.Templates = 
                new SelectList(query, "TemplateId", "Name", _selected);
        }

        public static void FillSystemTimeZoneSelectList(
            this BaseController _controller,
            string _selected = null
            )
        {
            var query = TimeZoneInfo.GetSystemTimeZones()
                .Select(z => new SelectListItem()
                {
                    Value = z.Id,
                    Text = z.DisplayName,
                })
                .OrderBy(z => z.Text)
                .ToList()
                ;
            _controller.ViewBag.TimeZones = 
                new SelectList(query, "Value", "Text", _selected);
        }
    }
}
