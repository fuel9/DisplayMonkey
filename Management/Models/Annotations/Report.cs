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
        MetadataType(typeof(Report.Annotations))
    ]
    public partial class Report : Frame
    {
        public Report(Frame _fromFrame, DisplayMonkeyEntities db) : 
            base(_fromFrame) 
        {
            init(db);
        }

        public Report() : base() { }

        protected override void init(DisplayMonkeyEntities _db)
        {
            base.init(_db);

            this.CacheInterval = Setting.GetDefaultCacheInterval(_db, this.FrameType);
        }

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
                Display(ResourceType = typeof(Resources), Name = "ReportServer"),
            ]
            public virtual ReportServer ReportServer { get; set; }
        }

        [NotMapped]
        public string FullPath
        {
            get 
            {
                if (this.ReportServer == null || this.Path == null)
                    return null;

                string
                    baseUrl = (this.ReportServer.BaseUrl ?? "").Trim(),
                    url = (this.Path ?? "").Trim(),
                    parameters = string.Empty;

                if (url.Contains("&"))
                {
                    int index = url.IndexOf("&");
                    if (index >= 0)
                    {
                        parameters = url.Substring(index);
                        url = url.Substring(0, index);
                    }
                }

                if (baseUrl.EndsWith("/"))
                    baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

                if (!url.StartsWith("/"))
                    url = "/" + url;


                url = string.Format(
                    "{0}?{1}{2}&rs:format=IMAGE",
                    baseUrl,
                    HttpUtility.UrlEncode(url), 
                    parameters
                    );
                return url;
            }
        }
    }
}