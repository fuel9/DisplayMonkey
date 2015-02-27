using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;

namespace DisplayMonkey
{
	public partial class _Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				// get host from URL first
				string theHost = Request.QueryString["host"];
				
				// otherwise, get display address
				if (string.IsNullOrEmpty(theHost))
				{
					theHost = Request.ServerVariables["REMOTE_HOST"];
					if (string.IsNullOrEmpty(theHost))
					{
						theHost = Request.ServerVariables["REMOTE_ADDR"];
					}
				}

				try
				{
					// list registered displays
					int i = 0;
					StringBuilder html = new StringBuilder();
					foreach (Display display in Display.List)
					{
						string url = string.Format("getCanvas.aspx?display={0}", display.DisplayId);

                        if (theHost != "::1" && display.Host == theHost && display.CanvasId > 0)
						{
							// this host is already registered -> take us to it
							HttpContext.Current.Response.Redirect(string.Format(
								url,
								display.DisplayId
								));
						}

						html.AppendFormat(
							"<li><a href=\"{0}\">{1}</a></li>",
							url,
							Server.HtmlEncode(display.Name)
							);

						if (i % 4 != 0)
							html.Append("<br>");
					}

					labelDisplays.Text = html.ToString();
				}

				catch (Exception ex)
				{
					Response.Write(ex.Message);	// TODO: error label
				}
			}
		}
	}
}
