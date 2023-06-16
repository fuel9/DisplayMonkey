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
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Azure.Identity;
using DisplayMonkey.Language;
using Microsoft.Graph;
using Microsoft.Graph.Models;

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
                        (currentEvent.Starts >= tomorrow ? currentEvent.Starts.ToShortDateString() + " " : "") +
                        currentEvent.Starts.ToShortTimeString(),
                        (currentEvent.Ends >= tomorrow ? currentEvent.Ends.ToShortDateString() + " " : "") +
                        currentEvent.Ends.ToShortTimeString()
                    );
                    TimeSpan gap = currentEvent.Ends.Subtract(locationTime);
                    if (gap.Hours > 0)
                        strCurrentStatus = string.Format(Resources.Outlook_EndsInHrsMin, (int)gap.TotalHours,
                            gap.Minutes);
                    else
                        strCurrentStatus = string.Format(Resources.Outlook_EndsInMin, gap.Minutes);
                }
                else if (firstEvent != null)
                {
                    availableTime = firstEvent.Starts.Subtract(locationTime);
                    if (availableTime.Hours > 0)
                        strCurrentStatus = string.Format(Resources.Outlook_AvailableForHrsMin,
                            (int)availableTime.TotalHours, availableTime.Minutes);
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
                        new { key = "subject", value = Resources.Outlook_Event },
                        new { key = "starts", value = Resources.Outlook_Starts },
                        new { key = "ends", value = Resources.Outlook_Ends },
                        new { key = "duration", value = Resources.Outlook_Duration },
                        new { key = "sensitivity", value = Resources.Outlook_Sensitivity },
                        new { key = "showAs", value = Resources.OutlookShowAs },
                        new { key = "noEvents", value = Resources.Outlook_NoEventsToday },
                        new { key = "bookingImpossible", value = Resources.Outlook_BookingImpossible },
                        new { key = "bookingSent", value = Resources.Outlook_BookingSent },
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
            public IEnumerable<EventEntry> events;
            public TimeSpan startTime = new TimeSpan(0, 0, 0);
            public TimeSpan endTime = new TimeSpan(23, 59, 59);
            public TimeZoneInfo timeZone = TimeZoneInfo.Local;
            public string DisplayName;

            public OutlookData(Outlook outlook, Location location, int reserveMinutes)
            {
                //OutlookData.initFromFrame(this, outlook, location, reserveMinutes);
            }

            #region -------- EWS Data Call --------f

            private OutlookData()
            {
            }

            public static async System.Threading.Tasks.Task<OutlookData> FromFrameAsync(Outlook outlook,
                Location location, int reserveMinutes)
            {
                var tenantId = ConfigurationManager.AppSettings["AzureActiveDirectoryTenantId"];
                var clientId = ConfigurationManager.AppSettings["AzureActiveDirectoryClientId"];
                if (tenantId == default || clientId == default)
                {
                    throw new Exception(
                        "App Settings must specify Azure Active Directory Client and Tenant ID to use Outlook frame");
                }

                var useNamePasswordCredential = new UsernamePasswordCredential(outlook.Account,
                    Encryptor.Current.Decrypt(outlook.Password), 
                    tenantId, 
                    clientId);
                
                // We don't provide a way for the user to consent to these scopes.
                // Either issue admin consent in AAD or use another method to trigger the user consent flow.
                var graphServiceClient = new GraphServiceClient(useNamePasswordCredential,
                    new[] { "user.read", "calendars.readwrite", "calendars.readwrite.shared", "MailboxSettings.Read" });
                
                // The data we'll now fill, and then return to the client.
                var data = new OutlookData();
                var locationTime = location.LocationTime;
                var locationToday = new DateTime(locationTime.Year, locationTime.Month, locationTime.Day);
                var windowBeg = locationToday;
                var windowEnd = locationToday.AddDays(1).AddMilliseconds(-1);

                // 1. Get the owner name of the specified mailboxes calendar and set the data.DisplayName property
                // 2. Check if the user has working hours set. If they do, set data.startTime and data.endTime and data.timeZone to those working hours
                // 3. If reserveMinutes > 0 then create a new event in the calendar for reserveMinutes number of minutes
                // 4. Get a list of events today
                // 5. Filter those events based on TBC criteria and map to data.events

                // Working Hours are stored in a users Mailbox Settings.
                var mailboxSettings = await graphServiceClient.Me.MailboxSettings.GetAsync(
                    config =>
                    {
                        config.QueryParameters.Select = new[] { "workingHours" };
                    });
                
                // If we have working hours set, then set the start and end times. Note we always assume Local time. This might be bad?
                // I'm unsure of the behaviour of the graph api when there are no working hours set.
                if (mailboxSettings.WorkingHours != null) 
                {
                    data.startTime = mailboxSettings.WorkingHours.StartTime?.DateTime.TimeOfDay ?? DateTime.Today.TimeOfDay;
                    data.endTime = mailboxSettings.WorkingHours.EndTime?.DateTime.TimeOfDay ??
                                   DateTime.Today.AddHours(23).AddMinutes(59).TimeOfDay;
                    data.timeZone = TimeZoneInfo.Local;
                }

                // Get the name of the owner of the calendar.
                var calendarDetails = await graphServiceClient.Users[outlook.Account].Calendar.GetAsync(c =>
                {
                    c.QueryParameters.Select = new[] { "owner" };
                });
                data.DisplayName = calendarDetails.Owner.Name;

                // Do we need to create an event in the calendar? 
                if (reserveMinutes > 0)
                {
                    _ = await graphServiceClient.Users[outlook.Account].Calendar.Events.PostAsync(new Event
                    {
                        Subject = outlook.BookingSubject,
                        Body = new ItemBody
                        {
                            Content = $"{Resources.Outlook_BookingOnDemand} | {Resources.DisplayMonkey}",
                        },
                        Start = new DateTimeTimeZone
                        {
                            DateTime = DateTime.Now.ToString("O")
                        },
                        End = new DateTimeTimeZone
                        {
                            DateTime = DateTime.Now.AddMinutes(reserveMinutes).ToString("O")
                        }
                    });
                }
                
                // Get list of events in the calendar
                var calendarView = await graphServiceClient.Users[outlook.Account].CalendarView.GetAsync(c =>
                {
                    c.Headers.Add("Prefer", "outlook.body-content-type=\"text\"");
                    c.QueryParameters.Select = new[] { "subject", "body", "start", "end", "bodyPreview", "createdDateTime", "sensitivity", "showAs"};
                    c.QueryParameters.StartDateTime = windowBeg.ToString("O");
                    c.QueryParameters.EndDateTime = windowEnd.ToString("O");
                });
                data.events = calendarView.Value
                    .Where(a =>
                        (outlook.Privacy != Models.OutlookPrivacy.OutlookPrivacy_NoClassified ||
                         a.Sensitivity == Sensitivity.Normal) &&
                        outlook.IsShowAsAllowed(a.ShowAs ?? FreeBusyStatus.Busy)
                    )
                    .Select(a => new EventEntry
                    {
                        Subject =
                            outlook.Privacy == Models.OutlookPrivacy.OutlookPrivacy_All ||
                            a.Sensitivity == Sensitivity.Normal
                                ? a.Subject
                                : DataAccess.StringResource($"EWS_Sensitivity_{a.Sensitivity.ToString()}"),
                        CreatedOn = a.CreatedDateTime?.DateTime ?? new DateTime(),
                        Starts = DateTime.Parse(a.Start.DateTime),
                        Ends = DateTime.Parse(a.End.DateTime),
                        Duration = DateTime.Parse(a.End.DateTime) - DateTime.Parse(a.Start.DateTime),
                        Sensitivity = a.Sensitivity ?? Sensitivity.Normal,
                        Today = locationToday,
                        ShowAs = a.ShowAs ?? FreeBusyStatus.Busy,
                    })
                    .ToList();

                return data;
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
            public FreeBusyStatus ShowAs { get; set; }

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
            public override IEnumerable<Type> SupportedTypes
            {
                get { return _supportedTypes; }
            }

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
                    foreach (var x in _sens.Where(e => e.Key == evt.Sensitivity)) // one and only
                    {
                        flags.Add(x.Value.Flag);
                        serialized["sensitivity"] = DataAccess.StringResource(x.Value.Resource);
                        break;
                    }

                    foreach (var x in _showAs.Where(e => e.Key == evt.ShowAs)) // one and only
                    {
                        flags.Add(x.Value.Flag);
                        serialized["showAs"] = DataAccess.StringResource(x.Value.Resource);
                        break;
                    }

                    serialized["flags"] = flags.ToArray();

                    DateTime tomorrow = evt.Today.AddDays(1);
                    serialized["starts"] = (evt.Starts >= tomorrow ? evt.Starts.ToShortDateString() + " " : "") +
                                           evt.Starts.ToShortTimeString();
                    serialized["ends"] = (evt.Ends >= tomorrow ? evt.Ends.ToShortDateString() + " " : "") +
                                         evt.Ends.ToShortTimeString();

                    if ((int)evt.Duration.TotalHours > 0)
                        serialized["duration"] = string.Format(Resources.Outlook_HrsMin, (int)evt.Duration.TotalHours,
                            evt.Duration.Minutes);
                    else
                        serialized["duration"] = string.Format(Resources.Outlook_Min, evt.Duration.Minutes);
                }

                return serialized;
            }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type,
                JavaScriptSerializer serializer)
            {
                throw new NotImplementedException();
            }

            #region Private members

            private static Dictionary<Sensitivity, EventEntryAttribute> _sens;
            private static Dictionary<FreeBusyStatus, EventEntryAttribute> _showAs;
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
                _sens.Add(Sensitivity.Normal,
                    new EventEntryAttribute { Flag = "normal", Resource = "EWS_Sensitivity_Normal" }); // 0
                _sens.Add(Sensitivity.Personal,
                    new EventEntryAttribute { Flag = "personal", Resource = "EWS_Sensitivity_Personal" }); // 1
                _sens.Add(Sensitivity.Private,
                    new EventEntryAttribute { Flag = "private", Resource = "EWS_Sensitivity_Private" }); // 2
                _sens.Add(Sensitivity.Confidential,
                    new EventEntryAttribute { Flag = "confidential", Resource = "EWS_Sensitivity_Confidential" }); // 3

                _showAs = new Dictionary<FreeBusyStatus, EventEntryAttribute>(6);
                _showAs.Add(FreeBusyStatus.Free,
                    new EventEntryAttribute { Flag = "free", Resource = "EWS_ShowAs_Free" }); // 0
                _showAs.Add(FreeBusyStatus.Tentative,
                    new EventEntryAttribute { Flag = "tentative", Resource = "EWS_ShowAs_Tentative" }); // 1
                _showAs.Add(FreeBusyStatus.Busy,
                    new EventEntryAttribute { Flag = "busy", Resource = "EWS_ShowAs_Busy" }); // 2
                _showAs.Add(FreeBusyStatus.Oof,
                    new EventEntryAttribute { Flag = "oof", Resource = "EWS_ShowAs_Oof" }); // 3
                _showAs.Add(FreeBusyStatus.WorkingElsewhere,
                    new EventEntryAttribute { Flag = "wew", Resource = "EWS_ShowAs_Wew" }); // 4
                _showAs.Add(FreeBusyStatus.Unknown,
                    new EventEntryAttribute { Flag = "noData", Resource = "EWS_ShowAs_NoData" }); // 5
            }

            #endregion
        }

        #endregion
    }
}