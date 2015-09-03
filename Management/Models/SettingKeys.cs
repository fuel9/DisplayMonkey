/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DisplayMonkey.Models
{
    public partial class Setting
    {
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

        private static Guid[] _keyGuids = new Guid[(int)Setting.Keys.Count];
        private static Dictionary<Guid, string> _keyRes = new Dictionary<Guid, string>((int)Setting.Keys.Count);

        static Setting()
        {
            //_keyRes.Add(
            //    _keyGuids[(int)Setting.Keys.ZZZ] = new Guid("12345678-90AB-CDEF-1234-567890ABCDEF"),
            //    "Settings_ZZZ"
            //    );

            // ---- General defaults

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

            // ---- Display defaults

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultDisplayReadyEventTimeout] = new Guid("3AC645E8-30B9-4473-A78C-69DBC4BFFAA6"),
                "Settings_DefaultDisplayReadyEventTimeout"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultDisplayPollInterval] = new Guid("7C993AC3-2E15-44DD-84B3-D13935BD1E43"),
                "Settings_DefaultDisplayPollInterval"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultDisplayErrorLength] = new Guid("2F43071D-C314-4C78-8438-76B474364258"),
                "Settings_DefaultDisplayErrorLength"
                );

            // ---- Panel defaults

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultPanelFadeLength] = new Guid("6865C5AE-8A91-4A44-B03A-EA81F69D6539"),
                "Settings_DefaultPanelFadeLength"
                );

            _keyRes.Add(
                _keyGuids[(int)Setting.Keys.DefaultFullPanelFadeLength] = new Guid("9A4989FC-4C2F-49F3-AD60-E120AD8F85FE"),
                "Settings_DefaultFullPanelFadeLength"
                );

            // ---- Frame defaults

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

            // ---- Frame cache defaults

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

        #endregion
    }
}

