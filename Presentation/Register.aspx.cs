/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
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
using System.Data;
using System.Text;
using System.Threading.Tasks;
using DisplayMonkey.Language;

namespace DisplayMonkey
{
	public partial class Register : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
                ctrError.Visible = false;

                labelHost.Text = Resources.Register_NoneFound;
				textName.ToolTip = Resources.Register_EnterNameForThisDisplay;
				buttonRegister.Text = Resources.Register_Register;

                // get host from URL first
                string host = Request.ServerVariables["REMOTE_HOST"];
				if (string.IsNullOrWhiteSpace(host))
				{
                    host = Request.ServerVariables["REMOTE_ADDR"];
				}

				if (string.IsNullOrWhiteSpace(host))
				{
                    buttonRegister.Enabled = false;
				}
				else
				{
                    labelHost.Text = host;
                    buttonRegister.Enabled = true;
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
                    ctrError.Title = Resources.Error;
                    ctrError.Message = ex.Message;
                    ctrError.Visible = true;
				}
			}
		}

		protected void Register_Click(object sender, EventArgs e)
		{
            try
            {
                ctrError.Visible = false;

                if (string.IsNullOrWhiteSpace(textName.Text))
                {
                    throw new Exception(Resources.NameRequired);
                }

                int canvas = 0;
                Int32.TryParse(listCanvas.SelectedValue, out canvas);
                if (canvas <= 0)
                {
                    throw new Exception(Resources.CanvasRequired);
                }

                int location = 0;
                Int32.TryParse(listLocation.SelectedValue, out location);
                if (location <= 0)
                {
                    throw new Exception(Resources.LocationRequired);
                }

                Display display = new Display()
                {
                    Host = labelHost.Text,
                    Name = textName.Text,
                    CanvasId = canvas,
                    LocationId = location,
                };

                display.Register();

                if (Display.AutoLoadMode == Models.DisplayAutoLoadModes.DisplayAutoLoadMode_Cookie)
                {
                    Response.Cookies["DisplayMonkey"]["DisplayId"] = display.DisplayId.ToString();
                    Response.Cookies["DisplayMonkey"].Expires = new DateTime(2038, 1, 1);
                }

				Response.Redirect("default.aspx");
            }

            catch (Exception ex)
            {
                ctrError.Title = Resources.Error;
                ctrError.Message = ex.Message;
                ctrError.DataBind();
                ctrError.Visible = true;
            }
		}
	}
}
