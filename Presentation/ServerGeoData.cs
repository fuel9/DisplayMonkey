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
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Script.Serialization;
using System.Globalization;
using System.IO;

namespace DisplayMonkey
{
    public class ServerGeoData
    {
        // server defaults
        public static double Latitude { get; private set; }
        public static double Longitude { get; private set; }
        public static TimeZoneInfo TimeZone { get; private set; }
        public static IPAddress ServerExternalIPAddress { get; private set; }

        public static int OffsetGMT
        {
            get
            {
                return (int)TimeZone.BaseUtcOffset.TotalMinutes;
            }
        }

        public int OffsetUtc
        {
            get
            {
                return (int)TimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
            }
        }

        public DateTime ServerTime
        {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZone);
            }
        }
        

        static ServerGeoData()
        {
            Latitude = 0;
            Longitude = 0;
            TimeZone = TimeZoneInfo.Local;
            ServerExternalIPAddress = IPAddress.Any;
            
            _getServerDefaults();
        }

        private static void _getServerDefaults()
        {
            // get GEO data from GeoBytes
            try
            {
                string 
                    json = null, 
                    url = @"http://api.ipify.org/?format=json"
                    ;

                using (WebClient client = new WebClient())
                {
                    json = Encoding.ASCII.GetString(client.DownloadData(url));
                }

                if (json == null)
                    return;

                JavaScriptSerializer js = new JavaScriptSerializer();
                Dictionary<string, object> data =
                    js.DeserializeObject(json) as Dictionary<string, object>
                    ;

                if (data == null)
                    return;

                // {"ip":"12.180.251.201"}
                string ipaddress = data["ip"] as string;
                if (ipaddress != null)
                {
                    IPAddress ip;
                    IPAddress.TryParse(ipaddress, out ip);
                    if (ip != null) ServerExternalIPAddress = ip;
                }

                url = string.Format(
                    @"https://freegeoip.net/json/{0}",
                    ServerExternalIPAddress
                    );

                using (WebClient client = new WebClient())
                {
                    json = Encoding.ASCII.GetString(client.DownloadData(url));
                }

                if (json == null)
                    return;

                data = js.DeserializeObject(json) as Dictionary<string, object>;

                if (data == null)
                    return;

                /*{
                    "ip" : "192.30.252.131",
                    "country_code" : "US",
                    "country_name" : "United States",
                    "region_code" : "CA",
                    "region_name" : "California",
                    "city" : "San Francisco",
                    "zip_code" : "94107",
                    "time_zone" : "America/Los_Angeles",
                    "latitude" : 37.77,
                    "longitude" : -122.394,
                    "metro_code" : 807
                }*/

                if (data["latitude"] != null)
                    Latitude = Convert.ToDouble(data["latitude"]);

                if (data["longitude"] != null)
                    Longitude = Convert.ToDouble(data["longitude"]);

                /*string timeZone = data["time_zone"] as string;
                timeZone = _timeZoneUnixToWindows(timeZone);
                if (timeZone != null)
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                }*/
            }

            catch (Exception)
            {
                // TODO
                //string xx = ex.ToString();
            }

        }

        private static string _timeZoneUnixToWindows(string windowsId)
        {
            if (windowsId != null)
            {
                XmlDocument xmlWindowsZones = new XmlDocument();
                xmlWindowsZones.Load(HttpContext.Current.Server.MapPath("~/files/windowsZones.xml"));
                XmlNode rootNode = xmlWindowsZones.DocumentElement;
                XmlNode winName = rootNode.SelectSingleNode(string.Format(
                    "//supplementalData/windowsZones/mapTimezones/mapZone[@type='{0}']/@other",
                    windowsId
                    ));
                if (winName != null)
                {
                    return winName.Value;
                }
            }

            return null;
        }
    }
}