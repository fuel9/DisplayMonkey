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
                //Frame.Duration = 600;

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
        }
    }

    public partial class Html
    {
        public void init(DisplayMonkeyEntities _db)
        {
            if (Frame != null)
            {
                Setting defTemplate = Setting.GetSetting(_db, Setting.Keys.DefaultTemplateHtml);
                if (defTemplate != null)
                {
                    string templateName = defTemplate.StringValue;
                    Frame.TemplateId = _db.Templates
                        .Where(t => t.Name == templateName && t.FrameType == FrameTypes.Html)
                        .FirstOrDefault()
                        .TemplateId
                        ;
                }
            }
        }
    }

    public partial class Memo
    {
        public void init(DisplayMonkeyEntities _db)
        {
            if (Frame != null)
            {
                Setting defTemplate = Setting.GetSetting(_db, Setting.Keys.DefaultTemplateMemo);
                if (defTemplate != null)
                {
                    string templateName = defTemplate.StringValue;
                    Frame.TemplateId = _db.Templates
                        .Where(t => t.Name == templateName && t.FrameType == FrameTypes.Memo)
                        .FirstOrDefault()
                        .TemplateId
                        ;
                }
            }
        }
    }

    public partial class Outlook
    {
        public void init(DisplayMonkeyEntities _db)
        {
            if (Frame != null)
            {
                Setting defCacheInt = Setting.GetSetting(_db, Setting.Keys.DefaultCacheIntervalOutlook);
                if (defCacheInt != null)
                {
                    Frame.CacheInterval = defCacheInt.IntValuePositive;
                }

                Setting defTemplate = Setting.GetSetting(_db, Setting.Keys.DefaultTemplateOutlook);
                if (defTemplate != null)
                {
                    string templateName = defTemplate.StringValue;
                    Frame.TemplateId = _db.Templates
                        .Where(t => t.Name == templateName && t.FrameType == FrameTypes.Outlook)
                        .FirstOrDefault()
                        .TemplateId
                        ;
                }
            }

            this.Mode = OutlookModes.OutlookMode_Today;
            this.ShowEvents = 0;
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
                //Frame.Duration = 600;

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
}
