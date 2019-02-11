/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2019 Fuel9 LLC and contributors
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
using DisplayMonkey.Language;
using DisplayMonkey.AzureUtil;
using System.Threading.Tasks;

namespace DisplayMonkey.Controllers
{
    public class OauthAccountController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        //
        // GET: /OauthAccount/

        public ActionResult Index()
        {
            return View(
                db.OauthAccounts
                    .OrderBy(x => x.Name)
                    .ToList()
                );
        }

        //
        // GET: /OauthAccount/Create

        public ActionResult Create()
        {
            OauthAccount account = new OauthAccount();
            FillProviders();
            return View(account);
        }

        //
        // POST: /OauthAccount/Create

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Provider,AppId,ClientId,ClientSecret")] OauthAccount account)
        {
            if (ModelState.IsValid)
            {
                db.OauthAccounts.Add(account);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            FillProviders(account.Provider);
            return View(account);
        }

        //
        // GET: /OauthAccount/Edit/5

        public ActionResult Edit(int id = 0)
        {
            OauthAccount account = db.OauthAccounts.Find(id);
            if (account == null)
            {
                return View("Missing", new MissingItem(id));
            }

            FillProviders(account.Provider);
            return View(account);
        }

        //
        // POST: /OauthAccount/Edit/5

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AccountId,Name,Provider,AppId,ClientId,ClientSecret")] OauthAccount account)
        {
            if (ModelState.IsValid)
            {
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            FillProviders(account.Provider);
            return View(account);
        }

        //
        // GET: /OauthAccount/Delete/5

        public ActionResult Delete(int id = 0)
        {
            OauthAccount account = db.OauthAccounts.Find(id);
            if (account == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(account);
        }

        //
        // POST: /OauthAccount/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OauthAccount account = db.OauthAccounts.Find(id);
            db.OauthAccounts.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void FillProviders(OauthProviders? selected = null)
        {
            ViewBag.Providers = selected.TranslatedSelectList();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}