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


namespace DisplayMonkey
{
	public class Weather : Frame
	{
        public int Woeid  { get; private set; }
        public string TemperatureUnit { get; private set; }

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
            string sql = string.Format(
                "SELECT TOP 1 * FROM Weather WHERE FrameId={0};",
                FrameId
                );

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    //FrameId = dr.IntOrZero("FrameId");
                }
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