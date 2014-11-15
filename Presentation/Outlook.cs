using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
	public class Outlook : Frame
	{
        public Outlook(int frameId, int panelId)
			: base()
		{
			PanelId = panelId;
			_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/outlook.htm");
			string sql = string.Format("SELECT TOP 1 * FROM Outlook WHERE FrameId={0}", frameId);

			/*using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
					Path = DataAccess.StringOrBlank(dr["Path"]);
					Mode = (PictureMode)DataAccess.IntOrZero(dr["Mode"]);
					Name = DataAccess.StringOrBlank(dr["Name"]);
				}
			}*/

            // TODO:
            Account = "denis.pavlichenko@permobil.com";
            Password = "den!S.85";
            Mailbox = "us_conf_uk@permobil.com";
		}

        public override string Payload
        {
            get
            {
                string html = "";
                try
                {                 
                    // EWS connection point
                    ServicePointManager.ServerCertificateValidationCallback = 
                        CertificateValidationCallBack
                        ;
                    ExchangeService service = new ExchangeService(Version)
                    {
                        Credentials = new WebCredentials(Account, Password),
                        Url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx"),
                    };

                    // get display name
                    string displayName = this.Mailbox;
                    var n = service.ResolveName(displayName);
                    if (n.Count > 0)
                    {
                        if (n[0].Contact != null)
                        {
                            displayName = 
                                    n[0].Contact.CompleteName.FullName 
                                ??  n[0].Contact.DisplayName
                                ??  displayName
                                ;
                        }
                        else if (n[0].Mailbox != null)
                        {
                            displayName = n[0].Mailbox.Name;
                        }
                    }
                    
                    // get availability
                    TimeZoneInfo timeZone = TimeZoneInfo.Local;
                    DateTime localTime = DateTime.Now;
                    TimeSpan startTime = new TimeSpan(0,0,0), endTime = new TimeSpan(23,59,59);
                    GetUserAvailabilityResults uars = service.GetUserAvailability(
                        new AttendeeInfo[] { new AttendeeInfo(this.Mailbox, MeetingAttendeeType.Required, true) }, 
                        new TimeWindow(DateTime.Today, DateTime.Today.AddDays(1)), 
                        AvailabilityData.FreeBusy
                        );
                    var u = uars.AttendeesAvailability[0];
                    if (u.WorkingHours != null)
                    {
                        startTime = u.WorkingHours.StartTime;
                        endTime = u.WorkingHours.EndTime;
                        timeZone = u.WorkingHours.TimeZone;
                    }

                    // mailbox calendar events
                    FindItemsResults<Item> findItems = null;
                    List<EventEntry> events = new List<EventEntry>();

                    // prep filter
                    FolderId folderId = new FolderId(WellKnownFolderName.Calendar, new Mailbox(this.Mailbox));
                    SearchFilter.SearchFilterCollection searchFilterCollection =
                        new SearchFilter.SearchFilterCollection()
                        {
                            LogicalOperator = LogicalOperator.And,
                        };
                    searchFilterCollection.Add(new SearchFilter.Exists(AppointmentSchema.Subject));
                    searchFilterCollection.Add(new SearchFilter.IsGreaterThanOrEqualTo(
                        AppointmentSchema.Start, DateTime.Now
                        ));
                    searchFilterCollection.Add(new SearchFilter.IsLessThanOrEqualTo(
                        AppointmentSchema.Start, DateTime.Now.AddHours(this.HourWindow)
                        ));
                    ItemView itemView = new ItemView(100)
                    {
                        PropertySet = new PropertySet(BasePropertySet.FirstClassProperties),
                    };

                    // get all events
                    do
                    {
                        findItems = service.FindItems(folderId, searchFilterCollection, itemView);
                        foreach (Item item in findItems)
                        {
                            if (item is Appointment)
                            {
                                Appointment appointment = item as Appointment;
                                events.Add(new EventEntry
                                {
                                    Subject = item.Subject,
                                    Starts = appointment.Start,
                                    Ends = appointment.End,
                                    Duration = appointment.Duration,
                                });
                            }
                        }
                    } 
                    while (findItems.MoreAvailable);
                    events.Sort();

                    JavaScriptSerializer oSerializer = new JavaScriptSerializer();
                    oSerializer.RegisterConverters(new [] { new EventEntryConverter() });
                    string json = oSerializer.Serialize(new
                    {
                        displayName = displayName,
                        timeZone = new
                        {
                            id = timeZone.Id,
                            daylightName = timeZone.DaylightName,
                            utcOffsetHours = timeZone.BaseUtcOffset.TotalHours,
                        },
                        currentTime = new
                        {
                            year = localTime.Year,
                            month = localTime.Month,
                            day = localTime.Day,
                            weekDay = localTime.DayOfWeek,
                            hour = localTime.Hour,
                            minute = localTime.Minute,
                            second = localTime.Second,
                        },
                        startTime = new
                        {
                            hours = startTime.Hours,
                            minutes = startTime.Minutes,
                        },
                        endTime = new
                        {
                            hours = endTime.Hours,
                            minutes = endTime.Minutes,
                        },
                        status = events.Count > 0 && events[0].Starts <= localTime && localTime < events[0].Ends ? "busy" : "available",
                        events = events,
                    });
                    
                    // load template
                    string template = File.ReadAllText(_templatePath);

                    // fill template
                    //if (FrameId > 0)
                    //{
                        HttpServerUtility util = HttpContext.Current.Server;
                        html = string.Format(
                            template,
                            json
                            );
                    //}
                }

                //catch (ServiceResponseException ex)
                catch (Exception ex)
                {
                    html = ex.Message;
                }

                return html;
            }
        }

        #region -------- EventEntry --------

        private class EventEntry : IComparable<EventEntry>
        {
            public string Subject { get; set; }
            public DateTime Starts { get; set; }
            public DateTime Ends { get; set; }
            public TimeSpan Duration { get; set; }

            public int CompareTo(EventEntry rhs)
            {
                int dateCheck = this.Starts.CompareTo(rhs.Starts);
                if (dateCheck == 0)
                    return this.Subject.CompareTo(rhs.Subject);
                else
                    return dateCheck;
            }
        }

        private class EventEntryConverter : JavaScriptConverter
        {
            public override IEnumerable<Type> SupportedTypes
            {
                get
                {
                    return new[] { typeof(EventEntry) };
                }
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                IDictionary<string, object> serialized = new Dictionary<string, object>();
                EventEntry evt = obj as EventEntry;
                if (evt != null)
                {
                    serialized["subject"] = evt.Subject;
                    serialized["starts"] = new
                    {
                        year = evt.Starts.Year,
                        month = evt.Starts.Month,
                        day = evt.Starts.Day,
                        hour = evt.Starts.Hour,
                        minute = evt.Starts.Minute,
                    };
                    serialized["ends"] = new
                    {
                        year = evt.Ends.Year,
                        month = evt.Ends.Month,
                        day = evt.Ends.Day,
                        hour = evt.Ends.Hour,
                        minute = evt.Ends.Minute,
                    };
                    serialized["duration"] = new
                    {
                        days = evt.Duration.Days,
                        hours = evt.Duration.Hours,
                        minutes = evt.Duration.Minutes,
                        //totalMinutes = evt.Duration.TotalMinutes
                    };
                }
                return serialized;
            }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region -------- EWS Callbacks --------

        protected static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;

            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }

        protected static bool CertificateValidationCallBack (
            object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors
            )
        {
            //return true;
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status
                        in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                            (status.Status ==
                                System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid. 
                            continue;
                        }
                        else
                        {
                            if (status.Status !=
                                    System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                // If there are any other errors in the certificate chain, the certificate is invalid,
                                // so the method returns false.
                                return false;
                            }
                        }
                    }
                }

                // When processing reaches this line, the only errors in the certificate chain are 
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange server installations, so return true.
                return true;
            }

            // In all other cases, return false.
            return false;
        }

        #endregion

        public string Account = "";
        public string Password = "";
        public string Mailbox = "";
        public int HourWindow = 48;
        public ExchangeVersion Version = ExchangeVersion.Exchange2010_SP2;
    }
}