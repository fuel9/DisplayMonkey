using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
//using System.Data;
using System.Configuration;
using System.Net;
//using System.Drawing;
//using System.Drawing.Imaging;
//using Microsoft.SharePoint.Client;
//using System.Security;

namespace DisplayMonkey
{
	/// <summary>
	/// Summary description for getHtml
	/// </summary>
	public class getHtml : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			HttpRequest Request = context.Request;
			HttpResponse Response = context.Response;

			try
			{
                int frameId = Convert.ToInt32(Request.QueryString["frame"]);

                Html html = new Html(frameId);

                //SecureString ssPassword = new SecureString();
                //foreach (char c in "xxxxxx")
                //{
                //    ssPassword.AppendChar(c);
                //}
                //client.Credentials = new SharePointOnlineCredentials("account", ssPassword);
                //client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                ////data = client.DownloadData(url);
                //string html = client.DownloadString(@"https://sharepoint.com/sites/PCI/SitePages/Home.aspx");

                // prevent client caching, return PNG
                Response.Clear();
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetSlidingExpiration(true);
                Response.Cache.SetNoStore();
                Response.ContentType = "text/html";
                Response.Output.Write(html.Content);
                Response.Output.Flush();
            }

			catch (Exception ex)
			{
				Response.Write(ex.Message);
			}
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}