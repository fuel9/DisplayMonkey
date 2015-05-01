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

                int contentId = DataAccess.IntOrZero(Request.QueryString["content"]);
                int frameId = DataAccess.IntOrZero(Request.QueryString["frame"]);

                byte[] data = null;
                int 
                    panelHeight = -1, 
                    panelWidth = -1, 
                    cacheInterval = 60
                    ;
                RenderModes mode = RenderModes.RenderMode_Crop;

                // images can either come from:
                // 1) frames, in which case they are constraint to panel dimensions, their own rendering mode and cacheing
                // 2) general content, in which case they will come unprocessed and cache for 60 minutes

                if (frameId > 0)
                {
                    Picture picture = new Picture(frameId);

                    if (picture.PanelId != 0)
                    {
                        Panel panel = new Panel(picture.PanelId);

                        data = HttpRuntime.Cache.GetOrAddAbsolute(
                            string.Format("picture_{0}_{1}", picture.FrameId, picture.Version),
                            () =>
                            {
                                Content content = new Content(picture.ContentId);
                                if (content.Data == null)
                                    return null;
                                using (MemoryStream trg = new MemoryStream())
                                using (MemoryStream src = new MemoryStream(content.Data))
                                {
                                    Picture.WriteImage(src, trg, panel.Width, panel.Height, picture.Mode);
                                    return trg.GetBuffer();
                                }
                            },
                            DateTime.Now.AddMinutes(picture.CacheInterval)
                            );
                    }
                }

                else if (contentId != 0)
                {
                    data = HttpRuntime.Cache.GetOrAddSliding(
                        string.Format("image_{0}_{1}x{2}_{3}", contentId, panelWidth, panelHeight, (int)mode),
                        () =>
                        {
                            Content content = new Content(contentId);
                            if (content.Data == null)
                                return null;
                            using (MemoryStream trg = new MemoryStream())
                            using (MemoryStream src = new MemoryStream(content.Data))
                            {
                                Picture.WriteImage(src, trg, -1, -1, RenderModes.RenderMode_Crop);
                                return trg.GetBuffer();
                            }
                        },
                        TimeSpan.FromMinutes(cacheInterval)
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
                        Picture.WriteImage(ms, Response.OutputStream, -1, -1, RenderModes.RenderMode_Crop);
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
