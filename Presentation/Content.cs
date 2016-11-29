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

        private Content()
        {
        }

        public static async Task<Content> GetDataAsync(int contentId)
        {
            Content content = null;

            using (SqlCommand cmd = new SqlCommand() 
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Content WHERE ContentId=@contentId",
            })
            {
                cmd.Parameters.AddWithValue("@contentId", contentId);
                await cmd.ExecuteReaderExtAsync((reader) =>
                {
                    content = new Content()
                    {
                        ContentId = reader.IntOrZero("ContentId"),
                        Name = reader.StringOrBlank("Name"),
                        Type = reader.ValueOrDefault<ContentTypes>("Type", ContentTypes.ContentType_Picture),
                        Data = reader.BytesOrNull("Data"),
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
                    Name = DataAccess.StringResource("Missing"),
                    Data = new byte[fs.Length],
                };
                await fs.ReadAsync(content.Data, 0, content.Data.Length);
                return content;
            }
        }
    }
}