using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
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
                byte[] v = new byte[8];
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

        #region -----  Key Identification  -----

        public enum Keys : int
        {
            MaxImageSize = 0,                       // max allowed size of uploaded image, Bytes
            MaxVideoSize,                           // max allowed size of uploaded video, Bytes
            PresentationSite,                       // URL root of the nearest DMP site when navigating to displays from DMM

            DefaultDisplayReadyEventTimeout,        // timeout interval to wait till a frame reports itself "ready" so as to smoothly continue presentation, sec, RC10
            DefaultDisplayPollInterval,             // display poll interval for hash sum check and panel idle length, sec, RC13
            DefaultDisplayErrorLength,              // default length for display errors, sec, RC13

            DefaultPanelFadeLength,                 // default panel frame fade/transition length, sec, RC13
            DefaultFullPanelFadeLength,             // default full screen panel frame fade/transition length, sec, RC13

            // default template by frame type, RC10
            DefaultTemplateClock,
            DefaultTemplateHtml,
            DefaultTemplateMemo,
            //DefaultTemplateNews,
            DefaultTemplateOutlook,
            DefaultTemplatePicture,
            DefaultTemplatePowerbi,                 // 1.2
            DefaultTemplateReport,
            DefaultTemplateVideo,
            DefaultTemplateWeather,
            DefaultTemplateYouTube,

            // default cache interval by frame type, min, RC10
            //DefaultCacheIntervalClock,
            //DefaultCacheIntervalHtml,
            //DefaultCacheIntervalMemo,
            //DefaultCacheIntervalNews,
            DefaultCacheIntervalOutlook,
            DefaultCacheIntervalPicture,
            DefaultCacheIntervalReport,
            DefaultCacheIntervalVideo,
            DefaultCacheIntervalWeather,
            //DefaultCacheIntervalYouTube,


            /// <summary>
            /// end with Count
            /// </summary>
            Count
        }

        private static Guid[] _keyGuids = new Guid[(int)Keys.Count];
        private static Dictionary<Guid, string> _keyRes = new Dictionary<Guid, string>((int)Keys.Count);

        static Setting()
        {
            //_keyRes.Add(
            //    _keyGuids[(int)Keys.ZZZ] = new Guid("12345678-90AB-CDEF-1234-567890ABCDEF"),
            //    "Settings_ZZZ"
            //    );

            // ---- General defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.MaxImageSize] = new Guid("9A0BC012-FF01-4103-8A75-A03B275B0AD1"),
                "Settings_MaxImageSize"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.MaxVideoSize] = new Guid("4CAB57C4-EFEF-4EDE-91A3-EFFD48660909"),
                "Settings_MaxVideoSize"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.PresentationSite] = new Guid("417D856B-7EC4-4CBD-A5EA-47BFC0F7B1F9"),
                "Settings_PresentationSite"
                );

            // ---- Display defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultDisplayReadyEventTimeout] = new Guid("3AC645E8-30B9-4473-A78C-69DBC4BFFAA6"),
                "Settings_DefaultDisplayReadyEventTimeout"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultDisplayPollInterval] = new Guid("7C993AC3-2E15-44DD-84B3-D13935BD1E43"),
                "Settings_DefaultDisplayPollInterval"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultDisplayErrorLength] = new Guid("2F43071D-C314-4C78-8438-76B474364258"),
                "Settings_DefaultDisplayErrorLength"
                );

            // ---- Panel defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultPanelFadeLength] = new Guid("6865C5AE-8A91-4A44-B03A-EA81F69D6539"),
                "Settings_DefaultPanelFadeLength"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultFullPanelFadeLength] = new Guid("9A4989FC-4C2F-49F3-AD60-E120AD8F85FE"),
                "Settings_DefaultFullPanelFadeLength"
                );

            // ---- Frame defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateClock] = new Guid("1D715B8D-2377-4FC6-B2A1-D025A4E48A56"),
                "Settings_DefaultTemplateClock"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateHtml] = new Guid("2409F3DB-06C7-4E16-A903-3D76FD0DB45A"),
                "Settings_DefaultTemplateHtml"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateMemo] = new Guid("366AC17F-C163-4F28-B233-EDA6B15A8142"),
                "Settings_DefaultTemplateMemo"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateOutlook] = new Guid("356FB4FA-C2B4-4A52-9420-1364C645F573"),
                "Settings_DefaultTemplateOutlook"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplatePicture] = new Guid("5565E940-0C32-4C6D-848D-96F99E8AE947"),
                "Settings_DefaultTemplatePicture"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplatePowerbi] = new Guid("0616083B-9FC4-4510-B3B3-519CD65CD5E4"),
                "Settings_DefaultTemplatePowerbi"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateReport] = new Guid("411DEF31-E1D6-41E9-A299-81D345EC1AB7"),
                "Settings_DefaultTemplateReport"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateVideo] = new Guid("4FA531DC-9957-4182-946D-3C8F3213057D"),
                "Settings_DefaultTemplateVideo"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateWeather] = new Guid("21BDCC5D-753B-47AF-A492-0FC32D90A335"),
                "Settings_DefaultTemplateWeather"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateYouTube] = new Guid("CAFCC975-8B0C-4027-8344-5719FCB0A213"),
                "Settings_DefaultTemplateYouTube"
                );

            // ---- Frame cache defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalOutlook] = new Guid("C12A15AA-3C10-407E-8368-A2397C7625BB"),
                "Settings_DefaultCacheIntervalOutlook"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalPicture] = new Guid("29851A7E-9F53-4759-98FF-F70AC0FD5A74"),
                "Settings_DefaultCacheIntervalPicture"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalReport] = new Guid("F1AFB295-75FB-4833-BE21-7DB5ED55ED92"),
                "Settings_DefaultCacheIntervalReport"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalVideo] = new Guid("32F35FC7-18CD-4C0B-82E2-FE584D6BE0C8"),
                "Settings_DefaultCacheIntervalVideo"
                );

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalWeather] = new Guid("C7C63812-2E51-42EF-A0C0-9258E48A5FF8"),
                "Settings_DefaultCacheIntervalWeather"
                );
        }

        public static Setting GetSetting(DisplayMonkeyEntities _db, Setting.Keys _id)
        {
            Guid key = _keyGuids[(int)_id];
            return _db.Settings.FirstOrDefault(s => s.Key == key);
        }

        public static int GetDefaultTemplate(DisplayMonkeyEntities _db, FrameTypes? _frameType)
        {
            int ret = 0;
            Keys key = Keys.Count;
            switch (_frameType)
            {
                case FrameTypes.Clock: key = Keys.DefaultTemplateClock; break;
                case FrameTypes.Html: key = Keys.DefaultTemplateHtml; break;
                case FrameTypes.Memo: key = Keys.DefaultTemplateMemo; break;
                case FrameTypes.Outlook: key = Keys.DefaultTemplateOutlook; break;
                case FrameTypes.Picture: key = Keys.DefaultTemplatePicture; break;
                case FrameTypes.Powerbi: key = Keys.DefaultTemplatePowerbi; break;
                case FrameTypes.Report: key = Keys.DefaultTemplateReport; break;
                case FrameTypes.Video: key = Keys.DefaultTemplateVideo; break;
                case FrameTypes.Weather: key = Keys.DefaultTemplateWeather; break;
                case FrameTypes.YouTube: key = Keys.DefaultTemplateYouTube; break;
                default: return ret;
            }

            Setting defTemplate = GetSetting(_db, key);
            if (defTemplate != null)
            {
                string templateName = defTemplate.StringValue;
                ret = _db.Templates
                    .Where(t => t.Name == templateName && t.FrameType == _frameType)
                    .FirstOrDefault()
                    .TemplateId
                    ;
            }

            return ret;
        }

        public static int GetDefaultCacheInterval(DisplayMonkeyEntities _db, FrameTypes? _frameType)
        {
            int ret = 0;
            Keys key = Keys.Count;
            switch (_frameType)
            {
                //case FrameTypes.Clock:
                //case FrameTypes.Html:
                //case FrameTypes.Memo:
                case FrameTypes.Outlook: key = Keys.DefaultCacheIntervalOutlook; break;
                case FrameTypes.Picture: key = Keys.DefaultCacheIntervalPicture; break;
                //case FrameTypes.Powerbi:
                case FrameTypes.Report: key = Keys.DefaultCacheIntervalReport; break;
                case FrameTypes.Video: key = Keys.DefaultCacheIntervalVideo; break;
                case FrameTypes.Weather: key = Keys.DefaultCacheIntervalWeather; break;
                //case FrameTypes.YouTube:
                default: return ret;
            }

            Setting defCacheInt = Setting.GetSetting(_db, key);
            if (defCacheInt != null)
            {
                ret = defCacheInt.IntValuePositive;
            }

            return ret;
        }

        #endregion
    }
}