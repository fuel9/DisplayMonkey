using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using Microsoft.Exchange.WebServices.Data;

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

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
                    Account = DataAccess.StringOrBlank(dr["Account"]);
                    Password = (byte[])dr["Password"];
                    Mode = DataAccess.IntOrZero(dr["Mode"]);
                    Revision = (ExchangeVersion)DataAccess.IntOrZero(dr["Revision"]);
                    HourWindow = DataAccess.IntOrZero(dr["HourWindow"]);
                    MaxItems = 10;  // TODO
                    Mailbox = DataAccess.StringOrBlank(dr["Mailbox"]);
                    if (Mailbox == "")
                        Mailbox = Account;
                    Name = DataAccess.StringOrBlank(dr["Name"]);
                    if (Name == "")
                        Name = Mailbox;
				}
			}
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

                    //HttpServerUtility util = HttpContext.Current.Server;
                    html = string.Format(
                        template
                        );
                }

                //catch (ServiceResponseException ex)
                catch (Exception ex)
                {
                    html = ex.Message;
                }

                return html;
            }
        }

        public string Name = null;
        public int Mode = 0;
        public string Account = null;
        public byte[] Password = null;
        public string Mailbox = null;
        public int HourWindow = 24;
        public int MaxItems = 10;
        public ExchangeVersion Revision = ExchangeVersion.Exchange2007_SP1;
    }
}