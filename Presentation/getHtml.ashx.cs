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
using System.IO;
//using System.Data;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
//using System.Drawing;
//using System.Drawing.Imaging;
//using Microsoft.SharePoint.Client;
//using System.Security;

namespace DisplayMonkey
{
	/// <summary>
	/// Summary description for getHtml
	/// </summary>
	public class getHtml : HttpTaskAsyncHandler
	{
		public override async Task ProcessRequestAsync(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

            int frameId = request.IntOrZero("frame");
            int trace = request.IntOrZero("trace");

            try
			{
                Html html = new Html(frameId);

                response.Clear();
                response.Cache.SetCacheability(HttpCacheability.NoCache);
                response.Cache.SetSlidingExpiration(true);
                response.Cache.SetNoStore();
                response.ContentType = "text/html";
                await response.Output.WriteAsync(html.Content);
                await response.Output.FlushAsync();
            }

			catch (Exception ex)
			{
                if (trace == 0)
                    response.Write(ex.Message);
                else
                    response.Write(ex.ToString());
            }
		}
	}
}