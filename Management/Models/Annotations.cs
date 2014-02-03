using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
//using System.Text;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Objects;
using System.Reflection;


namespace DisplayMonkey.Models
{
    [MetadataType(typeof(Level.Annotations))]
    public partial class Level 
    {
        internal sealed class Annotations
        {
            [
                Display(Name = "ID")
            ]
            public int LevelId { get; set; }

            [
                Required,
                StringLength(100),
            ]
            public string Name { get; set; }
        }
    }

    [MetadataType(typeof(Location.Annotations))]
    public partial class Location
    {
        internal sealed class Annotations
        {
            [
                Required,
            ]
            public int LevelId { get; set; }

            [
                Required,
                StringLength(100),
            ]
            public string Name { get; set; }

            [
                StringLength(1),
                Display(Name = "Temperature unit")
            ]
            public string TemperatureUnit { get; set; }

            [
                Display(Name = "Date format")
            ]
            public string DateFormat { get; set; }

            [
                Display(Name = "Time format")
            ]
            public string TimeFormat { get; set; }

            [
                Display(Name = "GMT offset, hours")
            ]
            public Nullable<int> OffsetGMT { get; set; }
        }

        public IEnumerable<Location> SelfAndChildren
        {
            get { return this.SelfAndChildren(l => l.Sublocations); }
        }
    }

    [MetadataType(typeof(Canvas.Annotations))]
    public partial class Canvas
    {
        internal sealed class Annotations
        {
            [
                Required,
                StringLength(100),
            ]
            public string Name { get; set; }

            [
                Range(1, Int32.MaxValue, ErrorMessage = "Please enter a valid positive integer number")
            ]
            public int Height { get; set; }

            [
                Range(1, Int32.MaxValue, ErrorMessage = "Please enter a valid positive integer number")
            ]
            public int Width { get; set; }

            [
                StringLength(20),
                Display(Name = "Back color")
            ]
            public string BackgroundColor { get; set; }

            [
                Display(Name = "Background")
            ]
            public Nullable<int> BackgroundImage { get; set; }
        }
    }

    [MetadataType(typeof(Panel.Annotations))]
    public partial class Panel
    {
        public bool IsFullscreen
        {
            get 
            { 
                return this.Canvas != null && this.Canvas.FullScreen != null 
                    ? this.PanelId == this.Canvas.FullScreen.PanelId 
                    : false; 
            }
        }
        
        internal sealed class Annotations
        {
            [
                Required
            ]
            public int CanvasId { get; set; }

            [
                Range(0, Int32.MaxValue, ErrorMessage = "Please enter a valid positive integer number"),
            ]
            public int Top { get; set; }

            [
                Range(0, Int32.MaxValue, ErrorMessage = "Please enter a valid positive integer number"),
            ]
            public int Left { get; set; }

            [
                Range(1, Int32.MaxValue, ErrorMessage = "Please enter a valid positive integer number"),
            ]
            public int Height { get; set; }

            [
                Range(1, Int32.MaxValue, ErrorMessage = "Please enter a valid positive integer number"),
            ]
            public int Width { get; set; }

            [
                Required,
                StringLength(100),
            ]
            public string Name { get; set; }
        }
    }

    [MetadataType(typeof(FullScreen.Annotations))]
    public partial class FullScreen
    {
        internal sealed class Annotations
        {
            [
                Range(0, Int32.MaxValue, ErrorMessage = "Please enter a valid positive integer number"),
                Display(Name = "Max recurrence interval")
            ]
            public Nullable<int> MaxIdleInterval { get; set; }
        }
    }

    [MetadataType(typeof(Frame.Annotations))]
    public partial class Frame
    {
        public static Expression<Func<Frame, bool>> FilterByFrameType(string frameType)
        {
            switch (frameType)
            {
                case "Clock":
                    return (frame => frame.Clock != null);
                case "Memo":
                    return (frame => frame.Memo != null);
                /*case "News":
                    return (frame => frame.News != null);*/
                case "Picture":
                    return (frame => frame.Picture != null);
                case "Report":
                    return (frame => frame.Report != null);
                case "Video":
                    return (frame => frame.Video != null);
                case "Weather":
                    return (frame => frame.Weather != null);
                default:
                    return (frame => true);
            }
        }

