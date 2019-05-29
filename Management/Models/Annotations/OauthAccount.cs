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
        MetadataType(typeof(OauthAccount.Annotations))
    ]
    public partial class OauthAccount
    {
        public void init(DisplayMonkeyEntities _db)
        {
            this.Provider = OauthProviders.OauthProvider_Yahoo;
        }

        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int AccountId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "OauthProvider"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "OauthProviderRequired"),
            ]
            public OauthProviders Provider { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "OauthAppId"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "OauthAppIdRequired"),
                StringLength(200, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string AppId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "OauthClientId"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "OauthClientIdRequired"),
                StringLength(4000, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string ClientId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "OauthClientSecret"),
                DataType(DataType.Password),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "OauthClientSecretRequired"),
                StringLength(4000, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string ClientSecret { get; set; }
        }
    }
}