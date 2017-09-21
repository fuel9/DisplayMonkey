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
using DisplayMonkey.Language;

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

        [ScriptIgnore]
        public string BookingSubject { get; private set; }  // 1.5.2

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
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 o.*, Account, Password, Url, EwsVersion FROM Outlook o inner join ExchangeAccount x on x.AccountId=o.AccountId WHERE o.FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
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
                    Privacy = (DisplayMonkey.Models.OutlookPrivacy)dr.IntOrZero("Privacy");
                    AllowReserve = dr.Boolean("AllowReserve");
                    ShowAsFlags = dr.IntOrZero("ShowAsFlags");
                    BookingSubject = dr.StringOrDefault("BookingSubject", Resources.Outlook_BookingOnDemand);
                    return false;
                });
            }
        }
    }
}