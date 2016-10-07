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
using System.Web;

namespace DisplayMonkey.Models
{
    public partial class Clock
    {
        public void init(DisplayMonkeyEntities _db)
        {
            if (Frame != null)
            {
                Setting defTemplate = Setting.GetSetting(_db, Setting.Keys.DefaultTemplateClock);
                if (defTemplate != null)
                {
                    string templateName = defTemplate.StringValue;
                    Frame.TemplateId = _db.Templates
                        .Where(t => t.Name == templateName && t.FrameType == FrameTypes.Clock)
                        .FirstOrDefault()
                        .TemplateId
                        ;
                }
            }

            ShowSeconds = true;
            ShowDate = true;
            ShowTime = true;
        }
    }

    public partial class Picture
    {
        public void init(DisplayMonkeyEntities _db)
        {
            if (Frame != null)
            {
                Setting defCacheInt = Setting.GetSetting(_db, Setting.Keys.DefaultCacheIntervalPicture);
                if (defCacheInt != null)
                {
                    Frame.CacheInterval = defCacheInt.IntValuePositive;
                }

                Setting defTemplate = Setting.GetSetting(_db, Setting.Keys.DefaultTemplatePicture);
                if (defTemplate != null)
                {
                    string templateName = defTemplate.StringValue;
                    Frame.TemplateId = _db.Templates
                        .Where(t => t.Name == templateName && t.FrameType == FrameTypes.Picture)
                        .FirstOrDefault()
                        .TemplateId
                        ;
                }
            }
        }
    }

    public partial class Report
    {
        public void init(DisplayMonkeyEntities _db)
        {
            if (Frame != null)
            {
                Setting defCacheInt = Setting.GetSetting(_db, Setting.Keys.DefaultCacheIntervalReport);
                if (defCacheInt != null)
                {
                    Frame.CacheInterval = defCacheInt.IntValuePositive;
                }

                Setting defTemplate = Setting.GetSetting(_db, Setting.Keys.DefaultTemplateReport);
                if (defTemplate != null)
                {
                    string templateName = defTemplate.StringValue;
                    Frame.TemplateId = _db.Templates
                        .Where(t => t.Name == templateName && t.FrameType == FrameTypes.Report)
                        .FirstOrDefault()
                        .TemplateId
                        ;
                }
            }
        }
    }

    public partial class Video
    {
        public void init(DisplayMonkeyEntities _db)
        {
            if (Frame != null)
            {
                Setting defCacheInt = Setting.GetSetting(_db, Setting.Keys.DefaultCacheIntervalVideo);
                if (defCacheInt != null)
                {
                    Frame.CacheInterval = defCacheInt.IntValuePositive;
                }

                Setting defTemplate = Setting.GetSetting(_db, Setting.Keys.DefaultTemplateVideo);
                if (defTemplate != null)
                {
                    string templateName = defTemplate.StringValue;
                    Frame.TemplateId = _db.Templates
                        .Where(t => t.Name == templateName && t.FrameType == FrameTypes.Video)
                        .FirstOrDefault()
                        .TemplateId
                        ;
                }
            }

            PlayMuted = true;
            AutoLoop = true;
        }
    }

    public partial class Weather
    {
        public void init(DisplayMonkeyEntities _db)
        {
            if (Frame != null)
            {
                Setting defCacheInt = Setting.GetSetting(_db, Setting.Keys.DefaultCacheIntervalWeather);
                if (defCacheInt != null)
                {
                    Frame.CacheInterval = defCacheInt.IntValuePositive;
                }

                Setting defTemplate = Setting.GetSetting(_db, Setting.Keys.DefaultTemplateWeather);
                if (defTemplate != null)
                {
                    string templateName = defTemplate.StringValue;
                    Frame.TemplateId = _db.Templates
                        .Where(t => t.Name == templateName && t.FrameType == FrameTypes.Weather)
                        .FirstOrDefault()
                        .TemplateId
                        ;
                }
            }
        }
    }

    public partial class Youtube
    {
        public void init(DisplayMonkeyEntities _db)
        {
            if (Frame != null)
            {
                Setting defTemplate = Setting.GetSetting(_db, Setting.Keys.DefaultTemplateYouTube);
                if (defTemplate != null)
                {
                    string templateName = defTemplate.StringValue;
                    Frame.TemplateId = _db.Templates
                        .Where(t => t.Name == templateName && t.FrameType == FrameTypes.YouTube)
                        .FirstOrDefault()
                        .TemplateId
                        ;
                }
            }

            Aspect = YTAspect.YTAspect_Auto;
            Quality = YTQuality.YTQuality_Default;
            Rate = YTRate.YTRate_Normal;
            Start = 0;
            Volume = 0;
            AutoLoop = true;
        }
    }



    public partial class Panel
    {
        public void init(DisplayMonkeyEntities _db)
        {
            Setting fadeLength = Setting.GetSetting(_db, Setting.Keys.DefaultPanelFadeLength);
            if (fadeLength != null)
            {
                this.FadeLength = fadeLength.DecimalValue;
            }
        }
    }

    public partial class FullScreen
    {
        public void init(DisplayMonkeyEntities _db)
        {
            Setting fadeLength = Setting.GetSetting(_db, Setting.Keys.DefaultFullPanelFadeLength);
            if (fadeLength != null)
            {
                this.Panel.FadeLength = fadeLength.DecimalValue;
            }
        }
    }

    public partial class Display
    {
        public void init(DisplayMonkeyEntities _db)
        {
            Setting readyTimeout = Setting.GetSetting(_db, Setting.Keys.DefaultDisplayReadyEventTimeout);
            if (readyTimeout != null)
            {
                this.ReadyTimeout = readyTimeout.IntValuePositive;
            }

            Setting errorLength = Setting.GetSetting(_db, Setting.Keys.DefaultDisplayErrorLength);
            if (errorLength != null)
            {
                this.ErrorLength = errorLength.IntValuePositive;
            }

            Setting pollInterval = Setting.GetSetting(_db, Setting.Keys.DefaultDisplayPollInterval);
            if (pollInterval != null)
            {
                this.PollInterval = pollInterval.IntValuePositive;
            }
        }
    }
}
