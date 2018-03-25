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
        }

        #region Public members

        [
            Display(ResourceType = typeof(Resources), Name = "Setting"),
            NotMapped,
        ]
        public string Name
        {
            get
            {
                return Resources.ResourceManager.GetString(_keyRes[this.Key].ResourceId) ?? this.Key.ToString();
            }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Description"),
            NotMapped,
        ]
        public string Description
        {
            get
            {
                string res = _keyRes[this.Key].ResourceId_Descr;
                if (res == null)
                    return "";
                return Resources.ResourceManager.GetString(res) ?? "";
            }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Group"),
            NotMapped,
        ]
        public string Group
        {
            get
            {
                string res = _keyRes[this.Key].ResourceId_Group;
                if (res == null)
                    return "";
                return Resources.ResourceManager.GetString(res) ?? "";
            }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Type"),
            NotMapped,
        ]
        public SettingTypes Type
        {
            get { return _keyRes[this.Key].Type; }
        }

        [
            NotMapped,
        ]
        public string ResourceId
        {
            get { return _keyRes[this.Key].ResourceId; }
        }

        [
            NotMapped,
        ]
        public string GroupResourceId
        {
            get { return _keyRes[this.Key].ResourceId_Group; }
        }

        [
            NotMapped,
        ]
        public int GroupOrder
        {
            get 
            { 
                switch (_keyRes[this.Key].ResourceId_Group)
                {
                    case "Settings_Group_General":
                        return 0;
                    case "Settings_Group_DisplayDefaults":
                        return 1;
                    case "Settings_Group_PanelDefaults":
                        return 2;
                    case "Settings_Group_FrameDefaults":
                        return 3;
                }

                return 100;
            }
        }

        [
            NotMapped,
        ]
        public bool Hidden  // 1.4.0
        {
            get
            {
                return _keyRes[this.Key].Hidden;
            }
        }

        #endregion

        #region -----  Value Management  -----

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            //Required(ErrorMessageResourceType = typeof(Resources),
            //    ErrorMessageResourceName = "IntegerRequired"),
            DisplayFormat(ApplyFormatInEditMode = false,
                DataFormatString = "{0:N0}"),
            NotMapped,
        ]
        public int IntValue
        {
            get { return this.RawValue == null ? 0 : BitConverter.ToInt32(this.RawValue.Reverse().ToArray(), 0); }
            set { this.RawValue = BitConverter.GetBytes(value).Reverse().ToArray(); }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            //Required(ErrorMessageResourceType = typeof(Resources),
            //    ErrorMessageResourceName = "PositiveIntegerRequired"),
            //(0, Int32.MaxValue,
            //    ErrorMessageResourceType = typeof(Resources),
            //    ErrorMessageResourceName = "PositiveIntegerRequired"),
            DisplayFormat(ApplyFormatInEditMode = false,
                DataFormatString = "{0:N0}"),
            NotMapped,
        ]
        public int IntValuePositive
        {
            get { return this.IntValue; }
            set { this.IntValue = value < 0 ? 0 : value; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            //Required(ErrorMessageResourceType = typeof(Resources),
            //    ErrorMessageResourceName = "DecimalRequired"),
            DisplayFormat(ApplyFormatInEditMode = false,
                DataFormatString = "{0:F}"),
            NotMapped,
        ]
        public double DecimalValue
        {
            get
            {
                byte[] v = new byte[8];
                if (this.RawValue != null)
                    Array.Copy(this.RawValue.Reverse().ToArray(), v, Math.Min(8, this.RawValue.Length));
                return BitConverter.ToDouble(v, 0);
            }
            set
            {
                this.RawValue = BitConverter.GetBytes(value).Reverse().ToArray();
            }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            //Required(ErrorMessageResourceType = typeof(Resources),
            //    ErrorMessageResourceName = "PositiveDecimalRequired"),
            //Range(0, Double.MaxValue,
            //    ErrorMessageResourceType = typeof(Resources),
            //    ErrorMessageResourceName = "PositiveDecimalRequired"),
            DisplayFormat(ApplyFormatInEditMode = false,
                DataFormatString = "{0:F}"),
            NotMapped,
        ]
        public double DecimalValuePositive
        {
            get { return this.DecimalValue; }
            set { this.DecimalValue = value < 0.0 ? 0.0 : value ; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Value"),
            StringLength(255, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            NotMapped,
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

        [
            NotMapped,
        ]
        public object AnyValue         // 1.4.0
        {
            set
            {
                switch (this.Type)
                {
                    case SettingTypes.SettingType_Int:
                        this.IntValue = (int)(value ?? 0);
                        break;

                    case SettingTypes.SettingType_IntPositive:
                        this.IntValuePositive = (int)(value ?? 0);
                        break;

                    case SettingTypes.SettingType_String:
                        this.StringValue = (string)value;
                        break;

                    case SettingTypes.SettingType_Decimal:
                        this.DecimalValue = (double)(value ?? 0.0);
                        break;

                    case SettingTypes.SettingType_DecimalPositive:
                        this.DecimalValuePositive = (double)(value ?? 0.0);
                        break;

                    case SettingTypes.SettingType_Binary:
                        this.RawValue = (byte[])value;
                        break;
                }
            }
        }

        #endregion

        #region ----- Util helpers -----

        public static Setting GetSetting(DisplayMonkeyEntities _db, Setting.Keys _id)
        {
            Guid key = _keyGuids[(int)_id];
            return _db.Settings.FirstOrDefault(s => s.Key == key);
        }

        public static void SaveSetting(DisplayMonkeyEntities _db, Setting.Keys _id, object _value)
        {
            Guid key = _keyGuids[(int)_id];
            Setting setting = _db.Settings.FirstOrDefault(s => s.Key == key);

            if (setting == null)
            {
                setting = new Setting() 
                { 
                    Key = key,
                    AnyValue = _value,
                };
                _db.Settings.Add(setting);
            }
            else
            {
                setting.AnyValue = _value;
                _db.Entry(setting).State = System.Data.Entity.EntityState.Modified;
            }

            _db.SaveChanges();
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

        public static string GetEwsTrackingPath(DisplayMonkeyEntities _db)
        {
            Setting s = Setting.GetSetting(_db, Keys.EWSTracingPath);
            if (s != null)
            {
                return s.StringValue;
            }

            return null;
        }

        public static IEncryptor GetEncryptor(DisplayMonkeyEntities _db)    // 1.4.0
        {
            EncryptionMethods encryptionMethod = EncryptionMethods.RsaContainer;
            IEncryptor encryptor = null;
            Setting s = Setting.GetSetting(_db, Keys.EncryptionMethod);
            if (s != null)
            {
                encryptionMethod = (EncryptionMethods)s.IntValuePositive;
            }

            switch (encryptionMethod)
            {
                case EncryptionMethods.RsaContainer:
                    encryptor = new RsaEncryptor();
                    break;

                case EncryptionMethods.Aes:
                    encryptor = new AesEncryptor();

                    s = Setting.GetSetting(_db, Keys.EncryptionIV);
                    if (s != null && s.RawValue != null)
                        (encryptor as AesEncryptor).IV = s.RawValue;
                    else
                        Setting.SaveSetting(_db, Keys.EncryptionIV, (encryptor as AesEncryptor).IV);

                    s = Setting.GetSetting(_db, Keys.EncryptionKey);
                    if (s != null && s.RawValue != null)
                        (encryptor as AesEncryptor).Key = s.RawValue;
                    else
                        Setting.SaveSetting(_db, Keys.EncryptionKey, (encryptor as AesEncryptor).Key);

                    break;
            }

            return encryptor;
        }

        #endregion

        #region -----  Properties  -----

        private class SettingProperties // 1.4.0
        {
            public string ResourceId { get; set; }
            public string ResourceId_Descr { get; set; }    // 1.4.0
            public string ResourceId_Group { get; set; }    // 1.4.0
            public SettingTypes Type { get; set; }          // 1.4.0
            public object DefaultValue { get; set; }        // 1.4.0
            public bool Hidden { get; set; }                // 1.4.0
        }

        public enum Keys : int
        {
            MaxImageSize = 0,                       // max allowed size of uploaded image, Bytes
            MaxVideoSize,                           // max allowed size of uploaded video, Bytes
            PresentationSite,                       // URL root of the nearest DMP site when navigating to displays from DMM
            EWSTracingPath,                         // EWS tracing path 1.3.2
            
            EncryptionMethod,                       // Encryption 1.4.0
            EncryptionIV,                           // Encryption 1.4.0
            EncryptionKey,                          // Encryption 1.4.0

            DefaultDisplayReadyEventTimeout,        // timeout interval to wait till a frame reports itself "ready" so as to smoothly continue presentation, sec, RC10
            DefaultDisplayPollInterval,             // display poll interval for hash sum check and panel idle length, sec, RC13
            DefaultDisplayErrorLength,              // default length for display errors, sec, RC13
            DisplayAutoLoadMode,                    // auto load mode 1.6.0

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
        private static Dictionary<Guid, SettingProperties> _keyRes = new Dictionary<Guid, SettingProperties>((int)Keys.Count);

        static Setting()
        {
            //_keyRes.Add(
            //    _keyGuids[(int)Keys.ZZZ] = new Guid("12345678-90AB-CDEF-1234-567890ABCDEF"),
            //    "Settings_ZZZ"
            //    );

            #region // ---- General defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.MaxImageSize] = new Guid("9A0BC012-FF01-4103-8A75-A03B275B0AD1"),
                new SettingProperties
                {
                    ResourceId = "Settings_MaxImageSize",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 0,
                    ResourceId_Group = "Settings_Group_General",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.MaxVideoSize] = new Guid("4CAB57C4-EFEF-4EDE-91A3-EFFD48660909"),
                new SettingProperties
                {
                    ResourceId = "Settings_MaxVideoSize",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 0,
                    ResourceId_Group = "Settings_Group_General",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.PresentationSite] = new Guid("417D856B-7EC4-4CBD-A5EA-47BFC0F7B1F9"),
                new SettingProperties
                {
                    ResourceId = "Settings_PresentationSite",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = null,
                    ResourceId_Group = "Settings_Group_General",
                    ResourceId_Descr = "Settings_PresentationSite_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.EWSTracingPath] = new Guid("9E31F16D-6974-4083-B2A9-CC25DECF7B16"),
                new SettingProperties
                {
                    ResourceId = "Settings_EWSTracingPath",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = null,
                    ResourceId_Group = "Settings_Group_Advanced",
                    ResourceId_Descr = "Settings_EWSTracingPath_Descr",
                });

            #endregion

            #region // ---- Encryption 1.4.0

            _keyRes.Add(
                _keyGuids[(int)Keys.EncryptionMethod] = new Guid("9C2F6600-1DC5-4F7B-9CDE-AF6C3FDC073B"),
                new SettingProperties
                {
                    ResourceId = "Settings_EncryptionMode",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 0,
                    ResourceId_Descr = "Settings_EncryptionMode_Descr",
                    ResourceId_Group = "Settings_Group_Advanced",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.EncryptionIV] = new Guid("8CA22FD4-2A2B-47D1-87A3-2B3E277E9DED"),
                new SettingProperties
                {
                    ResourceId = "Settings_EncryptionIV",
                    Type = SettingTypes.SettingType_Binary,
                    Hidden = true,
                    DefaultValue = null,
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.EncryptionKey] = new Guid("538D9121-5698-42D5-BA32-CD4548A100B3"),
                new SettingProperties
                {
                    ResourceId = "Settings_EncryptionKey",
                    Type = SettingTypes.SettingType_Binary,
                    Hidden = true,
                    DefaultValue = null,
                });

            #endregion

            #region // ---- Display defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultDisplayReadyEventTimeout] = new Guid("3AC645E8-30B9-4473-A78C-69DBC4BFFAA6"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultDisplayReadyEventTimeout",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 10,
                    ResourceId_Group = "Settings_Group_DisplayDefaults",
                    ResourceId_Descr = "Settings_DefaultDisplayReadyEventTimeout_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultDisplayPollInterval] = new Guid("7C993AC3-2E15-44DD-84B3-D13935BD1E43"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultDisplayPollInterval",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 60,
                    ResourceId_Group = "Settings_Group_DisplayDefaults",
                    ResourceId_Descr = "Settings_DefaultDisplayPollInterval_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultDisplayErrorLength] = new Guid("2F43071D-C314-4C78-8438-76B474364258"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultDisplayErrorLength",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 60,
                    ResourceId_Group = "Settings_Group_DisplayDefaults",
                    ResourceId_Descr = "Settings_DefaultDisplayErrorLength_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DisplayAutoLoadMode] = new Guid("AE1B2F10-9EC3-4429-97B5-C12D64575C41"),
                new SettingProperties
                {
                    ResourceId = "Settings_DisplayAutoLoadMode",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 0,
                    ResourceId_Group = "Settings_Group_DisplayDefaults",
                    ResourceId_Descr = "Settings_DisplayAutoLoadMode_Descr",
                });

            #endregion

            #region // ---- Panel defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultPanelFadeLength] = new Guid("6865C5AE-8A91-4A44-B03A-EA81F69D6539"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultPanelFadeLength",
                    Type = SettingTypes.SettingType_DecimalPositive,
                    DefaultValue = 0.25,
                    ResourceId_Group = "Settings_Group_PanelDefaults",
                    ResourceId_Descr = "Settings_DefaultPanelFadeLength_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultFullPanelFadeLength] = new Guid("9A4989FC-4C2F-49F3-AD60-E120AD8F85FE"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultFullPanelFadeLength",
                    Type = SettingTypes.SettingType_DecimalPositive,
                    DefaultValue = 1.0,
                    ResourceId_Group = "Settings_Group_PanelDefaults",
                    ResourceId_Descr = "Settings_DefaultPanelFadeLength_Descr",
                });

            #endregion

            #region // ---- Frame defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateClock] = new Guid("1D715B8D-2377-4FC6-B2A1-D025A4E48A56"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplateClock",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateHtml] = new Guid("2409F3DB-06C7-4E16-A903-3D76FD0DB45A"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplateHtml",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateMemo] = new Guid("366AC17F-C163-4F28-B233-EDA6B15A8142"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplateMemo",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateOutlook] = new Guid("356FB4FA-C2B4-4A52-9420-1364C645F573"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplateOutlook",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplatePicture] = new Guid("5565E940-0C32-4C6D-848D-96F99E8AE947"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplatePicture",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplatePowerbi] = new Guid("0616083B-9FC4-4510-B3B3-519CD65CD5E4"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplatePowerbi",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateReport] = new Guid("411DEF31-E1D6-41E9-A299-81D345EC1AB7"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplateReport",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateVideo] = new Guid("4FA531DC-9957-4182-946D-3C8F3213057D"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplateVideo",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateWeather] = new Guid("21BDCC5D-753B-47AF-A492-0FC32D90A335"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplateWeather",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultTemplateYouTube] = new Guid("CAFCC975-8B0C-4027-8344-5719FCB0A213"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultTemplateYouTube",
                    Type = SettingTypes.SettingType_String,
                    DefaultValue = "default",
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultTemplate_Descr",
                });

            #endregion

            #region // ---- Frame cache defaults

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalOutlook] = new Guid("C12A15AA-3C10-407E-8368-A2397C7625BB"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultCacheIntervalOutlook",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 0,
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultCacheInterval_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalPicture] = new Guid("29851A7E-9F53-4759-98FF-F70AC0FD5A74"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultCacheIntervalPicture",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 0,
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultCacheInterval_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalReport] = new Guid("F1AFB295-75FB-4833-BE21-7DB5ED55ED92"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultCacheIntervalReport",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 0,
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultCacheInterval_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalVideo] = new Guid("32F35FC7-18CD-4C0B-82E2-FE584D6BE0C8"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultCacheIntervalVideo",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 0,
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultCacheInterval_Descr",
                });

            _keyRes.Add(
                _keyGuids[(int)Keys.DefaultCacheIntervalWeather] = new Guid("C7C63812-2E51-42EF-A0C0-9258E48A5FF8"),
                new SettingProperties
                {
                    ResourceId = "Settings_DefaultCacheIntervalWeather",
                    Type = SettingTypes.SettingType_IntPositive,
                    DefaultValue = 0,
                    ResourceId_Group = "Settings_Group_FrameDefaults",
                    ResourceId_Descr = "Settings_DefaultCacheInterval_Descr",
                });

            #endregion
        }

        public static void Initialize(DisplayMonkeyEntities _db)
        {
            // refresh settings context
            ((System.Data.Entity.Infrastructure.IObjectContextAdapter)_db)
                .ObjectContext
                .Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, _db.Settings)
                ;

            var inDb = _db.Settings
                .Select(s => s.Key)
                .ToList()
                ;

            foreach (var k in _keyRes)
            {
                if (!inDb.Exists(s => s == k.Key))
                {
                    Setting setting = new Setting()
                    { 
                        Key = k.Key,
                        AnyValue = (k.Value as SettingProperties).DefaultValue,
                    };
                    _db.Settings.Add(setting);
                    _db.SaveChanges();
                }
            }
        }

        #endregion
    }
}