/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2018 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DisplayMonkey
{
    public partial class ErrorControl : System.Web.UI.UserControl
    {
        public string Title { get; set; }
        public string Message { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                updateControls();
            }
        }

        public override void DataBind()
        {
            updateControls();
            base.DataBind();
        }

        private void updateControls()
        {
            this.lblErrorDsc.Text = Server.HtmlEncode(this.Title) + ": " + Server.HtmlEncode(this.Message);
        }
    }
}