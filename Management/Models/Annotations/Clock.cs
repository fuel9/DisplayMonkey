using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    [
        MetadataType(typeof(Clock.Annotations))
    ]
    public partial class Clock : Frame
    {
        public Clock(Frame _fromFrame, DisplayMonkeyEntities db) : 
            base(_fromFrame) 
        {
            init(db);
        }

        public Clock() : base() { }

        protected override void init(DisplayMonkeyEntities _db)
        {
            base.init(_db);

            this.ShowSeconds = true;
            this.ShowDate = true;
            this.ShowTime = true;
        }

        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int FrameId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ClockType"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ClockTypeRequired"),
            ]
            public ClockTypes Type { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ShowDate"),
            ]
            public bool ShowDate { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ShowTime"),
            ]
            public bool ShowTime { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ShowSeconds"),
            ]
            public bool ShowSeconds { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "TimeZone"),
            ]
            public string TimeZone { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Label"),
            ]
            public string Label { get; set; }
        }
    }
}