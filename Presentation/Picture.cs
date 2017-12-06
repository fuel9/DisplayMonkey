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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using DisplayMonkey.Models;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
	public class Picture : Frame
	{
        public string Name { get; protected set; }
        public RenderModes Mode { get; protected set; }
        public int ContentId { get; private set; }
        
        public override string Hash 
        {
            get 
            {
                string hash = null;
                if (this.CacheInterval > 0)
                {
                    // attempt crc32 as a hash first
                    UInt64 crc32 = HttpRuntime.Cache.GetItemCrc32(this.CacheKey);

                    // otherwise use unique guid
                    hash = (crc32 != 0) ? crc32.ToString() : 
                        HttpRuntime.Cache.GetItemGuid(this.CacheKey).ToString();
                }
                else
                    hash = base.Hash;   // this will return frame version to avoid spinning browser cache
                
                return hash;
            } 
        }

        [ScriptIgnore]
        public string CacheKey
        {
            get
            {
                return string.Format("picture_{0}_{1}", this.FrameId, base.Version);
            }
        }
        
        public Picture(int frameId)
            : base(frameId)
        {
            _init();
        }

        public Picture(Frame frame)
            : base(frame)
        {
            _init();
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 i.*, Name FROM Picture i INNER JOIN Content c ON c.ContentId=i.ContentId WHERE i.FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    ContentId = dr.IntOrZero("ContentId");
                    Mode = (RenderModes)dr.IntOrZero("Mode");
                    Name = dr.StringOrBlank("Name");
                    return false;
                });
            }
        }

		public static void WriteImage(Stream inStream, Stream outStream, int boundWidth, int boundHeight, RenderModes mode)
		{
			Bitmap bmpSrc = null, bmpTrg = null;
            float scale = 1F;

            try
			{
				bmpSrc = new Bitmap(inStream);

				// convert TIFF to BMP, use only the first page
				FrameDimension fd = new FrameDimension(bmpSrc.FrameDimensionsList[0]);
				bmpSrc.SelectActiveFrame(fd, 0);

				// crop/fit/stretch
                int imageHeight = bmpSrc.Height, imageWidth = bmpSrc.Width, targetWidth, targetHeight;

				if (boundWidth < 0) boundWidth = imageWidth;
				if (boundHeight < 0) boundHeight = imageHeight;

				if (boundWidth != imageWidth || boundHeight != imageHeight)
				{
					switch (mode)
					{
						case RenderModes.RenderMode_Stretch:
							bmpTrg = new Bitmap(bmpSrc, new Size(boundWidth, boundHeight));
							break;

						case RenderModes.RenderMode_Fit:
							targetWidth = imageWidth;
                            targetHeight = imageHeight;

							// a. panel is greater than image: grow
							if (boundHeight > imageHeight && boundWidth > imageWidth)
							{
								scale = Math.Min((float)boundWidth / imageWidth, (float)boundHeight / imageHeight);
								targetHeight = Math.Min((int)((float)imageHeight * scale), boundHeight);
								targetWidth = Math.Min((int)((float)imageWidth * scale), boundWidth);
							}
							// b. image is greater than panel: shrink
							else
							{
								scale = Math.Max((float)imageWidth / boundWidth, (float)imageHeight / boundHeight);
								targetWidth = Math.Min((int)((float)imageWidth / scale), boundWidth);
								targetHeight = Math.Min((int)((float)imageHeight / scale), boundHeight);
							}
							bmpTrg = new Bitmap(bmpSrc, new Size(targetWidth, targetHeight));
							break;

						case RenderModes.RenderMode_Crop:
							targetWidth = Math.Min(boundWidth, imageWidth);
							targetHeight = Math.Min(boundHeight, imageHeight);
							bmpTrg = bmpSrc.Clone(
								new Rectangle(0, 0, targetWidth, targetHeight),
								bmpSrc.PixelFormat
								);
							break;

                        case RenderModes.RenderMode_Fill:
                            targetWidth = imageWidth;
                            targetHeight = imageHeight;
                            
                            // a. panel is greater than image: grow
                            if (boundHeight > imageHeight && boundWidth > imageWidth)
                            {
                                scale = Math.Max((float)boundWidth / imageWidth, (float)boundHeight / imageHeight);
                                targetHeight = (int)((float)imageHeight * scale);
                                targetWidth = (int)((float)imageWidth * scale);
                            }
                            // b. image is greater than panel: shrink
                            else
                            {
                                scale = Math.Min((float)imageWidth / boundWidth, (float)imageHeight / boundHeight);
                                targetWidth = (int)((float)imageWidth / scale);
                                targetHeight = (int)((float)imageHeight / scale);
                            }
                            bmpTrg = new Bitmap(bmpSrc, new Size(targetWidth, targetHeight));
                            break;
                    }

					bmpTrg.Save(outStream, ImageFormat.Png);
				}
				else
					bmpSrc.Save(outStream, ImageFormat.Png);
			}

			finally
			{
				if (bmpTrg != null) bmpTrg.Dispose();
				if (bmpSrc != null) bmpSrc.Dispose();
				GC.Collect();
			}
		}
	}
}