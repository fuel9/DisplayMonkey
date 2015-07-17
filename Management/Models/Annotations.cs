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
using System.Text.RegularExpressions;
using System.Collections;


namespace DisplayMonkey.Models
{
    public partial class TopContent
    {
        [
            Display(ResourceType = typeof(Resources), Name = "Count"),
        ]
        public int Count { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "Name"),
        ]
        public string Name { get; set; }
    }

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
        
        public virtual Canvas Canvas { get; set; }
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

        [
            NotMapped,
        ]
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
                /*Required(ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),*/
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
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PanelRequired"),
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
                //DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true),
            ]
            public Nullable<System.DateTime> BeginsOn { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "EndsOn"),
                DataType(DataType.DateTime,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "DateTimeExpected"),
                //DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true),
            ]
            public Nullable<System.DateTime> EndsOn { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Sort"), 
            ]
            public Nullable<int> Sort { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "DateCreated"),
            ]
            public System.DateTime DateCreated { get; protected set; }

            //public byte[] Version { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Template"),
            ]
            public int TemplateId { get; set; }
            
            [
                Display(ResourceType = typeof(Resources), Name = "CacheInterval"),
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public int CacheInterval { get; set; }     
       
            /**** frames ****/
            
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
               Display(ResourceType = typeof(Resources), Name = "Html"),
            ]
            public virtual Html Html { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Outlook"),
            ]
            public virtual Outlook Outlook { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Locations"), 
            ]
            public virtual ICollection<Location> Locations { get; set; }
        }
  
        public enum TimingOptions : int
        {
            TimingOption_Pending = 0,
            TimingOption_Current = 1,
            TimingOption_Expired = 2
        }

        [
            Display(ResourceType = typeof(Resources), Name = "FrameType"),
            NotMapped,
        ]
        public virtual FrameTypes? FrameType
        {
            get { return this.Template.FrameType; }
            set { }
        }

        [
            NotMapped,
        ]
        public string TranslatedType 
        { 
            get 
            { 
                return ((FrameTypes)this.FrameType).Translate(); 
            } 
        }

        public static Expression<Func<Frame, bool>> FilterByFrameType(FrameTypes? frameType)
        {
            if (frameType == null)
                return (f => true);
            else
                return (f => f.Template.FrameType == frameType.Value);
        }

        [
            NotMapped,
        ]
        public string Icon
        {
            get { return Frame.IconFromFrameType(this.FrameType); }
        }

        public static string IconFromFrameType(FrameTypes? _frameType)
        {
            if (_frameType.HasValue) switch (_frameType.Value)
            {
                case FrameTypes.Clock:
                    return "~/images/clock.png";

                case FrameTypes.Html:    
                    return "~/images/html.png";

                //case FrameTypes.News:
                //    return "~/images/news.png";

                case FrameTypes.Memo:
                    return "~/images/memo.png";

                case FrameTypes.Outlook:
                    return "~/images/calendar.png";

                case FrameTypes.Picture:
                    return "~/images/image.png";

                case FrameTypes.Report:
                    return "~/images/ssrs.png";

                case FrameTypes.Video:    
                    return "~/images/video_thmb.png";

                case FrameTypes.Weather:
                    return "~/images/weather.png";

                case FrameTypes.YouTube:   
                    return "~/images/youtube.png";
            }

            return "~/images/unknown.png";
        }
    }

    public class FrameSelector : Frame
    {
        [
            Display(ResourceType = typeof(Resources), Name = "FrameType"),
            Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "FrameTypeRequired"),
        ]
        public override FrameTypes? FrameType { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "Canvas"),
        ]
        public int CanvasId { get; set; }
    }

    public class LocationSelector : Frame
    {
        [  
            Display(ResourceType = typeof(Resources), Name = "ID"),
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
            NotMapped,
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
        MetadataType(typeof(Html.Annotations))
    ]
    public partial class Html
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
                Display(ResourceType = typeof(Resources), Name = "Content"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ContentRequired"),
            ]
            public string Content { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Frame"),
            ]
            public virtual Frame Frame { get; set; }
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
            NotMapped,
        ]
        public string PasswordUnmasked
        {
            get
            {
                //if (this.Password == null)
                //    return null;
                //else
                //    return RsaUtil.Decrypt(this.Password);
                return _pwdMask;
            }

            set
            {
                _passwordSet = true;
                if (value == null)
                    this.Password = null;
                else if (value != _pwdMask)
                    this.Password = RsaUtil.Encrypt(value);
                else
                    _passwordSet = false;
            }
        }

        private const string _pwdMask = "****************";
        public bool _passwordSet = false;
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
                Display(ResourceType = typeof(Resources), Name = "ShowSeconds"),
            ]
            public bool ShowSeconds { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Frame"),
            ]
            public virtual Frame Frame { get; set; }

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
        MetadataType(typeof(Youtube.Annotations))
    ]
    public partial class Youtube
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

    [
        MetadataType(typeof(ExchangeAccount.Annotations))
    ]
    public partial class ExchangeAccount
    {
        public void init(DisplayMonkeyEntities _db)
        {
            this.EwsVersion = OutlookEwsVersions.OutlookEwsVersion_Exchange2007_SP1;
            this.Url = "https://outlook.office365.com/EWS/Exchange.asmx";
        }

        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int AccountId { get; set; }
            
            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }
            
            [
                Display(ResourceType = typeof(Resources), Name = "Account"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AccountRequired"),
                RegularExpression(_emailMsk,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "EmailInvalid"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Account { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Password"),
            ]
            public byte[] Password { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "EwsVersion"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "EwsVersionRequired"),
            ]
            public OutlookEwsVersions EwsVersion { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "OutlookUrl"),
                StringLength(250, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Url { get; set; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Password"),
            DataType(DataType.Password),
            NotMapped,
        ]
        public string PasswordUnmasked
        {
            get
            {
                return _pwdMask;
            }

            set
            {
                _passwordSet = true;
                if (value == null)
                    this.Password = null;
                else if (value != _pwdMask)
                    this.Password = RsaUtil.Encrypt(value);
                else
                    _passwordSet = false;
            }
        }

        private const string _pwdMask = "****************";
        public bool _passwordSet = false;
        public const string _emailMsk = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
    }

    [
        MetadataType(typeof(Outlook.Annotations))
    ]
    public partial class Outlook
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int FrameId { get; set; }

            /*[
                Display(ResourceType = typeof(Resources), Name = "View"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ViewRequired"),
            ]
            public OutlookModes Mode { get; set; }*/

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                //Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ExchangeAccount"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ExchangeAccountRequired"),
            ]
            public int AccountId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Mailbox"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MailboxRequired"),
                RegularExpression(ExchangeAccount._emailMsk, 
                    ErrorMessageResourceType = typeof(Resources), 
                    ErrorMessageResourceName = "EmailInvalid"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Mailbox { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ShowEvents"),
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ShowEventsRequired"),
            ]
            public int ShowEvents { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ExchangeAccount"),
            ]
            public virtual ExchangeAccount ExchangeAccount { get; set; }
        }

        [
            NotMapped,
        ]
        public string NameOrMailboxOrAccount { get { return this.Name ?? this.Mailbox ?? this.ExchangeAccount.Name; } }
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
                Display(ResourceType = typeof(Resources), Name = "ShowErrors"),
            ]
            public bool ShowErrors { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "NoScroll"),
            ]
            public bool NoScroll { get; set; }

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
        }
    }

    [
        MetadataType(typeof(Template.Annotations))
    ]
    public partial class Template
    {
        internal class Annotations
        {
            //public int TemplateId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Content"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ContentRequired"),
            ]
            public string Html { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "FrameType"),
            ]
            public FrameTypes FrameType { get; set; }
            
            //public byte[] Version { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Frames"),
            ]
            public virtual ICollection<Frame> Frames { get; set; }
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

        #region -----  Value Management  -----
        [
            Display(ResourceType = typeof(Resources), Name = "Setting"),
        ]
        public string Name
        {
            get
            {
                return Resources.ResourceManager.GetString(ResourceId) ?? this.Key.ToString();
            }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            Required(ErrorMessageResourceType = typeof(Resources),
                ErrorMessageResourceName = "IntegerRequired"),
            DisplayFormat(ApplyFormatInEditMode = false,
                DataFormatString = "{0:N0}"),
        ]
        public int IntValue
        {
            get { return this.Value == null ? 0 : BitConverter.ToInt32(this.Value.Reverse().ToArray(), 0); }
            set { this.Value = BitConverter.GetBytes(value).Reverse().ToArray(); }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            Required(ErrorMessageResourceType = typeof(Resources),
                ErrorMessageResourceName = "PositiveIntegerRequired"),
            Range(0, Int32.MaxValue,
                ErrorMessageResourceType = typeof(Resources),
                ErrorMessageResourceName = "PositiveIntegerRequired"),
            DisplayFormat(ApplyFormatInEditMode = false, 
                DataFormatString = "{0:N0}"),
        ]
        public int IntValuePositive
        {
            get { return this.IntValue; }
            set { this.IntValue = value; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
        ]
        public string StringValue
        {
            get { return this.Value == null ? null : Encoding.Unicode.GetString(this.Value); }
            set { this.Value = value == null ? null : Encoding.Unicode.GetBytes(value); }
        }

        #endregion
    }
}