using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace DisplayMonkey
{
	public class Video : Frame
	{
		public Video(int frameId, int panelId)
		{
			PanelId = panelId;
			_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/video.htm");
			
			string sql = string.Format(
				"SELECT TOP 1 * FROM Video WHERE FrameId={0};",
				frameId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
					PlayMuted = DataAccess.Boolean(dr["PlayMuted"]);
					AutoLoop = DataAccess.Boolean(dr["AutoLoop"]);
				}
			}

			_list = VideoAlternative.List(frameId);
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
						// styles
						Panel panel = null;
						if (Panel.IsFullScreen(PanelId))
							panel = new FullScreenPanel(PanelId);
						else
							panel = new Panel(PanelId);

						string style = string.Format(
							"width:{0}px;height:{1}px;",
							panel.Width,
							panel.Height
							);

						// sources
						StringBuilder sources = new StringBuilder(_list.Count);
						foreach (VideoAlternative va in _list)
						{
							sources.AppendFormat(
								"<source src=\"getVideo.ashx?content={0}&frame={1}\" type=\"{2}\" />\r\n",
								va.ContentId,
								FrameId,
								va.MimeType
								);
						}
						
						// options
						StringBuilder options = new StringBuilder();
						if (PlayMuted)
							options.Append(" muted=\"true\"");
						if (AutoLoop)
							options.Append(" loop=\"true\"");

						// put all together
						html = string.Format(
							template,
							sources.ToString(),
							style,
							options.ToString()
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

		/*
			<style type="text/css" scoped>
			...
			</style>
		 * */

		public bool PlayMuted = true;
		public bool AutoLoop = true;

		private List<VideoAlternative> _list = null;

		private class VideoAlternative
		{
			public string Name = "";
			private byte[] Chunk = null;
			public int ContentId = 0;

			public string MimeType
			{
				get
				{
					string mimeType = null;
					try
					{
						if (null == (mimeType = MimeTypeParser.GetMimeTypeRaw(Chunk)))
						{
							if (null == (mimeType = MimeTypeParser.GetMimeTypeFromRegistry(Name)))
							{
								if (null == (mimeType = MimeTypeParser.GetMimeTypeFromList(Name)))
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
					return mimeType;
				}
			}

			public VideoAlternative()
			{
			}

			public static List<VideoAlternative> List(int frameId)
			{
				List<VideoAlternative> list = new List<VideoAlternative>();
				string sql = string.Format(
					"SELECT c.ContentId, Name, convert(varbinary(256),Data) Chunk FROM VideoAlternative a INNER JOIN Content c ON c.ContentId=a.ContentId WHERE a.FrameId={0};",
					frameId
					);

				using (DataSet ds = DataAccess.RunSql(sql))
				{
					int count = ds.Tables[0].Rows.Count;
					if (count > 0)
					{
						list.Capacity = count;
						foreach (DataRow dr in ds.Tables[0].Rows)
						{
							if (dr["Chunk"] != DBNull.Value)
							{
								VideoAlternative va = new VideoAlternative()
								{
									ContentId = DataAccess.IntOrZero(dr["ContentId"]),
									Name = DataAccess.StringOrBlank(dr["Name"]),
									Chunk = (byte[])dr["Chunk"],
								};
								list.Add(va);
							}
						}
					}
				}

				return list;
			}
		}
	}
}