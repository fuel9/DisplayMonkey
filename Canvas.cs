using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;


namespace DisplayMonkey
{
	public class Canvas
	{
		public Canvas(int displayId)
		{
			DisplayId = displayId;

			string sql = string.Format(
				"declare @c int; SELECT TOP 1 @c = CanvasId FROM DISPLAY WHERE DisplayId={0};" +
				"SELECT c.*, PanelId FROM CANVAS c INNER JOIN FULL_SCREEN s on s.CanvasId=c.CanvasId WHERE c.CanvasId=@c;" +
				"SELECT * FROM PANEL WHERE CanvasId=@c ORDER BY 1;",
				displayId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (0 == ds.Tables.Count)
					throw new Exception("Canvas not found");

				DataRow r = ds.Tables[0].Rows[0];
				CanvasId = DataAccess.IntOrZero(r["CanvasId"]);
				Title = DataAccess.StringOrBlank(r["Name"]);
				Height = DataAccess.IntOrZero(r["Height"]);
				Width = DataAccess.IntOrZero(r["Width"]);
				BackgroundColor = DataAccess.StringOrBlank(r["BackgroundColor"]);
				BackgroundImage = DataAccess.IntOrZero(r["BackgroundImage"]);
				
				int fullScreenPanelId = DataAccess.IntOrZero(r["PanelId"]);

				foreach (DataRow pr in ds.Tables[1].Rows)
				{
					Panel panel = null;
					int panelId = DataAccess.IntOrZero(pr["PanelId"]);

					if (panelId == fullScreenPanelId)
					{
						panel = new FullScreenPanel(panelId)
						{
							Width = this.Width,
							Height = this.Height,
						};

						InitialMaxIdleInterval = (panel as FullScreenPanel).IdleInterval;
					}
					else
					{
						panel = new Panel(panelId)
						{
							Top = DataAccess.IntOrZero(pr["Top"]),
							Left = DataAccess.IntOrZero(pr["Left"]),
							Width = DataAccess.IntOrZero(pr["Width"]),
							Height = DataAccess.IntOrZero(pr["Height"]),
						};
					}

					Panels.Add(panel);
				}
			}
		}

		public List<Panel> Panels = new List<Panel>();

		public int BackgroundImage = 0;
		public string BackgroundColor = "";
		public string Title = "Welcome to display monkey!";
		public bool IsAppleMobileSupported = true;
		public int CanvasId = 0;
		public int Height = 0;
		public int Width = 0;
		public int DisplayId = 0;
		public int InitialMaxIdleInterval = 0;

		public virtual string Head 
		{
			get 
			{
				StringBuilder head = new StringBuilder();

				Location location = new Location(DisplayId);
				
				// add meta
				if (IsAppleMobileSupported)
				{
					head.Append("<meta name=\"apple-mobile-web-app-capable\" content=\"yes\" />\r\n");
					head.Append("<meta name=\"apple-mobile-web-app-status-bar-style\" content=\"black\" />\r\n");
				}

				// scripts
				foreach (string js in _js_libs)
				{
					head.AppendFormat("<script src=\"{0}\" type=\"text/javascript\" charset=\"utf-8\"></script>\r\n", js);
				}
				head.Append("<script type=\"text/javascript\" charset=\"utf-8\"><!--\r\n_canvas=new Canvas({\r\n");
				head.AppendFormat("displayId:{0},\r\n", DisplayId);
				head.AppendFormat("temperatureUnit:'{0}',\r\n", location.TemperatureUnit);
				head.AppendFormat("dateFormat:'{0}',\r\n", location.DateFormat);
				head.AppendFormat("timeFormat:'{0}',\r\n", location.TimeFormat);
				head.AppendFormat("latitude:{0},\r\n", location.Latitude);
				head.AppendFormat("longitude:{0},\r\n", location.Longitude);
				head.AppendFormat("woeid:{0},\r\n", location.Woeid);
				head.AppendFormat("utcTime:'{0}',\r\n", DateTime.UtcNow.ToString());
				head.AppendFormat("gmtOffset:{0},\r\n", location.OffsetGMT);
				head.AppendFormat("initialIdleInterval:{0},\r\n", this.InitialMaxIdleInterval);
				head.AppendFormat("width:{0},\r\n", this.Width);
				head.AppendFormat("height:{0},\r\n", this.Height);
				if (this.BackgroundImage > 0) head.AppendFormat("backImage:{0},\r\n", this.BackgroundImage);
				if (this.BackgroundColor != "") head.AppendFormat("backColor:'{0}',\r\n", this.BackgroundColor);
				head.Append("});\r\n--></script>\r\n");
				
				// add styles
				head.Append("<link rel=\"stylesheet\" href=\"styles/style.css\" type=\"text/css\" />\r\n<style>\r\n");
				/*head.AppendFormat(
					"body {{background:gray;{0}}}\r\n",
					BackgroundImage == "" ? "" : string.Format("background-image:url({0});background-repeat:no-repeat;", BackgroundImage)
					);*/
				foreach (Panel p in Panels)
				{
					head.Append(p.Style);
				}
				head.Append("</style>\r\n");

				return head.ToString();
			} 
		}

		public string Body
		{
			get
			{
				StringBuilder body = new StringBuilder();

				body.AppendFormat("<div id=\"segments\" style=\"width:{0}px;height:{1}px;\">\r\n", Width, Height);
				foreach (Panel panel in Panels)
				{
					if (panel.GetType() == typeof(FullScreenPanel))
						body.Insert(0, panel.Element);
					else
						body.Append(panel.Element);
				}
				body.Append("</div>");

				return body.ToString();
			}
		}

		#region Private Members

		private static string[] _js_libs = new string[] {
			"js/pt/prototype.js", 
			"js/pt/prototype_ccs.js", 
			"js/pt/ajaxpanel.js", 
			"js/pt/scriptaculous.js",
			"js/moment.min.js",
			"js/panel.js"
		};

		#endregion

	}
}