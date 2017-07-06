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
using System.Net;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;
using System.Text.RegularExpressions;

namespace DisplayMonkey.Controllers
{
    public class OutlookController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        private static Regex _emailRgx = new Regex(Models.Constants.EmailMask);

        // GET: /Outlook/Details/5
        public ActionResult Details(int id = 0)
        {
            Outlook outlook = db.Frames.Find(id) as Outlook;
            if (outlook == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(outlook);
        }

        // GET: /Outlook/Create
        public ActionResult Create()
        {
            Frame frame = TempData[FrameController.SelectorFrameKey] as Frame;

            if (frame == null || frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            Outlook outlook = new Outlook(frame, db);

            this.FillTemplatesSelectList(db, FrameTypes.Outlook);
            FillModesSelectList();
            FillAccountsSelectList();
            FillPrivacySelectList();

            return View(outlook);
        }

        // POST: /Outlook/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Outlook outlook)
        {
            if (!string.IsNullOrWhiteSpace(outlook.Mailbox))
            {
                Match lnk = _emailRgx.Match(outlook.Mailbox);
                outlook.Mailbox = lnk.Success ? lnk.Value : "";
            }

            if (ModelState.IsValid)
            {
                db.Frames.Add(outlook);
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Outlook, outlook.TemplateId);
            FillModesSelectList(outlook.Mode);
            FillAccountsSelectList(outlook.AccountId);
            FillPrivacySelectList(outlook.Privacy);

            
            return View(outlook);
        }

        // GET: /Outlook/Edit/5
        public ActionResult Edit(int id = 0)
        {
            Outlook outlook = db.Frames.Find(id) as Outlook;
            if (outlook == null)
            {
                return View("Missing", new MissingItem(id));
            }

            this.FillTemplatesSelectList(db, FrameTypes.Outlook, outlook.TemplateId);
            FillModesSelectList(outlook.Mode);
            FillAccountsSelectList(outlook.AccountId);
            FillPrivacySelectList(outlook.Privacy);

            return View(outlook);
        }

        // POST: /Outlook/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Outlook outlook)
        {
            if (!string.IsNullOrWhiteSpace(outlook.Mailbox))
            {
                Match lnk = _emailRgx.Match(outlook.Mailbox);
                outlook.Mailbox = lnk.Success ? lnk.Value : null;
            }

            if (ModelState.IsValid)
            {
                db.Entry(outlook).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Outlook, outlook.TemplateId);
            FillModesSelectList(outlook.Mode);
            FillAccountsSelectList(outlook.AccountId);
            FillPrivacySelectList(outlook.Privacy);

            
            return View(outlook);
        }

        // GET: /Outlook/Delete/5
        public ActionResult Delete(int id = 0)
        {
            Outlook outlook = db.Frames.Find(id) as Outlook;
            if (outlook == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(outlook);
        }

        // POST: /Outlook/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Frame frame = db.Frames.Find(id);
            db.Frames.Remove(frame);
            db.SaveChanges();

            return RedirectToAction("Index", "Frame");
        }

        private void FillPrivacySelectList(OutlookPrivacy? selected = null)
        {
            ViewBag.Privacies = selected.TranslatedSelectList();
        }

        private void FillModesSelectList(OutlookModes? selected = null)
        {
            ViewBag.Modes = selected.TranslatedSelectList();
        }

        private void FillAccountsSelectList(object selected = null)
        {
            ViewBag.Accounts = new SelectList(db.ExchangeAccounts, "AccountId", "Name", selected);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
