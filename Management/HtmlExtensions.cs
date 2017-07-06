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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
//using System.Web.Mvc.Html;

namespace DisplayMonkey //System.Web.Mvc.Html
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString ShortSpan(this HtmlHelper htmlHelper, string txt, int len)
        {
            txt = txt ?? "";
            return MvcHtmlString.Create((txt.Length > len) ? txt.Substring(0, len) + "..." : txt);
        }

        public static MvcHtmlString ReadOnlyTextFor<TModel, TValue>(this HtmlHelper<TModel> html, System.Linq.Expressions.Expression<Func<TModel, TValue>> expression)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var value = metadata.Model == null
                    ? ""
                    : string.IsNullOrEmpty(metadata.DisplayFormatString)
                        ? metadata.Model.ToString()
                        : string.Format(metadata.DisplayFormatString, metadata.Model);
            TagBuilder tag;
            if (metadata.DataTypeName == "MultilineText")
            {
                tag = new TagBuilder("textarea");
                tag.GenerateId(fullName);
                tag.Attributes.Add("name", fullName);
                tag.Attributes.Add("readonly", "readonly");
                tag.InnerHtml = value;
            }
            else
            {
                tag = new TagBuilder("input");
                tag.GenerateId(fullName);
                tag.Attributes.Add("name", fullName);
                tag.Attributes.Add("type", "text");
                tag.Attributes.Add("readonly", "readonly");
                tag.Attributes.Add("value", value);
            }
            tag.MergeAttributes(html.GetUnobtrusiveValidationAttributes(name, metadata));
            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString ColorPickerFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            System.Linq.Expressions.Expression<Func<TModel, TValue>> expression
            )
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string name = ExpressionHelper.GetExpressionText(expression);
            string fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var value = metadata.Model == null
                    ? ""
                    : string.IsNullOrEmpty(metadata.DisplayFormatString)
                        ? metadata.Model.ToString()
                        : string.Format(metadata.DisplayFormatString, metadata.Model);
            TagBuilder tag;
            tag = new TagBuilder("input");
            tag.GenerateId(fullName);
            tag.Attributes.Add("name", fullName);
            tag.Attributes.Add("type", "color");
            tag.Attributes.Add("value", value);
            tag.MergeAttributes(html.GetUnobtrusiveValidationAttributes(name, metadata));
            return MvcHtmlString.Create(tag.ToString(TagRenderMode.SelfClosing));
        }

        #region Referrer // 1.5

        public static IHtmlString Referrer(this HtmlHelper _this)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                string prevUrl = _this.ViewContext.Controller.TempData[CameFrom] as string;

                if (!string.IsNullOrWhiteSpace(prevUrl))
                {
                    _this.ViewContext.Controller.TempData[CameFrom] = null;
                }
                else
                {
                    prevUrl = _this.ViewContext.HttpContext.Request.Form[CameFrom];

                    if (string.IsNullOrWhiteSpace(prevUrl))
                    {
                        Uri fromUrl = _this.ViewContext.HttpContext.Request.UrlReferrer;

                        if (fromUrl != null)
                        {
                            prevUrl = fromUrl.PathAndQuery;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(prevUrl))
                {
                    sb.AppendFormat("<input type='hidden' id='{0}' name='{0}' value='{1}' />", CameFrom, prevUrl);
                }
            }

            catch { }

            return _this.Raw(sb.ToString());
        }

        internal const string CameFrom = "_cameFrom";

        #endregion

        #region Image link

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

        #endregion

        #region Navigation

        /*[SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The purpose of these helpers is to use default parameters to simplify common usage.")]
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
        }*/

        #endregion

        #region Product version

        public static IHtmlString ProductVersion<T>(this HtmlHelper<T> html)
        {
            if (_version == null)
                _version = new HtmlString("v." + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            return _version;
        }

        private static HtmlString _version = null;

        #endregion

        #region Culture

        public static IHtmlString MetaAcceptLanguage<T>(this HtmlHelper<T> html)
        {
            string culture = HttpUtility.HtmlAttributeEncode(Thread.CurrentThread.CurrentCulture.ToString());
            string uiCulture = HttpUtility.HtmlAttributeEncode(Thread.CurrentThread.CurrentUICulture.ToString());
            return new HtmlString(String.Format("<meta name=\"culture\" content=\"{0}\"><meta name=\"ui-culture\" content=\"{1}\">", culture, uiCulture));
        }

        #endregion
    }
}