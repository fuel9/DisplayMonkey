using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace DisplayMonkey
{
	public class Panel
	{
		public Panel(int panelId, bool initWithData = false)
		{
			PanelId = panelId;

			if (!initWithData)
				return;

			string sql = string.Format(
				"SELECT TOP 1 * FROM PANEL WHERE PanelId={0};",
				panelId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					DataRow r = ds.Tables[0].Rows[0];
					Top = DataAccess.IntOrZero(r["Top"]);
					Left = DataAccess.IntOrZero(r["Left"]);
					Width = DataAccess.IntOrZero(r["Width"]);
					Height = DataAccess.IntOrZero(r["Height"]);
				}
			}
		}

		public int PanelId = 0;
		public int Top = 0;
		public int Left = 0;
		public int Width = 0;
		public int Height = 0;

		public string BorderColor = "";

		public virtual string Style 
		{ 
			get 
			{ 
				return string.Format(
					"#div{0} {{position:absolute;overflow:hidden;margin:auto;top:{1}px;left:{2}px;width:{3}px;height:{4}px;{5}}}\r\n",
					new object[] {
						PanelId, 
						Top, 
						Left, 
						Width, 
						Height,
						BorderColor == "" ? "" : string.Format("border:1px solid {0};", BorderColor), 
					}); 
			} 
		}

		public virtual string Element
		{
			get
			{
				return string.Format(
					"<div id=\"div{0}\" data-panel-id=\"{0}\"></div><div id=\"h_div{0}\" style=\"display:none\"></div>\r\n", 
					PanelId
					);
			}
		}
	}
}