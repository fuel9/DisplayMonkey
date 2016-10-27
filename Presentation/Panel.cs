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
using System.Text;
using System.Globalization;
using System.Data.SqlClient;

namespace DisplayMonkey
{
	public class Panel
	{
		public Panel()
		{
		}

		public Panel(int panelId)
		{
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Panel WHERE PanelId=@panelId",
            })
            {
                cmd.Parameters.AddWithValue("@panelId", panelId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    _initFromRow(dr);
                    return false;
                });
            }
        }

		private void _initFromRow(SqlDataReader r)
		{
			PanelId = r.IntOrZero("PanelId");
			Top = r.IntOrZero("Top");
			Left = r.IntOrZero("Left");
			Width = r.IntOrZero("Width");
			Height = r.IntOrZero("Height");
            FadeLength = r.ValueOrDefault<double>("FadeLength", 0);
			Name = r.StringOrBlank("Name").Trim();
			if (Name == "")
				Name = string.Format("Panel {0}", PanelId);
		}

        public static bool IsFullScreen(int panelId)
        {
            bool isFullScreen = false;
            
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Panel WHERE PanelId=@panelId",
            })
            {
                cmd.Parameters.AddWithValue("@panelId", panelId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    isFullScreen = true;
                    return false;
                });
            }

            return isFullScreen;
        }

        public static List<Panel> List(int canvasId)
		{
			List<Panel> list = new List<Panel>();

            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = 
                    "SELECT p.*, f.PanelId FsPanelId, c.Height CanvasHeight, c.Width CanvasWidth FROM Panel p " +
                    "INNER JOIN Canvas c on c.CanvasId=p.CanvasId " +
                    "LEFT JOIN FullScreen f on f.PanelId=p.PanelId WHERE p.CanvasId=@canvasId ORDER BY p.PanelId",
            })
            {
                cmd.Parameters.AddWithValue("@canvasId", canvasId);
                cmd.ExecuteReaderExt((r) =>
                {
                    Panel panel = null;
                    int panelId = r.IntOrZero("PanelId");
                    int fullScreenPanelId = r.IntOrZero("FsPanelId");

                    if (panelId == fullScreenPanelId)
                        panel = new FullScreenPanel()
                        {
                            PanelId = panelId,
                            Top = 0,
                            Left = 0,
                            Height = r.IntOrZero("CanvasHeight"),
                            Width = r.IntOrZero("CanvasWidth"),
                            Name = r.StringOrBlank("Name"),
                            FadeLength = r.ValueOrDefault<double>("FadeLength", 0),
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
                            FadeLength = r.ValueOrDefault<double>("FadeLength", 0),
                        };

                    if (panel.Name == "")
                        panel.Name = string.Format("Panel {0}", panelId);

                    list.Add(panel);
                    return true;
                });
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

