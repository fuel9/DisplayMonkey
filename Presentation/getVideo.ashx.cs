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
using System.Threading.Tasks;
//using System.Drawing;
//using System.Drawing.Imaging;

namespace DisplayMonkey
{
	/// <summary>
	/// Summary description for Image
	/// </summary>
	public class getVideo : HttpTaskAsyncHandler
	{
		public override async Task ProcessRequestAsync(HttpContext context)
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
                VideoAlternative va = new VideoAlternative(video, contentId);

                if (va.ContentId != 0)
                {
                    data = await HttpRuntime.Cache.GetOrAddAbsoluteAsync(
                        va.CacheKey,
                        async (expire) => 
                        {
                            expire.When = DateTime.Now.AddMinutes(video.CacheInterval);
                            
                            Content content = await Content.GetDataAsync(va.ContentId);
                            if (content == null)
                                return null;
                            mediaName = content.Name;
                            return content.Data;
                        });
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
	}
}
