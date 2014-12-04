using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace DisplayMonkey
{
	public class Display
	{
		public Display()
		{
		}
		
		public Display(int displayId)
		{
			string sql = string.Format("SELECT TOP 1 * FROM Display WHERE displayId={0}", displayId);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0)
				{
					DataRow r = ds.Tables[0].Rows[0];
					InitFromRow(r);
				}
			}
		}

		public Display(string host)
		{
			string sql = string.Format("SELECT TOP 1 * FROM Display WHERE Host='{0}'", host);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0)
				{
					DataRow r = ds.Tables[0].Rows[0];
					InitFromRow(r);
				}
			}
		}

		public void InitFromRow(DataRow r)
		{
			DisplayId = DataAccess.IntOrZero(r["DisplayId"]);
			CanvasId = DataAccess.IntOrZero(r["CanvasId"]);
			LocationId = DataAccess.IntOrZero(r["LocationId"]);
			Host = DataAccess.StringOrBlank(r["Host"]);
            ShowErrors = DataAccess.Boolean(r["ShowErrors"]);
			Name = DataAccess.StringOrBlank(r["Name"]);
			if (Name == "")
				Name = string.Format("Display {0}", DisplayId);
		}
	
		public static List<Display> List
		{
			get
			{
                List<Display> list = new List<Display>();
                string sql = "SELECT * FROM Display ORDER BY 1";
                using (DataSet ds = DataAccess.RunSql(sql))
                {
                    list.Capacity = ds.Tables[0].Rows.Count;

                    // list registered displays
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        Display display = new Display(DataAccess.IntOrZero(r["DisplayId"]));
                        list.Add(display);
                    }
                }
                return list;
			}
		}

		public void Register()
		{
			if (Host == "" || Name == "" || CanvasId == 0)
				return;

			using (SqlCommand cmd = new SqlCommand("sp_RegisterDisplay"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = Name;
				cmd.Parameters.Add("@host", SqlDbType.VarChar, 100).Value = Host;
				cmd.Parameters.Add("@canvasId", SqlDbType.Int).Value = CanvasId;
				cmd.Parameters.Add("@locationId", SqlDbType.Int).Value = LocationId;
				cmd.Parameters.Add("@displayId", SqlDbType.Int).Direction = ParameterDirection.Output;
				DataAccess.ExecuteNonQuery(cmd);
				DisplayId = DataAccess.IntOrZero(cmd.Parameters["@displayId"].Value);
			}
		}

        public static int GetHash(int displayId)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "select dbo.fn_GetDisplayHash(@displayId);";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@displayId", SqlDbType.Int).Value = displayId;
                using (DataSet ds = DataAccess.RunSql(cmd))
                {
                    return DataAccess.IntOrZero(ds.Tables[0].Rows[0][0]);
                }
            }
        }

        public static int GetIdleInterval(int displayId)
        {
            using (SqlCommand cmd = new SqlCommand("sp_GetIdleInterval"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@displayId", SqlDbType.Int);
                cmd.Parameters.Add("@idleInterval", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters["@displayId"].Value = displayId;
                DataAccess.ExecuteNonQuery(cmd);
                return DataAccess.IntOrZero(cmd.Parameters["@idleInterval"].Value);
            }
        }

        public string Name = "";
        public string Host = "";
        public int DisplayId = 0;
        public int CanvasId = 0;
        public int LocationId = 0;
        public bool ShowErrors = false;

        #region Private Members

        #endregion
    }
}