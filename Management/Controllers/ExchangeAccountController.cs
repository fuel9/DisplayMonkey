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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using DisplayMonkey.Language;

namespace DisplayMonkey.Controllers
{
    public class ExchangeAccountController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        private static Regex _emailRgx = new Regex(Models.Constants.EmailMask);

        #region -------- EWS Validation --------

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

        private Uri resolveAccount(ExchangeAccount account)
        {
            ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;

            ExchangeService service = new ExchangeService((ExchangeVersion)account.EwsVersion)
            {
                Credentials = new WebCredentials(
                    account.Account,
                    RsaUtil.Decrypt(account.Password)
                    ),
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(account.Url))
                {
                    service.Url = new Uri(account.Url);
                }
                else
                {
                    service.AutodiscoverUrl(account.Account, RedirectionUrlValidationCallback);
                }

                var match = service.ResolveName(account.Account);
                if (match.Count == 0)
                {
                    throw new Exception();
                }
            }

            catch (Exception)
            {
                ModelState.AddModelError("Account", Resources.CouldNotBeResolved);
            }

            return service.Url;
        }

        #endregion

        //
        // GET: /ExchangeAccount/

        public ActionResult Index()
        {
            return View(
                db.ExchangeAccounts
                    .OrderBy(x => x.Name)
                    .ToList()
                );
        }

        //
        // GET: /ExchangeAccount/Create

        public ActionResult Create()
        {
            ExchangeAccount ews = new ExchangeAccount();
            ews.init(db);
            FillVersionsSelectList();
            return View(ews);
        }

        //
        // POST: /ExchangeAccount/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Account,PasswordUnmasked,Url,EwsVersion")] ExchangeAccount ews)
        {
            Match lnk = _emailRgx.Match(ews.Account);
            ews.Account = lnk.Success ? lnk.Value : "";

            ews.Url = resolveAccount(ews).OriginalString;   // only at creation

            if (ModelState.IsValid)
            {
                db.ExchangeAccounts.Add(ews);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            FillVersionsSelectList(ews.EwsVersion);
            return View(ews);
        }

        //
        // GET: /ExchangeAccount/Edit/5

        public ActionResult Edit(int id = 0)
        {
            ExchangeAccount ews = db.ExchangeAccounts.Find(id);
            if (ews == null)
            {
                return View("Missing", new MissingItem(id));
            }

            FillVersionsSelectList(ews.EwsVersion);
            return View(ews);
        }

        //
        // POST: /ExchangeAccount/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AccountId,Name,Account,PasswordUnmasked,Url,EwsVersion")] ExchangeAccount ews)
        {
            Match lnk = _emailRgx.Match(ews.Account);
            ews.Account = lnk.Success ? lnk.Value : "";

            resolveAccount(ews);

            if (ModelState.IsValid)
            {
                db.Entry(ews).State = EntityState.Modified;
                db.Entry(ews).Property(l => l.Password).IsModified = ews.PasswordSet;
                //db.Entry(ews).Property(l => l.Url).IsModified = false;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            FillVersionsSelectList(ews.EwsVersion);
            return View(ews);
        }

        //
        // GET: /ExchangeAccount/Delete/5

        public ActionResult Delete(int id = 0)
        {
            ExchangeAccount ews = db.ExchangeAccounts.Find(id);
            if (ews == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(ews);
        }

        //
        // POST: /ExchangeAccount/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ExchangeAccount ews = db.ExchangeAccounts.Find(id);
            db.ExchangeAccounts.Remove(ews);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void FillVersionsSelectList(OutlookEwsVersions? selected = null)
        {
            ViewBag.EwsVersions = selected.TranslatedSelectList();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}