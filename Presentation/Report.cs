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
using System.Web.Script.Serialization;
using DisplayMonkey.Models;

namespace DisplayMonkey
{
	public class Report : Frame
	{
        public string Name { get; protected set; }
        public RenderModes Mode { get; protected set; }
        
        public override string Hash
        {
            get
            {
                string hash = null;
                if (this.CacheInterval > 0)
                {
                    uint crc32 = HttpRuntime.Cache.GetItemCrc32(this.CacheKey);
                    hash = (crc32 != 0) ? crc32.ToString() :
                        HttpRuntime.Cache.GetItemGuid(this.CacheKey).ToString();
                }
                else
                    hash = Guid.NewGuid().ToString();   // get fresh version of report every time

                //System.Diagnostics.Debug.Print(string.Format("???: key={0} hash={1}", this.CacheKey, hash));
                return hash;
            }
        }

        [ScriptIgnore]
        public string CacheKey
        {
            get
            {
                return string.Format("report_{0}_{1}", this.FrameId, base.Version);
            }
        }
        

        [ScriptIgnore]
        public string Path { get; private set; }
        [ScriptIgnore]
        public string BaseUrl { get; private set; }
        [ScriptIgnore]
        public string User { get; private set; }
        [ScriptIgnore]
        public string Domain { get; private set; }
        [ScriptIgnore]
        public byte[] Password { get; private set; }
        [ScriptIgnore]
        public string Url
        {
            get
            {
                string baseUrl = this.BaseUrl, url = this.Path, parameters = string.Empty;

                if (url.Contains("&")) // if url contains Parameters
                {
                    int index = url.IndexOf("&"); 
                    if (index >= 0)
                    {
                        parameters = url.Substring(index); // select the parameters
                        url = url.Substring(0, index);      // strip parametrs away from the original url
                    }
                }

                if (baseUrl.EndsWith("/"))
                    baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

                if (!url.StartsWith("/"))
                    url = "/" + url;

                url = string.Format(
                    "{0}?{1}{2}&rs:format=IMAGE",
                    baseUrl,
                    HttpUtility.UrlEncode(url),
                    parameters
                    );
                return url;
            }
        }

        public Report(Frame frame)
            : base(frame)
        {
            _init();
        }

        public Report(int frameId)
            : base(frameId)
        {
            _init();
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = 
                    "SELECT TOP 1 r.*, s.BaseUrl, s.[User], s.Domain, s.Password FROM Report r " +
                    "INNER JOIN ReportServer s on s.ServerId=r.ServerId WHERE FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    Path = dr.StringOrBlank("Path").Trim();
                    Mode = (RenderModes)dr.IntOrZero("Mode");
                    Name = dr.StringOrBlank("Name");

                    BaseUrl = dr.StringOrBlank("BaseUrl").Trim();
                    User = dr.StringOrBlank("User").Trim();
                    Domain = dr.StringOrBlank("Domain").Trim();
                    Password = (byte[])dr["Password"];
                    return false;
                });
            }
        }
	}
}