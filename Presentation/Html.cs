using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DisplayMonkey
{
	public class Html : Frame
	{
		public Html(int frameId, int panelId)
		{
			PanelId = panelId;
			_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/html.htm");
			string sql = string.Format("SELECT TOP 1 * FROM Html WHERE FrameId={0}", frameId);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
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
						HttpServerUtility util = HttpContext.Current.Server;
						html = string.Format(
							template,
							GetUrl(),
                            ""
							);
					}
				}

				catch (Exception ex)
				{
					html = ex.Message;
				}

				return html;
			}
		}

        private string GetUrl()
        {
            return string.Format(
                "getHtml.ashx?frame={0}",
                FrameId
                );
        }
    }
}