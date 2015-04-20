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

namespace DisplayMonkey
{
	public class Picture : Frame
	{
        public string Name { get; protected set; }
        public RenderModes Mode { get; protected set; }
        public int ContentId { get; private set; }
        
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
            string sql = string.Format(
                "SELECT TOP 1 i.*, Name FROM Picture i INNER JOIN Content c ON c.ContentId=i.ContentId WHERE i.FrameId={0};",
                FrameId
                );

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    ContentId = dr.IntOrZero("ContentId");
                    Mode = (RenderModes)dr.IntOrZero("Mode");
                    Name = dr.StringOrBlank("Name");
                }
            }

            //_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/picture/");
        }

		public static void WriteImage(Stream inStream, Stream outStream, int boundWidth, int boundHeight, RenderModes mode)
		{
			Bitmap bmpSrc = null, bmpTrg = null;

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
							float scale = 1F;
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