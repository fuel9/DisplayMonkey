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
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
    public partial class getDisplayData : IHttpHandler
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
                DisplayData data = DisplayData.Refresh(displayId);
				JavaScriptSerializer oSerializer = new JavaScriptSerializer();
                json = oSerializer.Serialize(data);
			}

            catch (Exception ex)
            {
                JavaScriptSerializer s = new JavaScriptSerializer();
                json = s.Serialize(new
                {
                    Error = ex.Message,
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