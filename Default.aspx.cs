using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DisplayMonkey
{
	public partial class _Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				// get host from URL first
				string host = Request.QueryString["host"];
				
				// otherwise, get display address
				if (string.IsNullOrEmpty(host))
				{
					host = Request.ServerVariables["REMOTE_HOST"];
					if (string.IsNullOrEmpty(host))
					{
						host = Request.ServerVariables["REMOTE_ADDR"];
					}
				}

				Label rd = (Label)FindControl("RegisteredDisplays");
				Label l_da = (Label)FindControl("lb_Display_Address");
				if (l_da != null)
					l_da.Text = host;
				
				// see if we can find an existing registered display
				// if so, try and redirect to the correct display.
				Display display = new Display(host);
				if (display.DisplayId > 0 && display.CanvasId > 0)
				{
					HttpContext.Current.Response.Redirect(string.Format(
						"getCanvas.aspx?display={0}", 
						display.DisplayId
						));
				}
			}
		}

		protected void Register_Click(object sender, EventArgs e)
		{
			/* TODO
			 * DataAccess da = new DataAccess();
			RadioButton dtP = (RadioButton)FindControl("Portrait");
			RadioButton dtL = (RadioButton)FindControl("Landscape");
			TextBox dn = (TextBox)FindControl("tB_Display_Name");
			Label l_da = (Label)FindControl("lb_Display_Address");
			if (dn != null)
			{
				if (dn.Text != "")
				{
					string usr = HttpContext.Current.User.Identity.Name.Replace("PERMOBIL\\", "").Replace("permobil\\", "").Replace("Permobil\\", "");
					string dt = da.registerDisplay(l_da.Text, dn.Text, (dtP.Checked ? "P" : "L"), usr);

					if (dt != null)
					{
						if (dt.CompareTo("P") == 0)
							HttpContext.Current.Response.Redirect("portrait.htm");
						else if (dt.CompareTo("L") == 0)
							HttpContext.Current.Response.Redirect("landscape.htm");
					}
				}
			}
			 */
		}
	}
}
