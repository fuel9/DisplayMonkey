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
					Latitude = (double)dr["Latitude"];
					Longitude = (double)dr["Longitude"];
					TemperatureUnit = ((string)dr["TemperatureUnit"]).ToLower();
					DateFormat = (string)dr["DateFormat"];
					TimeFormat = (string)dr["TimeFormat"];
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

		public double Latitude;
		public double Longitude;
		public string TemperatureUnit;
		public int Woeid;
		public string DateFormat;
		public string TimeFormat;
		//public DateTime LocalTime;
		public double OffsetGMT;

	}
}