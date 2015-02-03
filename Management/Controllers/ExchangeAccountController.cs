using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;
using System.Text.RegularExpressions;

namespace DisplayMonkey.Controllers
{
    public class ExchangeAccountController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        private static Regex _emailRgx = new Regex(Models.ExchangeAccount._emailMsk);

        //
        // GET: /ExchangeAccount/

        public ActionResult Index()
        {
            return View(db.ExchangeAccounts.ToList());
        }

        //
        // GET: /ExchangeAccount/Create

        public ActionResult Create()
        {
            ExchangeAccount ews = new ExchangeAccount();
            ews.init();
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

            if (ModelState.IsValid)
            {
                db.Entry(ews).State = EntityState.Modified;
                db.Entry(ews).Property(l => l.Password).IsModified = ews._passwordSet;
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