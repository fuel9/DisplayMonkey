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
using DisplayMonkey.Language;


namespace DisplayMonkey
{
    public class OAuthAccount
    {
        public int Provider { get; private set; }
        public int AccountId { get; private set; }
        public string AppId { get; private set; }
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }

        public OAuthAccount(int provider, int accountId)
        {
            Provider = provider;
            AccountId = accountId;

            _init();
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM OauthAccount WHERE AccountId=@accountId and Provider=@provider",
            })
            {
                cmd.Parameters.AddWithValue("@accountId", AccountId);
                cmd.Parameters.AddWithValue("@provider", Provider);
                cmd.ExecuteReaderExt((dr) =>
                {
                    AppId = dr.StringOrBlank("AppId").Trim();
                    ClientId = dr.StringOrBlank("ClientId").Trim();
                    ClientSecret = dr.StringOrBlank("ClientSecret").Trim();
                    return false;
                });
            }
        }
    }
}