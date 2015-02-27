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
	public partial class Register : System.Web.UI.Page
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

				if (theHost != null)
				{
					labelHost.Text = theHost;
				}
				else
				{
					buttonRegister.Enabled = false;
				}

				try
				{
					// list canvases
					listCanvas.AutoPostBack = false;
					foreach (Canvas canvas in Canvas.List)
					{
						ListItem list = new ListItem(
							canvas.Name,
							canvas.CanvasId.ToString()
							);
						listCanvas.Items.Add(list);
					}

					// list locations
					listLocation.AutoPostBack = false;
					foreach (Location loc in Location.List())
					{
						ListItem list = new ListItem(
							loc.Name,
							loc.LocationId.ToString()
							);
						listLocation.Items.Add(list);
					}
				}

				catch (Exception ex)
				{
					Response.Write(ex.Message);	// TODO: error label
				}
			}
		}

		protected void Register_Click(object sender, EventArgs e)
		{
			if (textName.Text != "" && 
                labelHost.Text != "::1" &&
                listCanvas.SelectedIndex >= 0 && 
                listLocation.SelectedIndex >= 0
                )
			{
				try
				{
					Display display = new Display()
					{
						Host = labelHost.Text,
						Name = textName.Text,
						CanvasId = DataAccess.IntOrZero(listCanvas.SelectedValue),
						LocationId = DataAccess.IntOrZero(listLocation.SelectedValue),
					};

					display.Register();

					HttpContext.Current.Response.Redirect("default.aspx");
				}

				catch (Exception ex)
				{
					Response.Write(ex.Message);	// TODO: error label
				}
			}
		}
	}
}
