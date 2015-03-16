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
		}

		public void InitFromRow(DataRow r)
		{
            LocationId = r.IntOrZero("LocationId");
            LevelId = r.IntOrZero("LevelId");
			TemperatureUnit = r.StringOrBlank("TemperatureUnit").ToLower();
			DateFormat = r.StringOrBlank("DateFormat");
			TimeFormat = r.StringOrBlank("TimeFormat");
            Name = r.StringOrBlank("Name");
            if (Name == "")
                Name = string.Format("Location {0}", LocationId);

            int? offsetGMT = r["OffsetGMT"] as Nullable<int>;
            if (offsetGMT != null) OffsetGMT = offsetGMT.Value;

            double? latitude = r["Latitude"] as Nullable<double>;
            if (latitude != null) Latitude = Convert.ToDecimal(latitude.Value);

            double? longitude = r["Longitude"] as Nullable<double>;
            if (longitude != null) Longitude = Convert.ToDecimal(longitude.Value);

            int? woeid = r["Woeid"] as Nullable<int>;
            if (woeid != null) Woeid = woeid.Value;
            Culture = r.StringOrBlank("Culture");
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
                    string name2 = r.StringOrBlank("Name2").Trim();
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
		public string TemperatureUnit = "C";
		public int Woeid = 0;
		public string DateFormat = "LL";
		public string TimeFormat = "LT";
		public string Name = "";
        public int OffsetGMT = ServerGeoData.OffsetGMT;
        public decimal Latitude = ServerGeoData.Latitude;
        public decimal Longitude = ServerGeoData.Longitude;
        public string Culture = "";

        public DateTime LocationTime
        {
            get 
            {
                return DateTime.UtcNow.AddHours(OffsetGMT); 
            }
        }
    }
}