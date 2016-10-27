/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using DisplayMonkey.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DisplayMonkey
{
    public class Content
    {
        public int ContentId { get; private set; }
        public byte[] Data { get; private set; }
        public string Name { get; private set; }
        public ContentTypes Type { get; private set; }

        /*public Content(int contentId)
        {
			string sql = string.Format(
				"SELECT TOP 1 * FROM Content WHERE ContentId={0}; ",
                contentId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
                    ContentId = dr.IntOrZero("ContentId");
					Name = dr.StringOrBlank("Name");
                    Type = (ContentTypes)dr.IntOrZero("Type");
                    if (dr["Data"] != DBNull.Value)
                        Data = (byte[])dr["Data"];
                }
			}
        }*/

        private Content()
        {
        }

        public static async Task<Content> GetDataAsync(int contentId)
        {
            Content content = null;

            using (SqlCommand cmd = new SqlCommand(_sql))
            {
                cmd.Parameters.AddWithValue("@contentId", contentId);
                await cmd.ExecuteReaderAsync((reader) =>
                {
                    content = new Content()
                    {
                        ContentId = reader.IntOrZero("ContentId"),
                        Name = reader.StringOrBlank("Name"),
                        Type = reader.ValueOrDefault<ContentTypes>("Type", ContentTypes.ContentType_Picture),
                        Data = reader.ValueOrNull<byte[]>("Data"),
                    };
                    return false;
                });
            }

            return content;
        }

        public static async Task<Content> GetMissingContentAsync()
        {
            //data = File.ReadAllBytes("~/files/404.png");
            using (FileStream fs = File.Open("~/files/404.png", FileMode.Open))
            {
                Content content = new Content()
                {
                    ContentId = 0,
                    Type = ContentTypes.ContentType_Picture,
                    Name = "Missing",
                    Data = new byte[fs.Length],
                };
                await fs.ReadAsync(content.Data, 0, content.Data.Length);
                return content;
            }
        }

        private static readonly string _sql = "SELECT TOP 1 * FROM Content WHERE ContentId=@contentId;";
    }
}