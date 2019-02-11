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
            this.Provider = WeatherProviders.WeatherProvider_Yahoo; // TODO: make a parameter as more providers are added
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
                Display(ResourceType = typeof(Resources), Name = "WeatherProvider"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "WeatherProviderRequired"),
            ]
            public Nullable<WeatherProviders> Provider { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "WeatherProviderAccount"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "WeatherProviderAccountRequired"),
            ]
            public Nullable<int> AccountId { get; set; }
        }
    }
}