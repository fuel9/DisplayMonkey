using System;
using System.Web;
using System.Web.UI;
//using System.Collections.Specialized;
using System.Net.Http;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.IO;

namespace DisplayMonkey.PowerbiUtil
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

    public class Token
    {
        public static TokenInfo GetGrantTypePassword(string _clientId, string _clientSecret, string _user, string _password)
        {
            TokenInfo token = null;
            
            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>();
            vals.Add(new KeyValuePair<string, string>("scope", "openid"));
            vals.Add(new KeyValuePair<string, string>("resource", "https://analysis.windows.net/powerbi/api"));

            vals.Add(new KeyValuePair<string, string>("client_id", _clientId));
            vals.Add(new KeyValuePair<string, string>("client_secret", _clientSecret));

            vals.Add(new KeyValuePair<string, string>("grant_type", "password"));
            vals.Add(new KeyValuePair<string, string>("username", _user));
            vals.Add(new KeyValuePair<string, string>("password", _password));

            string TenantId = "common";
            string url = string.Format("https://login.windows.net/{0}/oauth2/token", TenantId);

            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.PostAsync(url, new FormUrlEncodedContent(vals)).Result;
            string responseData = "";
            if (response.IsSuccessStatusCode)
            {
                using (Stream data = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader reader = new StreamReader(data, System.Text.Encoding.UTF8))
                {
                    responseData = reader.ReadToEnd();
                }
            }

            if (!string.IsNullOrWhiteSpace(responseData))
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.RegisterConverters(new[] { new TokenInfoConverter() });
                token = jss.Deserialize<TokenInfo>(responseData); 
            }

            return token;
        }

        private class TokenInfoConverter : JavaScriptConverter
        {
            public override IEnumerable<Type> SupportedTypes { get { return new[] { typeof(TokenInfo) }; } }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer) { throw new NotImplementedException(); }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
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

