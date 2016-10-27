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

namespace DisplayMonkey
{
    public class Html : Frame
	{
        [ScriptIgnore]
        public string Content { get; private set; }

        public Html(int frameId)
            : base(frameId)
        {
            _init();
        }
        
        public Html(Frame frame)
            : base(frame)
		{
            _init();
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Html WHERE FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", this.FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    Content = dr.StringOrBlank("Content");
                    return false;
                });
            }
        }
    }
}
