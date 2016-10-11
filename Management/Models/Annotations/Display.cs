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
        MetadataType(typeof(Display.Annotations))
    ]
    public partial class Display
    {
        public Display(DisplayMonkeyEntities _db, int _canvasId, int _locationId)
        {
            CanvasId = _canvasId;
            LocationId = _locationId;
            init(_db);
        }

        public Display() { }
        
        protected void init(DisplayMonkeyEntities _db)
        {
            Setting readyTimeout = Setting.GetSetting(_db, Setting.Keys.DefaultDisplayReadyEventTimeout);
            if (readyTimeout != null)
            {
                this.ReadyTimeout = readyTimeout.IntValuePositive;
            }

            Setting errorLength = Setting.GetSetting(_db, Setting.Keys.DefaultDisplayErrorLength);
            if (errorLength != null)
            {
                this.ErrorLength = errorLength.IntValuePositive;
            }

            Setting pollInterval = Setting.GetSetting(_db, Setting.Keys.DefaultDisplayPollInterval);
            if (pollInterval != null)
            {
                this.PollInterval = pollInterval.IntValuePositive;
            }
        }

        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int DisplayId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Canvas"),
            ]
            public int CanvasId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Location"),
            ]
            public int LocationId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Host"),
            ]
            public string Host { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ErrorLength"),
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public int ErrorLength { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "PollInterval"),
                Range(1, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public int PollInterval { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "NoScroll"),
            ]
            public bool NoScroll { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "NoCursor"),
            ]
            public bool NoCursor { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Canvas"),
            ]
            public virtual Canvas Canvas { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Location"),
            ]
            public virtual Location Location { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ReadyTimeout"),
                Range(1, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public int ReadyTimeout { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "RecycleTime"),
            ]
            public Nullable<System.TimeSpan> RecycleTime { get; set; }
        }

        [
            NotMapped,
            Display(ResourceType = typeof(Resources), Name = "RecycleTime"),
        ]
        public string AutoRefreshAt
        {
            get
            {
                string x = null;
                if (this.RecycleTime.HasValue)
                    x = this.RecycleTime.Value.ToString(@"h\:mm");
                return x;
            }

            set
            {
                TimeSpan x;
                if (TimeSpan.TryParse(value, out x))
                    this.RecycleTime = x;
                else
                    this.RecycleTime = null;
            }
        }
    }
}