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
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
    public partial class getDisplayData : HttpTaskAsyncHandler
    {
		public override async Task ProcessRequestAsync(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

            int displayId = request.IntOrZero("display");
            int trace = request.IntOrZero("trace");

			string json = "";
				
			try
			{
                DisplayData data = await DisplayData.RefreshAsync(displayId);
				JavaScriptSerializer oSerializer = new JavaScriptSerializer();
                json = oSerializer.Serialize(data);
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