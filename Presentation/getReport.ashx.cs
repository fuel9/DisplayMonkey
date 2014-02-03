using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
using System.Configuration;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;

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

				string sql = string.Format(
					"SELECT TOP 1 p.Mode, p.Path, l.Width, l.Height, s.BaseUrl, s.[User], s.Domain, s.Password FROM Report p INNER JOIN ReportServer s on s.ServerId=p.ServerId INNER JOIN Frame f on f.FrameId=p.FrameId INNER JOIN Panel l on l.PanelId=f.PanelId WHERE p.FrameId={0}; ",
					frameId
					);

				byte[] data = null;
				int panelHeight = 0, panelWidth = 0;
				PictureMode mode = PictureMode.CROP;
                string user = null, domain = null, password = null;
                string baseUrl = "", url = "";

				using (DataSet ds = DataAccess.RunSql(sql))
				{
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						DataRow dr = ds.Tables[0].Rows[0];
						mode = (PictureMode)DataAccess.IntOrZero(dr["Mode"]);
						url = DataAccess.StringOrBlank(dr["Path"]);
						panelHeight = DataAccess.IntOrZero(dr["Height"]);
						panelWidth = DataAccess.IntOrZero(dr["Width"]);
                        baseUrl = DataAccess.StringOrBlank(dr["BaseUrl"]);
                        user = DataAccess.StringOrBlank(dr["User"]);
                        domain = DataAccess.StringOrBlank(dr["Domain"]);
                        password = RsaUtil.Decrypt((byte[])dr["Password"]);
                    }
				}

				// report URL
				url = string.Format(
					"{0}?{1}&rs:format=IMAGE",
                    baseUrl,
					url
					);

				//throw new Exception(url);

				// get response from report server
				WebClient client = new WebClient();
                if (!string.IsNullOrEmpty(user))
                {
                    client.Credentials = new NetworkCredential(
                        user,
                        password,
                        domain
                        );
                }
				data = client.DownloadData(url);

				//TiffBitmapDecoder decoder = new TiffBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
				//BitmapSource bitmapSource = decoder.Frames[0];

				using (MemoryStream ms = new MemoryStream(data))
				{
					Picture.WriteImage(ms, Response.OutputStream, panelWidth, panelHeight, mode);
				}

				// return PNG
				Response.ContentType = "image/png";
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