using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DisplayMonkey
{
	public class FullScreenPanel : Panel
	{
		public FullScreenPanel()
			: base()
		{
		}

		public FullScreenPanel(int panelId)
		{
			string sql = string.Format(
				"SELECT TOP 1 c.* FROM FullScreen s INNER JOIN Canvas c ON c.CanvasId=s.CanvasId WHERE PanelId={0};",
				PanelId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					DataRow r = ds.Tables[0].Rows[0];
					InitFromRow(r);
				}
			}
		}

		public void InitFromRow(DataRow r)
		{
			Top = Left = 0;
			Width = DataAccess.IntOrZero(r["Width"]);
			Height = DataAccess.IntOrZero(r["Height"]);
		}

		public static bool Exists(int panelId)
		{
			string sql = string.Format(
				"SELECT 1 FROM FullScreen WHERE PanelId={0};",
				panelId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					return true;
				}
			}

			return false;
		}

		public override string Style
		{
			get
			{
				StringBuilder style = new StringBuilder();

				style.AppendFormat(
					"#full {{width:{0}px;height:{1}px;}}\r\n",
					Width,
					Height
					);

				style.AppendFormat(
					"#full>img {{max-width:{0}px;max-height:{1}px;}}\r\n",
					Width,
					Height
					);

				return style.ToString();
			}
		}
		
		public override string Element
		{
			get
			{
				return string.Format(
					"<div id=\"screen\"><div id=\"full\" data-panel-id=\"{0}\"></div><div id=\"h_full\" style=\"display:none\"></div></div>\r\n",
					PanelId
					);
			}
		}

		public int IdleInterval
		{
			get
			{
				using (SqlCommand cmd = new SqlCommand("sp_GetIdleInterval", DataAccess.Connection))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("@panelId", SqlDbType.Int).Value = PanelId;
					cmd.Parameters.Add("@idleInterval", SqlDbType.Int).Direction = ParameterDirection.Output;
					cmd.Parameters["@panelId"].Value = PanelId;

					DataAccess.ExecuteNonQuery(cmd);

					return DataAccess.IntOrZero(cmd.Parameters["@idleInterval"].Value);
				}
			}
		}
	}
}