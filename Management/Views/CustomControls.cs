using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace System.Web.Mvc.Html
{
    public static class CustomControls
    {
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
    }
}