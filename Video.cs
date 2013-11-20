using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DisplayMonkey
{
	public class Video : Frame
	{
		public Video(int frameId, int panelId)
		{
			PanelId = panelId;
			_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/video.htm");
			
			string sql = string.Format(
				"SELECT TOP 1 i.*, Name FROM VIDEO i INNER JOIN CONTENT c ON c.ContentId=i.ContentId WHERE i.FrameId={0};",
				frameId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
					ContentId = DataAccess.IntOrZero(dr["ContentId"]);
					Name = DataAccess.StringOrBlank(dr["Name"]);
					PlayMuted = DataAccess.Boolean(dr["PlayMuted"]);
					AutoLoop = DataAccess.Boolean(dr["AutoLoop"]);
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
						Panel panel = null;
						if (FullScreenPanel.Exists(PanelId))
							panel = new FullScreenPanel(PanelId);
						else
							panel = new Panel(PanelId);

						string src = string.Format(
							"getVideo.ashx?content={0}&frame={1}", 
							ContentId, 
							FrameId
							);

						string style = string.Format(
							"width:{0}px;height:{1}px;", 
							panel.Width, 
							panel.Height
							);

						string options = "";

						if (PlayMuted)
							options += " muted=\"true\"";

						if (AutoLoop)
							options += " loop=\"true\"";

						html = string.Format(
							template,
							src,
							style,
							options
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

		public string Name = "";
		public int ContentId = 0;
		public bool PlayMuted = true;
		public bool AutoLoop = true;
	}
}