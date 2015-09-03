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
//using System.Drawing;
//using System.Drawing.Imaging;

namespace DisplayMonkey
{
	/// <summary>
	/// Summary description for getReport
	/// </summary>
	public class getReport : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			HttpRequest Request = context.Request;
			HttpResponse Response = context.Response;

			try
			{
                // set headers, prevent client caching, return PNG
                Response.Clear();
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetSlidingExpiration(true);
                Response.Cache.SetNoStore();
                Response.ContentType = "image/png";

                int frameId = Convert.ToInt32(Request.QueryString["frame"]);

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

                    data = HttpRuntime.Cache.GetOrAddAbsolute(
                        report.CacheKey,
                        () =>
                        {
                            // get response from report server
                            WebClient client = new WebClient();
                            if (!string.IsNullOrWhiteSpace(report.User))
                            {
                                client.Credentials = new NetworkCredential(
                                    report.User.Trim(),
                                    RsaUtil.Decrypt(report.Password),
                                    report.Domain.Trim()
                                    );
                            }

                            byte[] repBytes = client.DownloadData(report.Url);

                            if (repBytes == null)
                                return null;

                            using (MemoryStream trg = new MemoryStream())
                            using (MemoryStream src = new MemoryStream(repBytes))
                            {
                                Picture.WriteImage(src, trg, panelWidth, panelHeight, mode);
                                return trg.GetBuffer();
                            }
                        },
                        DateTime.Now.AddMinutes(report.CacheInterval)
                        );
                }

                if (data != null)
                {
                    Response.OutputStream.Write(data, 0, data.Length);
                }

                else
                {
                    data = File.ReadAllBytes("~/files/404.png");
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        Picture.WriteImage(ms, Response.OutputStream, panelWidth, panelHeight, mode);
                    }
                }
            }

			catch (Exception ex)
			{
				Response.Write(ex.Message);
			}

            finally
            {
                Response.OutputStream.Flush();
            }
        }

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}