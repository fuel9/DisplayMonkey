/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

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
using System.Data.Entity.Core.Objects;
using System.Reflection;

using Microsoft.Exchange.WebServices.Data;

using DisplayMonkey.Language;
using System.Text.RegularExpressions;
using System.Collections;


namespace DisplayMonkey.Models
{
    public static class Constants
    {
        public const string PasswordMask = "****************";
        public const string EmailMask = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
    }

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

        [
            Display(ResourceType = typeof(Resources), Name = "FadeLength"),
            Range(0, Int32.MaxValue,
                ErrorMessageResourceType = typeof(Resources),
                ErrorMessageResourceName = "PositiveIntegerRequired"),
        ]
        public double FadeLength 
        { 
            get; 
            set; 
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
                return Constants.PasswordMask;
            }

            set
            {
                PasswordSet = true;
                if (value == null)
                    this.Password = null;
                else if (value != Constants.PasswordMask)
                    this.Password = RsaUtil.Encrypt(value);
                else
                    PasswordSet = false;
            }
        }

        [
            NotMapped,
        ]
        public bool PasswordSet { get; private set; }
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
                RegularExpression(Constants.EmailMask,
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
                return Constants.PasswordMask;
            }

            set
            {
                PasswordSet = true;
                if (value == null)
                    this.Password = null;
                else if (value != Constants.PasswordMask)
                    this.Password = RsaUtil.Encrypt(value);
                else
                    PasswordSet = false;
            }
        }

        [
            NotMapped,
        ]
        public bool PasswordSet { get; private set; }
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
            public byte[] RawValue { get; set; }

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

        public string ResourceId
        {
            get { return _keyRes[this.Key]; }
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
            get { return this.RawValue == null ? 0 : BitConverter.ToInt32(this.RawValue.Reverse().ToArray(), 0); }
            set { this.RawValue = BitConverter.GetBytes(value).Reverse().ToArray(); }
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
            Required(ErrorMessageResourceType = typeof(Resources),
                ErrorMessageResourceName = "DecimalRequired"),
            DisplayFormat(ApplyFormatInEditMode = false,
                DataFormatString = "{0:F}"),
        ]
        public double DecimalValue
        {
            get 
            { 
                byte [] v = new byte[8];
                if (this.RawValue != null)
                    Array.Copy(this.RawValue.Reverse().ToArray(), v, 8);
                return BitConverter.ToDouble(v, 0); 
            }
            set 
            {
                this.RawValue = BitConverter.GetBytes(value).Reverse().ToArray();
            }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            Required(ErrorMessageResourceType = typeof(Resources),
                ErrorMessageResourceName = "PositiveDecimalRequired"),
            Range(0, Double.MaxValue,
                ErrorMessageResourceType = typeof(Resources),
                ErrorMessageResourceName = "PositiveDecimalRequired"),
            DisplayFormat(ApplyFormatInEditMode = false,
                DataFormatString = "{0:F}"),
        ]
        public double DecimalValuePositive
        {
            get { return this.DecimalValue; }
            set { this.DecimalValue = value; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            StringLength(255, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
        ]
        public string StringValue
        {
            get 
            {
                if (this.RawValue == null)
                {
                    return null;
                }
                else
                {
                    return System.Text.Encoding.Unicode.GetString(this.RawValue).TrimEnd('\0');
                }
            }
            set 
            {
                if (value == null)
                {
                    this.RawValue = null;
                }
                else
                {
                    byte[] buf = System.Text.Encoding.Unicode.GetBytes(value);
                    if (buf.Length < 8)
                    {
                        byte[] buf2 = new byte[8];
                        buf.CopyTo(buf2, 0);
                        buf = buf2;
                    }
                    this.RawValue = buf;
                }
            }
        }

        #endregion
    }
}