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

namespace DisplayMonkey
{
	/// <summary>
    /// Summary description for YahooWeather
	/// </summary>
    public class getYahooWeather : HttpTaskAsyncHandler
	{
        public override async Task ProcessRequestAsync(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

            int frameId = request.IntOrZero("frame");
            int woeid = request.IntOrZero("woeid");
            string tempUnit = request.StringOrBlank("tempU");
            int trace = request.IntOrZero("trace");

            string json = "";

            try
            {
                Weather weather = new Weather(frameId);

                if (weather.FrameId != 0)
                {
                    // get RSS feed
                    Dictionary<string, object> map = await HttpRuntime.Cache.GetOrAddAbsoluteAsync(
                        string.Format("weather_{0}_{1}_{2}_{3}", weather.FrameId, weather.Version, tempUnit, woeid),
                        async (expire) => 
                        {
                            expire.When = DateTime.Now.AddMinutes(weather.CacheInterval);
                            return await GetYahooWeatherAsync(tempUnit, woeid); 
                        });

                    JavaScriptSerializer oSerializer = new JavaScriptSerializer();
                    json = oSerializer.Serialize(map);
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

        private async Task<Dictionary<string, object>> GetYahooWeatherAsync(string temperatureUnit, int woeid)
        {
            // get repsonse from yahoo
            string response = "", url = string.Format(
                @"http://query.yahooapis.com/v1/public/yql?q=select+*+from+weather.forecast+where+woeid%3D{1}+and+u%3D%22{0}%22",
                temperatureUnit,
                woeid
                );
            using (WebClient client = new WebClient())
            {
                response = Encoding.ASCII.GetString(await client.DownloadDataTaskAsync(url));
            }

            Dictionary<string, object> map = new Dictionary<string, object>();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(response);
            XmlElement channel = xml.SelectSingleNode(@"//channel[1]") as XmlElement;
            if (channel != null)
            {
                XmlElement e = null;

                // wind
                e = channel.SelectSingleNode(@"*[local-name()='wind']") as XmlElement;
                if (e != null)
                {
                    Dictionary<string, object> o = new Dictionary<string, object>();
                    o.Add("chill", e.GetAttribute("chill"));
                    o.Add("direction", e.GetAttribute("direction"));
                    o.Add("speed", e.GetAttribute("speed"));
                    map.Add(e.LocalName, o);
                }

                // location
                e = channel.SelectSingleNode(@"*[local-name()='location']") as XmlElement;
                if (e != null)
                {
                    Dictionary<string, object> o = new Dictionary<string, object>();
                    o.Add("city", e.GetAttribute("city"));
                    o.Add("region", e.GetAttribute("region"));
                    o.Add("country", e.GetAttribute("country"));
                    map.Add(e.LocalName, o);
                }

                // units
                e = channel.SelectSingleNode(@"*[local-name()='units']") as XmlElement;
                if (e != null)
                {
                    Dictionary<string, object> o = new Dictionary<string, object>();
                    o.Add("temperature", e.GetAttribute("temperature"));
                    o.Add("distance", e.GetAttribute("distance"));
                    o.Add("pressure", e.GetAttribute("pressure"));
                    o.Add("speed", e.GetAttribute("speed"));
                    map.Add(e.LocalName, o);
                }

                // atmosphere
                e = channel.SelectSingleNode(@"*[local-name()='atmosphere']") as XmlElement;
                if (e != null)
                {
                    Dictionary<string, object> o = new Dictionary<string, object>();
                    o.Add("humidity", e.GetAttribute("humidity"));
                    o.Add("visibility", e.GetAttribute("visibility"));
                    o.Add("pressure", e.GetAttribute("pressure"));
                    o.Add("rising", e.GetAttribute("rising"));
                    map.Add(e.LocalName, o);
                }

                // astronomy
                e = channel.SelectSingleNode(@"*[local-name()='astronomy']") as XmlElement;
                if (e != null)
                {
                    Dictionary<string, object> o = new Dictionary<string, object>();
                    o.Add("sunrise", e.GetAttribute("sunrise"));
                    o.Add("sunset", e.GetAttribute("sunset"));
                    map.Add(e.LocalName, o);
                }

                // item
                XmlElement item = channel.SelectSingleNode(@"item[1]") as XmlElement;
                if (item != null)
                {
                    // condition
                    e = item.SelectSingleNode(@"*[local-name()='condition']") as XmlElement;
                    if (e != null)
                    {
                        Dictionary<string, object> o = new Dictionary<string, object>();
                        o.Add("text", e.GetAttribute("text"));
                        o.Add("code", e.GetAttribute("code"));
                        o.Add("temp", e.GetAttribute("temp"));
                        o.Add("date", e.GetAttribute("date"));
                        map.Add(e.LocalName, o);
                    }

                    // forecast
                    XmlNodeList forecastList = item.SelectNodes(@"*[local-name()='forecast']");
                    if (forecastList.Count > 0)
                    {
                        List<object> fmap = new List<object>(forecastList.Count);
                        foreach (XmlNode n in forecastList)
                        {
                            Dictionary<string, object> o = new Dictionary<string, object>();
                            XmlElement f = n as XmlElement;
                            o.Add("day", f.GetAttribute("day"));
                            o.Add("date", f.GetAttribute("date"));
                            o.Add("low", f.GetAttribute("low"));
                            o.Add("high", f.GetAttribute("high"));
                            o.Add("text", f.GetAttribute("text"));
                            o.Add("code", f.GetAttribute("code"));
                            fmap.Add(o);
                        }
                        map.Add("forecast", fmap);
                    }
                }
            }

            return map;
        }
	}
}
