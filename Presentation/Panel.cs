using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using System.Globalization;

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
                    _initFromRow(r);
				}
			}
		}

		private void _initFromRow(DataRow r)
		{
			PanelId = r.IntOrZero("PanelId");
			Top = r.IntOrZero("Top");
			Left = r.IntOrZero("Left");
			Width = r.IntOrZero("Width");
			Height = r.IntOrZero("Height");
            FadeLength = r.DoubleOrZero("FadeLength");
			Name = r.StringOrBlank("Name").Trim();
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
				int fullScreenPanelId = fs.IntOrZero("PanelId");

				// list canvas panels
				foreach (DataRow r in ds.Tables[0].Rows)
				{
					Panel panel = null;
					int panelId = r.IntOrZero("PanelId");

					if (panelId == fullScreenPanelId)
						panel = new FullScreenPanel()
						{
							PanelId = panelId,
							Top = 0,
							Left = 0,
							Height = fs.IntOrZero("Height"),
							Width = fs.IntOrZero("Width"),
							Name = r.StringOrBlank("Name"),
                            FadeLength = r.DoubleOrZero("FadeLength"),
						};
					else
						panel = new Panel()
						{
							PanelId = panelId,
							Top = r.IntOrZero("Top"),
							Left = r.IntOrZero("Left"),
							Height = r.IntOrZero("Height"),
							Width = r.IntOrZero("Width"),
							Name = r.StringOrBlank("Name"),
                            FadeLength = r.DoubleOrZero("FadeLength"),
                        };

					if (panel.Name == "")
						panel.Name = string.Format("Panel {0}", panelId);

					list.Add(panel);
				}
			}
			return list;
		}

        public int PanelId { get; private set; }
        public int Top { get; protected set; }
        public int Left { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public string Name { get; protected set; }
        public double FadeLength { get; protected set; }   // RC13

		public virtual string Style 
		{ 
			get 
			{ 
				return string.Format(
					"#panel{0}, #x_panel{0} {{position:absolute;overflow:hidden;margin:auto;top:{1}px;left:{2}px;width:{3}px;height:{4}px;}}\n",
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
                return new StringBuilder()
                    .AppendFormat(CultureInfo.InvariantCulture,
                        "<div class=\"panel\" id=\"panel{0}\" data-panel-id=\"{0}\" data-panel-width=\"{1}\" data-panel-height=\"{2}\" data-fade-length=\"{3}\"></div>\n", 
                        PanelId,
                        Width,
                        Height,
                        FadeLength
                    )
                    .ToString()
                    ;
			}
		}
	}
}

