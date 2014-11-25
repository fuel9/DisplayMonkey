using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace DisplayMonkey
{
    public partial class getFrame : IHttpHandler
	{
        public bool IsReusable { get { return false; } }

		public void ProcessRequest(HttpContext context)
		{
			HttpRequest Request = context.Request;
			HttpResponse Response = context.Response;

			int frameId = DataAccess.IntOrZero(Request.QueryString["frame"]);
			int panelId = DataAccess.IntOrZero(Request.QueryString["panel"]);
			int displayId = DataAccess.IntOrZero(Request.QueryString["display"]);
			string type = DataAccess.StringOrBlank(Request.QueryString["type"]);

			string html = "";

			try
			{
				if (panelId == 0)
				{
					panelId = Frame.GetPanelId(frameId);
				}

				if (type == "")
				{
					type = Frame.GetFrameType(frameId);
				}

				switch (type)
				{
                    case "CLOCK":
                        html = new Clock(frameId, panelId, displayId).Payload;
                        break;

                    case "HTML":
                        html = new Html(frameId, panelId).Payload;
                        break;

                    case "MEMO":
                        html = new Memo(frameId, panelId).Payload;
						break;

                    case "OUTLOOK":
                        html = new Outlook(frameId, panelId).Payload;
                        break;

                    case "PICTURE":
                        html = new Picture(frameId, panelId).Payload;
                        break;

                    case "REPORT":
                        html = new Report(frameId, panelId).Payload;
                        break;

                    case "VIDEO":
                        html = new Video(frameId, panelId).Payload;
                        break;

                    case "WEATHER":
                        int woeid = DataAccess.IntOrZero(Request.QueryString["woeid"]);
                        string tempUnit = Request.QueryString["temperatureUnit"];
                        html = new Weather(frameId, panelId, displayId, woeid, tempUnit).Payload;
                        break;

                    case "YOUTUBE":
                        html = new YouTube(frameId, panelId).Payload;
                        break;

					case "NEWS":
					default:
						html = string.Format("Content type {0} not implemented", type);
						break;
				}
			}

			catch (Exception ex)
			{
				html = context.Server.HtmlEncode(ex.ToString()).Replace("\n", "<br />");
			}

			// set headers
            Response.Clear();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetSlidingExpiration(true);
            Response.Cache.SetNoStore();
            Response.Write(html);
            Response.Flush();
		}
	}
}