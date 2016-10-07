using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    [
        MetadataType(typeof(Youtube.Annotations))
    ]
    public partial class Youtube : Frame
    {
        public Youtube(Frame _fromFrame, DisplayMonkeyEntities db) : 
            base(_fromFrame) 
        {
            init(db);
        }

        public Youtube() : base() { }

        protected override void init(DisplayMonkeyEntities _db)
        {
            base.init(_db);

            this.Aspect = YTAspect.YTAspect_Auto;
            this.Quality = YTQuality.YTQuality_Default;
            this.Rate = YTRate.YTRate_Normal;
            this.Start = 0;
            this.Volume = 0;
            this.AutoLoop = true;
        }

        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int FrameId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "YoutubeId"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "YoutubeIdRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string YoutubeId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Volume"),
                Range(0, 100,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "VolumeRangeError"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "VolumeRequired"),
            ]
            public int Volume { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "AutoLoop"),
            ]
            public bool AutoLoop { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "YTAspect"),
                //Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "YTAspectRequired"),
            ]
            public YTAspect Aspect { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "YTQuality"),
                //Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AspectRequired"),
            ]
            public YTQuality Quality { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "YTStart"),
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "YTStartRequired"),
            ]
            public int Start { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "YTRate"),
                //Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AspectRequired"),
            ]
            public YTRate Rate { get; set; }
        }
    }
}