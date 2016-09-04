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
using System.IO;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
	public class Outlook : Frame
	{
        public string Name { get; private set; }

        public bool AllowReserve { get; private set; }
        public int ShowEvents { get; private set; }
        
        [ScriptIgnore]
        public int Mode { get; private set; }   // Reserved
        [ScriptIgnore]
        public string Account { get; private set; }
        [ScriptIgnore]
        public byte[] Password { get; private set; }
        [ScriptIgnore]
        public string Mailbox { get; private set; }
        [ScriptIgnore]
        public ExchangeVersion EwsVersion { get; private set; }
        [ScriptIgnore]
        public string URL { get; private set; } // e.g. https://outlook.office365.com/EWS/Exchange.asmx

        private int ShowAsFlags { get; set; }

        public bool IsShowAsAllowed(LegacyFreeBusyStatus flag)
        {
            return (ShowAsFlags & (1 << (int)flag)) != 0;
        }

        [ScriptIgnore]
        public DisplayMonkey.Models.OutlookPrivacy Privacy { get; private set; }   // TODO: 0 = show everything, 1 = normal only, 2 = sensitivity as subject

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
                        this.Mailbox = this.Account;
                    this.Name = dr.StringOrBlank("Name").Trim();
                    this.URL = dr.StringOrBlank("Url").Trim();
                    this.Privacy = (DisplayMonkey.Models.OutlookPrivacy)dr.IntOrZero("Privacy");
                    this.AllowReserve = dr.Boolean("AllowReserve");
                    this.ShowAsFlags = dr.IntOrZero("ShowAsFlags");
                }
            }
        }
    }
}