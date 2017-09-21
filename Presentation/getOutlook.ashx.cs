/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using Microsoft.Exchange.WebServices.Data;
using System.Net;
using DisplayMonkey.Language;

namespace DisplayMonkey
{
    public partial class getOutlook : HttpTaskAsyncHandler
    {
        public override async System.Threading.Tasks.Task ProcessRequestAsync(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

            int frameId = request.IntOrZero("frame");
            int panelId = request.IntOrZero("panel");
            int displayId = request.IntOrZero("display");
            string culture = request.StringOrBlank("culture");
            int reserveMinutes = request.IntOrZero("reserveMinutes");
            int trace = request.IntOrZero("trace");

			string json = "";
				
			try
			{
                // set culture
                Outlook outlook = new Outlook(frameId);
                Location location = new Location(displayId);

                if (string.IsNullOrWhiteSpace(culture))
                    culture = location.Culture;
                
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(culture);
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }

                // EWS: get data
                OutlookData data = await HttpRuntime.Cache.GetOrAddAbsoluteAsync(
                    string.Format("outlook_{0}_{1}_{2}", location.LocationId, outlook.FrameId, outlook.Version),
                    async (expire) => 
                    {
                        expire.When = DateTime.Now.AddMinutes(outlook.CacheInterval);
                        return await OutlookData.FromFrameAsync(outlook, location, reserveMinutes);
                    });

                // ---------------------- culture-specific starts here --------------------- //
                DateTime
                    locationTime = location.LocationTime,
                    locationToday = new DateTime(locationTime.Year, locationTime.Month, locationTime.Day)
                    ;

                //int showEvents = outlook.ShowEvents;
                List<EventEntry> currentList = data.events
                    .Where(e => e.Ends >= locationTime)
                    .Take(Math.Max(1, outlook.ShowEvents))
                    .ToList()
                    ;
                
                EventEntry 
                    currentEvent = null,
                    firstEvent = currentList.FirstOrDefault()
                    ;

                if (firstEvent != null && firstEvent.Starts <= locationTime)
                {
                    currentEvent = firstEvent;
                }

                DateTime endTime = locationToday.Add(data.endTime);
                string 
                    strCurrentEvent = "",
                    strCurrentStatus = string.Format(
                        Resources.Outlook_AvailableUntil,
                        endTime.ToShortTimeString()
                        )
                    ;
                TimeSpan availableTime = new TimeSpan(0);
                    
                if (currentEvent != null)
                {
                    DateTime tomorrow = locationToday.AddDays(1);
                    strCurrentEvent = string.Format("{0}, {1} - {2}", 
                        currentEvent.Subject,
                        (currentEvent.Starts >= tomorrow ? currentEvent.Starts.ToShortDateString() + " " : "") + currentEvent.Starts.ToShortTimeString(),
                        (currentEvent.Ends >= tomorrow ? currentEvent.Ends.ToShortDateString() + " " : "") + currentEvent.Ends.ToShortTimeString()
                        );
                    TimeSpan gap = currentEvent.Ends.Subtract(locationTime);
                    if (gap.Hours > 0)
                        strCurrentStatus = string.Format(Resources.Outlook_EndsInHrsMin, (int)gap.TotalHours, gap.Minutes);
                    else
                        strCurrentStatus = string.Format(Resources.Outlook_EndsInMin, gap.Minutes);
                }
                else if (firstEvent != null)
                {
                    availableTime = firstEvent.Starts.Subtract(locationTime);
                    if (availableTime.Hours > 0)
                        strCurrentStatus = string.Format(Resources.Outlook_AvailableForHrsMin, (int)availableTime.TotalHours, availableTime.Minutes);
                    else
                        strCurrentStatus = string.Format(Resources.Outlook_AvailableForMin, availableTime.Minutes);
                }
                else if (locationTime < endTime)
                {
                    availableTime = endTime.Subtract(locationTime);
                }

                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.RegisterConverters(new[] { new EventEntryConverter() });
                json = jss.Serialize(new
                {
                    currentTime = new
                    {
                        year = locationTime.Year,
                        month = locationTime.Month,
                        day = locationTime.Day,
                        weekDay = locationTime.DayOfWeek,
                        hour = locationTime.Hour,
                        minute = locationTime.Minute,
                        second = locationTime.Second,
                        timeZone = new
                        {
                            id = data.timeZone.Id,
                            daylightName = data.timeZone.DaylightName,
                            utcOffsetHours = data.timeZone.BaseUtcOffset.TotalHours,
                        },
                    },
                    mailbox = data.DisplayName,
                    startTime = new
                    {
                        hour = data.startTime.Hours,
                        minute = data.startTime.Minutes,
                    },
                    endTime = new
                    {
                        hour = data.endTime.Hours,
                        minute = data.endTime.Minutes,
                    },
                    currentEvent = strCurrentEvent,
                    currentStatus = strCurrentStatus,
                    available = new
                    {
                        days = (int)availableTime.TotalDays,
                        hours = (int)availableTime.TotalHours,
                        minutes = (int)availableTime.TotalMinutes,
                    },
                    events = new
                    {
                        items = currentList,
                    },
                    labels = new[] 
                    {
                        new {key = "subject", value = Resources.Outlook_Event},
                        new {key = "starts", value = Resources.Outlook_Starts},
                        new {key = "ends", value = Resources.Outlook_Ends},
                        new {key = "duration", value = Resources.Outlook_Duration},
                        new {key = "sensitivity", value = Resources.Outlook_Sensitivity},
                        new {key = "showAs", value = Resources.OutlookShowAs},
                        new {key = "noEvents", value = Resources.Outlook_NoEventsToday},
                        new {key = "bookingImpossible", value = Resources.Outlook_BookingImpossible},
                        new {key = "bookingSent", value = Resources.Outlook_BookingSent},
                    },
                });
			}

