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
            : base(panelId)
		{
            Top = Left = 0;
            
            string sql = string.Format(
				"SELECT TOP 1 c.* FROM FullScreen s INNER JOIN Canvas c ON c.CanvasId=s.CanvasId WHERE PanelId={0};",
			    PanelId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					DataRow r = ds.Tables[0].Rows[0];
                    InitFromCanvasRow(r);
				}
			}
		}

		public void InitFromCanvasRow(DataRow r)
		{
			Width = DataAccess.IntOrZero(r["Width"]);
			Height = DataAccess.IntOrZero(r["Height"]);
		}

		public override string Style
		{
			get
			{
				StringBuilder style = new StringBuilder();

				style.AppendFormat(
                    "#full, #x_full {{overflow:hidden;width:{0}px;height:{1}px;}}\n",
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
                    "<div id=\"screen\"><div id=\"full\" data-panel-id=\"{0}\"></div></div>\n",
                    PanelId
					);
			}
		}
	}
}