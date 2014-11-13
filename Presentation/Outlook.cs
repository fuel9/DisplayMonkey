using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DisplayMonkey
{
	public class Outlook : Frame
	{
        public Outlook(int frameId, int panelId)
			: base()
		{
			PanelId = panelId;
			_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/outlook.htm");
			string sql = string.Format("SELECT TOP 1 * FROM Outlook WHERE FrameId={0}", frameId);

			/*using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
					Path = DataAccess.StringOrBlank(dr["Path"]);
					Mode = (PictureMode)DataAccess.IntOrZero(dr["Mode"]);
					Name = DataAccess.StringOrBlank(dr["Name"]);
				}
			}*/
		}

        public override string Payload
        {
            get
            {
                string html = "";
                try
                {
                    // load template
                    string template = File.ReadAllText(_templatePath);

                    // fill template
                    if (FrameId > 0)
                    {
                        HttpServerUtility util = HttpContext.Current.Server;
                        html = string.Format(
                            template,
                            util.HtmlEncode(""),
                            util.HtmlEncode("")
                            );
                    }
                }

                catch (Exception ex)
                {
                    html = ex.Message;
                }

                // return html
                return html;
            }
        }

		//public string Path = "";
	}
}