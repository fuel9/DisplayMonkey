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
            help.HRef = Info.HelpUri(Request, "presentation").OriginalString;
            help.Attributes["onclick"] = string.Format(
                "window.open('{0}','{1}','width=640,height=480,resizable=1,scrollbars=1'); return false;",
                help.HRef, 
                Resources.Help
                );
        }
    }
}