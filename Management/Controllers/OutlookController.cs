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

        private static Regex _emailRgx = new Regex(Models.ExchangeAccount._emailMsk);

        // GET: /Outlook/Details/5
        public ActionResult Details(int id = 0)
        {
            this.SaveReferrer(true);
            
            Outlook outlook = db.Outlooks.Find(id);
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

            Outlook outlook = new Outlook()
            {
                Frame = frame,
            };

            outlook.init(db);

            this.FillTemplatesSelectList(db, FrameTypes.Outlook);
            FillModesSelectList();
            FillAccountsSelectList();

            return View(outlook);
        }

        // POST: /Outlook/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FrameId,Mode,Name,AccountId,Mailbox,ShowEvents")] Outlook outlook, Frame frame)
        {
            if (!string.IsNullOrWhiteSpace(outlook.Mailbox))
            {
                Match lnk = _emailRgx.Match(outlook.Mailbox);
                outlook.Mailbox = lnk.Success ? lnk.Value : "";
            }

            if (ModelState.IsValid)
            {
                outlook.Frame = frame;
                db.Outlooks.Add(outlook);
                db.SaveChanges();

                return this.RestoreReferrer() ?? RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Outlook, outlook.Frame.TemplateId);
            FillModesSelectList(outlook.Mode);
            FillAccountsSelectList(outlook.AccountId);

            outlook.Frame = frame;
            
            return View(outlook);
        }

        // GET: /Outlook/Edit/5
        public ActionResult Edit(int id = 0)
        {
            Outlook outlook = db.Outlooks.Find(id);
            if (outlook == null)
            {
                return View("Missing", new MissingItem(id));
            }

            this.FillTemplatesSelectList(db, FrameTypes.Outlook, outlook.Frame.TemplateId);
            FillModesSelectList(outlook.Mode);
            FillAccountsSelectList(outlook.AccountId);

            return View(outlook);
        }

        // POST: /Outlook/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FrameId,Mode,Name,AccountId,Mailbox,ShowEvents")] Outlook outlook, Frame frame)
        {
            if (!string.IsNullOrWhiteSpace(outlook.Mailbox))
            {
                Match lnk = _emailRgx.Match(outlook.Mailbox);
                outlook.Mailbox = lnk.Success ? lnk.Value : null;
            }

            if (ModelState.IsValid)
            {
                db.Entry(frame).State = EntityState.Modified;
                db.Entry(outlook).State = EntityState.Modified;
                db.SaveChanges();

                return this.RestoreReferrer() ?? RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Outlook, outlook.Frame.TemplateId);
            FillModesSelectList(outlook.Mode);
            FillAccountsSelectList(outlook.AccountId);

            outlook.Frame = frame;
            
            return View(outlook);
        }

        // GET: /Outlook/Delete/5
        public ActionResult Delete(int id = 0)
        {
            Outlook outlook = db.Outlooks.Find(id);
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

            return this.RestoreReferrer(true) ?? RedirectToAction("Index", "Frame");
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
