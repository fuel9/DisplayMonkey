using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
    public partial class getNextFrame : IHttpHandler
    {
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest Request = context.Request;
            HttpResponse Response = context.Response;

			int panelId = DataAccess.IntOrZero(Request.QueryString["panel"]);
            int displayId = DataAccess.IntOrZero(Request.QueryString["display"]);
            int frameId = DataAccess.IntOrZero(Request.QueryString["frame"]);

			string json = "";
				
			try
			{
                Frame nci = Frame.GetNextFrame(panelId, displayId, frameId);
				JavaScriptSerializer oSerializer = new JavaScriptSerializer();
				json = oSerializer.Serialize(nci);
			}

			catch (Exception ex)
			{
                JavaScriptSerializer s = new JavaScriptSerializer();
                json = s.Serialize(new
                {
                    Error = ex.Message,
                    //Stack = ex.StackTrace,
                    Data = new
                    {
                        FrameId = frameId,
                        PanelId = panelId,
                        DisplayId = displayId,
                    },
                });
            }

            Response.Clear();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetSlidingExpiration(true);
            Response.Cache.SetNoStore();
            Response.ContentType = "application/json";
            Response.Write(json);
            Response.Flush();
        }
    }
}