			catch (Exception ex)
			{
                JavaScriptSerializer s = new JavaScriptSerializer();
                if (trace == 0)
                    json = s.Serialize(new
                    {
                        Error = ex.Message,
                        Data = new
                        {
                            FrameId = frameId,
                            PanelId = panelId,
                            DisplayId = displayId,
                            Culture = culture
                        },
                    });
                else
                    json = s.Serialize(new
                    {
                        Error = ex.Message,
                        Stack = ex.StackTrace,
                        Data = new
                        {
                            FrameId = frameId,
                            PanelId = panelId,
                            DisplayId = displayId,
                            Culture = culture
                        },
                    });
            }

            response.Clear();
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetSlidingExpiration(true);
            response.Cache.SetNoStore();
            response.ContentType = "application/json";
			response.Write(json);
            response.Flush();
        }

        #region -------- OutlookData --------

        private class OutlookData
        {
            public IEnumerable<EventEntry> events = null;
            public TimeSpan startTime = new TimeSpan(0, 0, 0);
            public TimeSpan endTime = new TimeSpan(23, 59, 59);
            public TimeZoneInfo timeZone = TimeZoneInfo.Local;
            public string DisplayName;

            public OutlookData(Outlook outlook, Location location, int reserveMinutes)
            {
                //OutlookData.initFromFrame(this, outlook, location, reserveMinutes);
            }

            #region -------- EWS Data Call --------f

            private static ExtendedPropertyDefinition PR_TextBody = new ExtendedPropertyDefinition(0x1000, MapiPropertyType.String);
            private static ExtendedPropertyDefinition PR_Sensitivity = new ExtendedPropertyDefinition(0x0036, MapiPropertyType.Integer);

            private OutlookData()
            {
            }

