using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    [
        MetadataType(typeof(Location.Annotations))
    ]
    public partial class Location
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "IDRequired"),
            ]
            public int LocationId { get; set; }

            [Display(ResourceType = typeof(Resources), Name = "Level"),
             Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "LevelRequired"),
         ]
            public int LevelId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "TemperatureUnit"),
                StringLength(1, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string TemperatureUnit { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Latitude"),
            ]
            public Nullable<double> Latitude { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Longitude"),
            ]
            public Nullable<double> Longitude { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "DateFormat"),
            ]
            public string DateFormat { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "TimeFormat"),
            ]
            public string TimeFormat { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "TimeZone"),
            ]
            public string TimeZone { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Cultre"), // important spelling!!!
            ]
            public string Culture { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Level"),
            ]
            public virtual Level Level { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Area"),
            ]
            public virtual Location Area { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Displays"),
            ]
            public virtual ICollection<Display> Displays { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Frames"),
            ]
            public virtual ICollection<Frame> Frames { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Sublocations"),
            ]
            public virtual ICollection<Location> Sublocations { get; set; }
        }

        [
            NotMapped,
        ]
        public IEnumerable<Location> SelfAndChildren
        {
            get { return this.SelfAndChildren(l => l.Sublocations); }
        }

        [
            NotMapped,
        ]
        public string TempUnitTranslated
        {
            get
            {
                if (this.TemperatureUnit == null)
                    return null;
                else
                    return Resources.ResourceManager.GetString(this.TemperatureUnit);
            }
        }

        [
            NotMapped,
        ]
        public string TempUnitOrDefault
        {
            get
            {
                return this.TempUnitTranslated ?? Resources.FromArea;
            }
        }

        [
            NotMapped,
        ]
        public string DateFmtOrDefault
        {
            get { return this.DateFormat ?? Resources.FromArea; }
        }

        [
            NotMapped,
        ]
        public string TimeFmtOrDefault
        {
            get { return this.TimeFormat ?? Resources.FromArea; }
        }

        [
            NotMapped,
        ]
        public string LatitudeOrDefault
        {
            get
            {
                if (this.Latitude.HasValue)
                    return this.Latitude.ToString();
                else
                    return Resources.FromArea;
            }
        }

        [
            NotMapped,
        ]
        public string LongitudeOrDefault
        {
            get
            {
                if (this.Longitude.HasValue)
                    return this.Longitude.ToString();
                else
                    return Resources.FromArea;
            }
        }

        [
            NotMapped,
        ]
        public string WoeidOrDefault
        {
            get
            {
                if (this.Woeid.HasValue)
                    return this.Woeid.ToString();
                else
                    return Resources.FromArea;
            }
        }

        [
            NotMapped,
        ]
        public string TimeZoneOrDefault
        {
            get
            {
                if (this.TimeZone != null)
                    return this.TimeZone;
                else
                    return Resources.FromArea;
            }
        }

        [
            NotMapped,
        ]
        public string CultureOrDefault
        {
            get
            {
                return Info.SupportedCultures
                    .Where(c => c.Name == this.Culture)
                    .Select(c => c.DisplayName)
                    .FirstOrDefault() ?? Resources.FromAreaOrServerDefault
                    ;
            }
        }
    }
}