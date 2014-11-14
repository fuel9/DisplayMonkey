using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using Microsoft.Exchange.WebServices.Data;

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

        private struct CalendarEntry 
        {
            public string Name;
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
                    TimeZoneInfo timeZone;
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

                    // mailbox calendar items
                    // prep filter
                    FolderId folderId = new FolderId(WellKnownFolderName.Calendar, new Mailbox(this.Mailbox));
                    SearchFilter.SearchFilterCollection searchFilterCollection =
                        new SearchFilter.SearchFilterCollection()
                        {
                            LogicalOperator = LogicalOperator.And,
                        };
                    searchFilterCollection.Add(new SearchFilter.Exists(AppointmentSchema.Subject));
                    searchFilterCollection.Add(new SearchFilter.IsGreaterThanOrEqualTo(
                        AppointmentSchema.DateTimeCreated, DateTime.Now
                        ));
                    searchFilterCollection.Add(new SearchFilter.IsGreaterThanOrEqualTo(
                        AppointmentSchema.DateTimeCreated, DateTime.Now.AddHours(this.HourWindow)
                        ));


                    
                    
                    // load template
                    string template = File.ReadAllText(_templatePath);

                    // fill template
                    if (FrameId > 0)
                    {
                        HttpServerUtility util = HttpContext.Current.Server;
                        html = string.Format(
                            template,
                            util.HtmlEncode(""),
                            util.HtmlEncode("")
                            );
                    }
                }

                //catch (ServiceResponseException ex)
                catch (Exception ex)
                {
                    html = ex.Message;
                }

                return html;
            }
        }

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
            else
            {
                // In all other cases, return false.
                return false;
            }
        }

        public string Account = "";
        public string Password = "";
        public string Mailbox = "";
        public int HourWindow = 24;
        public ExchangeVersion Version = ExchangeVersion.Exchange2010_SP2;
    }
}