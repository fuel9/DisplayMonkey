using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Diagnostics;

namespace DisplayMonkey
{
	public partial class getCanvas : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				// get host from URL first
				int displayId = DataAccess.IntOrZero(Request.QueryString["display"]);

				try
				{
					Canvas canvas = Canvas.InitFromDisplay(displayId);

					// assemble page
					this.lTitle.Text = canvas.Name;
					this.lHead.Text = canvas.Head;
					this.lContent.Text = canvas.Body;
				}

				catch (Exception ex)
				{
					// back to display selection
					this.lContent.Text = Server.HtmlEncode(ex.Message);
					//HttpContext.Current.Response.Redirect("default.aspx");
				}
			}
		}
	}
}