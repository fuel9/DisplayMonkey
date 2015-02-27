using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml;
using System.Web.Script.Serialization;


namespace DisplayMonkey
{
	public class Clock : Frame
	{
		public Clock(int frameId, int panelId, int displayId)
		{
			PanelId = panelId;
			string sql = string.Format(
				"SELECT TOP 1 * FROM Clock WHERE FrameId={0};",
				frameId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
					ShowDate = (bool)dr["ShowDate"];
					ShowTime = (bool)dr["ShowTime"];
                    Type = DataAccess.IntOrZero(dr["Type"]);
                    if (dr["OffsetGMT"] != DBNull.Value)
                        OffsetGmt = (int?)dr["OffsetGMT"];
                    if (dr["Label"] != DBNull.Value)
                        Label = (string)dr["Label"];
                }
			}
        
            _templatePath = HttpContext.Current.Server.MapPath("~/files/frames/clock.htm");
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
                        html = string.Format(template, 
                            FrameId,
                            new JavaScriptSerializer().Serialize(new
                            {
                                id = this.FrameId,
                                type = this.Type,
                                showDate = this.ShowDate,
                                showTime = this.ShowTime,
                                offsetGmt = this.OffsetGmt,
                                showSeconds = this.ShowSeconds,
                                cssClass = this.CssClass,
                            }),
                            this.Label
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

		public bool ShowDate;
		public bool ShowTime;
        public int Type;
        public int? OffsetGmt;
        public string Label;

        public string CssClass;             // TODO
        public bool ShowSeconds = true;     // TODO
	}
}
