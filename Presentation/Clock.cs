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
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml;
using System.Web.Script.Serialization;


namespace DisplayMonkey
{
    public class Clock : Frame
	{
        public bool ShowDate { get; private set; }
        public bool ShowTime { get; private set; }
        public bool ShowSeconds { get; private set; }
        public int Type { get; private set; }
        public string Label { get; private set; }

        public DateTime? LocationTime
        {
            get
            {
                if (TimeZone != null) 
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.TimeZone);
                return null;
            }
        }

        [ScriptIgnore]
        public TimeZoneInfo TimeZone { get; private set; }

        public Clock(Frame frame)
            : base(frame)
        {
            _init();
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand() 
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Clock WHERE FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", this.FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    this.ShowDate = dr.Boolean("ShowDate");
                    this.ShowTime = dr.Boolean("ShowTime");
                    this.ShowSeconds = dr.Boolean("ShowSeconds");
                    this.Type = dr.IntOrZero("Type");
                    this.Label = dr.StringOrDefault("Label", null);

                    string szTimeZoneId = dr.StringOrDefault("TimeZone", null);
                    if (szTimeZoneId != null)
                        this.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(szTimeZoneId);

                    return false;
                });
            }
        }
    }
}
