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
using System.IO;
using System.Data;
//using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace DisplayMonkey
{
	/// <summary>
    /// Summary description for YahooWeather
	/// </summary>
    public class getWeather : HttpTaskAsyncHandler
	{
        public override async Task ProcessRequestAsync(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

            int frameId = request.IntOrZero("frame");
            int woeid = request.IntOrZero("woeid");
            string tempUnit = request.StringOrBlank("tempU");
            int trace = request.IntOrZero("trace");

            string key = WebConfigurationManager.AppSettings["WundergroundKey"];
            string language = request.StringOrBlank("culture");
            if (language.Length > 2)
            {
                language = language.Substring(0, 2).ToUpper();
            }
            else
            {
                language = "EN"; 
            }
            string location = request.StringOrBlank("location");
            double latitude = request.DoubleOrZero("latitude");
            double longitude = request.DoubleOrZero("longitude");
            if (location.Length == 0)
            {
                location = $"{latitude},{longitude}";
            }

            string json = "";

            try
            {
                Weather weather = new Weather(frameId);

                if (weather.FrameId != 0)
                {
                    json = await HttpRuntime.Cache.GetOrAddAbsoluteAsync(
                        string.Format("weather_{0}_{1}_{2}_{3}_{4}", weather.FrameId, weather.Version, key, language, location),
                        async (expire) => 
                        {
                            expire.When = DateTime.Now.AddMinutes(weather.CacheInterval);
                            return await GetWeatherAsync(key, language, location); 
                        });
                }

                else
                    throw new Exception("Incorrect frame data");
            }

            catch (Exception ex)
            {
                JavaScriptSerializer s = new JavaScriptSerializer();
                if (trace == 0)
                    json = s.Serialize(new
                    {
                        Error = ex.Message,
                        Data = new
                        {
                            WoeId = woeid,
                            TemperatureUnit = tempUnit,
                        },
                    });
                else
                    json = s.Serialize(new
                    {
                        Error = ex.Message,
                        Stack = ex.StackTrace,
                        Data = new
                        {
                            WoeId = woeid,
                            TemperatureUnit = tempUnit,
                        },
                    });
            }

            response.Clear();
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetSlidingExpiration(true);
            response.Cache.SetNoStore();
            response.ContentType = "application/json";
            response.Write(json);
            response.Flush();
		}

        private async Task<string> GetWeatherAsync(string key, string language, string location)
        {
            string url = $"http://api.wunderground.com/api/{key}/conditions/forecast/lang:{language}/q/{location}.json";

            string response = "";
            using (WebClient client = new WebClient())
            {
                response = Encoding.ASCII.GetString(await client.DownloadDataTaskAsync(url));
            }

            return response;
        }
	}
}
