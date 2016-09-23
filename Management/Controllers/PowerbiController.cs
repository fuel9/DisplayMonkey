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
    public class PowerbiController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        // GET: /Powerbi/Details/5
        public ActionResult Details(int id = 0)
        {
            this.SaveReferrer(true);
            
            Powerbi powerbi = db.Frames.Find(id) as Powerbi;
            if (powerbi == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(powerbi);
        }

        // GET: /Powerbi/Create
        public ActionResult Create()
        {
            Frame frame = TempData[FrameController.SelectorFrameKey] as Frame;

            if (frame == null || frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            Powerbi powerbi = new Powerbi(frame, db);

            this.FillTemplatesSelectList(db, FrameTypes.Powerbi);
            FillTypesSelectList();
            FillAccountsSelectList();
            

            return View(powerbi);
        }

        // POST: /Powerbi/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Powerbi powerbi)
        {
            if (ModelState.IsValid)
            {
                db.Frames.Add(powerbi);
                db.SaveChanges();

                return this.RestoreReferrer() ?? RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Powerbi, powerbi.TemplateId);
            FillTypesSelectList(powerbi.Type);
            FillAccountsSelectList(powerbi.AccountId);
            

            return View(powerbi);
        }

        // GET: /Powerbi/Edit/5
        public ActionResult Edit(int id = 0)
        {
            Powerbi powerbi = db.Frames.Find(id) as Powerbi;
            if (powerbi == null)
            {
                return View("Missing", new MissingItem(id));
            }

            this.FillTemplatesSelectList(db, FrameTypes.Powerbi, powerbi.TemplateId);
            FillTypesSelectList(powerbi.Type);
            FillAccountsSelectList(powerbi.AccountId);
            

            return View(powerbi);
        }

        // POST: /Powerbi/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Powerbi powerbi)
        {
            if (ModelState.IsValid)
            {
                db.Entry(powerbi).State = EntityState.Modified;
                db.SaveChanges();

                return this.RestoreReferrer() ?? RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Powerbi, powerbi.TemplateId);
            FillTypesSelectList(powerbi.Type);
            FillAccountsSelectList(powerbi.AccountId);
            
            
            return View(powerbi);
        }

        // GET: /Powerbi/Delete/5
        public ActionResult Delete(int id = 0)
        {
            Powerbi powerbi = db.Frames.Find(id) as Powerbi;
            if (powerbi == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(powerbi);
        }

        // POST: /Powerbi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Frame frame = db.Frames.Find(id);
            db.Frames.Remove(frame);
            db.SaveChanges();

            return this.RestoreReferrer(true) ?? RedirectToAction("Index", "Frame");
        }

        private void FillTypesSelectList(PowerbiTypes? selected = null)
        {
            ViewBag.Modes = selected.TranslatedSelectList();
        }

        private void FillAccountsSelectList(object selected = null)
        {
            ViewBag.Accounts = new SelectList(db.AzureAccounts, "AccountId", "Name", selected);
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
