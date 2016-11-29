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
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Web.Script.Serialization;
using System.Threading.Tasks;

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
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "exec dbo.sp_GetLocationDetails @displayId",
            })
            {
                cmd.Parameters.AddWithValue("@displayId", displayId);
                cmd.ExecuteReaderExt((r) =>
                {
                    _initFromRow(r);
                    return false;
                });
            }
        }

        public static List<Location> List(int levelId = 0)
        {
            List<Location> list = new List<Location>();

            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText =
                    "select l.*, v.Name Name2 from Location l inner join Level v on v.LevelId=l.LevelId " +
                    "where @levelId=0 or l.LevelId=@levelId order by v.Name, l.Name;",
            })
            {
                cmd.Parameters.AddWithValue("@levelId", levelId);
                cmd.ExecuteReaderExt((r) =>
                {
                    Location loc = new Location();
                    loc._initFromRow(r);
                    string name2 = r.StringOrBlank("Name2").Trim();
                    loc.Name = string.Format("{0} : {1}",
                        name2 == "" ? string.Format("Level {0}", loc.LevelId) : name2,
                        loc.Name
                        );
                    list.Add(loc);
                    return true;
                });
            }

            return list;
        }

        private Location()
        {
        }

        private void _initFromRow(SqlDataReader r)
		{
            LocationId = r.IntOrZero("LocationId");
            LevelId = r.IntOrZero("LevelId");
            TemperatureUnit = r.StringOrDefault("TemperatureUnit", "C").ToLower();
            DateFormat = r.StringOrDefault("DateFormat", "LL");
            TimeFormat = r.StringOrDefault("TimeFormat", "LT");
            Latitude = r.ValueOrDefault<double>("Latitude", ServerGeoData.Latitude);
            Longitude = r.ValueOrDefault<double>("Longitude", ServerGeoData.Longitude);
            Woeid = r.IntOrZero("Woeid");
            Culture = r.StringOrBlank("Culture");

            Name = r.StringOrBlank("Name");
            if (Name == "")
                Name = string.Format("Location {0}", LocationId);

            TimeZone = TimeZoneInfo.FindSystemTimeZoneById(
                r.StringOrDefault("TimeZone", ServerGeoData.TimeZone.Id)
                );
        }
    }
}