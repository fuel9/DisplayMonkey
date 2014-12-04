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

        private static Regex _emailRgx = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b");

        // GET: /Outlook/Details/5
        public ActionResult Details(int id = 0)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            Outlook outlook = db.Outlook.Find(id);
            if (outlook == null)
            {
                //return HttpNotFound();
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

            FillModesSelectList();
            FillVersionsSelectList();

            return View(outlook);
        }

        // POST: /Outlook/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FrameId,Mode,Name,Account,PasswordUnmasked,Mailbox,EwsVersion,ShowEvents")] Outlook outlook, Frame frame)
        {
            if (ModelState.IsValid)
            {
                Match lnk = _emailRgx.Match(outlook.Account);
                if (lnk.Success) outlook.Account = lnk.Value;
                if (!string.IsNullOrWhiteSpace(outlook.Mailbox))
                {
                    lnk = _emailRgx.Match(outlook.Mailbox);
                    if (lnk.Success) outlook.Mailbox = lnk.Value;
                }

                outlook.Frame = frame;
                db.Outlook.Add(outlook);
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index");
            }

            FillModesSelectList();
            FillVersionsSelectList();

            outlook.Frame = frame;
            
            return View(outlook);
        }

        // GET: /Outlook/Edit/5
        public ActionResult Edit(int id = 0)
        {
            Outlook outlook = db.Outlook.Find(id);
            if (outlook == null)
            {
                return View("Missing", new MissingItem(id));
            }
            
            FillModesSelectList();
            FillVersionsSelectList();

            return View(outlook);
        }

        // POST: /Outlook/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FrameId,Mode,Name,Account,PasswordUnmasked,Mailbox,EwsVersion,ShowEvents")] Outlook outlook, Frame frame)
        {
            if (ModelState.IsValid)
            {
                Match lnk = _emailRgx.Match(outlook.Account);
                if (lnk.Success) outlook.Account = lnk.Value;
                if (!string.IsNullOrWhiteSpace(outlook.Mailbox))
                {
                    lnk = _emailRgx.Match(outlook.Mailbox);
                    if (lnk.Success) outlook.Mailbox = lnk.Value;
                }

                db.Entry(frame).State = EntityState.Modified;
                db.Entry(outlook).State = EntityState.Modified;
                db.Entry(outlook).Property(l => l.Url).IsModified = false;
                db.Entry(outlook).Property(l => l.Password).IsModified = outlook._passwordSet;
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index");
            }

            FillModesSelectList();
            FillVersionsSelectList();

            outlook.Frame = frame;
            
            return View(outlook);
        }

        // GET: /Outlook/Delete/5
        public ActionResult Delete(int id = 0)
        {
            Outlook outlook = db.Outlook.Find(id);
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

            return Navigation.Restore() ?? RedirectToAction("Index", "Frame");
        }

        private void FillModesSelectList(OutlookModes? selected = null)
        {
            ViewBag.Modes = selected.TranslatedSelectList();
        }

        private void FillVersionsSelectList(OutlookEwsVersions? selected = null)
        {
            ViewBag.EwsVersions = selected.TranslatedSelectList();
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
