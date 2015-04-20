using DisplayMonkey.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DisplayMonkey.Controllers
{
    public class BaseController : Controller
    {
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string culture = null, uiCulture = null;

            // attempt to read the culture cookie from Request
            HttpCookie cultureCookie = Request.Cookies["accept-language"];
            if (cultureCookie != null)
            {
                string lang = cultureCookie.Value;
                if (CultureHelper.IsValid(lang))
                    culture = lang;
                uiCulture = CultureHelper.GetSupported(lang);
            }

            // fallback to browser preferred culture
            if ((culture == null || uiCulture == null) && 
                Request.UserLanguages != null && 
                Request.UserLanguages.Length > 0)
            {
                foreach (string lang in Request.UserLanguages)
                {
                    if (culture == null && CultureHelper.IsValid(lang))
                        culture = lang;
                    
                    if (uiCulture == null)
                        uiCulture = CultureHelper.GetSupported(lang);
                    
                    if (culture != null && uiCulture != null)
                        break;
                }
            }

            // set current thread culture if any
            CultureHelper.SetCurrentCulture(culture);       // locale
            CultureHelper.SetCurrentUICulture(uiCulture);   // resource manager

            // execute controller
            return base.BeginExecuteCore(callback, state);
        }

        // NOT USED CURRENTLY
        public ActionResult SetCulture(string culture)
        {
            // Validate input
            culture = CultureHelper.GetSupported(culture);

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

        public static void FillCacheModeSelectList(
            this BaseController _controller,
            CacheModes _selected
            )
        {
            _controller.ViewBag.CacheModes = 
                new Nullable<CacheModes>(_selected).TranslatedSelectList(valueAsText: false);
        }

        public static void FillCacheModeSelectList(
            this BaseController _controller
            )
        {
            CacheModes? _selected = null;
            _controller.ViewBag.CacheModes = 
                _selected.TranslatedSelectList(valueAsText: false);
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
