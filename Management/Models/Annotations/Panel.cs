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
        MetadataType(typeof(Panel.Annotations))
    ]
    public partial class Panel
    {
        public Panel(DisplayMonkeyEntities _db)
        {
            init(_db);
        }
        
        protected virtual void init(DisplayMonkeyEntities _db)
        {
            Setting fadeLength = Setting.GetSetting(_db, Setting.Keys.DefaultPanelFadeLength);
            if (fadeLength != null)
            {
                this.FadeLength = fadeLength.DecimalValue;
            }
        }
        
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "IDRequired"),
            ]
            public int PanelId { get; set; }

            [Display(ResourceType = typeof(Resources), Name = "Canvas"),
             Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "CanvasRequired"),
         ]
            public int CanvasId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Top"),
                Required(ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),

            ]
            public int Top { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Left"),
                Required(ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public int Left { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Height"),
                Required(ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
                Range(1, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public int Height { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Width"),
                Required(ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
                Range(1, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public int Width { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "FadeLength"),
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public double FadeLength { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Canvas"),
            ]
            public virtual Canvas Canvas { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Frames"),
            ]
            public virtual ICollection<Frame> Frames { get; set; }

            /*[
                Display(ResourceType = typeof(Resources), Name = "FullScreens"),
            ]
            public virtual ICollection<FullScreen> FullScreens { get; set; }*/
        }

        [
            NotMapped,
        ]
        public bool IsFullscreen
        {
            get
            {
                return this is FullScreen;
            }
        }
    }
}