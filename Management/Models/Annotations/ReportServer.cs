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
        MetadataType(typeof(ReportServer.Annotations))
    ]
    public partial class ReportServer
    {
        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int ServerId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "BaseURL"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "BaseURLRequired"),
            ]
            public string BaseUrl { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "User"),
            ]
            public string User { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Password"),
            ]
            public byte[] Password { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Domain"),
            ]
            public string Domain { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Reports"),
            ]
            public virtual ICollection<Report> Reports { get; set; }
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