        [
            Display(Name = "Frame type")
        ]
        public virtual string FrameType
        {
            get 
            {
                if (this.Clock != null) return "Clock";
                if (this.Memo != null) return "Memo";
                //if (this.News != null) return "News";
                if (this.Picture != null) return "Picture";
                if (this.Report != null) return "Report";
                if (this.Video != null) return "Video";
                if (this.Weather != null) return "Weather";
                return "Unknown";
            }

            set { }
        }

        public bool ShowDuration
        {
            get { return this.Clock == null && this.Weather == null; }
        }

        public static object[] FrameTypes = new[] 
        {
            new  {FrameType = "Clock"},
            new  {FrameType = "Memo"},
            //new  {FrameType = "News"},
            new  {FrameType = "Picture"},
            new  {FrameType = "Report"},
            new  {FrameType = "Video"},
            new  {FrameType = "Weather"},
        };
        
        internal sealed class Annotations
        {
            [
                Range(1, Int32.MaxValue, ErrorMessage = "Number must be greater than 1"),
            ]
            public int Duration { get; set; }

            [
                Display(Name = "Begins on"),
                DataType(DataType.DateTime),
                //DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true),
            ]
            public Nullable<System.DateTime> BeginsOn { get; set; }

            [
                Display(Name = "Expires on"),
                DataType(DataType.DateTime),
                //DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true),
            ]
            public Nullable<System.DateTime> EndsOn { get; set; }
        }
    }

    public class FrameSelector : Frame
    {
        [
            Display(Name = "Frame type"),
            Required
        ]
        public override string FrameType { get; set; }
    }

    public class LocationSelector : Frame
    {
        [
            Display(Name = "Location"),
            Required
        ]
        public int LocationId { get; set; }

        [
            Display(Name = "Location name"),
            Required
        ]
        public string LocationName { get; set; }
    }

    [MetadataType(typeof(Memo.Annotations))]
    public partial class Memo
    {
        [
            Display(Name = "Short body")
        ]
        public string ShortBody 
        {
            get
            {
                return this.Body.Length > 100 ? this.Body.Substring(0, 100) + "..." : this.Body;
            }
        }

        internal sealed class Annotations
        {
        }
    }

    [MetadataType(typeof(Content.Annotations))]
    public partial class Content
    {
        public const int ContentType_Picture = 0;
        public const int ContentType_Video = 1;

        public class ContentType
        {
            public int Type { get; set; }
            public string Name { get; set; }
        }

        public static Content.ContentType [] ContentTypes = new [] 
        {
            new Content.ContentType {Type = ContentType_Picture, Name = "Picture"},
            new Content.ContentType {Type = ContentType_Video, Name = "Video"},
        };

        public const int RenderMode_Crop = 0;
        public const int RenderMode_Stretch = 1;
        public const int RenderMode_Fit = 2;

        public class RenderMode
        {
            public int Mode { get; set; }
            public string Name { get; set; }
        }

        public static Content.RenderMode [] RenderModes = new [] 
        {
            new Content.RenderMode {Mode = RenderMode_Crop, Name = "Crop"},
            new Content.RenderMode {Mode = RenderMode_Stretch, Name = "Stretch"},
            new Content.RenderMode {Mode = RenderMode_Fit, Name = "Fit"},
        };

        [
            Display(Name = "Size")
        ]
        public Nullable<int> Size { get; set; }
        
        internal sealed class Annotations
        {
            [
                DataType(DataType.Upload),
                Display(Name = "Media source")
            ]
            public byte[] Data { get; set; }

            [
                Required,
                StringLength(100),
            ]
            public string Name { get; set; }
        }
    }

    public partial class ContentWithSize
    {
        public int ContentId { get; set; }

        [
            Display(Name = "Size")
        ]
        public Nullable<int> Size { get; set; }

        [
            Display(Name = "Type")
        ]
        public int Type { get; set; }

        [
            Display(Name = "Name")
        ]
        public string Name { get; set; }
    }

    [MetadataType(typeof(ReportServer.Annotations))]
    public partial class ReportServer
    {
        internal sealed class Annotations
        {
            [
                Required,
                StringLength(100),
            ]
            public string Name { get; set; }

