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
    public class Html : Frame
	{
        [ScriptIgnore]
        public string Content { get; private set; }

        public Html(int frameId)
            : base(frameId)
        {
            _init();
        }
        
        public Html(Frame frame)
            : base(frame)
		{
            _init();
        }

        private void _init()
        {
            string sql = string.Format("SELECT TOP 1 * FROM Html WHERE FrameId={0}", FrameId);

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    Content = dr.StringOrBlank("Content");
                }
            }
        }
    }
}
