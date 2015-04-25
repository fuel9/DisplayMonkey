using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    public partial class Setting
    {
        #region -----  Key Identification  -----

        public enum Keys : int
        {
            MaxImageSize = 0,
            MaxVideoSize,
            PresentationSite,

            // default template by frame type
            DefaultTemplateClock,
            DefaultTemplateHtml,
            DefaultTemplateMemo,
            //DefaultTemplateNews,
            DefaultTemplateOutlook,
            DefaultTemplatePicture,
            DefaultTemplateReport,
            DefaultTemplateVideo,
            DefaultTemplateWeather,
            DefaultTemplateYouTube,

            // default cache interval by frame type
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

        static Setting()
        {
            //_keyRes.Add(
            //    _keyGuids[(int)Setting.Keys.ZZZ] = new Guid("12345678-90AB-CDEF-1234-567890ABCDEF"),
            //    "Settings_ZZZ"
            //    );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.MaxImageSize] = new Guid("9A0BC012-FF01-4103-8A75-A03B275B0AD1"),
                "Settings_MaxImageSize"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.MaxVideoSize] = new Guid("4CAB57C4-EFEF-4EDE-91A3-EFFD48660909"),
                "Settings_MaxVideoSize"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.PresentationSite] = new Guid("417D856B-7EC4-4CBD-A5EA-47BFC0F7B1F9"),
                "Settings_PresentationSite"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultTemplateClock] = new Guid("1D715B8D-2377-4FC6-B2A1-D025A4E48A56"),
                "Settings_DefaultTemplateClock"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultTemplateHtml] = new Guid("2409F3DB-06C7-4E16-A903-3D76FD0DB45A"),
                "Settings_DefaultTemplateHtml"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultTemplateMemo] = new Guid("366AC17F-C163-4F28-B233-EDA6B15A8142"),
                "Settings_DefaultTemplateMemo"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultTemplateOutlook] = new Guid("356FB4FA-C2B4-4A52-9420-1364C645F573"),
                "Settings_DefaultTemplateOutlook"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultTemplatePicture] = new Guid("5565E940-0C32-4C6D-848D-96F99E8AE947"),
                "Settings_DefaultTemplatePicture"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultTemplateReport] = new Guid("411DEF31-E1D6-41E9-A299-81D345EC1AB7"),
                "Settings_DefaultTemplateReport"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultTemplateVideo] = new Guid("4FA531DC-9957-4182-946D-3C8F3213057D"),
                "Settings_DefaultTemplateVideo"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultTemplateWeather] = new Guid("21BDCC5D-753B-47AF-A492-0FC32D90A335"),
                "Settings_DefaultTemplateWeather"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultTemplateYouTube] = new Guid("CAFCC975-8B0C-4027-8344-5719FCB0A213"),
                "Settings_DefaultTemplateYouTube"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultCacheIntervalOutlook] = new Guid("C12A15AA-3C10-407E-8368-A2397C7625BB"),
                "Settings_DefaultCacheIntervalOutlook"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultCacheIntervalPicture] = new Guid("29851A7E-9F53-4759-98FF-F70AC0FD5A74"),
                "Settings_DefaultCacheIntervalPicture"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultCacheIntervalReport] = new Guid("F1AFB295-75FB-4833-BE21-7DB5ED55ED92"),
                "Settings_DefaultCacheIntervalReport"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultCacheIntervalVideo] = new Guid("32F35FC7-18CD-4C0B-82E2-FE584D6BE0C8"),
                "Settings_DefaultCacheIntervalVideo"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultCacheIntervalWeather] = new Guid("C7C63812-2E51-42EF-A0C0-9258E48A5FF8"),
                "Settings_DefaultCacheIntervalWeather"
                );
        }

        public static Setting GetSetting(DisplayMonkeyEntities _db, Setting.Keys _id)
        {
            Guid key = _keyGuids[(int)_id];
            return _db.Settings.FirstOrDefault(s => s.Key == key);
        }

        public string ResourceId
        {
            get { return _keyRes[this.Key]; }
        }


        private static Guid[] _keyGuids = new Guid[(int)Setting.Keys.Count];
        private static Dictionary<Guid, string> _keyRes = new Dictionary<Guid, string>((int)Setting.Keys.Count);

        #endregion

    }
}