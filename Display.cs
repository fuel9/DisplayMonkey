using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace DisplayMonkey
{
	public class Display
	{
		public Display(string host)
		{
			string sql = string.Format("SELECT TOP 1 * FROM DISPLAY WHERE Host='{0}'", host);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0)
				{
					DataRow r = ds.Tables[0].Rows[0];
					DisplayId = (int)r["DisplayId"];
					CanvasId = (int)r["CanvasId"];
					Name = (string)r["Name"];
				}
			}
		}

		public string Name = "";
		public int DisplayId = 0;
		public int CanvasId = 0;
	}
}