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
using System.Text;
using System.Globalization;

namespace DisplayMonkey
{
	public class FullScreenPanel : Panel
	{
		public FullScreenPanel()
			: base()
		{
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
                return new StringBuilder()
                    .AppendFormat(CultureInfo.InvariantCulture,
                        "<div id=\"screen\"><div class=\"fullpanel\" id=\"full\" data-panel-id=\"{0}\" data-panel-width=\"{1}\" data-panel-height=\"{2}\" data-fade-length=\"{3}\"></div></div>\n",
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