            public static async System.Threading.Tasks.Task<OutlookData> FromFrameAsync(Outlook outlook, Location location, int reserveMinutes)
            {
                OutlookData data = new OutlookData();
                DateTime
                    locationTime = location.LocationTime,
                    locationToday = new DateTime(locationTime.Year, locationTime.Month, locationTime.Day),
                    windowBeg = locationToday, //locationTime,
                    windowEnd = locationToday.AddDays(1).AddMilliseconds(-1)
                    ;

                // EWS: create connection point
                ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;

                ExchangeService service = new ExchangeService(outlook.EwsVersion)
                {
                    Credentials = new WebCredentials(
                        outlook.Account,
                        Encryptor.Current.Decrypt(outlook.Password) // 1.4.0
                        ),
                };

                // https://msdn.microsoft.com/en-us/library/office/dn458789%28v=exchg.150%29.aspx?f=255&MSPPError=-2147217396#bk_howmaintained
                // https://social.msdn.microsoft.com/Forums/lync/en-US/bd032e3d-2501-40ba-a2b0-29a404685c35/error-exchange-web-services-are-not-currently-available?forum=exchangesvrdevelopment
                service.HttpHeaders.Add("X-AnchorMailbox", outlook.Account);

                // EWS: get URL
                if (!string.IsNullOrWhiteSpace(outlook.URL))
                {
                    service.Url = new Uri(outlook.URL);
                }
                else
                {
                    service.Url = await HttpRuntime.Cache.GetOrAddSlidingAsync(
                        string.Format("exchange_account_{0}", outlook.Account),
                        async (expire) =>
                        {
                            expire.After = TimeSpan.FromMinutes(60);
                            return await System.Threading.Tasks.Task.Run(() => 
                            {
                                service.AutodiscoverUrl(outlook.Account, RedirectionUrlValidationCallback);
                                return service.Url;
                            });
                        });
                }

                // mailbox: get display name
                data.DisplayName = outlook.Name;
                if (string.IsNullOrWhiteSpace(data.DisplayName))
                {
                    var match = await System.Threading.Tasks.Task.Run(() => service.ResolveName(outlook.Mailbox));
                    if (match.Count > 0)
                    {
                        if (match[0].Contact != null)
                        {
                            data.DisplayName =
                                    match[0].Contact.CompleteName.FullName
                                ?? match[0].Contact.DisplayName
                                ?? data.DisplayName
                                ;
                        }

                        else if (match[0].Mailbox != null)
                        {
                            data.DisplayName =
                                match[0].Mailbox.Name
                                ?? data.DisplayName
                                ;
                        }
                    }
                    //else
                    //    throw new ApplicationException(string.Format("Mailbox {0} not found", mailbox));
                }

                // mailbox: get availability
                GetUserAvailabilityResults uars = await System.Threading.Tasks.Task.Run(() =>
                {
                    return service.GetUserAvailability(
                        new AttendeeInfo[] { new AttendeeInfo(outlook.Mailbox, MeetingAttendeeType.Required, true) },
                        new TimeWindow(locationToday, locationToday.AddDays(1)),
                        AvailabilityData.FreeBusy
                        );
                });
                var u = uars.AttendeesAvailability[0];
                if (u.WorkingHours != null)
                {
                    data.startTime = u.WorkingHours.StartTime;
                    data.endTime = u.WorkingHours.EndTime;
                    data.timeZone = u.WorkingHours.TimeZone;
                }

                if (reserveMinutes > 0)
                {
                    Appointment appointment = new Appointment(service)
                    {
                        Subject = outlook.BookingSubject,
                        Body = string.Format("{0} | {1}", Resources.Outlook_BookingOnDemand, Resources.DisplayMonkey),
                        Start = DateTime.Now,
                        End = DateTime.Now.AddMinutes(reserveMinutes),
                    };

                    if (outlook.Mailbox == outlook.Account)
                    {
                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            appointment.Save(SendInvitationsMode.SendToNone);
                            return true;
                        });
                    }
                    else
                    {
                        appointment.Resources.Add(outlook.Mailbox);
                        await System.Threading.Tasks.Task.Run(() => 
                        {
                            appointment.Save(SendInvitationsMode.SendOnlyToAll);
                            return true;
                        });
                    }
                }

                // events: prep filter
                FolderId folderId = new FolderId(WellKnownFolderName.Calendar, new Mailbox(outlook.Mailbox));
                CalendarFolder calendar = await System.Threading.Tasks.Task.Run(() => CalendarFolder.Bind(service, folderId, new PropertySet()));
                CalendarView cView = new CalendarView(windowBeg, windowEnd)
                {
                    PropertySet = new PropertySet(
                        //BasePropertySet.FirstClassProperties,
                        AppointmentSchema.Subject,
                        AppointmentSchema.DateTimeCreated,
                        AppointmentSchema.Start,
                        AppointmentSchema.End,
                        AppointmentSchema.Duration,
                        AppointmentSchema.Sensitivity,
                        AppointmentSchema.LegacyFreeBusyStatus
                        )
                };

                // events: get list
                var appointments = await System.Threading.Tasks.Task.Run(() => calendar.FindAppointments(cView));
                data.events = appointments
                    //.FindAppointments(cView)
                    .Where(a => 
                        (outlook.Privacy != Models.OutlookPrivacy.OutlookPrivacy_NoClassified || a.Sensitivity == Sensitivity.Normal) && 
                        (outlook.IsShowAsAllowed(a.LegacyFreeBusyStatus))
                        )
                    .OrderBy(i => i.Start)
                    .ThenBy(i => i.DateTimeCreated)
                    .ThenBy(i => i.Subject)
                    //.Take(Math.Max(1, outlook.ShowEvents))
                    .Select(a => new EventEntry
                    {
                        Subject =
                            outlook.Privacy == Models.OutlookPrivacy.OutlookPrivacy_All || a.Sensitivity == Sensitivity.Normal ? a.Subject :
                            DataAccess.StringResource(string.Format("EWS_Sensitivity_{0}", a.Sensitivity.ToString())),
                        CreatedOn = a.DateTimeCreated,
                        Starts = a.Start,
                        Ends = a.End,
                        Duration = a.Duration,
                        Sensitivity = a.Sensitivity,
                        Today = locationToday,
                        ShowAs = a.LegacyFreeBusyStatus,
                    })
                    .ToList()
                    ;

