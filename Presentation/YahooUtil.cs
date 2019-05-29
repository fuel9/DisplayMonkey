/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2019 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

// Based on Yahoo Weather API C# Sample Code
// Code sample offered under the terms of the CC0 Public Domain designation. 
// See https://creativecommons.org/publicdomain/zero/1.0/legalcode/ for terms.
// Author: Eugene Plotnikov


using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMonkey
{
    public class YahooUtil
    {
        const string cURL = "https://weather-ydn-yql.media.yahoo.com/forecastrss";

        const string cOAuthVersion = "1.0";
        const string cOAuthSignMethod = "HMAC-SHA1";
        const string cUnitID = "u=";
        const string cFormat = "xml";

        const string cWeatherID = "woeid={0}";


        public static async Task<string> GetYahooWeatherDataAsync(
            string appId, 
            string consumerKey, 
            string consumerSecret,
            string unit = "c",          // Metric units
            int woeid = 727232          // Amsterdam, The Netherlands
            )
        {
            unit = cUnitID + unit;
            string sWoeid = string.Format(cWeatherID, woeid);

            string lURL = cURL + "?" + sWoeid + "&" + unit + "&format=" + cFormat;

            byte[] lDataBuffer = null;

            using (var lClt = new WebClient())
            {
                lClt.Headers.Set("Content-Type", "application/" + cFormat);
                lClt.Headers.Add("Yahoo-App-Id", appId);
                lClt.Headers.Add("Authorization", _get_auth(consumerKey, consumerSecret, unit, sWoeid));

                Console.WriteLine("Downloading Yahoo weather report . . .");

                lDataBuffer = await lClt.DownloadDataTaskAsync(lURL);
            }

            string lOut = Encoding.ASCII.GetString(lDataBuffer);

            Console.WriteLine(lOut);

            return lOut;
        }

        private static string _get_timestamp()
        {
            TimeSpan lTS = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(lTS.TotalSeconds).ToString();
        }  // end _get_timestamp

        private static string _get_nonce()
        {
            return Convert.ToBase64String(
             new ASCIIEncoding().GetBytes(
              DateTime.Now.Ticks.ToString()
             )
            );
        }  // end _get_nonce

        private static string _get_auth(string consumerKey, string consumerSecret, string unit, string woeid)
        {
            string retVal;
            string lNonce = _get_nonce();
            string lTimes = _get_timestamp();
            string lCKey = string.Concat(consumerSecret, "&");
            string lSign = string.Format(  // note the sort order !!!
             "format={0}&" +
             "oauth_consumer_key={1}&" +
             "oauth_nonce={2}&" +
             "oauth_signature_method={3}&" +
             "oauth_timestamp={4}&" +
             "oauth_version={5}&" +
             "{6}&{7}",
             cFormat,
             consumerKey,
             lNonce,
             cOAuthSignMethod,
             lTimes,
             cOAuthVersion,
             unit,
             woeid
            );

            lSign = string.Concat(
             "GET&", Uri.EscapeDataString(cURL), "&", Uri.EscapeDataString(lSign)
            );

            using (var lHasher = new HMACSHA1(Encoding.ASCII.GetBytes(lCKey)))
            {
                lSign = Convert.ToBase64String(
                 lHasher.ComputeHash(Encoding.ASCII.GetBytes(lSign))
                );
            }  // end using

            retVal = "OAuth " +
                   "oauth_consumer_key=\"" + consumerKey + "\", " +
                   "oauth_nonce=\"" + lNonce + "\", " +
                   "oauth_timestamp=\"" + lTimes + "\", " +
                   "oauth_signature_method=\"" + cOAuthSignMethod + "\", " +
                   "oauth_signature=\"" + lSign + "\", " +
                   "oauth_version=\"" + cOAuthVersion + "\"";

            return retVal;

        }  // end _get_auth
    }
}

