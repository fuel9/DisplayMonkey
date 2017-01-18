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
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

			byte[] data = null;
			string mediaName = "";

            int frameId = request.IntOrZero("frame");
            int contentId = request.IntOrZero("content");
            int trace = request.IntOrZero("trace");
            
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
                if (trace == 0)
                    throw new HttpException(500, ex.Message);
                else
                    throw new HttpException(500, ex.ToString());
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

				response.ContentType = mimeType;
				response.AddHeader("Content-Disposition", string.Format(
					"attachment; filename=\"{0}\"",
					mediaName
					));
				response.AddHeader("Content-Length", data.Length.ToString());
			}

			response.BinaryWrite(data);
            response.Flush();
		}
	}
}
