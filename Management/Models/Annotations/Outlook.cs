using DisplayMonkey.Language;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    [
        MetadataType(typeof(Outlook.Annotations))
    ]
    public partial class Outlook
    {
        public Outlook(Frame _fromFrame, DisplayMonkeyEntities db) : 
            base(_fromFrame) 
        {
            init(db);
        }

        public Outlook() : base() { }

        protected override void init(DisplayMonkeyEntities _db)
        {
            base.init(_db);

            this.CacheInterval = Setting.GetDefaultCacheInterval(_db, this.FrameType);

            this.Mode = OutlookModes.OutlookMode_Today;
            this.Privacy = OutlookPrivacy.OutlookPrivacy_All;
            this.ShowEvents = 0;
            this.ShowAsFlags = -1;
        }

        internal class Annotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "ID"),
            ]
            public int FrameId { get; set; }

            /*[
                Display(ResourceType = typeof(Resources), Name = "View"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ViewRequired"),
            ]
            public OutlookModes Mode { get; set; }*/

            [
                Display(ResourceType = typeof(Resources), Name = "Privacy"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PrivacyRequired"),
            ]
            public OutlookPrivacy Privacy { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "OutlookAllowReserve"),
            ]
            public bool AllowReserve { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Name"),
                //Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequired"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Name { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ExchangeAccount"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ExchangeAccountRequired"),
            ]
            public int AccountId { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "Mailbox"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MailboxRequired"),
                RegularExpression(Constants.EmailMask,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "EmailInvalid"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string Mailbox { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ShowEvents"),
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
                Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ShowEventsRequired"),
            ]
            public int ShowEvents { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "ExchangeAccount"),
            ]
            public virtual ExchangeAccount ExchangeAccount { get; set; }

            [
                Display(ResourceType = typeof(Resources), Name = "BookingSubject"),
                StringLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxLengthExceeded"),
            ]
            public string BookingSubject { get; set; }
        }

        [
            NotMapped,
        ]
        public string NameOrMailboxOrAccount { get { return this.Name ?? this.Mailbox ?? this.ExchangeAccount.Name; } }

        [
            NotMapped,
            Display(ResourceType = typeof(Resources), Name = "EWS_ShowAs_Free"),
        ]
        public bool ShowAsFree
        {
            get { return (this.ShowAsFlags & (1 << (int)LegacyFreeBusyStatus.Free)) != 0; }
            set
            {
                int mask = (1 << (int)LegacyFreeBusyStatus.Free);
                if (value)
                    this.ShowAsFlags |= mask;
                else
                    this.ShowAsFlags &= ~mask;
            }
        }

        [
            NotMapped,
            Display(ResourceType = typeof(Resources), Name = "EWS_ShowAs_Tentative"),
        ]
        public bool ShowAsTentative
        {
            get { return (this.ShowAsFlags & (1 << (int)LegacyFreeBusyStatus.Tentative)) != 0; }
            set
            {
                int mask = (1 << (int)LegacyFreeBusyStatus.Tentative);
                if (value)
                    this.ShowAsFlags |= mask;
                else
                    this.ShowAsFlags &= ~mask;
            }
        }

        [
            NotMapped,
            Display(ResourceType = typeof(Resources), Name = "EWS_ShowAs_Busy"),
        ]
        public bool ShowAsBusy
        {
            get { return (this.ShowAsFlags & (1 << (int)LegacyFreeBusyStatus.Busy)) != 0; }
            set
            {
                int mask = (1 << (int)LegacyFreeBusyStatus.Busy);
                if (value)
                    this.ShowAsFlags |= mask;
                else
                    this.ShowAsFlags &= ~mask;
            }
        }

        [
            NotMapped,
            Display(ResourceType = typeof(Resources), Name = "EWS_ShowAs_Oof"),
        ]
        public bool ShowAsOof
        {
            get { return (this.ShowAsFlags & (1 << (int)LegacyFreeBusyStatus.OOF)) != 0; }
            set
            {
                int mask = (1 << (int)LegacyFreeBusyStatus.OOF);
                if (value)
                    this.ShowAsFlags |= mask;
                else
                    this.ShowAsFlags &= ~mask;
            }
        }

        [
            NotMapped,
            Display(ResourceType = typeof(Resources), Name = "EWS_ShowAs_Wew"),
        ]
        public bool ShowAsWew
        {
            get { return (this.ShowAsFlags & (1 << (int)LegacyFreeBusyStatus.WorkingElsewhere)) != 0; }
            set
            {
                int mask = (1 << (int)LegacyFreeBusyStatus.WorkingElsewhere);
                if (value)
                    this.ShowAsFlags |= mask;
                else
                    this.ShowAsFlags &= ~mask;
            }
        }

        [
            NotMapped,
            Display(ResourceType = typeof(Resources), Name = "EWS_ShowAs_NoData"),
        ]
        public bool ShowAsNoData
        {
            get { return (this.ShowAsFlags & (1 << (int)LegacyFreeBusyStatus.NoData)) != 0; }
            set
            {
                int mask = (1 << (int)LegacyFreeBusyStatus.NoData);
                if (value)
                    this.ShowAsFlags |= mask;
                else
                    this.ShowAsFlags &= ~mask;
            }
        }
    }

}