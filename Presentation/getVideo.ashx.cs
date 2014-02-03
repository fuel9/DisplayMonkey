using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;

namespace DisplayMonkey
{
	/// <summary>
	/// Summary description for Image
	/// </summary>
	public class getVideo : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			HttpRequest Request = context.Request;
			HttpResponse Response = context.Response;

			byte[] data = null;
			string mediaName = "";

			try
			{
				int contentId = Convert.ToInt32(Request.QueryString["content"]);

				string sql = string.Format(
					"SELECT TOP 1 Data, Name FROM Content WHERE ContentId={0}; ",
					contentId
					);

				using (DataSet ds = DataAccess.RunSql(sql))
				{
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						DataRow dr = ds.Tables[0].Rows[0];
						if (dr["Data"] != DBNull.Value)
							data = (byte[])dr["Data"];
						mediaName = DataAccess.StringOrBlank(dr["Name"]);
					}
				}
			}

			catch (Exception ex)
			{
				throw new HttpException(500, ex.Message);
			}


			if (data == null)
			{
				throw new HttpException(404, "File is empty");
			}

			else
			{
				string mimeType = null;

				try
				{
					if (null == (mimeType = MimeTypeParser.GetMimeTypeRaw(data)))
					{
						if (null == (mimeType = MimeTypeParser.GetMimeTypeFromRegistry(mediaName)))
						{
							if (null == (mimeType = MimeTypeParser.GetMimeTypeFromList(mediaName)))
							{
								mimeType = "application/octet-stream";
							}
						}
					}
				}

				catch
				{
					mimeType = "application/octet-stream";
				}

				Response.ContentType = mimeType;
				Response.AddHeader("Content-Disposition", string.Format(
					"attachment; filename=\"{0}\"",
					mediaName
					));
				Response.AddHeader("Content-Length", data.Length.ToString());
			}

			Response.BinaryWrite(data);
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
