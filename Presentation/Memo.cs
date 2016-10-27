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
using System.Runtime.Serialization;

namespace DisplayMonkey
{
    public class Memo : Frame
	{
        public string Subject { get; private set; }
        public string Body { get; private set; }

        public Memo(Frame frame)
            : base(frame)
        {
            _init();
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Memo WHERE FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    Subject = dr.StringOrBlank("Subject");
                    Body = dr.StringOrBlank("Body");
                    return false;
                });
            }
        }
	}
}