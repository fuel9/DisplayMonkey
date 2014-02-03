using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DisplayMonkey
{
	public class Report : Picture
	{
		public Report(int frameId, int panelId)
			: base()
		{
			PanelId = panelId;
			_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/report.htm");
			string sql = string.Format("SELECT TOP 1 * FROM Report WHERE FrameId={0}", frameId);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
					Path = DataAccess.StringOrBlank(dr["Path"]);
					Mode = (PictureMode)DataAccess.IntOrZero(dr["Mode"]);
					Name = DataAccess.StringOrBlank(dr["Name"]);
				}
			}
		}

		protected override string GetUrl()
		{
			return string.Format(
				"getReport.ashx?frame={0}",
				FrameId
				);
		}

		public string Path = "";
	}
}