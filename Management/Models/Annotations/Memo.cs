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
        MetadataType(typeof(Memo.Annotations))
    ]
    public partial class Memo : Frame
    {
        public Memo(Frame _fromFrame, DisplayMonkeyEntities db) : 
            base(_fromFrame) 
        {
            init(db);
        }

        public Memo() : base() { }

        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int FrameId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Subject"),
            ]
            public string Subject { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Body"),
            ]
            public string Body { get; set; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "ShortBody"),
            NotMapped,
        ]
        public string ShortBody
        {
            get
            {
                return this.Body == null ? null : this.Body.Length > 100 ? this.Body.Substring(0, 100) + "..." : this.Body;
            }
        }
    }
}