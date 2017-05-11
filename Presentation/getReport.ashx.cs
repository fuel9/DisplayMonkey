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
using DisplayMonkey.Models;
using System.Threading.Tasks;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Imaging;

namespace DisplayMonkey
{
	/// <summary>
	/// Summary description for getReport
	/// </summary>
    public class getReport : HttpTaskAsyncHandler
	{
		public override async Task ProcessRequestAsync(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

            int frameId = request.IntOrZero("frame");
            int trace = request.IntOrZero("trace");

            try
			{
                // set headers, prevent client caching, return PNG
                response.Clear();
                response.Cache.SetCacheability(HttpCacheability.NoCache);
                response.Cache.SetSlidingExpiration(true);
                response.Cache.SetNoStore();
                response.ContentType = "image/png";

                byte[] data = null;
                int panelHeight = -1, panelWidth = -1;
                RenderModes mode = RenderModes.RenderMode_Crop;

                Report report = new Report(frameId);

                if (report.FrameId != 0)
                {
                    Panel panel = new Panel(report.PanelId);
                    if (panel.PanelId != 0)
                    {
                        panelWidth = panel.Width;
                        panelHeight = panel.Height;
                    }

                    mode = report.Mode;

                    //TiffBitmapDecoder decoder = new TiffBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    //BitmapSource bitmapSource = decoder.Frames[0];

                    data = await HttpRuntime.Cache.GetOrAddAbsoluteAsync(
                        report.CacheKey,
                        async (expire) =>
                        {
                            expire.When = DateTime.Now.AddMinutes(report.CacheInterval);

                            // get response from report server
                            WebClient client = new WebClient();
                            if (!string.IsNullOrWhiteSpace(report.User))
                            {
                                client.Credentials = new NetworkCredential(
                                    report.User.Trim(),
                                    Encryptor.Current.Decrypt(report.Password),
                                    report.Domain.Trim()
                                    );
                            }

                            byte[] repBytes = await client.DownloadDataTaskAsync(report.Url);

                            if (repBytes == null)
                                return null;

                            using (MemoryStream trg = new MemoryStream())
                            using (MemoryStream src = new MemoryStream(repBytes))
                            {
                                await Task.Run(() => Picture.WriteImage(src, trg, panelWidth, panelHeight, mode));
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
                Debug.Print(string.Format("getReport error: {0}", ex.Message));
                if (trace == 0)
                    response.Write(ex.Message);
                else
                    response.Write(ex.ToString());
			}

            /*finally
            {
                Response.OutputStream.Flush();
            }*/
        }
	}
}