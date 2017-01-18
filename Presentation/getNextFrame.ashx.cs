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
using System.Web.Script.Serialization;
using DisplayMonkey.Language;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMonkey
{
    public partial class getNextFrame : HttpTaskAsyncHandler
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            int panelId = request.IntOrZero("panel");
            int displayId = request.IntOrZero("display");
            int frameId = request.IntOrZero("frame");
            string culture = request.StringOrBlank("culture");
            int trace = request.IntOrZero("trace");
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

                JavaScriptSerializer s = new JavaScriptSerializer();
                Frame nci = await Frame.GetNextFrameAsync(panelId, displayId, frameId);
                json = s.Serialize(nci);
			}

			catch (Exception ex)
			{
                JavaScriptSerializer s = new JavaScriptSerializer();
                if (trace == 0)
                    json = s.Serialize(new
                    {
                        Error = ex.Message,
                        Data = new
                        {
                            FrameId = frameId,
                            PanelId = panelId,
                            DisplayId = displayId,
                        },
                    });
                else
                    json = s.Serialize(new
                    {
                        Error = ex.Message,
                        Stack = ex.StackTrace,
                        Data = new
                        {
                            FrameId = frameId,
                            PanelId = panelId,
                            DisplayId = displayId,
                        },
                    });
            }

            response.Clear();
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetSlidingExpiration(true);
            response.Cache.SetNoStore();
            response.ContentType = "application/json";
            response.Write(json);
            response.Flush();
        }
    }
}