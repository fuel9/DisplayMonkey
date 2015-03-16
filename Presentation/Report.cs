using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
	public class Report : Frame
	{
        public string Name { get; protected set; }
        public PictureMode Mode { get; protected set; }

        [ScriptIgnore]
        public string Path { get; private set; }
        [ScriptIgnore]
        public string BaseUrl { get; private set; }
        [ScriptIgnore]
        public string User { get; private set; }
        [ScriptIgnore]
        public string Domain { get; private set; }
        [ScriptIgnore]
        public byte[] Password { get; private set; }

        public Report(Frame frame)
            : base(frame)
        {
            _init();
        }

        public Report(int frameId, int panelId = 0)
            : base(frameId, panelId)
        {
            _init();
        }

        private void _init()
        {
            string sql = string.Format("SELECT TOP 1 r.*, s.BaseUrl, s.[User], s.Domain, s.Password FROM Report r INNER JOIN ReportServer s on s.ServerId=r.ServerId WHERE FrameId={0}", FrameId);

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    Path = dr.StringOrBlank("Path").Trim();
                    Mode = (PictureMode)dr.IntOrZero("Mode");
                    Name = dr.StringOrBlank("Name");

                    BaseUrl = dr.StringOrBlank("BaseUrl").Trim();
                    User = dr.StringOrBlank("User").Trim();
                    Domain = dr.StringOrBlank("Domain").Trim();
                    Password = (byte[])dr["Password"];
                }
            }

            _templatePath = HttpContext.Current.Server.MapPath("~/files/frames/report/default.htm");
        }
	}
}