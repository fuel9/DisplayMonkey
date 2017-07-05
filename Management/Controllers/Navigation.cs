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
using System.Text;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Controllers;


namespace DisplayMonkey
{
    /*public static class Navigation
    {
        private const string _cameFrom = "_cameFrom";

        private static Stack<string> _stack
        {
            get
            {
                Stack<string> stack = HttpContext.Current.Session[_cameFrom] as Stack<string>;
                if (stack == null)
                {
                    HttpContext.Current.Session[_cameFrom] =
                        (stack = new Stack<string>());
                }
                return stack;
            }
        }

        public static void SaveCurrent()
        {
            if (HttpContext.Current.Request.Url != null)
            {
                Stack<string> stack = _stack;
                string
                    //lastUrl = stack.Last(),
                    thisUrl = HttpContext.Current.Request.Url.PathAndQuery;
                //if (lastUrl != thisUrl)
                if (stack.Count() > 0)
                    stack.Pop();
                stack.Push(thisUrl);
            }
        }

        public static void SavePrevious()
        {
            if (HttpContext.Current.Request.UrlReferrer != null)
            {
                Stack<string> stack = _stack;
                string
                    //lastUrl = stack.Last(), 
                    thisUrl = HttpContext.Current.Request.UrlReferrer.PathAndQuery;
                //if (lastUrl != thisUrl)
                if (stack.Count() > 0)
                    stack.Pop();
                stack.Push(thisUrl);
            }
            else
                SaveCurrent();
        }

        public static void Restore()
        {
            Stack<string> stack = _stack;
            if (stack.Count() > 0)
            {
                string url = stack.Pop();
                HttpContext.Current.Response.Redirect(url);
            }
        }

        public static string fetchLastUrl()
        {
            return _stack.Pop();
        }
    }*/

    public static class Navigation
    {
        // call this method before entering CRUD action
        public static void SaveReferrer(
            this BaseController _this, 
            bool _notForDelete = false
            )
        {
            Uri fromUrl = _this.Request.Url;
            if (fromUrl == null)
            {
                //_this.Session[_cameFrom] = null;
                _this.ViewData[_cameFrom] = null;
            }
            else
            {
                var referer = new ReferrerData()
                {
                    ReferrerUrl = fromUrl.PathAndQuery,
                    NotForDelete = _notForDelete,
                };

                _this.ViewData[_cameFrom] = referer;
                //_this.Session[_cameFrom] = referer;
            }
        }

        // call this method before exiting from successful CRUD action
        public static ActionResult RestoreReferrer(
            this BaseController _this, 
            bool _hasDeleted = false
            )
        {
            try
            {
                //ReferrerData data = _this.Session[_cameFrom] as ReferrerData;
                ReferrerData data = new ReferrerData()
                {
                    ReferrerUrl = _this.HttpContext.Request[_cameFrom],
                    NotForDelete = _this.HttpContext.Request[_notForDelete] == "true",
                };

                if (data != null &&
                    !(_hasDeleted && data.NotForDelete) && 
                    !string.IsNullOrWhiteSpace(data.ReferrerUrl))
                {
                    return new RedirectResult(data.ReferrerUrl);
                }

                return null;
            }

            finally
            {
                _this.Session[_cameFrom] = null;
            }
        }

        public static IHtmlString Referrer(this HtmlHelper _this)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                string prevUrl = _this.ViewContext.HttpContext.Request.Form[_cameFrom];
                if (!string.IsNullOrWhiteSpace(prevUrl))
                {
                    sb.AppendFormat(_template, _cameFrom, prevUrl);
                }
                else
                {
                    Uri fromUrl = _this.ViewContext.HttpContext.Request.UrlReferrer;
                    if (fromUrl != null)
                    {
                        sb.AppendFormat(_template, _cameFrom, fromUrl.PathAndQuery);
                    }
                }
            }

            catch { }

            return _this.Raw(sb.ToString());
        }

        internal const string _cameFrom = "_cameFrom", _notForDelete = "_notForDelete";
        private const string _template = "<input type='hidden' id='{0}' name='{0}' value='{1}' />";

        private class ReferrerData
        {
            public string ReferrerUrl { get; set; }
            public bool NotForDelete { get; set; }
        }
    }
}

namespace DisplayMonkey.Controllers
{
    public partial class BaseController : Controller
    {
        public ActionResult RestoreReferrer(
            string _defaultAction
            )
        {
            try
            {
                string referrer = Request.Form[Navigation._cameFrom];

                if (!string.IsNullOrWhiteSpace(referrer))
                {
                    return new RedirectResult(referrer);
                }
            }

            catch { }

            return RedirectToAction(_defaultAction);
        }
    }
}