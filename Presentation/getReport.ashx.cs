using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
//using System.Data;
using System.Configuration;
using System.Net;
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
				int frameId = Convert.ToInt32(Request.QueryString["frame"]);

                Report report = new Report(frameId);
                Panel panel = new Panel(report.PanelId);

				byte[] data = null;
				int panelHeight = panel.Height, panelWidth = panel.Width;
				PictureMode mode = report.Mode;
                string user = report.User, domain = report.Domain;
                string baseUrl = report.BaseUrl, url = report.Path;
                byte[] password = report.Password;

                if (baseUrl.EndsWith("/"))
                    baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

                if (!url.StartsWith("/"))
                    url = "/" + url;
                
                // report URL
				url = string.Format(
					"{0}?{1}&rs:format=IMAGE",
                    baseUrl,
					HttpUtility.UrlEncode(url)
					);

				//throw new Exception(url);

				// get response from report server
				WebClient client = new WebClient();
                if (!string.IsNullOrWhiteSpace(user))
                {
                    client.Credentials = new NetworkCredential(
                        user.Trim(),
                        RsaUtil.Decrypt(password),
                        domain.Trim()
                        );
                }
				data = client.DownloadData(url);

				//TiffBitmapDecoder decoder = new TiffBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
				//BitmapSource bitmapSource = decoder.Frames[0];

                // prevent client caching, return PNG
                Response.Clear();
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetSlidingExpiration(true);
                Response.Cache.SetNoStore();
                Response.ContentType = "image/png";
                
                using (MemoryStream ms = new MemoryStream(data))
				{
					Picture.WriteImage(ms, Response.OutputStream, panelWidth, panelHeight, mode);
				}

                Response.OutputStream.Flush();
            }

			catch (Exception ex)
			{
				Response.Write(ex.Message);
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