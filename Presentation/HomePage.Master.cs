using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string appPath = Request.ApplicationPath;
            string relPath = Request.Path;
            if (appPath != "/")
            {
                relPath = relPath.Replace(appPath, "");
            }

            string helpUrl = string.Format(
                "http://www.displaymonkey.org/dm/documentation/presentation/{0}/{1}{2}", 
                Resources.HelpVersion, 
                Thread.CurrentThread.CurrentUICulture.ToString().Substring(0,2),
                relPath
                );

            help.HRef = helpUrl;
            help.Attributes["onclick"] = string.Format(
                "window.open('{0}','{1}','width=640,height=480,resizable=1,scrollbars=1'); return false;",
                helpUrl, 
                Resources.Help
                );
        }
    }
}