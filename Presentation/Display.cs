/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DisplayMonkey.Models;

namespace DisplayMonkey
{
    public class DisplayData
    {
        public int DisplayId { get; private set; }
        public int IdleInterval { get; private set; }
        public int Hash { get; private set; }
        public Nullable<TimeSpan> RecycleTime { get; private set; }

        private DisplayData()
        {
        }

        public static async Task<DisplayData> RefreshAsync(int displayId)
        {
            DisplayData data = new DisplayData()
            {
                DisplayId = 0,
                IdleInterval = 0,
                Hash = 0,
                RecycleTime = null
            };
            
            using (SqlCommand cmd = new SqlCommand("sp_GetDisplayData"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@displayId", displayId);

                await cmd.ExecuteReaderExtAsync((reader) =>
                {
                    data.DisplayId = reader.IntOrZero("DisplayId");
                    data.IdleInterval = reader.IntOrZero("IdleInterval");
                    data.Hash = reader.IntOrZero("Hash");

                    if (reader["RecycleTime"] != DBNull.Value)
                    {
                        data.RecycleTime = (TimeSpan)reader["RecycleTime"];
                    }

                    return false;
                });
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
        public bool NoCursor { get; set; }
        public int ReadyTimeout { get; set; }
        public int PollInterval { get; set; }   // RC13
        public int ErrorLength { get; set; }    // RC13
        public Nullable<TimeSpan> RecycleTime { get; set; }    // RC15

        public Display()
		{
		}
		
		public Display(int displayId)
		{
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Display WHERE displayId=@displayId",
            })
            {
                cmd.Parameters.AddWithValue("@displayId", displayId);
                cmd.ExecuteReaderExt((r) =>
                {
                    _initFromRow(r);
                    return false;
                });
            }
        }

		public Display(string host)
		{
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Display WHERE Host=@host",
            })
            {
                cmd.Parameters.AddWithValue("@host", host);
                cmd.ExecuteReaderExt((r) =>
                {
                    _initFromRow(r);
                    return false;
                });
            }
        }

		private void _initFromRow(SqlDataReader r)
		{
			DisplayId = r.IntOrZero("DisplayId");
			CanvasId = r.IntOrZero("CanvasId");
			LocationId = r.IntOrZero("LocationId");
			Host = r.StringOrBlank("Host").Trim();
            NoScroll = r.Boolean("NoScroll");
            NoCursor = r.Boolean("NoCursor");
            ReadyTimeout = r.IntOrZero("ReadyTimeout");
            PollInterval = r.IntOrZero("PollInterval");
            ErrorLength = r.IntOrZero("ErrorLength");

            Name = r.StringOrBlank("Name").Trim();
			if (Name == "")
				Name = string.Format("Display {0}", DisplayId);
            
            if (r["RecycleTime"] != DBNull.Value)
            {
                RecycleTime = (TimeSpan)r["RecycleTime"];
            }
        }

        public static DisplayAutoLoadModes AutoLoadMode         // 1.6.0
        {
            get
            {
                DisplayAutoLoadModes mode = DisplayAutoLoadModes.DisplayAutoLoadMode_IP;

                using (SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    CommandText = "SELECT TOP 1 [Value] FROM Settings WHERE [Key]='AE1B2F10-9EC3-4429-97B5-C12D64575C41'"
                })
                {
                    cmd.ExecuteReaderExt(dr =>
                    {
                        byte[] v = dr.BytesOrNull("Value");
                        mode = (DisplayAutoLoadModes)(v == null ? 0 : BitConverter.ToInt32(v.Reverse().ToArray(), 0));
                        return false;
                    });
                }

                return mode;
            }
        }

        public string Url                                       // 1.6.0
        {
            get
            {
                return string.Format("getCanvas.aspx?display={0}", DisplayId);
            }
        }
	
		public static List<Display> List
		{
			get
			{
                List<Display> list = new List<Display>();

                using (SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    CommandText = "SELECT * FROM Display ORDER BY Name",
                })
                {
                    cmd.ExecuteReaderExt((r) =>
                    {
                        Display display = new Display();
                        display._initFromRow(r);
                        list.Add(display);
                        return true;
                    });
                }

                return list;
			}
		}

		public bool Register()
		{
			if (Name == "" || CanvasId == 0 || LocationId == 0)
				return false;

            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_RegisterDisplay",
            })
            {
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@name", Name);
				cmd.Parameters.AddWithValue("@host", Host ?? "");
				cmd.Parameters.AddWithValue("@canvasId", CanvasId);
				cmd.Parameters.AddWithValue("@locationId", LocationId);
				cmd.Parameters.Add("@displayId", SqlDbType.Int).Direction = ParameterDirection.Output;

                cmd.ExecuteNonQueryExt();

				//DataAccess.ExecuteNonQuery(cmd);
				DisplayId = cmd.Parameters["@displayId"].IntOrZero();
			}

			return true;
		}
    }
}
