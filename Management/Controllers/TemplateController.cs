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
    public class TemplateController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        // GET: /Template/
        public ActionResult Index(FrameTypes? frameType)
        {
            IQueryable<Template> list = db.Templates;

            if (frameType.HasValue)
            {
                list = list
                    .Where(s => s.FrameType == frameType.Value)
                    ;
            }

            list = list
                .OrderBy(t => t.FrameType)
                .ThenBy(t => t.Name)
                ;

            FillFrameTypesSelectList();

            return View(list.ToList());
        }

        // GET: /Template/Create
        public ActionResult Create()
        {
            FillFrameTypesSelectList();
            return View();
        }

        // POST: /Template/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include="TemplateId,Name,Html,FrameType,Version")] Template template)
        {
            if (ModelState.IsValid)
            {
                db.Templates.Add(template);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            FillFrameTypesSelectList(template.FrameType);
            return View(template);
        }

        // GET: /Template/Edit/5
        public ActionResult Edit(System.Int32 id = 0)
        {
            Template template = db.Templates.Find(id);
            if (template == null)
            {
                return View("Missing", new MissingItem(id));
            }

            FillFrameTypesSelectList(template.FrameType);
            return View(template);
        }

        // POST: /Template/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "TemplateId,Name,Html,FrameType,Version")] Template template)
        {
            if (ModelState.IsValid)
            {
                db.Entry(template).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            FillFrameTypesSelectList(template.FrameType);
            return View(template);
        }

        // GET: /Template/Delete/5
        public ActionResult Delete(System.Int32 id = 0)
        {
            Template template = db.Templates.Find(id);
            if (template == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(template);
        }

        // POST: /Template/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Template template = db.Templates.Find(id);
            db.Templates.Remove(template);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void FillFrameTypesSelectList(FrameTypes? selected = null)
        {
            ViewBag.FrameTypes = selected.TranslatedSelectList();
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
