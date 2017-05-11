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
        MetadataType(typeof(AzureAccount.Annotations))
    ]
    public partial class AzureAccount
    {
        public void init(DisplayMonkeyEntities _db)
        {
            this.Resource = AzureResources.AzureResource_PowerBi;
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
                Display(ResourceType = typeof(Resources), Name = "AzureResource"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AzureResourceRequired"),
            ]
            public AzureResources Resource { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "AzureClientId"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AzureClientIdRequired"),
                StringLength(36, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string ClientId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "AzureClientSecret"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AzureClientSecretRequired"),
                StringLength(500, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string ClientSecret { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "AzureTenantId"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string TenantId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "User"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AccountRequired"),
                RegularExpression(Constants.EmailMask,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "EmailInvalid"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string User { get; set; }
        }

        #region Password fields

        [
            Display(ResourceType = typeof(Resources), Name = "Password"),
            DataType(DataType.Password),
            NotMapped,
        ]
        public string PasswordUnmasked
        {
            get
            {
                return Constants.PasswordMask;
            }

            set
            {
                _passwordUnmasked = value;
                PasswordSet = (_passwordUnmasked != Constants.PasswordMask);
            }
        }

        [
            NotMapped,
        ]
        public bool PasswordSet { get; private set; }

        public void UpdatePassword(DisplayMonkeyEntities _db)
        {
            if (PasswordSet)
            {
                this.Password = Setting.GetEncryptor(_db).Encrypt(_passwordUnmasked);
            }
        }

        private string _passwordUnmasked;

        #endregion
    }
}