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
        MetadataType(typeof(Canvas.Annotations))
    ]
    public partial class Canvas
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int CanvasId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

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
                StringLength(20, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
                Display(ResourceType = typeof(Resources), Name = "BackgroundColor"),
            ]
            public string BackgroundColor { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "BackgroundImage"),
            ]
            public Nullable<int> BackgroundImage { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Content"),
            ]
            public virtual Content Content { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Displays"),
            ]
            public virtual ICollection<Display> Displays { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Panels"),
            ]
            public virtual ICollection<Panel> Panels { get; set; }
        }

        [
            NotMapped,
            Display(ResourceType = typeof(Resources), Name = "FullScreen"),
        ]
        public FullScreen FullScreen
        {
            get
            {
                return this.Panels.OfType<FullScreen>().FirstOrDefault();
            }
        }
    }

    public class CanvasCopy
    {
        [
            Display(ResourceType = typeof(Resources), Name = "ID"),
        ]
        public int CanvasId { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "Name"),
            Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
            StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
        ]
        public string Name { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "CopyPanels"),
        ]
        public bool CopyPanels { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "CopyFrames"),
        ]
        public bool CopyFrames { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "CopyFrameLocations"),
        ]
        public bool CopyFrameLocations { get; set; }
    }
}