using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
	public class Outlook : Frame
	{
        public string Name { get; private set; }
        
        [ScriptIgnore]
        public int Mode { get; private set; }
        [ScriptIgnore]
        public string Account { get; private set; }
        [ScriptIgnore]
        public byte[] Password { get; private set; }
        [ScriptIgnore]
        public string Mailbox { get; private set; }
        [ScriptIgnore]
        public int ShowEvents { get; private set; }
        [ScriptIgnore]
        public ExchangeVersion EwsVersion { get; private set; }
        [ScriptIgnore]
        public string URL { get; private set; }

        public Outlook(int frameId, int panelId = 0)
            : base(frameId, panelId)
        {
            _init();
        }

        public Outlook(Frame frame)
            : base(frame)
        {
            _init();
        }

        private void _init()
        {
            string sql = string.Format(
                "SELECT TOP 1 o.*, Account, Password, Url, EwsVersion FROM Outlook o inner join ExchangeAccount x on x.AccountId=o.AccountId WHERE o.FrameId={0}", 
                FrameId
                );

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    Account = dr.StringOrBlank("Account").Trim();
                    Password = (byte[])dr["Password"];
                    Mode = dr.IntOrZero("Mode");
                    EwsVersion = (ExchangeVersion)dr.IntOrZero("EwsVersion");
                    ShowEvents = dr.IntOrZero("ShowEvents");
                    if (ShowEvents < 0)
                        ShowEvents = 0;
                    Mailbox = dr.StringOrBlank("Mailbox").Trim();
                    if (string.IsNullOrWhiteSpace(Mailbox))
                        Mailbox = Account;
                    Name = dr.StringOrBlank("Name").Trim();
                    URL = dr.StringOrBlank("Url").Trim();
                }
            }

            _templatePath = HttpContext.Current.Server.MapPath("~/files/frames/outlook/default.htm");
        }
    }
}