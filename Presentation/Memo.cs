using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;

namespace DisplayMonkey
{
    public class Memo : Frame
	{
        public string Subject { get; private set; }
        public string Body { get; private set; }

        public Memo(Frame frame)
            : base(frame)
        {
            _init();
        }

        private void _init()
        {
            string sql = string.Format("SELECT TOP 1 * FROM Memo WHERE FrameId={0}", FrameId);

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    Subject = dr.StringOrBlank("Subject");
                    Body = dr.StringOrBlank("Body");
                }
            }

            //_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/memo/");
        }
	}
}