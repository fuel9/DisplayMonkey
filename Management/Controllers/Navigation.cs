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
        private const string _cameFrom = "_cameFrom";

        private class ReferrerData
        {
            public string ReferrerUrl { get; set; }
            public bool NotForDelete { get; set; }
        }

        // call this method before entering CRUD action
        public static void SaveReferrer(
            this BaseController _this, 
            bool _notForDelete = false
            )
        {
            Uri fromUrl = _this.Request.Url;
            if (fromUrl == null)
            {
                _this.Session[_cameFrom] = null;
            }
            else
            {
                _this.Session[_cameFrom] = new ReferrerData()
                {
                    ReferrerUrl = fromUrl.PathAndQuery,
                    NotForDelete = _notForDelete,
                };
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
                ReferrerData data = _this.Session[_cameFrom] as ReferrerData;

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
    }
}