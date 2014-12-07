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
                int contentId = DataAccess.IntOrZero(Request.QueryString["content"]);
                int frameId = DataAccess.IntOrZero(Request.QueryString["frame"]);

                // set headers, prevent client caching, return PNG
                Response.Clear();
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetSlidingExpiration(true);
                Response.Cache.SetNoStore();
                Response.ContentType = "image/png";

                string sql;
                byte[] data = null, cache = null;
                int panelHeight = -1, panelWidth = -1;
                PictureMode mode = PictureMode.CROP;

                if (frameId > 0)
                {
                    sql = string.Format(
                        "SELECT TOP 1 p.Mode, p.ContentId, l.Width, l.Height FROM Picture p INNER JOIN Frame f on f.FrameId=p.FrameId INNER JOIN Panel l on l.PanelId=f.PanelId WHERE p.FrameId={0};",
                        frameId
                        );

                    using (DataSet ds = DataAccess.RunSql(sql))
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            mode = (PictureMode)DataAccess.IntOrZero(dr["Mode"]);
                            panelHeight = DataAccess.IntOrZero(dr["Height"]);
                            panelWidth = DataAccess.IntOrZero(dr["Width"]);
                            contentId = DataAccess.IntOrZero(dr["ContentId"]);
                        }

                        else
                        {
                            data = File.ReadAllBytes("~/files/404.png");
                            using (MemoryStream ms = new MemoryStream(data))
                            {
                                Picture.WriteImage(ms, Response.OutputStream, panelWidth, panelHeight, mode);
                            }
                            return;
                        }
                    }
                }

                cache = HttpRuntime.Cache.GetOrAddSliding(
                    string.Format("image_{0}_{1}x{2}_{3}", contentId, panelWidth, panelHeight, (int)mode),
                    () =>
                    {
                        sql = string.Format(
                            "SELECT TOP 1 * FROM Content WHERE ContentId={0}; ",
                            contentId
                            );

                        using (DataSet ds = DataAccess.RunSql(sql))
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow dr = ds.Tables[0].Rows[0];
                                if (dr["Data"] != DBNull.Value)
                                {
                                    using (MemoryStream trg = new MemoryStream())
                                    using (MemoryStream src = new MemoryStream((byte[])dr["Data"]))
                                    {
                                        Picture.WriteImage(src, trg, panelWidth, panelHeight, mode);
                                        return trg.GetBuffer();
                                    }
                                }
                            }
                        }

                        return null;
                    },
                    TimeSpan.FromHours(1)
                    );

                if (cache != null)
                {
                    Response.OutputStream.Write(cache, 0, cache.Length);
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

		public bool IsReusable { get { return false; } }
	}
}
