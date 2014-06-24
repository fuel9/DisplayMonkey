using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
//using System.Web.Mvc.Html;

namespace System.Web.Mvc.Html
{
    /*public static class HtmlNavigatorExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The purpose of these helpers is to use default parameters to simplify common usage.")]
        private static MvcHtmlString NavigatorInternal<T>(this HtmlHelper<T> htmlHelper, int pages, int page, string actionName, string controllerName = null, string protocol = null, string hostName = null, string fragment = null, object routeValues = null, object htmlAttributes = null)
        {
            string url = UrlHelper.GenerateUrl(
                null, // routeName
                actionName, controllerName, protocol, hostName, fragment,
                routeValues as RouteValueDictionary ?? new RouteValueDictionary(routeValues),
                htmlHelper.RouteCollection, htmlHelper.ViewContext.RequestContext,
                true // includeImplicitMvcValues
                );

            TagBuilder a1 = new TagBuilder("a")
            {
                InnerHtml = String.Empty
            };

            tagImg.MergeAttribute(
                "src",
                UrlHelper.GenerateContentUrl(imageSrc, htmlHelper.ViewContext.HttpContext)
                );

            TagBuilder tagLink = new TagBuilder("a")
            {
                InnerHtml = tagImg.ToString()
            };


            IDictionary<string, object> attribs = new Dictionary<string, object>();
            attribs.AddValues(htmlAttributes);
            tagLink.MergeAttributes(attribs);
            tagLink.MergeAttribute("href", url);

            return MvcHtmlString.Create(tagLink.ToString(TagRenderMode.Normal));
        }
    }*/

    
    public static class HtmlImgLinkExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The purpose of these helpers is to use default parameters to simplify common usage.")]
        public static MvcHtmlString ActionImgLink<T>(this HtmlHelper<T> htmlHelper, string imageSrc, string actionName)
        {
            return htmlHelper.ActionImgLinkInternal<T>(imageSrc, actionName);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The purpose of these helpers is to use default parameters to simplify common usage.")]
        public static MvcHtmlString ActionImgLink<T>(this HtmlHelper<T> htmlHelper, string imageSrc, string actionName, object routeValues)
        {
            return htmlHelper.ActionImgLinkInternal<T>(imageSrc, actionName, routeValues: routeValues);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The purpose of these helpers is to use default parameters to simplify common usage.")]
        public static MvcHtmlString ActionImgLink<T>(this HtmlHelper<T> htmlHelper, string imageSrc, string actionName, string controllerName)
        {
            return htmlHelper.ActionImgLinkInternal<T>(imageSrc, actionName, controllerName);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The purpose of these helpers is to use default parameters to simplify common usage.")]
        public static MvcHtmlString ActionImgLink<T>(this HtmlHelper<T> htmlHelper, string imageSrc, string actionName, object routeValues, object htmlAttributes)
        {
            return htmlHelper.ActionImgLinkInternal<T>(imageSrc, actionName, routeValues: routeValues, htmlAttributes: htmlAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The purpose of these helpers is to use default parameters to simplify common usage.")]
        public static MvcHtmlString ActionImgLink<T>(this HtmlHelper<T> htmlHelper, string imageSrc, string actionName, string controllerName, object routeValues, object htmlAttributes)
        {
            return htmlHelper.ActionImgLinkInternal<T>(imageSrc, actionName, controllerName, routeValues: routeValues, htmlAttributes: htmlAttributes);
        }




        private static void AddValues(this IDictionary<string, object> attributes, object values)
        {
            if (values != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    object obj2 = descriptor.GetValue(values);
                    attributes.Add(descriptor.Name, obj2);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The purpose of these helpers is to use default parameters to simplify common usage.")]
        private static MvcHtmlString ActionImgLinkInternal<T>(this HtmlHelper<T> htmlHelper, string imageSrc, string actionName, string controllerName = null, string protocol = null, string hostName = null, string fragment = null, object routeValues = null, object htmlAttributes = null)
        {
            string url = UrlHelper.GenerateUrl(
                null, // routeName
                actionName, controllerName, protocol, hostName, fragment,
                routeValues as RouteValueDictionary ?? new RouteValueDictionary(routeValues),
                htmlHelper.RouteCollection, htmlHelper.ViewContext.RequestContext,
                true // includeImplicitMvcValues
                );

            TagBuilder tagImg = new TagBuilder("img")
            {
                InnerHtml = String.Empty
            };

            tagImg.MergeAttribute(
                "src",
                UrlHelper.GenerateContentUrl(imageSrc, htmlHelper.ViewContext.HttpContext)
                );

            TagBuilder tagLink = new TagBuilder("a")
            {
                InnerHtml = tagImg.ToString()
            };


            IDictionary<string, object> attribs = new Dictionary<string, object>();
            attribs.AddValues(htmlAttributes);
            tagLink.MergeAttributes(attribs);
            tagLink.MergeAttribute("href", url);

            return MvcHtmlString.Create(tagLink.ToString(TagRenderMode.Normal));
        }
    }
}