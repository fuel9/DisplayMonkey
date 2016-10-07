using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace DisplayMonkey.Models
{
    [
        MetadataType(typeof(Frame.AnnotationsBase))
    ]
    public partial class Frame
    {
        public Frame(Frame _fromFrame)
        {
            //this.FrameId = _fromFrame.FrameId;
            this.PanelId = _fromFrame.PanelId;
            this.Duration = _fromFrame.Duration;
            this.BeginsOn = _fromFrame.BeginsOn;
            this.EndsOn = _fromFrame.EndsOn;
            this.Sort = _fromFrame.Sort;
            //this.DateCreated = _fromFrame.DateCreated;
            //this.Version = _fromFrame.Version;
            this.TemplateId = _fromFrame.TemplateId;
            this.CacheInterval = _fromFrame.CacheInterval;
        }

        protected virtual void init(DisplayMonkeyEntities _db)
        {
            this.TemplateId = Setting.GetDefaultTemplate(_db, this.FrameType);
        }

        internal class AnnotationsBase
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

            /*[
               Display(ResourceType = typeof(Resources), Name = "Memo"),
            ]
            public virtual Memo Memo { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "News"), 
            ]
            public virtual News News { get; set; }*/

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

            /*[
               Display(ResourceType = typeof(Resources), Name = "Html"),
            ]
            public virtual Html Html { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Outlook"),
            ]
            public virtual Outlook Outlook { get; set; }*/

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
            get 
            { 
                //case FrameTypes.Clock: key = Keys.DefaultTemplateClock; break;

                if (this is Html) 
                    return FrameTypes.Html;

                if (this is Memo) 
                    return FrameTypes.Memo;
                
                if (this is Outlook) 
                    return FrameTypes.Outlook;

                //case FrameTypes.Picture: key = Keys.DefaultTemplatePicture; break;

                else if (this is Powerbi) 
                    return FrameTypes.Powerbi;

                //case FrameTypes.Report: key = Keys.DefaultTemplateReport; break;
                //case FrameTypes.Video: key = Keys.DefaultTemplateVideo; break;
                //case FrameTypes.Weather: key = Keys.DefaultTemplateWeather; break;
                //case FrameTypes.YouTube: key = Keys.DefaultTemplateYouTube; break;
                
                else if (this.Template != null) 
                    return this.Template.FrameType;

                return null;
            }
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

                    case FrameTypes.Powerbi:
                        return "~/images/powerbi.png";

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

}