using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace DisplayMonkey
{
	public class Frame
	{
		public int FrameId = 0;
		public int PanelId = 0;
		public int Duration = 0;
		public string FrameType = null;
		public string URL = null;
        public string Html = "";

		public static string GetFrameType(int frameId)
		{
			string type = "";
			string sql = string.Format("SELECT top 1 FrameType from Frame_Type_View WHERE FrameId={0}", frameId);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					type = DataAccess.StringOrBlank(ds.Tables[0].Rows[0][0]);
				}
			}
			return type;
		}

		public static int GetPanelId(int frameId)
		{
			int panelId = 0;
			string sql = string.Format("SELECT top 1 PanelId from Frame WHERE FrameId={0}", frameId);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					panelId = DataAccess.IntOrZero(ds.Tables[0].Rows[0][0]);
				}
			}
			return panelId;
		}

        public static Frame GetNextFrame(int panelId, int displayId, int frameId)
		{
            Frame nci = new Frame()
            {
                PanelId = panelId
            };

            using (SqlCommand cmd = new SqlCommand("sp_GetNextFrame"))
            {
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@panelId", SqlDbType.Int).Value = panelId;
                cmd.Parameters.Add("@displayId", SqlDbType.Int).Value = displayId;
                cmd.Parameters.Add("@lastFrameId", SqlDbType.Int).Value = frameId;
				cmd.Parameters.Add("@nextFrameId", SqlDbType.Int).Direction = ParameterDirection.Output;
				cmd.Parameters.Add("@duration", SqlDbType.Int).Direction = ParameterDirection.Output;
				cmd.Parameters.Add("@frameType", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;

                DataAccess.ExecuteNonQuery(cmd);

                if (cmd.Parameters["@nextFrameId"].Value != DBNull.Value)
                    nci.FrameId = (int)cmd.Parameters["@nextFrameId"].Value;
                if (cmd.Parameters["@duration"].Value != DBNull.Value)
                    nci.Duration = (int)cmd.Parameters["@duration"].Value;
                if (cmd.Parameters["@frameType"].Value != DBNull.Value)
                    nci.FrameType = (string)cmd.Parameters["@frameType"].Value;
            }

            return nci;
		}

		public virtual string Payload { get { return ""; } }

		#region Protected Members

		protected string _templatePath;

		#endregion

		#region Private Members

		#endregion
	}
}