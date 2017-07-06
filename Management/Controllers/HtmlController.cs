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
//using System.Net;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;

namespace DisplayMonkey.Controllers
{
    public class HtmlController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        // GET: /Html/Details/5
        public ActionResult Details(int id = 0)
        {
            Html html = db.Frames.Find(id) as Html;
            if (html == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(html);
        }

        // GET: /Html/Create
        public ActionResult Create()
        {
            Frame frame = TempData[FrameController.SelectorFrameKey] as Frame;

            if (frame == null || frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            Html html = new Html(frame, db)
            {
                Panel = db.Panels
                    .Include(p => p.Canvas)
                    .FirstOrDefault(p => p.PanelId == frame.PanelId),
            };

            this.FillTemplatesSelectList(db, FrameTypes.Html);
            
            return View(html);
        }

        // POST: /Html/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Html html)
        {
            if (ModelState.IsValid)
            {
                db.Frames.Add(html);
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            html.Panel = db.Panels
                .Include(p => p.Canvas)
                .FirstOrDefault(p => p.PanelId == html.PanelId)
                ;

            this.FillTemplatesSelectList(db, FrameTypes.Html, html.TemplateId);
            
            return View(html);
        }

        // GET: /Html/Edit/5
        public ActionResult Edit(int id = 0)
        {
            Html html = db.Frames.Find(id) as Html;
            if (html == null)
            {
                return View("Missing", new MissingItem(id));
            }

            this.FillTemplatesSelectList(db, FrameTypes.Html, html.TemplateId);

            return View(html);
        }

        // POST: /Html/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Html html)
        {
            if (ModelState.IsValid)
            {
                db.Entry(html).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }


            this.FillTemplatesSelectList(db, FrameTypes.Html, html.TemplateId);
            
            return View(html);
        }

        // GET: /Html/Delete/5
        public ActionResult Delete(int id = 0)
        {
            Html html = db.Frames.Find(id) as Html;
            if (html == null)
            {
                return View("Missing", new MissingItem(id));
            }

            return View(html);
        }

        //
        // GET: /Media/Preview/5

        //[Authorize]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Preview(int id)
        {
            Html html = db.Frames.Find(id) as Html;
            if (html == null)
            {
                return HttpNotFound();
            }

            return Content(html.Content);
        }

        // POST: /Html/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Frame frame = db.Frames.Find(id);
            db.Frames.Remove(frame);
            db.SaveChanges();

            return RedirectToAction("Index", "Frame");
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
