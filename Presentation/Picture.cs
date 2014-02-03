using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace DisplayMonkey
{
	public enum PictureMode { CROP = 0, STRETCH = 1, FIT = 2 }

	public class Picture : Frame
	{
		public Picture()
		{
		}

		public Picture(int frameId, int panelId)
		{
			PanelId = panelId > 0 ? panelId : GetPanelId(frameId);
			_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/picture.htm");
			string sql = string.Format(
				"SELECT TOP 1 i.*, Name FROM Picture i INNER JOIN Content c ON c.ContentId=i.ContentId WHERE i.FrameId={0};",
				frameId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
					ContentId = DataAccess.IntOrZero(dr["ContentId"]);
					Mode = (PictureMode)DataAccess.IntOrZero(dr["Mode"]);
					Name = DataAccess.StringOrBlank(dr["Name"]);
					//PanelWidth = DataAccess.IntOrZero(dr["Width"]);
					//PanelHeight = DataAccess.IntOrZero(dr["Height"]);
				}
			}
		}

		public override string Html
		{
			get
			{
				string html = "";
				try
				{
					// load template
					string template = File.ReadAllText(_templatePath);

					// fill template
					if (FrameId > 0)
					{
						string style = "";

						if (Mode != PictureMode.CROP)
						{
							Panel panel = null;

							if (FullScreenPanel.Exists(PanelId))
								panel = new FullScreenPanel(PanelId);
							else
								panel = new Panel(PanelId);

							switch (Mode)
							{
								case PictureMode.FIT:
									style = string.Format(
										"max-width:{0}px;max-height:{1}px;", 
										panel.Width, 
										panel.Height
										);
									break;

								case PictureMode.STRETCH:
									style = string.Format(
										"width:{0}px;height:{1}px;", 
										panel.Width, 
										panel.Height
										);
									break;
							}
						}

						html = string.Format(template, 
							GetUrl(),
							style,
							Name
							);
					}
				}

				catch (Exception ex)
				{
					html = ex.Message;
				}

				// return html
				return html;
			}
		}

		public static void WriteImage(Stream inStream, Stream outStream, int panelWidth, int panelHeight, PictureMode mode)
		{
			Bitmap bmpSrc = null, bmpTrg = null;

			try
			{
				bmpSrc = new Bitmap(inStream);

				// convert TIFF to BMP, use only the first page
				FrameDimension fd = new FrameDimension(bmpSrc.FrameDimensionsList[0]);
				bmpSrc.SelectActiveFrame(fd, 0);

				// crop/fit/stretch
				int imageHeight = bmpSrc.Height, imageWidth = bmpSrc.Width;

				if (panelWidth < 0) panelWidth = imageWidth;
				if (panelHeight < 0) panelHeight = imageHeight;

				if (panelWidth != imageWidth || panelHeight != imageHeight)
				{
					switch (mode)
					{
						case PictureMode.STRETCH:
							bmpTrg = new Bitmap(bmpSrc, new Size(panelWidth, panelHeight));
							break;

						case PictureMode.FIT:
							int targetWidth = imageWidth, targetHeight = imageHeight;
							float scale = 1F;
							// a. panel is greater than image: grow
							if (panelHeight > imageHeight && panelWidth > imageWidth)
							{
								scale = Math.Min((float)panelWidth / imageWidth, (float)panelHeight / imageHeight);
								targetHeight = Math.Min((int)((float)imageHeight * scale), panelHeight);
								targetWidth = Math.Min((int)((float)imageWidth * scale), panelWidth);
							}
							// b. image is greater than panel: shrink
							else
							{
								scale = Math.Max((float)imageWidth / panelWidth, (float)imageHeight / panelHeight);
								targetWidth = Math.Min((int)((float)imageWidth / scale), panelWidth);
								targetHeight = Math.Min((int)((float)imageHeight / scale), panelHeight);
							}
							bmpTrg = new Bitmap(bmpSrc, new Size(targetWidth, targetHeight));
							break;

						case PictureMode.CROP:
							targetWidth = Math.Min(panelWidth, imageWidth);
							targetHeight = Math.Min(panelHeight, imageHeight);
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
	
		protected virtual string GetUrl()
		{
			return string.Format(
				"getImage.ashx?content={0}&frame={1}", 
				ContentId, 
				FrameId
				);
		}

		public string Name = "";
		public int ContentId = 0;
		public PictureMode Mode = PictureMode.CROP;

		protected string _templatePath;
	}
}