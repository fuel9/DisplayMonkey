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
        public string URL { get; private set; } // e.g. https://outlook.office365.com/EWS/Exchange.asmx

        public Outlook(int frameId)
            : base(frameId)
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
                this.FrameId
                );

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    this.Account = dr.StringOrBlank("Account").Trim();
                    this.Password = (byte[])dr["Password"];
                    this.Mode = dr.IntOrZero("Mode");
                    this.EwsVersion = (ExchangeVersion)dr.IntOrZero("EwsVersion");
                    this.ShowEvents = dr.IntOrZero("ShowEvents");
                    if (this.ShowEvents < 0)
                        this.ShowEvents = 0;
                    this.Mailbox = dr.StringOrBlank("Mailbox").Trim();
                    if (string.IsNullOrWhiteSpace(Mailbox))
                        this.Mailbox = Account;
                    this.Name = dr.StringOrBlank("Name").Trim();
                    this.URL = dr.StringOrBlank("Url").Trim();
                }
            }

            //_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/outlook/");
        }
    }
}