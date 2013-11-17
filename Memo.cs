using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DisplayMonkey
{
	public class Memo : Frame
	{
		public Memo(int frameId, int panelId, HttpServerUtility server)
		{
			PanelId = panelId;
			_templatePath = server.MapPath("~/files/frames/memo.htm");
			string sql = string.Format("SELECT TOP 1 * FROM MEMO WHERE FrameId={0}", frameId);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
					Subject = DataAccess.StringOrBlank(dr["Subject"]);
					Body = DataAccess.StringOrBlank(dr["Body"]);
				}
			}
		}

		public string Html
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
						html = string.Format(template, Subject, Body);
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

		public string Subject;
		public string Body;

		private string _templatePath;
	}
}