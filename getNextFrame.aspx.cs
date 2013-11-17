using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
    public partial class getNextFrame : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
				int panelId = DataAccess.IntOrZero(Request.QueryString["panel"]);
                int frameId = DataAccess.IntOrZero(Request.QueryString["frame"]);
                //int featureId = DataAccess.IntOrZero(Request.QueryString["feature"]);

				string json = "";
				
				try
				{
					Frame nci = Frame.GetNextFrame(panelId, frameId);
					JavaScriptSerializer oSerializer = new JavaScriptSerializer();
					json = oSerializer.Serialize(nci);
				}

				catch (Exception)
				{
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