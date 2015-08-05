using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace DisplayMonkey
{
    public class DisplayData
    {
        public int DisplayId { get; private set; }
        public int IdleInterval { get; private set; }
        public int Hash { get; private set; }

        private DisplayData()
        {
        }

        public static DisplayData Refresh(int displayId)
        {
            DisplayData data = new DisplayData()
            {
                DisplayId = 0,
                IdleInterval = 0,
                Hash = 0
            };
            
            using (SqlCommand cmd = new SqlCommand("sp_GetDisplayData"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@displayId", displayId);
                using (DataSet ds = DataAccess.RunSql(cmd))
                {
                    if (ds.Tables.Count > 0)
                    {
                        DataRow r = ds.Tables[0].Rows[0];
                        data.DisplayId = r.IntOrZero("DisplayId");
                        data.IdleInterval = r.IntOrZero("IdleInterval");
                        data.Hash = r.IntOrZero("Hash");
                    }
                }
            }
            
            return data;
        }
    }
    
    public class Display
	{
        public int DisplayId { get; private set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int CanvasId { get; set; }
        public int LocationId { get; set; }
        public bool NoScroll { get; set; }
        public int ReadyTimeout { get; set; }
        public int PollInterval { get; set; }   // RC13
        public int ErrorLength { get; set; }    // RC13

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
                    _initFromRow(r);
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
                    _initFromRow(r);
				}
			}
		}

		private void _initFromRow(DataRow r)
		{
			DisplayId = r.IntOrZero("DisplayId");
			CanvasId = r.IntOrZero("CanvasId");
			LocationId = r.IntOrZero("LocationId");
			Host = r.StringOrBlank("Host").Trim();
            NoScroll = r.Boolean("NoScroll");
            ReadyTimeout = r.IntOrZero("ReadyTimeout");
            PollInterval = r.IntOrZero("PollInterval");
            ErrorLength = r.IntOrZero("ErrorLength");
            Name = r.StringOrBlank("Name").Trim();
			if (Name == "")
				Name = string.Format("Display {0}", DisplayId);
		}
	
		public static List<Display> List
		{
			get
			{
                List<Display> list = new List<Display>();
                string sql = "SELECT * FROM Display ORDER BY Name";
                using (DataSet ds = DataAccess.RunSql(sql))
                {
                    list.Capacity = ds.Tables[0].Rows.Count;

                    // list registered displays
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        Display display = new Display(r.IntOrZero("DisplayId"));
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
				DisplayId = cmd.Parameters["@displayId"].IntOrZero();
			}
		}
    }
}