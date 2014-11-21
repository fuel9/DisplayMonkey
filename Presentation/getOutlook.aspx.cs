using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using Microsoft.Exchange.WebServices.Data;
using System.Net;

namespace DisplayMonkey
{
    public partial class getOutlook : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int frameId = DataAccess.IntOrZero(Request.QueryString["frame"]);
                int panelId = DataAccess.IntOrZero(Request.QueryString["panel"]);
                int displayId = DataAccess.IntOrZero(Request.QueryString["display"]);

				string json = "";
				
				try
				{
                    Outlook outlook = new Outlook(frameId, panelId);
                    
                    // EWS connection point
                    ServicePointManager.ServerCertificateValidationCallback =
                        CertificateValidationCallBack
                        ;
                    ExchangeService service = new ExchangeService(outlook.Revision)
                    {
                        Credentials = new WebCredentials(
                            outlook.Account, 
                            RsaUtil.Decrypt(outlook.Password)
                            ),
                        Url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx"),
                    };
                    //AutodiscoverUrl(service, outlook.Account);    // TODO: activate before release

                    // get display name
                    string displayName = outlook.Mailbox;
                    var n = service.ResolveName(displayName);
                    if (n.Count > 0)
                    {
                        if (n[0].Contact != null)
                        {
                            displayName =
                                    n[0].Contact.CompleteName.FullName
                                ?? n[0].Contact.DisplayName
                                ?? displayName
                                ;
                        }
                        else if (n[0].Mailbox != null)
                        {
                            displayName = n[0].Mailbox.Name;
                        }
                    }

                    // get availability
                    TimeZoneInfo timeZone = TimeZoneInfo.Local;
                    TimeSpan startTime = new TimeSpan(0, 0, 0), endTime = new TimeSpan(23, 59, 59);
                    GetUserAvailabilityResults uars = service.GetUserAvailability(
                        new AttendeeInfo[] { new AttendeeInfo(outlook.Mailbox, MeetingAttendeeType.Required, true) },
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
                    List<EventEntry> events = new List<EventEntry>();

                    // prep filter
                    DateTime 
                        localTime = DateTime.Now, 
                        windowBeg = localTime.AddDays(-10),
                        windowEnd = localTime.AddHours(outlook.HourWindow);
                    FolderId folderId = new FolderId(WellKnownFolderName.Calendar, new Mailbox(outlook.Mailbox));
                    CalendarFolder calendar = CalendarFolder.Bind(service, folderId, new PropertySet());
                    CalendarView cView = new CalendarView(windowBeg, windowEnd)
                    {
                        PropertySet = new PropertySet(
                            AppointmentSchema.Subject,
                            AppointmentSchema.DateTimeCreated,
                            AppointmentSchema.Start,
                            AppointmentSchema.End,
                            AppointmentSchema.Duration
                            )
                    };

                    var appointments = calendar
                        .FindAppointments(cView)
                        .Where(i => localTime < i.End)
                        .OrderBy(i => i.Start)
                        .ThenBy(i => i.DateTimeCreated)
                        .ThenBy(i => i.Subject)
                        .Take(outlook.MaxItems)
                        ;
                    foreach (Appointment a in appointments)
                    {
                        events.Add(new EventEntry
                        {
                            Subject = a.Subject,
                            CreatedOn = a.DateTimeCreated,
                            Starts = a.Start,
                            Ends = a.End,
                            Duration = a.Duration,
                        });
                    }

                    //events.Sort();

                    EventEntry currentEvent = null, firstEvent = events
                        .FirstOrDefault()
                        ;

                    if (firstEvent != null && firstEvent.Starts <= localTime)
                    {
                        currentEvent = firstEvent;
                    }

                    string 
                        strCurrentEvent = "",
                        strCurrentStatus = string.Format("Available in the next {0} hrs", outlook.HourWindow)  // TODO: translate
                        ;
                    
                    if (currentEvent != null)
                    {
                        strCurrentEvent = string.Format("{0}, {1} - {2}", 
                            currentEvent.Subject, 
                            currentEvent.Starts.ToShortTimeString(),            // TODO: locale
                            currentEvent.Ends.ToShortTimeString()               // TODO: locale
                            );
                        TimeSpan gap = currentEvent.Ends.Subtract(localTime);
                        if (gap.Hours > 0)
                            strCurrentStatus = string.Format("{0} {1} hrs {2} min", "Ends in", gap.Hours, gap.Minutes);     // TODO: translate
                        else
                            strCurrentStatus = string.Format("{0} {1} min", "Ends in", gap.Minutes);                        // TODO: translate
                    }

                    else if (firstEvent != null)
                    {
                        TimeSpan gap = firstEvent.Starts.Subtract(localTime);
                        if (gap.Hours > 0)
                            strCurrentStatus = string.Format("{0} {1} hrs {2} min", "Available for next", gap.Hours, gap.Minutes);     // TODO: translate
                        else
                            strCurrentStatus = string.Format("{0} {1} min", "Available for next", gap.Minutes);             // TODO: translate
                    }
                    
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    jss.RegisterConverters(new[] { new EventEntryConverter() });
                    json = jss.Serialize(new
                    {
                        currentTime = new
                        {
                            year = localTime.Year,
                            month = localTime.Month,
                            day = localTime.Day,
                            weekDay = localTime.DayOfWeek,
                            hour = localTime.Hour,
                            minute = localTime.Minute,
                            second = localTime.Second,
                            timeZone = new
                            {
                                id = timeZone.Id,
                                daylightName = timeZone.DaylightName,
                                utcOffsetHours = timeZone.BaseUtcOffset.TotalHours,
                            },
                        },
                        mailbox = outlook.Name == "" ? displayName : outlook.Name,
                        startTime = new
                        {
                            hour = startTime.Hours,
                            minute = startTime.Minutes,
                        },
                        endTime = new
                        {
                            hour = endTime.Hours,
                            minute = endTime.Minutes,
                        },
                        currentEvent = strCurrentEvent,
                        currentStatus = strCurrentStatus,
                        events = new
                        {
                            head = EventEntryConverter.Head(),
                            items = events,
                            noEvents = events.Count == 0 ? EventEntryConverter.NoEventsMessage(outlook.HourWindow) : "",
                        },
                    });
				}

				catch (Exception ex)
				{
                    JavaScriptSerializer s = new JavaScriptSerializer();
                    json = s.Serialize(new
                    {
                        Error = ex.Message,
                        //Stack = ex.StackTrace,
                        Data = new
                        {
                            FrameId = frameId,
                            PanelId = panelId,
                            DisplayId = displayId,
                        },
                    });
				}

				Response.ExpiresAbsolute = DateTime.Now;
                Response.Expires = -1441;
                Response.CacheControl = "no-cache";
                Response.AddHeader("Pragma", "no-cache");
                Response.AddHeader("Pragma", "no-store");
                Response.AddHeader("cache-control", "no-cache");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoServerCaching();
				Response.Write(json);
            }
        }

        #region -------- EventEntry --------

        private class EventEntry : IComparable<EventEntry>
        {
            public string Subject { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime Starts { get; set; }
            public DateTime Ends { get; set; }
            public TimeSpan Duration { get; set; }

            public int CompareTo(EventEntry rhs)
            {
                int startsCheck = this.Starts.CompareTo(rhs.Starts);
                if (startsCheck == 0)
                {
                    int createdCheck = this.CreatedOn.CompareTo(rhs.CreatedOn);
                    if (createdCheck == 0)
                        return this.Subject.CompareTo(rhs.Subject);
                    else
                        return createdCheck;
                }
                else
                    return startsCheck;
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

            public static object[] Head()
            {
                return new[] 
                {
                    new {cls = "col1", name = "Event"},     // TODO: translate
                    new {cls = "col2", name = "Starts"},    // TODO: translate
                    new {cls = "col3", name = "Ends"},      // TODO: translate
                    new {cls = "col4", name = "Duration"}   // TODO: translate
                };
            }

            public static string NoEventsMessage(int hourWindow)
            {
                return string.Format("No events in the next {0} hrs", hourWindow);   // TODO: translate
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                IDictionary<string, object> serialized = new Dictionary<string, object>();
                EventEntry evt = obj as EventEntry;
                if (evt != null)
                {
                    DateTime tomorrow = DateTime.Today.AddDays(1);
                    serialized["col1"] = evt.Subject;
                    serialized["col2"] = (evt.Starts >= tomorrow ? evt.Starts.ToShortDateString() + " " : "") + evt.Starts.ToShortTimeString();    // TODO: locale
                    serialized["col3"] = (evt.Ends >= tomorrow ? evt.Ends.ToShortDateString() + " " : "") + evt.Ends.ToShortTimeString();      // TODO: locale
                    serialized["col4"] = string.Format("{0} hrs {1} min", (int)evt.Duration.TotalHours, evt.Duration.Minutes);  // TODO: translate
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

        private static void AutodiscoverUrl(ExchangeService service, string account)
        {
            string key = string.Format("outlook_{0}", account);

            if (HttpRuntime.Cache[key] != null)
            {
                string url = HttpRuntime.Cache[key] as string;
                service.Url = new Uri(url);
            }

            else
            {
                service.AutodiscoverUrl(account, RedirectionUrlValidationCallback);

                HttpRuntime.Cache.Insert(
                    key,
                    service.Url.OriginalString,
                    null,
                    System.Web.Caching.Cache.NoAbsoluteExpiration,
                    new TimeSpan(1, 0, 0)
                    );
            }
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
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

        private static bool CertificateValidationCallBack(
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
    }
}