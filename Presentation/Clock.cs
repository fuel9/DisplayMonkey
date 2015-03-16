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
        public bool ShowDate { get; private set; }
        public bool ShowTime { get; private set; }
        public int Type { get; private set; }
        public int? OffsetGmt { get; private set; }
        public string Label { get; private set; }

        [ScriptIgnore]
        public bool ShowSeconds = true;     // TODO: add column

        public Clock(Frame frame)
            : base(frame)
        {
            string sql = string.Format(
                "SELECT TOP 1 * FROM Clock WHERE FrameId={0};",
                frame.FrameId
                );

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    FrameId = dr.IntOrZero("FrameId");
                    this.ShowDate = dr.Boolean("ShowDate");
                    this.ShowTime = dr.Boolean("ShowTime");
                    this.Type = dr.IntOrZero("Type");
                    if (dr["OffsetGMT"] != DBNull.Value)
                        this.OffsetGmt = (int?)dr.IntOrZero("OffsetGMT");
                    this.Label = dr.StringOrDefault("Label", null);
                }
            }

            _templatePath = HttpContext.Current.Server.MapPath("~/files/frames/clock/default.htm");
        }
	}
}
