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
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
	public class Location
	{
        public int LocationId { get; private set; }
        public int LevelId { get; private set; }
        public string TemperatureUnit { get; private set; }
        public int Woeid { get; private set; }
        public string DateFormat { get; private set; }
        public string TimeFormat { get; private set; }
        public string Name { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public string Culture { get; private set; }

        [ScriptIgnore]
        public TimeZoneInfo TimeZone { get; private set; }

        public int OffsetGMT
        {
            get
            {
                return (int)this.TimeZone.BaseUtcOffset.TotalMinutes;
            }
        }

        public int OffsetUtc
        {
            get
            {
                return (int)this.TimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
            }
        }

        public DateTime LocationTime
        {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.TimeZone);
            }
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
                    _initFromRow(dr);
				}
			}
		}

        private Location()
        {
        }

        private void _initFromRow(DataRow r)
		{
            this.LocationId = r.IntOrZero("LocationId");
            this.LevelId = r.IntOrZero("LevelId");
            this.TemperatureUnit = r.StringOrDefault("TemperatureUnit", "C").ToLower();
            this.DateFormat = r.StringOrDefault("DateFormat", "LL");
            this.TimeFormat = r.StringOrDefault("TimeFormat", "LT");
            this.Name = r.StringOrBlank("Name");
            if (this.Name == "")
                this.Name = string.Format("Location {0}", this.LocationId);

            this.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(
                r.StringOrDefault("TimeZone", ServerGeoData.TimeZone.Id)
                );

            double? latitude = r["Latitude"] as Nullable<double>;
            if (latitude != null)
                this.Latitude = latitude.Value;
            else
                this.Latitude = ServerGeoData.Latitude;

            double? longitude = r["Longitude"] as Nullable<double>;
            if (longitude != null)
                this.Longitude = longitude.Value;
            else
                this.Longitude = ServerGeoData.Longitude;

            int? woeid = r["Woeid"] as Nullable<int>;
            this.Woeid = woeid ?? 0;

            this.Culture = r.StringOrBlank("Culture");
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
					loc._initFromRow(r);
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
    }
}