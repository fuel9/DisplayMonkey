using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
//using System.Drawing;
//using System.Drawing.Imaging;

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

            int frameId = DataAccess.IntOrZero(Request.QueryString["frame"]);
            int contentId = DataAccess.IntOrZero(Request.QueryString["content"]);
            
            try
			{
                Video video = new Video(frameId);

                if (contentId != 0)
                {
                    data = HttpRuntime.Cache.GetOrAddAbsolute(
                        string.Format("video_{0}_{1}_{2}", video.FrameId, video.Version, contentId),
                        () => 
                        { 
                            Content content = new Content(contentId);
                            if (content.ContentId == 0)
                                return null;
                            mediaName = content.Name;
                            return content.Data;
                        },
                        DateTime.Now.AddMinutes(video.CacheInterval)
                        );
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
            Response.Flush();
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
