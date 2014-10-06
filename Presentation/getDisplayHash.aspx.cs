using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
    public partial class getDisplayHash : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int displayId = DataAccess.IntOrZero(Request.QueryString["display"]);

				string json = "";
				
				try
				{
                    int hash = Display.GetHash(displayId);
					JavaScriptSerializer oSerializer = new JavaScriptSerializer();
					json = oSerializer.Serialize(new {DisplayId = displayId, Hash = hash});
				}

				catch (Exception ex)
				{
                    string s = ex.ToString();
				}

				Response.ExpiresAbsolute = DateTime.Now;
                Response.Expires = -1441;
                Response.CacheControl = "no-cache";
                Response.AddHeader("Pragma", "no-cache");
                Response.AddHeader("Pragma", "no-store");
                Response.AddHeader("cache-control", "no-cache");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoServerCaching();
				Response.Write(json);
            }
        }
    }
}