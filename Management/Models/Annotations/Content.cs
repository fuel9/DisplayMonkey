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
        MetadataType(typeof(Content.Annotations))
    ]
    public partial class Content
    {
        internal class Annotations
        {
            [
               Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int ContentId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "MediaSource"),
                DataType(DataType.Upload),
            ]
            public byte[] Data { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Type"),
            ]
            public ContentTypes Type { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Canvas"),
            ]
            public virtual ICollection<Canvas> Canvas { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Pictures"),
            ]
            public virtual ICollection<Picture> Pictures { get; set; }

            [
               Display(ResourceType = typeof(Resources), Name = "Videos"),
            ]
            public virtual ICollection<Video> Videos { get; set; }
        }

        [
            Display(ResourceType = typeof(Resources), Name = "Size"),
            NotMapped,
        ]
        public Nullable<int> Size { get; set; }
    }

    public partial class ContentWithSize
    {
        public int ContentId { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "Size"),
        ]
        public Nullable<int> Size { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "Type"),
        ]
        public ContentTypes Type { get; set; }

        [
            Display(ResourceType = typeof(Resources), Name = "Name"),
        ]
        public string Name { get; set; }
    }
}