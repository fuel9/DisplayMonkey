using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Xml;

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
			string url, xml1, xml2;
			XmlDocument doc = new XmlDocument();
			using (WebClient client = new WebClient())
			{
				// get GEO data
				url = string.Format(
					@"http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20geo.placefinder%20where%20text%3D%22{0}%2C{1}%22%20and%20gflags%3D%22R%22",
					Latitude,
					Longitude
					);
				xml1 = Encoding.ASCII.GetString(client.DownloadData(url));

				// get time data
				url = string.Format(
					@"http://ws.geonames.org/timezone?lat={0}&lng={1}",
					Latitude,
					Longitude
					);
				xml2 = Encoding.ASCII.GetString(client.DownloadData(url));
			}

			doc.LoadXml(xml1);
			Woeid = Convert.ToInt32(doc.SelectSingleNode("//woeid").InnerText);

			doc.LoadXml(xml2);
			//string localTime = doc.SelectSingleNode("//time").InnerText;
			//LocalTime = Convert.ToDateTime(localTime);
			OffsetGMT = Convert.ToDouble(doc.SelectSingleNode("//gmtOffset").InnerText);
		}

		public void InitFromRow(DataRow r)
		{
			LocationId = DataAccess.IntOrZero(r["LocationId"]);
			Latitude = DataAccess.DoubleOrZero(r["Latitude"]);
			Longitude = DataAccess.DoubleOrZero(r["Longitude"]);
			TemperatureUnit = DataAccess.StringOrBlank(r["TemperatureUnit"]).ToLower();
			DateFormat = DataAccess.StringOrBlank(r["DateFormat"]);
			TimeFormat = DataAccess.StringOrBlank(r["TimeFormat"]);
			Name = DataAccess.StringOrBlank(r["Name"]);
			if (Name == "")
				Name = string.Format("Location {0}", LocationId);
		}

		public static List<Location> List(int levelId = 0)
		{
			List<Location> list = new List<Location>();
			string sql = string.Format(
				"DECLARE @levelId INT; SET @levelId={0}; IF(@levelId=0) SELECT @levelId = MIN(LevelId) FROM LEVEL;" +
				"SELECT * FROM LOCATION WHERE LevelId=@levelId ORDER BY 1;",
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
					list.Add(loc);
				}
			}
			return list;
		}

		public int LocationId = 0;
		public double Latitude = 0;
		public double Longitude = 0;
		public string TemperatureUnit = "C";
		public int Woeid = 0;
		public string DateFormat = "LL";
		public string TimeFormat = "LT";
		public double OffsetGMT = 0;
		public string Name = "";

	}
}