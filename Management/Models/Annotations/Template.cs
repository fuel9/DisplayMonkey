using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
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
}