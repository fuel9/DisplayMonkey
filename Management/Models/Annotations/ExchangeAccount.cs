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
        MetadataType(typeof(ExchangeAccount.Annotations))
    ]
    public partial class ExchangeAccount
    {
        public void init(DisplayMonkeyEntities _db)
        {
            this.EwsVersion = OutlookEwsVersions.OutlookEwsVersion_Exchange2007_SP1;
            this.Url = "https://outlook.office365.com/EWS/Exchange.asmx";
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
                Display(ResourceType = typeof(Resources), Name = "Account"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AccountRequired"),
                RegularExpression(Constants.EmailMask,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "EmailInvalid"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Account { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Password"),
            ]
            public byte[] Password { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "EwsVersion"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "EwsVersionRequired"),
            ]
            public OutlookEwsVersions EwsVersion { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "OutlookUrl"),
                StringLength(250, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Url { get; set; }
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