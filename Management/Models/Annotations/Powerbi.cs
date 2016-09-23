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
    public class Powerbi
    {
        public Powerbi(Frame _fromFrame, DisplayMonkeyEntities db) : 
            base(_fromFrame) 
        {
            init(db);
        }
        
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "AzureAccount"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AzureAccountRequired"),
            ]
            public int AccountId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "PowerbiType"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PowerbiTypeRequired"),
            ]
            public PowerbiTypes Type { get; set; }

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
        }
    }
}