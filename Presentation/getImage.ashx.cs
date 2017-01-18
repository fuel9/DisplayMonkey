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
//using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using DisplayMonkey.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DisplayMonkey
{
	/// <summary>
	/// Summary description for Image
	/// </summary>
	public class getImage : HttpTaskAsyncHandler
	{
		public override async Task ProcessRequestAsync(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

            int contentId = request.IntOrZero("content");
            int frameId = request.IntOrZero("frame");
            int trace = request.IntOrZero("trace");

            try
            {
                // set headers, prevent client caching, return PNG
                response.Clear();
                response.Cache.SetCacheability(HttpCacheability.NoCache);
                response.Cache.SetSlidingExpiration(true);
                response.Cache.SetNoStore();

                //Response.Headers.Add("Pragma-directive", "no-cache");
                //Response.Headers.Add("Cache-directive", "no-cache");
                //Response.Headers.Add("Cache-control", "no-cache");
                //Response.Headers.Add("Pragma", "no-cache");
                //Response.Headers.Add("Expires", "0");

                response.ContentType = "image/png";

                byte[] data = null;

                // images can either come from:
                // 1) frames, in which case they are constraint to panel dimensions, their own rendering mode and cacheing
                // 2) general content, in which case they will come unprocessed and cache for 60 minutes

                if (frameId > 0)
                {
                    Picture picture = new Picture(frameId);

                    if (picture.PanelId != 0)
                    {
                        Panel panel = new Panel(picture.PanelId);

                        data = await HttpRuntime.Cache.GetOrAddAbsoluteAsync(
                            picture.CacheKey,
                            async (expire) =>
                            {
                                expire.When = DateTime.Now.AddMinutes(picture.CacheInterval);
                                Content content = await Content.GetDataAsync(picture.ContentId); // new Content(picture.ContentId);
                                if (content.Data == null)
                                    return null;
                                using (MemoryStream trg = new MemoryStream())
                                using (MemoryStream src = new MemoryStream(content.Data))
                                {
                                    Picture.WriteImage(src, trg, panel.Width, panel.Height, picture.Mode);
                                    return trg.GetBuffer();
                                }
                            });
                    }
                }

                else if (contentId != 0)
                {
                    data = await HttpRuntime.Cache.GetOrAddSlidingAsync(
                        string.Format("image_{0}_{1}x{2}_{3}", contentId, -1, -1, (int)RenderModes.RenderMode_Crop),
                        async (expire) =>
                        {
                            expire.After = TimeSpan.FromMinutes(60);
                            Content content = await Content.GetDataAsync(contentId); // new Content(contentId);
                            if (content.Data == null)
                                return null;
                            using (MemoryStream trg = new MemoryStream())
                            using (MemoryStream src = new MemoryStream(content.Data))
                            {
                                await Task.Run(() => Picture.WriteImage(src, trg, -1, -1, RenderModes.RenderMode_Crop));
                                return trg.GetBuffer();
                            }
                        });
                }

                if (data != null)
                {
                    await response.OutputStream.WriteAsync(data, 0, data.Length);
                }

                else
                {
                    Content missingContent = await Content.GetMissingContentAsync();
                    using (MemoryStream ms = new MemoryStream(missingContent.Data))
                    {
                        await Task.Run(() => Picture.WriteImage(ms, response.OutputStream, -1, -1, RenderModes.RenderMode_Crop));
                    }
                }

                await response.OutputStream.FlushAsync();
            }

            catch (Exception ex)
            {
                Debug.Print(string.Format("getImage error: {0}", ex.Message));
                if (trace == 0)
                    response.Write(ex.Message);
                else
                    response.Write(ex.ToString());
            }

            /*finally
            {
                await Response.OutputStream.FlushAsync();
            }*/
		}
	}
}
