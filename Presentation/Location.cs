using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Xml;
using System.Globalization;

namespace DisplayMonkey
{
	public class Location
	{
		public Location()
		{
		}
		
		public Location(int displayId)
		{
			string sql = string.Format(
				"exec dbo.sp_GetLocationDetails @displayId={0};",
				displayId
				);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					InitFromRow(dr);
				}
			}

			// translate LAT/LNG to WOEID
			// get local time
			string url, xml;
			XmlDocument doc = new XmlDocument();

            // get GEO data
            url = string.Format(CultureInfo.InvariantCulture,
                @"http://query.yahooapis.com/v1/public/yql?q=select+*+from+geo.placefinder+where+text%3D%22{0}%2C{1}%22+and+gflags%3D%22R%22",
                Latitude,
                Longitude
                );

            using (WebClient client = new WebClient())
			{
				xml = Encoding.ASCII.GetString(client.DownloadData(url));
			}

			doc.LoadXml(xml);
			Woeid = Convert.ToInt32(doc.SelectSingleNode("//woeid").InnerText);

            // get time offset data
            if (!_offsetGMT.HasValue)
            {
                url = string.Format(CultureInfo.InvariantCulture,
                    @"http://ws.geonames.org/timezone?lat={0}&lng={1}",
                    Latitude,
                    Longitude
                    );
                using (WebClient client = new WebClient())
                {
                    xml = Encoding.ASCII.GetString(client.DownloadData(url));
                }

                doc.LoadXml(xml);
                //string localTime = doc.SelectSingleNode("//time").InnerText;
                //LocalTime = Convert.ToDateTime(localTime);
                XmlNode nodeStatus = doc.SelectSingleNode("//status");
                if (nodeStatus != null)
                {
                    throw new ApplicationException(string.Format(
                        "Error in GEO lookup: {0}\nURL: {1}",
                        nodeStatus.Attributes["message"].Value,
                        url
                        ));
                }
                _offsetGMT = Convert.ToDouble(doc.SelectSingleNode("//gmtOffset").InnerText, CultureInfo.InvariantCulture);
            }
		}

		public void InitFromRow(DataRow r)
		{
            LocationId = DataAccess.IntOrZero(r["LocationId"]);
            LevelId = DataAccess.IntOrZero(r["LevelId"]);
            Latitude = DataAccess.DoubleOrZero(r["Latitude"]);
			Longitude = DataAccess.DoubleOrZero(r["Longitude"]);
			TemperatureUnit = DataAccess.StringOrBlank(r["TemperatureUnit"]).ToLower();
			DateFormat = DataAccess.StringOrBlank(r["DateFormat"]);
			TimeFormat = DataAccess.StringOrBlank(r["TimeFormat"]);
            Name = DataAccess.StringOrBlank(r["Name"]);
            if (Name == "")
                Name = string.Format("Location {0}", LocationId);
            object o = r["OffsetGMT"];
            if (o != DBNull.Value)
                _offsetGMT = Convert.ToDouble(o);
        }

		public static List<Location> List(int levelId = 0)
		{
			List<Location> list = new List<Location>();
			string sql = string.Format(
				"select l.*, v.Name Name2 from Location l inner join Level v on v.LevelId=l.LevelId " +
                "WHERE {0}=0 or l.LevelId={0} order by v.Name, l.Name;",
				levelId
				);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				list.Capacity = ds.Tables[0].Rows.Count;

				// list level locations
				foreach (DataRow r in ds.Tables[0].Rows)
				{
					Location loc = new Location();
					loc.InitFromRow(r);
                    string name2 = DataAccess.StringOrBlank(r["Name2"]);
                    loc.Name = string.Format("{0} : {1}",
                        name2 == "" ? string.Format("Level {0}", loc.LevelId) : name2,
                        loc.Name
                        );
                    list.Add(loc);
				}
			}
			return list;
		}

        public int LocationId = 0;
        public int LevelId = 0;
        public double Latitude = 0;
		public double Longitude = 0;
		public string TemperatureUnit = "C";
		public int Woeid = 0;
		public string DateFormat = "LL";
		public string TimeFormat = "LT";
		//public double OffsetGMT = 0;
		public string Name = "";

        public DateTime LocalTime
        {
            get 
            { 
                return DateTime.UtcNow.AddHours(_offsetGMT.HasValue ? _offsetGMT.Value : 0); 
            }
        }

        private Nullable<double> _offsetGMT = null;
    }
}