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
	public class YouTube : Frame
	{
        public YouTube(int frameId, int panelId)
		{
			PanelId = panelId;
			_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/youtube.htm");
			
			string sql = string.Format(
				"SELECT TOP 1 * FROM Youtube WHERE FrameId={0};",
				frameId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
                    YoutubeId = DataAccess.StringOrBlank(dr["YoutubeId"]);
					AutoLoop = DataAccess.Boolean(dr["AutoLoop"]);
                    Volume = DataAccess.IntOrZero(dr["Volume"]);
                    Aspect = DataAccess.IntOrZero(dr["Aspect"]);
                    Quality = DataAccess.IntOrZero(dr["Quality"]);
                    Start = DataAccess.IntOrZero(dr["Start"]);
                    Rate = DataAccess.IntOrZero(dr["Rate"]);
                }
			}
		}

        public override string Payload
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

						// put all together
						html = string.Format(
							template,
                            FrameId,
                            YoutubeId,
							AutoLoop ? 1 : 0,
                            Volume,
                            panel.Width,
                            panel.Height,
                            Aspect,
                            Quality,
                            Start,
                            Rate
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

        public string YoutubeId = null;
		public bool AutoLoop = true;
        public int Volume = 0;
        public int Aspect = 0;
        public int Quality = 0;
        public int Start = 0;
        public int Rate = 0;
    }
}