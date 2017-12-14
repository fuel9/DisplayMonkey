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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml;
using DisplayMonkey.Language;
using DisplayMonkey.Models;

namespace DisplayMonkey
{
	public class Weather : Frame
	{
        public int Woeid  { get; private set; }
        public string TemperatureUnit { get; private set; }
        public WeatherTypes Type { get; protected set; }
        public string Location { get; protected set; }

        public Weather(int frameId)
            : base(frameId)
        {
            _init();
        }

        public Weather(Frame frame)
            : base(frame)
        {
            _init();
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Weather WHERE FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", this.FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    Type = (WeatherTypes)dr.IntOrZero("Type");
                    Location = dr.StringOrBlank("Location");
                    return false;
                });
            }

            // TODO: add own Woeid to Weather model
            /*Location location = new Location(DisplayId);
            if (location.LocationId != 0)
            {
                Woeid = location.Woeid;
                TemperatureUnit = location.TemperatureUnit;
            }
            else
            {
                Woeid = 56199578;      // Old Sacramento
                TemperatureUnit = "f";
            }*/
        }
	}
}