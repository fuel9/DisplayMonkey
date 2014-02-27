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

        public static void SaveCurrent()
        {
            if (HttpContext.Current.Request.Url != null)
            {
                HttpContext.Current.Session[_cameFrom] =
                    HttpContext.Current.Request.Url.PathAndQuery;
            }
            else
                HttpContext.Current.Session[_cameFrom] = null;
        }

        public static ActionResult Restore()
        {
            if (HttpContext.Current.Session[_cameFrom] != null)
            {
                string url = HttpContext.Current.Session[_cameFrom] as string;
                HttpContext.Current.Session[_cameFrom] = null;
                //HttpContext.Current.Response.Redirect(url);
                return new RedirectResult(url);
            }

            return null;
        }
    }
}