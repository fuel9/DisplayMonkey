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
//using System.Web.UI;
//using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Net;
using DisplayMonkey.Language;
using DisplayMonkey.AzureUtil;
using System.Threading.Tasks;

namespace DisplayMonkey
{
    public partial class getAzureToken : HttpTaskAsyncHandler
    {
        public override async Task ProcessRequestAsync(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

            int frameId = request.IntOrZero("frame");
            int panelId = request.IntOrZero("panel");
            int displayId = request.IntOrZero("display");
            string culture = request.StringOrBlank("culture");
            int trace = request.IntOrZero("trace");

			string json = "";
				
			try
			{
                // set culture
                Powerbi powerbi = new Powerbi(frameId);
                Location location = new Location(displayId);

                if (string.IsNullOrWhiteSpace(culture))
                    culture = location.Culture;
                
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(culture);
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }

                JavaScriptSerializer jss = new JavaScriptSerializer();
                json = jss.Serialize(new
                {
                    accessToken = await powerbi.GetAccessTokenAsync(),
                });
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
                            Culture = culture,
                            Details = ex.GetType() == typeof(AzureTokenException) ? (ex as AzureTokenException).Details : null,
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
                            Culture = culture,
                            Details = ex.GetType() == typeof(AzureTokenException) ? (ex as AzureTokenException).Details : null,
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