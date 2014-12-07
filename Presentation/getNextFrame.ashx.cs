using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using DisplayMonkey.Language;

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

            int frameId = DataAccess.IntOrZero(Request.QueryString["frame"]);
            int panelId = DataAccess.IntOrZero(Request.QueryString["panel"]);
            int displayId = DataAccess.IntOrZero(Request.QueryString["display"]);
            string culture = DataAccess.StringOrBlank(Request.QueryString["culture"]);

			string json = "";
				
			try
			{
                // set culture
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(culture);
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }
                
                Frame nci = Frame.GetNextFrame(panelId, displayId, frameId);
                switch (nci.FrameType)
                {
                    case "CLOCK":
                        nci.Html = new Clock(nci.FrameId, panelId, displayId).Payload;
                        break;

                    case "HTML":
                        nci.Html = new Html(nci.FrameId, panelId).Payload;
                        break;

                    case "MEMO":
                        nci.Html = new Memo(nci.FrameId, panelId).Payload;
                        break;

                    case "OUTLOOK":
                        nci.Html = new Outlook(nci.FrameId, panelId).Payload;
                        break;

                    case "PICTURE":
                        nci.Html = new Picture(nci.FrameId, panelId).Payload;
                        break;

                    case "REPORT":
                        nci.Html = new Report(nci.FrameId, panelId).Payload;
                        break;

                    case "VIDEO":
                        nci.Html = new Video(nci.FrameId, panelId).Payload;
                        break;

                    case "WEATHER":
                        int woeid = DataAccess.IntOrZero(Request.QueryString["woeid"]);
                        string tempUnit = Request.QueryString["tempU"];
                        nci.Html = new Weather(nci.FrameId, panelId, displayId, woeid, tempUnit).Payload;
                        break;

                    case "YOUTUBE":
                        nci.Html = new YouTube(nci.FrameId, panelId).Payload;
                        break;

                    case "NEWS":
                    default:
                        nci.Html = string.Format(Resources.ErrorContentTypeNotImplemented, nci.FrameType);
                        break;
                }

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