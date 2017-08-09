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
        MetadataType(typeof(Powerbi.Annotations))
    ]
    public partial class Powerbi : Frame
    {
        public Powerbi(Frame _fromFrame, DisplayMonkeyEntities db) : 
            base(_fromFrame) 
        {
            init(db);
        }

        public Powerbi() : base() { }

        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "AzureAccount"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AzureAccountRequired"),
            ]
            public int AccountId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "PowerbiType"),
                //Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PowerbiTypeRequired"),
            ]
            public Nullable<PowerbiTypes> Type { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "PowerbiUrl"),
            ]
            public string Url { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "AzureAccount"),
            ]
            public virtual AzureAccount AzureAccount { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "PowerbiDashboard"),
            ]
            public Nullable<System.Guid> DashboardGuid { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "PowerbiTile"),
            ]
            public Nullable<System.Guid> TileGuid { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "PowerbiReport"),
            ]
            public Nullable<System.Guid> ReportGuid { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "PowerbiGroup"),
            ]
            public Nullable<System.Guid> GroupGuid { get; set; }
        }
    }
}