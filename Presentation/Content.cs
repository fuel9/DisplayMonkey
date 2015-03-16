using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace DisplayMonkey
{
    public enum ContentType { PICTURE = 0, VIDEO = 1 }

    public class Content
    {
        public int ContentId { get; private set; }
        public byte[] Data { get; private set; }
        public string Name { get; private set; }
        public ContentType Type { get; private set; }

        public Content(int contentId)
        {
			string sql = string.Format(
				"SELECT TOP 1 * FROM Content WHERE ContentId={0}; ",
                contentId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
                    ContentId = dr.IntOrZero("ContentId");
					Name = dr.StringOrBlank("Name");
                    Type = (ContentType)dr.IntOrZero("Type");
                    if (dr["Data"] != DBNull.Value)
                        Data = (byte[])dr["Data"];
                }
			}
        }
    }
}