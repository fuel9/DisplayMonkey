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

namespace DisplayMonkey
{
	public class YouTube : Frame
	{
        public string YoutubeId  { get; private set; }
        public bool AutoLoop { get; private set; }
        public int Volume { get; private set; }
        public int Aspect { get; private set; }
        public int Quality { get; private set; }
        public int Start { get; private set; }
        public int Rate { get; private set; }

        public YouTube(Frame frame)
            : base(frame)
        {
            _init();
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Youtube WHERE FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", this.FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    YoutubeId = dr.StringOrBlank("YoutubeId").Trim();
                    AutoLoop = dr.Boolean("AutoLoop");
                    Volume = dr.IntOrZero("Volume");
                    Aspect = dr.IntOrZero("Aspect");
                    Quality = dr.IntOrZero("Quality");
                    Start = dr.IntOrZero("Start");
                    Rate = dr.IntOrZero("Rate");
                    return false;
                });
            }
        }
    }
}