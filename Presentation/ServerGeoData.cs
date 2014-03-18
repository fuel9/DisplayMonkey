using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
//using System.Xml;
using System.Web.Script.Serialization;
using System.Globalization;
using System.IO;

namespace DisplayMonkey
{
    public class ServerGeoData
    {
        // server defaults
        public static decimal Latitude { get; private set; }
        public static decimal Longitude { get; private set; }
        public static int OffsetGMT { get; private set; }
        public static IPAddress ServerExternalIPAddress { get; private set; }

        static ServerGeoData()
        {
            Latitude = 0;
            Longitude = 0;
            OffsetGMT = 0;
            ServerExternalIPAddress = IPAddress.Any;
            
            GetServerDefaults();
        }
        
        public static void GetServerDefaults()
        {
            // get GEO data from GeoBytes
            try 
            {
                string json = null;

                //json = File.OpenText("e://geo.txt").ReadToEnd();
                string url =
                    @"http://www.geobytes.com/IpLocator.htm?GetLocation&template=json.txt";
                using (WebClient client = new WebClient())
                {
                    json = Encoding.ASCII.GetString(client.DownloadData(url));
                }

                if (json != null)
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    Dictionary<string, object> data = 
                        js.DeserializeObject(json) as Dictionary<string, object>;

                    if (data != null)
                    {
                        Dictionary<string, object> data2 = 
                            data["geobytes"] as Dictionary<string, object>;

                        string ipaddress = data2["ipaddress"] as string;
                        if (ipaddress != null) 
                        {
                            IPAddress ip;
                            IPAddress.TryParse(ipaddress, out ip);
                            if (ip != null) ServerExternalIPAddress = ip;
                        }

                        Nullable<decimal> latitude = data2["latitude"] as Nullable<decimal>;
                        if (latitude != null) Latitude = latitude.Value;

                        Nullable<decimal> longitude = data2["longitude"] as Nullable<decimal>;
                        if (longitude != null) Longitude = longitude.Value;

                        string timezone = data2["timezone"] as string;
                        if (timezone != null)
                        {
                            if (timezone.Contains(':')) 
                                timezone = timezone.Split(new char[] {':'})[0];
                            OffsetGMT = Convert.ToInt32(timezone);
                        }
                    }
                }
            }

            catch (Exception)
            {
                //string xx = ex.ToString();
            }

        }
    }
}