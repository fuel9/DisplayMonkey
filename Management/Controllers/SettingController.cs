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

namespace DisplayMonkey.Controllers
{
    public class SettingController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        // GET: /Setting/
        public ActionResult Index()
        {
            Setting.Initialize(db);
            
            var list = db.Settings
                .ToList()

                .Where(s => !s.Hidden)                 // 1.4.0
                .OrderBy(s => s.GroupOrder)            // 1.4.0
                .ThenBy(s => s.GroupResourceId)        // 1.4.0
                .ThenBy(s => s.ResourceId)
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
        public ActionResult Edit(Setting setting)
        {
            if (ModelState.IsValid)
            {
                db.Settings.Attach(setting);
                db.Entry(setting).Property(p => p.RawValue).IsModified = true;
                db.SaveChanges();

                return RedirectToAction("Index");
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
