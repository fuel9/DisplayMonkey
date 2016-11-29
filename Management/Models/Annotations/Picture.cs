using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    [
        MetadataType(typeof(Picture.Annotations))
    ]
    public partial class Picture : Frame
    {
        public Picture(Frame _fromFrame, DisplayMonkeyEntities db) : 
            base(_fromFrame) 
        {
            init(db);
        }

        public Picture() : base() { }

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
                Display(ResourceType = typeof(Resources), Name = "RenderMode"),
            ]
            public RenderModes Mode { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "LinkedMedia"),
            ]
            public int ContentId { get; set; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "MediaOptions"),
        ]
        public Nullable<int> SavedContentId { get; set; }

        public static string[] SupportedFormats = new string[] {
            "BMP", "GIF", "JPG", "JPEG", "PNG", "TIF", "TIFF"
        };
    }
}