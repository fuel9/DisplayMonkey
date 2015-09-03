/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DisplayMonkey
{
    public partial class HomePage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            help.HRef = Info.HelpUri(Request, "presentation").OriginalString;
            help.Attributes["onclick"] = string.Format(
                "window.open('{0}','{1}','width=1280,height=900,resizable=1,scrollbars=1'); return false;",
                help.HRef, 
                Resources.Help
                );
        }

        public static IHtmlString ProductVersion
        {
            get
            {
                if (_version == null)
                    _version = new HtmlString("v." + Assembly.GetExecutingAssembly().GetName().Version.ToString());
                return _version;
            }
        }

        private static HtmlString _version = null;
    }
}