                return data;
            }

            #endregion

            #region -------- EWS Callbacks --------

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

        #endregion

        #region -------- EventEntry --------

        private class EventEntry : IComparable<EventEntry>
        {
            public string Subject { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime Starts { get; set; }
            public DateTime Ends { get; set; }
            public TimeSpan Duration { get; set; }
            public DateTime Today { get; set; }
            public Sensitivity Sensitivity { get; set; }
            public LegacyFreeBusyStatus ShowAs { get; set; }

            public int CompareTo(EventEntry rhs)
            {
                int startsCheck = this.Starts.CompareTo(rhs.Starts);
                if (startsCheck != 0)
                    return startsCheck;

                int createdCheck = this.CreatedOn.CompareTo(rhs.CreatedOn);
                if (createdCheck != 0)
                    return createdCheck;
                
                return this.Subject.CompareTo(rhs.Subject);
            }
        }

        private class EventEntryConverter : JavaScriptConverter
        {
            public override IEnumerable<Type> SupportedTypes { get { return _supportedTypes; } }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                IDictionary<string, object> serialized = new Dictionary<string, object>();
                EventEntry evt = obj as EventEntry;
                if (evt != null)
                {
                    serialized["subject"] = evt.Subject;
                    serialized["sensitivity"] = "";
                    serialized["showAs"] = "";

                    List<string> flags = new List<string>();
                    foreach (var x in _sens.Where(e => e.Key == evt.Sensitivity))   // one and only
                    {
                        flags.Add(x.Value.Flag);
                        serialized["sensitivity"] = DataAccess.StringResource(x.Value.Resource);
                        break;
                    }
                    foreach (var x in _showAs.Where(e => e.Key == evt.ShowAs))   // one and only
                    {
                        flags.Add(x.Value.Flag);
                        serialized["showAs"] = DataAccess.StringResource(x.Value.Resource);
                        break;
                    }
                    serialized["flags"] = flags.ToArray();
                    
                    DateTime tomorrow = evt.Today.AddDays(1);
                    serialized["starts"] = (evt.Starts >= tomorrow ? evt.Starts.ToShortDateString() + " " : "") + evt.Starts.ToShortTimeString();
                    serialized["ends"] = (evt.Ends >= tomorrow ? evt.Ends.ToShortDateString() + " " : "") + evt.Ends.ToShortTimeString();
                    
                    if ((int)evt.Duration.TotalHours > 0)
                        serialized["duration"] = string.Format(Resources.Outlook_HrsMin, (int)evt.Duration.TotalHours, evt.Duration.Minutes);
                    else
                        serialized["duration"] = string.Format(Resources.Outlook_Min, evt.Duration.Minutes);
                }
                return serialized;
            }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                throw new NotImplementedException();
            }

            #region Private members

            private static Dictionary<Sensitivity, EventEntryAttribute> _sens;
            private static Dictionary<LegacyFreeBusyStatus, EventEntryAttribute> _showAs;
            private static IEnumerable<Type> _supportedTypes;

            private class EventEntryAttribute
            {
                public string Flag { get; set; }
                public string Resource { get; set; }
            }

            static EventEntryConverter()
            {
                _supportedTypes = new[] { typeof(EventEntry) };

                _sens = new Dictionary<Sensitivity, EventEntryAttribute>(4);
                _sens.Add(Sensitivity.Normal, new EventEntryAttribute { Flag = "normal", Resource = "EWS_Sensitivity_Normal" });    // 0
                _sens.Add(Sensitivity.Personal, new EventEntryAttribute { Flag = "personal", Resource = "EWS_Sensitivity_Personal" });    // 1
                _sens.Add(Sensitivity.Private, new EventEntryAttribute { Flag = "private", Resource = "EWS_Sensitivity_Private" });  // 2
                _sens.Add(Sensitivity.Confidential, new EventEntryAttribute { Flag = "confidential", Resource = "EWS_Sensitivity_Confidential" });    // 3

                _showAs = new Dictionary<LegacyFreeBusyStatus, EventEntryAttribute>(6);
                _showAs.Add(LegacyFreeBusyStatus.Free, new EventEntryAttribute { Flag = "free", Resource = "EWS_ShowAs_Free" }); // 0
                _showAs.Add(LegacyFreeBusyStatus.Tentative, new EventEntryAttribute { Flag = "tentative", Resource = "EWS_ShowAs_Tentative" });   // 1
                _showAs.Add(LegacyFreeBusyStatus.Busy, new EventEntryAttribute { Flag = "busy", Resource = "EWS_ShowAs_Busy" }); // 2
                _showAs.Add(LegacyFreeBusyStatus.OOF, new EventEntryAttribute { Flag = "oof", Resource = "EWS_ShowAs_Oof" });   // 3
                _showAs.Add(LegacyFreeBusyStatus.WorkingElsewhere, new EventEntryAttribute { Flag = "wew", Resource = "EWS_ShowAs_Wew" });  // 4
                _showAs.Add(LegacyFreeBusyStatus.NoData, new EventEntryAttribute { Flag = "noData", Resource = "EWS_ShowAs_NoData" }); // 5
            }

            #endregion
        }

        #endregion
    }
}