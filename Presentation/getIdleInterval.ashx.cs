using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
    public partial class getIdleInterval : IHttpHandler
    {
        public bool IsReusable { get { return false; } }

		public void ProcessRequest(HttpContext context)
		{
			HttpRequest Request = context.Request;
			HttpResponse Response = context.Response;

			int displayId = DataAccess.IntOrZero(Request.QueryString["display"]);
 
			string json = "";
				
			try
			{
                JavaScriptSerializer oSerializer = new JavaScriptSerializer();
                json = oSerializer.Serialize(new { IdleInterval = Display.GetIdleInterval(displayId) });
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