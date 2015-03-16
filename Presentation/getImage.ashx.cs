using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
//using System.Data;
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
                // set headers, prevent client caching, return PNG
                Response.Clear();
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetSlidingExpiration(true);
                Response.Cache.SetNoStore();
                Response.ContentType = "image/png";

                byte[] data = null, cache = null;
                int panelHeight = -1, panelWidth = -1;
                PictureMode mode = PictureMode.CROP;

                int contentId = DataAccess.IntOrZero(Request.QueryString["content"]);
                int frameId = DataAccess.IntOrZero(Request.QueryString["frame"]);

                if (frameId > 0)
                {
                    Picture picture = new Picture(frameId);

                    if (picture.ContentId != 0)
                    {
                        Panel panel = new Panel(picture.PanelId);
                        panelHeight = panel.Height;
                        panelWidth = panel.Width;
                        mode = picture.Mode;
                        contentId = picture.ContentId;
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

                cache = HttpRuntime.Cache.GetOrAddSliding(
                    string.Format("image_{0}_{1}x{2}_{3}", contentId, panelWidth, panelHeight, (int)mode),
                    () =>
                    {
                        Content content = new Content(contentId);
                        if (content.Data != null)
                        {
                            using (MemoryStream trg = new MemoryStream())
                            using (MemoryStream src = new MemoryStream(content.Data))
                            {
                                Picture.WriteImage(src, trg, panelWidth, panelHeight, mode);
                                return trg.GetBuffer();
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