            [
                Display(Name = "Base URL"),
                Required
            ]
            public string BaseUrl { get; set; }

            [
                Display(Name = "Account")
            ]
            public string User { get; set; }
        }

        [
            DataType(DataType.Password),
            Display(Name = "Password")
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

    [MetadataType(typeof(Report.Annotations))]
    public partial class Report
    {
        internal sealed class Annotations
        {
            [
                Required,
                StringLength(100),
            ]
            public string Name { get; set; }

            [
                Display(Name = "Report path"),
                Required
            ]
            public string Path { get; set; }

            [
                Display(Name = "Render mode"),
                Required
            ]
            public int Mode { get; set; }

            [
                Display(Name = "Report server")
            ]
            public int ServerId { get; set; }
        }
    }

    [MetadataType(typeof(Clock.Annotations))]
    public partial class Clock
    {
        public const int ClockType_Text = 0;
        public const int ClockType_Analog = 1;
        //public const int ClockType_Digital = 2;

        public class ClockType
        {
            public int Type { get; set; }
            public string Name { get; set; }
        }

        public static Clock.ClockType[] ClockTypes = new [] 
        {
            new Clock.ClockType {Type = ClockType_Text, Name = "Text"},
            new Clock.ClockType {Type = ClockType_Analog, Name = "Analog"},
            //new Clock.ClockType {Type = ClockType_Digital, Name = "Digital"},
        };

        public void SetDefaultDuration(int value = 3600)
        {
            if (Frame != null)
                Frame.Duration = value;
        }

        internal sealed class Annotations
        {
            [
                Display(Name = "Clock type"),
                Required
            ]
            public int Type { get; set; }

            [
                Display(Name = "Show date")
            ]
            public bool ShowDate { get; set; }

            [
                Display(Name = "Show time")
            ]
            public bool ShowTime { get; set; }
        }
    }

    [MetadataType(typeof(Weather.Annotations))]
    public partial class Weather
    {
        public const int WeatherType_Current = 0;
        //public const int WeatherType_Week = 1;

        public class WeatherType
        {
            public int Type { get; set; }
            public string Name { get; set; }
        }

        public static Weather.WeatherType[] WeatherTypes = new[] 
        {
            new Weather.WeatherType {Type = WeatherType_Current, Name = "Current conditions"},
            //new Weather.WeatherType {Type = WeatherType_Week, Name = "One week forecast"},
        };

        public void SetDefaultDuration(int value = 600)
        {
            if (Frame != null)
                Frame.Duration = value;
        }

        internal sealed class Annotations
        {
            [
                Display(Name = "Display type"),
                Required
            ]
            public int Type { get; set; }
        }
    }

    [MetadataType(typeof(Picture.Annotations))]
    public partial class Picture
    {
        [
            Display(Name = "Media options")
        ]
        public Nullable<int> SavedContentId { get; set; }

        public static string[] SupportedFormats = new string[] {
            "BMP", "GIF", "JPG", "JPEG", "PNG", "TIF", "TIFF"
        };

        internal sealed class Annotations
        {
            [
                Display(Name = "Render mode")
            ]
            public int Mode { get; set; }

            [
                Display(Name = "Linked media")
            ]
            public int ContentId { get; set; }
        }
    }

    [MetadataType(typeof(Video.Annotations))]
    public partial class Video
    {
        [
            Display(Name = "Media options")
        ]
        public Nullable<int> SavedContentId { get; set; }

        public static string[] SupportedFormats = new string[] {
            "AVI", "MP4", "MPG", "MPEG", "OGG", "WEBM"
        };

        internal class Annotations
        {
            [
                Display(Name = "Play muted")
            ]
            public bool PlayMuted { get; set; }

            [
                Display(Name = "Auto-loop")
            ]
            public bool AutoLoop { get; set; }

            [
                Display(Name = "Linked media")
            ]
            public virtual ICollection<Content> Contents { get; set; }
        }
    }

    [MetadataType(typeof(Display.Annotations))]
    public partial class Display
    {
        internal class Annotations
        {
            [
                Required,
                StringLength(100),
            ]
            public string Name { get; set; }
            
            [
                Display(Name = "IP address")
            ]
            public string Host { get; set; }
        }
    }










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
}