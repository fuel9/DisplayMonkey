using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Text;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Objects;
using System.Reflection;

using DisplayMonkey.Language;


namespace DisplayMonkey.Models
{
    [
        MetadataType(typeof(Level.Annotations)),
    ]
    public partial class Level 
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int LevelId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType=typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Locations"),
            ]
            public virtual ICollection<Location> Locations { get; set; }
        }
    }

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

            [   Display(ResourceType = typeof(Resources), Name = "Level"),
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
                Display(ResourceType = typeof(Resources), Name = "OffsetGMT"),
            ]
            public Nullable<int> OffsetGMT { get; set; }

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

        public IEnumerable<Location> SelfAndChildren
        {
            get { return this.SelfAndChildren(l => l.Sublocations); }
        }
    }

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
                Display(ResourceType = typeof(Resources), Name = "FullScreen"),
            ]
            public virtual FullScreen FullScreen { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Displays"),
            ]
            public virtual ICollection<Display> Displays { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Panels"),
            ]
            public virtual ICollection<Panel> Panels { get; set; }
        }
    }

    [
        MetadataType(typeof(Panel.Annotations))
    ]
    public partial class Panel
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "IDRequired"),
            ]
            public int PanelId { get; set; }

            [   Display(ResourceType = typeof(Resources), Name = "Canvas"),
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
                Display(ResourceType = typeof(Resources), Name = "Canvas"),
            ]
            public virtual Canvas Canvas { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Frames"),
            ]
            public virtual ICollection<Frame> Frames { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "FullScreens"),
            ]
            public virtual ICollection<FullScreen> FullScreens { get; set; }
        }

        public bool IsFullscreen
        {
            get
            {
                return this.Canvas != null && this.Canvas.FullScreen != null
                    ? this.PanelId == this.Canvas.FullScreen.PanelId
                    : false;
            }
        }
    }

    [
        MetadataType(typeof(FullScreen.Annotations))
    ]
    public partial class FullScreen
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int PanelId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Canvas"),
            ]
            public int CanvasId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "MaxIdleInterval"),
                Required(ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public Nullable<int> MaxIdleInterval { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Canvas"),
            ]
            public virtual Canvas Canvas { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Panel"),
            ]
            public virtual Panel Panel { get; set; }
        }
    }

    [
        MetadataType(typeof(Frame.Annotations))
    ]
    public partial class Frame
    {
        internal class Annotations
        {
            [
               Display(ResourceType = typeof(Resources), Name = "ID"), 
            ]
            public int FrameId { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Panel"),
            ]
            public int PanelId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Duration"),
                Range(1, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public int Duration { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "BeginsOn"),
                DataType(DataType.DateTime,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "DateTimeExpected"),
                //DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true),
            ]
            public Nullable<System.DateTime> BeginsOn { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "EndsOn"),
                DataType(DataType.DateTime,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "DateTimeExpected"),
                //DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true),
            ]
            public Nullable<System.DateTime> EndsOn { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Sort"), 
            ]
            public Nullable<int> Sort { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Clock"), 
            ]
            public virtual Clock Clock { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Panel"), 
            ]
            public virtual Panel Panel { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Memo"), 
            ]
            public virtual Memo Memo { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "News"), 
            ]
            public virtual News News { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Picture"), 
            ]
            public virtual Picture Picture { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Report"), 
            ]
            public virtual Report Report { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Video"), 
            ]
            public virtual Video Video { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Weather"), 
            ]
            public virtual Weather Weather { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Locations"), 
            ]
            public virtual ICollection<Location> Locations { get; set; }
        }
  

        public enum FrameTypes
        {
            Clock,
            Memo,
            //News,
            Picture,
            Report,
            Video,
            Weather,
        }

        [
            Display(ResourceType = typeof(Resources), Name = "FrameType"),
        ]
        public virtual FrameTypes? FrameType
        {
            get
            {
                if (this.Clock != null) return FrameTypes.Clock;
                if (this.Memo != null) return FrameTypes.Memo;
                //if (this.News != null) return FrameTypes.News;
                if (this.Picture != null) return FrameTypes.Picture;
                if (this.Report != null) return FrameTypes.Report;
                if (this.Video != null) return FrameTypes.Video;
                if (this.Weather != null) return FrameTypes.Weather;
                return null;
            }

            set { }
        }

        public string TranslatedType { get { return ((FrameTypes)this.FrameType).Translate(); } }

        public static Expression<Func<Frame, bool>> FilterByFrameType(FrameTypes? frameType)
        {
            switch (frameType)
            {
                case FrameTypes.Clock:
                    return (frame => frame.Clock != null);
                case FrameTypes.Memo:
                    return (frame => frame.Memo != null);
                //case FrameTypes.News:
                //    return (frame => frame.News != null);
                case FrameTypes.Picture:
                    return (frame => frame.Picture != null);
                case FrameTypes.Report:
                    return (frame => frame.Report != null);
                case FrameTypes.Video:
                    return (frame => frame.Video != null);
                case FrameTypes.Weather:
                    return (frame => frame.Weather != null);
                default:
                    return (frame => true);
            }
        }

        public bool ShowDuration
        {
            get { return this.Clock == null && this.Weather == null; }
        }
    }

    public class FrameSelector : Frame
    {
        [
            Display(ResourceType = typeof(Resources), Name = "FrameType"),
            Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "FrameTypeRequired"),
        ]
        public override FrameTypes? FrameType { get; set; }
    }

    public class LocationSelector : Frame
    {
        [  Display(ResourceType = typeof(Resources), Name = "ID"),
           Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "IDRequired"),
        ]
        public int LocationId { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "LocationName"),
            Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "LocationNameRequired"),
        ]
        public string LocationName { get; set; }
    }

    [
        MetadataType(typeof(Memo.Annotations))
    ]
    public partial class Memo
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int FrameId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Subject"),
            ]
            public string Subject { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Body"),
            ]
            public string Body { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Frame"),
            ]
            public virtual Frame Frame { get; set; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "ShortBody"),
        ]
        public string ShortBody 
        {
            get
            {
                return this.Body.Length > 100 ? this.Body.Substring(0, 100) + "..." : this.Body;
            }
        }
    }

    [
        MetadataType(typeof(Content.Annotations))
    ]
    public partial class Content
    {
        internal class Annotations
        {
            [
               Display(ResourceType = typeof(Resources), Name = "ID"), 
            ]
            public int ContentId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "MediaSource"),
                DataType(DataType.Upload),
            ]
            public byte[] Data { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Type"), 
            ]
            public ContentTypes Type { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Canvas"), 
            ]
            public virtual ICollection<Canvas> Canvas { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Pictures"), 
            ]
            public virtual ICollection<Picture> Pictures { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Videos"), 
            ]
            public virtual ICollection<Video> Videos { get; set; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Size"),
        ]
        public Nullable<int> Size { get; set; }
    }

    public partial class ContentWithSize
    {
        public int ContentId { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "Size"),
        ]
        public Nullable<int> Size { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "Type"),
        ]
        public ContentTypes Type { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "Name"),
        ]
        public string Name { get; set; }
    }

    [
        MetadataType(typeof(ReportServer.Annotations))
    ]
    public partial class ReportServer
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int ServerId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "BaseURL"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "BaseURLRequired"),
            ]
            public string BaseUrl { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "User"),
            ]
            public string User { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Password"),
            ]
            public byte[] Password { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Domain"),
            ]
            public string Domain { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Reports"),
            ]
            public virtual ICollection<Report> Reports { get; set; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Password"),
            DataType(DataType.Password),
        ]
        public string PasswordUnmasked
        {
            get
            {
                if (this.Password == null) 
                    return null;
                else
                    return RsaUtil.Decrypt(this.Password);
            }
            
            set
            {
                if (value == null)
                    this.Password = null;
                else
                    this.Password = RsaUtil.Encrypt(value);
            }
        }
    }

    [
        MetadataType(typeof(Report.Annotations))
    ]
    public partial class Report
    {
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
                Display(ResourceType = typeof(Resources), Name = "ReportPath"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ReportPathRequired"),
            ]
            public string Path { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "RenderMode"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "RenderModeRequired"),
            ]
            public RenderModes Mode { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Server"),
            ]
            public int ServerId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Frame"),
            ]
            public virtual Frame Frame { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ReportServer"),
            ]
            public virtual ReportServer ReportServer { get; set; }
        }
    }

    [
        MetadataType(typeof(Clock.Annotations))
    ]
    public partial class Clock
    {
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
                Display(ResourceType = typeof(Resources), Name = "Frame"),
            ]
            public virtual Frame Frame { get; set; }
        }

        public void SetDefaultDuration(int value = 3600)
        {
            if (Frame != null)
                Frame.Duration = value;
        }
    }

    [
        MetadataType(typeof(Weather.Annotations))
    ]
    public partial class Weather
    {
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
                Display(ResourceType = typeof(Resources), Name = "Frame"),
            ]
            public virtual Frame Frame { get; set; }
        }

        public void SetDefaultDuration(int value = 600)
        {
            if (Frame != null)
                Frame.Duration = value;
        }
    }

    [
        MetadataType(typeof(Picture.Annotations))
    ]
    public partial class Picture
    {
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

            [
                Display(ResourceType = typeof(Resources), Name = "Frame"),
            ]
            public virtual Frame Frame { get; set; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "MediaOptions"),
        ]
        public Nullable<int> SavedContentId { get; set; }

        public static string[] SupportedFormats = new string[] {
            "BMP", "GIF", "JPG", "JPEG", "PNG", "TIF", "TIFF"
        };
    }

    [
        MetadataType(typeof(Video.Annotations))
    ]
    public partial class Video
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int FrameId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "PlayMuted"),
            ]
            public bool PlayMuted { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "AutoLoop"),
            ]
            public bool AutoLoop { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "LinkedMedia"),
            ]
            public virtual ICollection<Content> Contents { get; set; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "MediaOptions"),
        ]
        public Nullable<int> SavedContentId { get; set; }

        public static string[] SupportedFormats = new string[] {
            "AVI", "MP4", "MPG", "MPEG", "OGG", "WEBM"
        };
    }

    [
        MetadataType(typeof(Display.Annotations))
    ]
    public partial class Display
    {
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
                Display(ResourceType = typeof(Resources), Name = "Canvas"),
            ]
            public virtual Canvas Canvas { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Location"),
            ]
            public virtual Location Location { get; set; }
        }
    }

    [
        MetadataType(typeof(Setting.Annotations))
    ]
    public partial class Setting
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "Key"),
            ]
            public System.Guid Key { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Value"),
            ]
            public byte[] Value { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Type"),
            ]
            public SettingTypes Type { get; set; }
        }

        public static Guid Key_MaxImageSize { get; private set; }
        public static Guid Key_MaxVideoSize { get; private set; }
        public static Guid Key_PresentationSite { get; private set; }

        static Setting()
        {
            Key_MaxImageSize = new Guid("9A0BC012-FF01-4103-8A75-A03B275B0AD1");
            Key_MaxVideoSize = new Guid("4CAB57C4-EFEF-4EDE-91A3-EFFD48660909");
            Key_PresentationSite = new Guid("417D856B-7EC4-4CBD-A5EA-47BFC0F7B1F9");
        }
        
        [
            Display(ResourceType = typeof(Resources), Name = "Setting"),
        ]
        public string Name
        {
            get
            {
                if (this.Key == Key_MaxImageSize)
                    return Resources.Settings_MaxImageSize;

                if (this.Key == Key_MaxVideoSize)
                    return Resources.Settings_MaxVideoSize;

                if (this.Key == Key_PresentationSite)
                    return Resources.Settings_PresentationSite;

                return this.Key.ToString();
            }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            Required(ErrorMessageResourceType = typeof(Resources),
                ErrorMessageResourceName = "PositiveIntegerRequired"),
            Range(0, Int32.MaxValue,
                ErrorMessageResourceType = typeof(Resources),
                ErrorMessageResourceName = "PositiveIntegerRequired"),
            DisplayFormat(ApplyFormatInEditMode = false, 
                DataFormatString = "{0:0,###}"),
        ]
        public int IntValuePositive
        {
            get { return this.Value == null ? 0 : BitConverter.ToInt32(this.Value.Reverse().ToArray(), 0); }
            set { this.Value = BitConverter.GetBytes(value).Reverse().ToArray(); }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
        ]
        public string StringValue
        {
            get { return this.Value == null ? null : Encoding.ASCII.GetString(this.Value); }
            set { this.Value = value == null ? null : Encoding.ASCII.GetBytes(value); }
        }
    }






    #region Sundry


    public static class LinqExtension
    {
        public static IEnumerable<T> SelfAndChildren<T>(
            this T self,
            Func<T, IEnumerable<T>> children
            )
        {
            yield return self;
            IEnumerable<T> elements = children(self);
            foreach (T c in elements.SelectMany(e => e.SelfAndChildren(children)))
            {
                yield return c;
            }
        }
    }







    /*public partial class DisplayMonkeyEntities
    {
        static void ObjectContext_SavingChanges(object sender, EventArgs args)
        {
            ObjectContext context = sender as ObjectContext;
            if (context != null)
            {
                foreach (ObjectStateEntry e in context.ObjectStateManager.GetObjectStateEntries(EntityState.Modified))
                {
                    Type type = e.Entity.GetType();
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        var test = property.GetCustomAttributes();
                        
                        var metaDataList = property.GetCustomAttributes(typeof(MetadataTypeAttribute), false);
                        foreach (MetadataTypeAttribute metaData in metaDataList)
                        {
                            properties = metaData.MetadataClassType.GetProperties();
                            foreach (PropertyInfo subproperty in properties)
                            {
                                var attributes = subproperty.GetCustomAttributes(typeof(NeverUpdateAttribute), false);
                                if (attributes.Length > 0)
                                    e.RejectPropertyChanges(property.Name);
                            }
                        }
                    }
                }
            }
        }
    }

    public class NeverUpdateAttribute : System.Attribute
    {
    }*/

    #endregion
}