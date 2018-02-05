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
using DisplayMonkey.Models;
using DisplayMonkey.Language;

namespace DisplayMonkey
{
	public partial class _Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
                this.ctrError.Visible = false;

                string theHost = "";
                int displayId = Int32.MinValue;
                DisplayAutoLoadModes mode = Display.AutoLoadMode;

                switch (mode)
                {
                    case DisplayAutoLoadModes.DisplayAutoLoadMode_IP:
                        theHost = Request.ServerVariables["REMOTE_HOST"];
                        if (string.IsNullOrEmpty(theHost))
                        {
                            theHost = Request.ServerVariables["REMOTE_ADDR"];
                        }
                        break;

                    case DisplayAutoLoadModes.DisplayAutoLoadMode_Cookie:
                        var cookie = Request.Cookies["DisplayMonkey"];
                        if (cookie != null && cookie["DisplayId"] != null)
                        {
                            Int32.TryParse(cookie["DisplayId"], out displayId);
                        }
                        break;
                }

				try
				{
                    // list registered displays
					StringBuilder html = new StringBuilder();

					foreach (Display display in Display.List)
					{
                        switch (mode)
                        {
                            case DisplayAutoLoadModes.DisplayAutoLoadMode_IP:
                                if (theHost != "::1" && theHost == display.Host)
                                {
                                    Response.Redirect(display.Url);
                                }
                                break;

                            case DisplayAutoLoadModes.DisplayAutoLoadMode_Cookie:
                                if (displayId == display.DisplayId)
                                {
                                    Response.Redirect(display.Url);
                                }
                                break;
                        }

                        html.AppendFormat(
						    "<li><a href=\"{0}\">{1}</a></li>",
						    display.Url,
						    Server.HtmlEncode(display.Name)
						    );
					}

					labelDisplays.Text = html.ToString();
                }

                catch (Exception ex)
				{
                    this.ctrError.Title = Resources.Error;
                    this.ctrError.Message = ex.Message;
                    this.ctrError.Visible = true;
				}
			}
		}
	}
}
