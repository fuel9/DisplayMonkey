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

			using (SqlCommand cmd = new SqlCommand())
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "sp_RegisterDisplay";
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
            if (_fn_GetDisplayHash == null)
            {
                _fn_GetDisplayHash = new SqlCommand("select dbo.fn_GetDisplayHash(@displayId);");
                _fn_GetDisplayHash.CommandType = CommandType.Text;
                _fn_GetDisplayHash.Parameters.Add("@displayId", SqlDbType.Int);
            }
            _fn_GetDisplayHash.Connection = DataAccess.Connection;
            _fn_GetDisplayHash.Parameters["@displayId"].Value = displayId;

            using (DataSet ds = DataAccess.RunSql(_fn_GetDisplayHash))
            {
                return DataAccess.IntOrZero(ds.Tables[0].Rows[0][0]);
            }
        }

        public static int GetIdleInterval(int displayId)
        {
            if (_sp_GetIdleInterval == null)
            {
                _sp_GetIdleInterval = new SqlCommand("sp_GetIdleInterval");
                _sp_GetIdleInterval.CommandType = CommandType.StoredProcedure;
                _sp_GetIdleInterval.Parameters.Add("@displayId", SqlDbType.Int);
                _sp_GetIdleInterval.Parameters.Add("@idleInterval", SqlDbType.Int).Direction = ParameterDirection.Output;
            }
            _sp_GetIdleInterval.Connection = DataAccess.Connection;
            _sp_GetIdleInterval.Parameters["@displayId"].Value = displayId;

            DataAccess.ExecuteNonQuery(_sp_GetIdleInterval);
            return DataAccess.IntOrZero(_sp_GetIdleInterval.Parameters["@idleInterval"].Value);
        }

        public string Name = "";
        public string Host = "";
        public int DisplayId = 0;
        public int CanvasId = 0;
        public int LocationId = 0;
        public bool ShowErrors = false;

        #region Private Members

        private static SqlCommand _sp_GetIdleInterval = null;
        private static SqlCommand _fn_GetDisplayHash = null;

        #endregion
    }
}