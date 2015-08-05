using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;

namespace DisplayMonkey.Controllers
{
    public class SettingController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        // GET: /Setting/
        public ActionResult Index()
        {
            this.SaveReferrer();

            var list = db.Settings
                .ToList()
                .OrderBy(s => s.ResourceId)     // <-- must be behind .ToList() call
                ;

            return View(list);
        }

        // GET: /Setting/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setting setting = db.Settings.Find(id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            return View(setting);
        }

        // POST: /Setting/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Key,IntValue,IntValuePositive,StringValue,DecimalValue,DecimalValuePositive")] Setting setting)
        {
            if (ModelState.IsValid)
            {
                db.Settings.Attach(setting);
                db.Entry(setting).Property(p => p.Value).IsModified = true;
                db.SaveChanges();

                return this.RestoreReferrer() ?? RedirectToAction("Index");
            }
            return View(setting);
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
