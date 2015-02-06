using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace DisplayMonkey
{
	public class Panel
	{
		public Panel()
		{
		}

		public Panel(int panelId)
		{
			PanelId = panelId;

			string sql = string.Format(
				"SELECT TOP 1 * FROM Panel WHERE PanelId={0};",
				panelId
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
			PanelId = DataAccess.IntOrZero(r["PanelId"]);
			Top = DataAccess.IntOrZero(r["Top"]);
			Left = DataAccess.IntOrZero(r["Left"]);
			Width = DataAccess.IntOrZero(r["Width"]);
			Height = DataAccess.IntOrZero(r["Height"]);
			Name = DataAccess.StringOrBlank(r["Name"]);
			if (Name == "")
				Name = string.Format("Panel {0}", PanelId);
		}

        public static bool IsFullScreen(int panelId)
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

        public static List<Panel> List(int canvasId)
		{
			List<Panel> list = new List<Panel>();
			string sql = string.Format(
				"SELECT * FROM Panel WHERE CanvasId={0} ORDER BY 1;" +
				"SELECT TOP 1 c.*, PanelId FROM FullScreen s INNER JOIN Canvas c ON c.CanvasId=s.CanvasId WHERE s.CanvasId={0};",
				canvasId
				);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				list.Capacity = ds.Tables[0].Rows.Count;

				DataRow fs = ds.Tables[1].Rows[0];
				int fullScreenPanelId = DataAccess.IntOrZero(fs["PanelId"]);

				// list canvas panels
				foreach (DataRow r in ds.Tables[0].Rows)
				{
					Panel panel = null;
					int panelId = DataAccess.IntOrZero(r["PanelId"]);

					if (panelId == fullScreenPanelId)
						panel = new FullScreenPanel()
						{
							PanelId = panelId,
							Top = 0,
							Left = 0,
							Height = DataAccess.IntOrZero(fs["Height"]),
							Width = DataAccess.IntOrZero(fs["Width"]),
							Name = DataAccess.StringOrBlank(r["Name"]),
						};
					else
						panel = new Panel()
						{
							PanelId = panelId,
							Top = DataAccess.IntOrZero(r["Top"]),
							Left = DataAccess.IntOrZero(r["Left"]),
							Height = DataAccess.IntOrZero(r["Height"]),
							Width = DataAccess.IntOrZero(r["Width"]),
							Name = DataAccess.StringOrBlank(r["Name"]),
						};

					if (panel.Name == "")
						panel.Name = string.Format("Panel {0}", panelId);

					list.Add(panel);
				}
			}
			return list;
		}

		public int PanelId = 0;
		public int Top = 0;
		public int Left = 0;
		public int Width = 0;
		public int Height = 0;
		public string Name = "";

		public virtual string Style 
		{ 
			get 
			{ 
				return string.Format(
					"#div{0}, #x_div{0} {{position:absolute;overflow:hidden;margin:auto;top:{1}px;left:{2}px;width:{3}px;height:{4}px;}}\n",
					PanelId, 
					Top, 
					Left, 
					Width, 
					Height
    				);
			} 
		}

		public virtual string Element
		{
			get
			{
				return string.Format(
                    "<div id=\"div{0}\" data-panel-id=\"{0}\"></div>\n",
                    PanelId
					);
			}
		}
	}
}

