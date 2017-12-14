using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    [
        MetadataType(typeof(Weather.Annotations))
    ]
    public partial class Weather : Frame
    {
        public Weather(Frame _fromFrame, DisplayMonkeyEntities db) : 
            base(_fromFrame) 
        {
            init(db);
        }

        public Weather() : base() { }

        protected override void init(DisplayMonkeyEntities _db)
        {
            base.init(_db);

            this.CacheInterval = Setting.GetDefaultCacheInterval(_db, this.FrameType);
        }

        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int FrameId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "WeatherType"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "WeatherTypeRequired"),
            ]
            public WeatherTypes Type { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Location"),
            ]
            public string Location { get; set; }


        }
    }
}