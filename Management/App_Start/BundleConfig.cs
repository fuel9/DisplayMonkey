using System.Web;
using System.Web.Optimization;

namespace DisplayMonkey
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Scripts ==========================================================================================
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jqsimplemenu.js",                            // DPA: menu
                        "~/Scripts/json2.js"                                    // DPA: http://www.devcurry.com/2010/12/resolve-json-is-undefined-error-in.html
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jsquery").Include(
                        "~/Scripts/jscolor/jscolor.js"                          // DPA: colorpicker
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery-ui-i18n.js"                           // DPA: i18n
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"
                        ));

            //bundles.Add(new ScriptBundle("~/bundles/jhtmlarea").Include(        // DPA: jHtmlArea bundle
            //            "~/Scripts/jHtmlArea-{version}.js",
            //            "~/Scripts/jHtmlArea.ColorPickerMenu-{version}.js"
            //            ));

            //bundles.Add(new ScriptBundle("~/bundles/redactor").Include(        // DPA: Redactor bundle
            //            "~/Scripts/redactor/redactor.js"
            //            ));

            bundles.Add(new ScriptBundle("~/bundles/cleditor").Include(        // DPA: CLEditor bundle
                        "~/Scripts/jquery.cleditor.js"
                        ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/canvas").Include(
                        "~/Scripts/canvas-preview.js"));

            bundles.Add(new ScriptBundle("~/bundles/panel").Include(
                        "~/Scripts/panel-preview.js"));

            
            // Styles ==========================================================================================
            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/site.css",
                        "~/Content/jqsimplemenu.css"                            // DPA: menu
                        ));

            //bundles.Add(new StyleBundle("~/Content/jhtmlareacss").Include(      // DPA: jHtmlArea style bundle
            //            "~/Content/jHtmlArea/jHtmlArea.css",
            //            //"~/Content/jHtmlArea/jHtmlArea.Editor.css",           // DPA: avoid, this screws up body element
            //            "~/Content/jHtmlArea/jHtmlArea.ColorPickerMenu.css"
            //            ));

            //bundles.Add(new StyleBundle("~/Content/redactorcss").Include(      // DPA: Redactor style bundle
            //            "~/Content/redactor/redactor.css"
            //            //"~/Content/redactor/redactor-iframe.css"
            //            ));

            bundles.Add(new StyleBundle("~/Content/cleditorcss").Include(      // DPA: CLEditor style bundle
                        "~/Content/jquery.cleditor.css"
                        ));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        /*"~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css",*/
                        "~/Content/themes/base/jquery-ui.css",
                        "~/Content/themes/base/jquery-ui.all.css"
                        ));
        }
    }
}