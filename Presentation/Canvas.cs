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
using System.Text;
using System.Data;
using System.Globalization;
using System.Data.SqlClient;

namespace DisplayMonkey
{
	public class Canvas
	{
		public Canvas()
		{
		}
		
		public Canvas(int canvasId)
		{
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Canvas WHERE CanvasId=@canvasId",
            })
            {
                cmd.Parameters.AddWithValue("@canvasId", canvasId);
                cmd.ExecuteReaderExt((r) =>
                {
                    _initFromRow(r);
                    return false;
                });
            }

            if (this.DisplayId != 0)
                this.Display = new Display(this.DisplayId);
		}

		public static Canvas InitFromDisplay(int displayId)
		{
            Canvas canvas = null;

            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT c.* FROM Display d INNER JOIN Canvas c on c.CanvasId=d.CanvasId WHERE DisplayId=@displayId",
            })
            {
                cmd.Parameters.AddWithValue("@displayId", displayId);
                cmd.ExecuteReaderExt((r) =>
                {
                    canvas = new Canvas()
                    {
                        DisplayId = displayId,
                    };
                    canvas._initFromRow(r);
                    return false;
                });
            }

            if (canvas == null)
                throw new Exception("Canvas not found");


            canvas.Display = new Display(displayId);
            canvas.Location = new Location(displayId);
			canvas.Panels = Panel.List(canvas.CanvasId);

			return canvas;
		}
		
		private void _initFromRow(SqlDataReader r)
		{
			CanvasId = r.IntOrZero("CanvasId");
			Height = r.IntOrZero("Height");
			Width = r.IntOrZero("Width");
			BackgroundColor = r.StringOrBlank("BackgroundColor");
			if (BackgroundColor == "")
				BackgroundColor = "transparent";
			BackgroundImage = r.IntOrZero("BackgroundImage");
			Name = r.StringOrBlank("Name");
			if (Name == "")
				Name = string.Format("Canvas {0}", CanvasId);
		}

		public static List<Canvas> List
		{
			get
			{
				List<Canvas> list = new List<Canvas>();

                using (SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    CommandText = "SELECT * FROM Canvas ORDER BY Name",
                })
                {
                    cmd.ExecuteReaderExt((r) =>
                    {
                        Canvas canvas = new Canvas(r.IntOrZero("CanvasId"));
                        list.Add(canvas);
                        return true;
                    });
                }

				return list;
			}
		}
		
		public List<Panel> Panels = new List<Panel>();

		public int BackgroundImage = 0;
		public string BackgroundColor = "";
		public bool IsAppleMobileSupported = true;  // TODO
		public int CanvasId = 0;
		public int Height = 0;
		public int Width = 0;
		public int DisplayId = 0;
        public string Name = "";
        //public int FullScreenPanelId = 0;

        public Display Display { get; private set; }
        public Location Location { get; private set; }

		public virtual string Head 
		{
			get 
			{
				StringBuilder head = new StringBuilder();

				// add meta
                head.AppendFormat(CultureInfo.InvariantCulture, "<meta name=\"server-latitude\" content=\"{0}\" />\n", ServerGeoData.Latitude);
                head.AppendFormat(CultureInfo.InvariantCulture, "<meta name=\"server-longitude\" content=\"{0}\" />\n", ServerGeoData.Longitude);
                head.AppendFormat(CultureInfo.InvariantCulture, "<meta name=\"server-time-zone\" content=\"{0}\" />\n", ServerGeoData.TimeZone.Id);
                head.AppendFormat(CultureInfo.InvariantCulture, "<meta name=\"server-offset-gmt\" content=\"{0}\" />\n", ServerGeoData.OffsetGMT);
                head.AppendFormat(CultureInfo.InvariantCulture, "<meta name=\"server-external-ip\" content=\"{0}\" />\n", ServerGeoData.ServerExternalIPAddress.ToString());
                if (IsAppleMobileSupported)
				{
					head.Append("<meta name=\"apple-mobile-web-app-capable\" content=\"yes\" />\n");
                    head.Append("<meta name=\"apple-mobile-web-app-status-bar-style\" content=\"black\" />\n");
                }

                // add styles
                head.Append("<link rel=\"stylesheet\" href=\"styles/style.css\" type=\"text/css\" />\n");
                head.Append("<link rel=\"stylesheet\" href=\"styles/custom.css\" type=\"text/css\" />\n");
                head.Append("<style type=\"text/css\">\n");
                foreach (Panel p in Panels)
                {
                    head.Append(p.Style);
                }
                head.Append("</style>\n");

				// scripts
				foreach (string js in _jsLibs)
				{
					head.AppendFormat("<script src=\"{0}\" type=\"text/javascript\" charset=\"utf-8\"></script>\n", js);
				}
				head.Append("<script type=\"text/javascript\" charset=\"utf-8\"><!--\r\n_canvas=new DM.Canvas({\n");
                head.AppendFormat(CultureInfo.InvariantCulture, "displayId:{0},\n", DisplayId);
                head.AppendFormat(CultureInfo.InvariantCulture, "temperatureUnit:'{0}',\n", this.Location.TemperatureUnit);
				head.AppendFormat(CultureInfo.InvariantCulture, "dateFormat:'{0}',\n", this.Location.DateFormat);
				head.AppendFormat(CultureInfo.InvariantCulture, "timeFormat:'{0}',\n", this.Location.TimeFormat);
				head.AppendFormat(CultureInfo.InvariantCulture, "latitude:{0},\n", this.Location.Latitude);
				head.AppendFormat(CultureInfo.InvariantCulture, "longitude:{0},\n", this.Location.Longitude);
                head.AppendFormat(CultureInfo.InvariantCulture, "woeid:{0},\n", this.Location.Woeid);
                head.AppendFormat(CultureInfo.InvariantCulture, "culture:'{0}',\n", this.Location.Culture);
                head.AppendFormat(CultureInfo.InvariantCulture, "locationTime:'{0}',\n", this.Location.LocationTime);
                head.AppendFormat(CultureInfo.InvariantCulture, "utcTime:'{0}',\n", DateTime.UtcNow);
				head.AppendFormat(CultureInfo.InvariantCulture, "width:{0},\n", this.Width);
                head.AppendFormat(CultureInfo.InvariantCulture, "height:{0},\n", this.Height);
                if (this.BackgroundImage > 0) 
					head.AppendFormat(CultureInfo.InvariantCulture, "backImage:{0},\n", this.BackgroundImage);
				if (this.BackgroundColor != "") 
					head.AppendFormat(CultureInfo.InvariantCulture, "backColor:'{0}',\n", this.BackgroundColor);
                head.AppendFormat(CultureInfo.InvariantCulture, "pollInterval:{0},\n", this.Display.PollInterval);
                head.AppendFormat(CultureInfo.InvariantCulture, "errorLength:{0},\n", this.Display.ErrorLength);
                head.AppendFormat(CultureInfo.InvariantCulture, "noScroll:{0},\n", this.Display.NoScroll ? "true" : "false");
                head.AppendFormat(CultureInfo.InvariantCulture, "noCursor:{0},\n", this.Display.NoCursor ? "true" : "false");
                head.AppendFormat(CultureInfo.InvariantCulture, "readyTimeout:{0},\n", this.Display.ReadyTimeout);
                head.Append("});\n--></script>\n<style></style>\n");
				
				return head.ToString();
			}
		}

		public string Body
		{
			get
			{
				StringBuilder body = new StringBuilder();
                body.AppendFormat(CultureInfo.InvariantCulture,
                    "<div id=\"segments\" style=\"width:{0}px;height:{1}px;\">\n",
                    Width,
                    Height
                    );
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

		private static readonly string[] _jsLibs = new string[] 
        {
#if (DEBUG)
            // prototype & script-aculo-us
            "scripts/prototype.js",
            "scripts/scriptaculous/scriptaculous.js",

            // jquery
            //"scripts/jquery-2.1.4.js",

            // moment.js
            "scripts/moment-with-locales.js",

            // canvas:
			"files/js/canvas.js"
			
            // frame scripts
			, "files/js/clock.js"
            , "files/js/iframe.js"
			, "files/js/memo.js"
            , "files/js/outlook.js"
            , "files/js/picture.js"
            , "files/js/video.js"
            , "files/js/weather.js"
            , "files/js/youtube.js"
            , "files/js/powerbi.js"

            // add new frame js code here
#else
            // prototype & script-aculo-us
            "scripts/prototype.js",
            "scripts/scriptaculous/scriptaculous.js",

            // jquery
            //"scripts/jquery-2.1.4.min.js",

            // moment.js
            "scripts/moment-with-locales.min.js",

            // canvas:
			"files/js/canvas.js"
			
            // frame scripts
			, "files/js/clock.js"
            , "files/js/iframe.js"
			, "files/js/memo.js"
            , "files/js/outlook.js"
            , "files/js/picture.js"
            , "files/js/video.js"
            , "files/js/weather.js"
            , "files/js/youtube.js"
            , "files/js/powerbi.js"

            // add new frame js code here
#endif
		};

		#endregion

	}
}