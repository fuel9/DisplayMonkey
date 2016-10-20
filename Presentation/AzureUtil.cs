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
using System.Web;
using System.Web.UI;
//using System.Collections.Specialized;
using System.Net.Http;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace DisplayMonkey.AzureUtil
{
    public class TokenInfo
    {
        public string TokenType { get; set; }
        public string Scope { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime NotBefore { get; set; }
        public string Resource { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
    }

    public class AzureTokenException : ApplicationException
    {
        public AzureTokenException(string error, string error_description)
            : base(error)
        {
            Details = error_description
                .Replace("\r\n",", ")
                ;
        }

        public string Details { get; private set; }

        public override string ToString()
        {
            return this.Details;
        }
    }

    public class Token
    {
        public static async Task<TokenInfo> GetGrantTypePasswordAsync(
            Models.AzureResources _resource,
            string _clientId,
            string _clientSecret,
            string _user,
            string _password,
            string _tenantId = null
            )
        {
            TokenInfo token = null;

            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>();
            vals.Add(new KeyValuePair<string, string>("scope", "openid"));

            switch (_resource)
            {
                case Models.AzureResources.AzureResource_PowerBi:
                    vals.Add(new KeyValuePair<string, string>("resource", "https://analysis.windows.net/powerbi/api"));     // TODO: move URL to config file
                    break;

                default:
                    throw new ApplicationException("Resource is required.");
            }

            if (string.IsNullOrWhiteSpace(_clientId))
                throw new ApplicationException("Client ID is required.");
            vals.Add(new KeyValuePair<string, string>("client_id", _clientId.Trim()));

            if (!string.IsNullOrWhiteSpace(_clientSecret))
                vals.Add(new KeyValuePair<string, string>("client_secret", _clientSecret.Trim()));

            vals.Add(new KeyValuePair<string, string>("grant_type", "password"));
            vals.Add(new KeyValuePair<string, string>("username", (_user ?? "").Trim()));
            vals.Add(new KeyValuePair<string, string>("password", (_password ?? "").Trim()));

            string url = string.Format("https://login.windows.net/{0}/oauth2/token", (_tenantId ?? "common").Trim());     // TODO: move URL to config file

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsync(url, new FormUrlEncodedContent(vals));
            string responseData = "";
            using (Stream data = await response.Content.ReadAsStreamAsync())
            using (StreamReader reader = new StreamReader(data, System.Text.Encoding.UTF8))
            {
                responseData = await reader.ReadToEndAsync();
            }

            if (!string.IsNullOrWhiteSpace(responseData))
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.RegisterConverters(new[] { new TokenInfoConverter() });
                if (response.IsSuccessStatusCode)
                    token = jss.Deserialize<TokenInfo>(responseData);
                else
                    throw jss.Deserialize<AzureTokenException>(responseData);    // cannot throw from inside converter (different call context)
            }

            return token;
        }

        private class TokenInfoConverter : JavaScriptConverter
        {
            public override IEnumerable<Type> SupportedTypes { get { return new[] { typeof(TokenInfo), typeof(AzureTokenException) }; } }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer) { throw new NotImplementedException(); }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                if (type == typeof(AzureTokenException))
                {
                    //"{\"error\":\"invalid_request\",\"error_description\":\"AADSTS90014: The request body must contain the following parameter: 'client_id'.\\r\\nTrace ID: a778b02f-0985-44d4-8ff0-fbc46ba046de\\r\\nCorrelation ID: f35a6c18-c5f0-45cd-96e7-23ed7163f504\\r\\nTimestamp: 2016-08-22 21:17:39Z\",\"error_codes\":[90014],\"timestamp\":\"2016-08-22 21:17:39Z\",\"trace_id\":\"a778b02f-0985-44d4-8ff0-fbc46ba046de\",\"correlation_id\":\"f35a6c18-c5f0-45cd-96e7-23ed7163f504\"}"
                    return new AzureTokenException(
                        (string)dictionary["error"], 
                        (string)dictionary["error_description"]
                        );
                }
                else
                {
                    return new TokenInfo()
                    {
                        TokenType = (string)dictionary["token_type"],
                        Scope = (string)dictionary["scope"],
                        ExpiresIn = Convert.ToInt32(dictionary["expires_in"]),
                        ExpiresOn = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(Convert.ToDouble(dictionary["expires_on"])),
                        NotBefore = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(Convert.ToDouble(dictionary["not_before"])),
                        Resource = (string)dictionary["resource"],
                        AccessToken = (string)dictionary["access_token"],
                        RefreshToken = (string)dictionary["refresh_token"],
                        IdToken = (string)dictionary["id_token"],
                    };
                }
            }
        }
    }
}

