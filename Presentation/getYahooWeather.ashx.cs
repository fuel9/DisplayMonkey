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
//using System.Xml;
using System.Web.Script.Serialization;
using System.Diagnostics;

namespace DisplayMonkey
{
	/// <summary>
    /// Summary description for YahooWeather
	/// </summary>
    public class getYahooWeather : IHttpHandler
	{
        public void ProcessRequest(HttpContext context)
		{
			HttpRequest Request = context.Request;
			HttpResponse Response = context.Response;

            int woeid = DataAccess.IntOrZero(Request.QueryString["woeid"]);
            string tempUnit = DataAccess.StringOrBlank(Request.QueryString["tempU"]);
            string json = "";

            try
            {
                // get RSS feed
                string key = string.Format("weather_{0}_{1}", tempUnit, woeid);
                Dictionary<string, object> map = HttpRuntime.Cache.GetOrAddAbsolute(
                    key,
                    () => { return GetYahooWeather(tempUnit, woeid); },
                    DateTime.UtcNow.AddMinutes(1)
                    );

                JavaScriptSerializer oSerializer = new JavaScriptSerializer();
                json = oSerializer.Serialize(map);
            }

            catch (Exception ex)
            {
                JavaScriptSerializer s = new JavaScriptSerializer();
                json = s.Serialize(new
                {
                    Error = ex.Message,
                    //Stack = ex.StackTrace,
                    Data = new
                    {
                        WoeId = woeid,
                        TemperatureUnit = tempUnit,
                    },
                });
            }

            Response.Clear();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetSlidingExpiration(true);
            Response.Cache.SetNoStore();
            Response.ContentType = "application/json";
            Response.Write(json);
            Response.Flush();
		}

        private Dictionary<string, object> GetYahooWeather(string temperatureUnit, int woeid)
        {
            // get repsonse from yahoo
            // TODO: switch to YQL: http://query.yahooapis.com/v1/public/yql?q=select+*+from+weather.forecast+where+woeid%3D2502265+and+u%3D%22c%22
            string response = "", url = string.Format(
                @"http://weather.yahooapis.com/forecastrss?u={0}&w={1}",
                temperatureUnit,
                woeid
                );
            using (WebClient client = new WebClient())
            {
                response = Encoding.ASCII.GetString(client.DownloadData(url));
            }

            // get RSS data strips
            Dictionary<string, string> dic = null;
            Dictionary<string, object> map = new Dictionary<string, object>();
            map.Add("forecast", new List<Dictionary<string, string>>());
            MatchCollection strips = new Regex(@"(?<=<yweather:).+(?=\/>)").Matches(response);
            foreach (Match strip in strips)
            {
                int i = 0;
                string strip_name = "";
                MatchCollection fields = new Regex("(^\\w+(?=\\s))|(\\w+=\"[^\"]*\")").Matches(strip.Value);
                foreach (Match field in fields)
                {
                    if (0 == i++)
                    {
                        strip_name = field.Value;
                        if (strip_name == "forecast")
                        {
                            List<Dictionary<string, string>> lst = map[strip_name] as List<Dictionary<string, string>>;
                            lst.Add(dic = new Dictionary<string, string>());
                        }
                        else
                        {
                            map.Add(strip_name, dic = new Dictionary<string, string>());
                        }
                    }
                    else
                    {
                        string[] pair = field.Value.Split(new char[] { '=' });
                        dic.Add(pair[0], pair[1].Replace("\"", ""));
                    }
                }
            }

            return map;
        }

        public bool IsReusable { get { return false; } }
	}
}
