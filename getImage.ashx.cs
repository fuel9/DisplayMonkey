using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
//using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;

namespace DisplayMonkey
{
	/// <summary>
	/// Summary description for Image
	/// </summary>
	public class getImage : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			HttpRequest Request = context.Request;
			HttpResponse Response = context.Response;

			try
			{
				int contentId = Convert.ToInt32(Request.QueryString["content"]);
				int frameId = Convert.ToInt32(Request.QueryString["frame"]);

				string sql = string.Format(
					"SELECT TOP 1 * FROM CONTENT WHERE ContentId={0}; " +
					"if ({1}>0) SELECT TOP 1 p.Mode, l.Width, l.Height FROM PICTURE p INNER JOIN FRAME f on f.FrameId=p.FrameId INNER JOIN PANEL l on l.PanelId=f.PanelId WHERE p.FrameId={1}; ",
					contentId,
					frameId
					);

				byte[] data = null;
				int panelHeight = -1, panelWidth = -1;
				PictureMode mode = PictureMode.CROP;

				using (DataSet ds = DataAccess.RunSql(sql))
				{
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						DataRow dr = ds.Tables[0].Rows[0];
						//imageHeight = DataAccess.IntOrZero(dr["Height"]);
						//imageWidth = DataAccess.IntOrZero(dr["Width"]);
						if (dr["Data"] != DBNull.Value) data = (byte[])dr["Data"];
					}

					if (frameId > 0 && ds.Tables[1].Rows.Count > 0)
					{
						DataRow dr = ds.Tables[1].Rows[0];
						mode = (PictureMode)DataAccess.IntOrZero(dr["Mode"]);
						panelHeight = DataAccess.IntOrZero(dr["Height"]);
						panelWidth = DataAccess.IntOrZero(dr["Width"]);
					}
				}

				if (data == null)
				{
					data = File.ReadAllBytes("~/files/404.png");
				}

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
