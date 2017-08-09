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
    public class YouTubeController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        private static Regex _youTubeLink = new Regex(@"((?<=(?:v|i)=)[a-zA-Z0-9-]+(?=&))|(?<=(?:v|i)\/)[^&\n]+|(?<=embed\/)[^""&\n]+|(?<=(?:v|i)=)[^&\n]+|(?<=youtu.be\/)[^&\n]+");

        // GET: /YouTube/Details/5
        public ActionResult Details(int id = 0)
        {
            Youtube youtube = db.Frames.Find(id) as Youtube;
            if (youtube == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(youtube);
        }

        // GET: /YouTube/Create
        public ActionResult Create()
        {
            Frame frame = TempData[FrameController.SelectorFrameKey] as Frame;

            if (frame == null || frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            Youtube youtube = new Youtube(frame, db);

            this.FillTemplatesSelectList(db, FrameTypes.YouTube);
            FillAspectsSelectList();
            FillQualitySelectList();
            FillRatesSelectList();

            return View(youtube);
        }

        // POST: /YouTube/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Youtube youtube)
        {
            if (ModelState.IsValid)
            {
                Match lnk = _youTubeLink.Match(youtube.YoutubeId);
                if (lnk.Success) 
                    youtube.YoutubeId = lnk.Value;

                db.Frames.Add(youtube);
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.YouTube, youtube.TemplateId);
            FillAspectsSelectList();
            FillQualitySelectList();
            FillRatesSelectList();


            return View(youtube);
        }

        // GET: /YouTube/Edit/5
        public ActionResult Edit(int id = 0)
        {
            Youtube youtube = db.Frames.Find(id) as Youtube;
            if (youtube == null)
            {
                return View("Missing", new MissingItem(id));
            }

            this.FillTemplatesSelectList(db, FrameTypes.YouTube, youtube.TemplateId);
            FillAspectsSelectList(youtube.Aspect);
            FillQualitySelectList();
            FillRatesSelectList();

            return View(youtube);
        }

        // POST: /YouTube/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Youtube youtube)
        {
            if (ModelState.IsValid)
            {
                Match lnk = _youTubeLink.Match(youtube.YoutubeId);
                if (lnk.Success) youtube.YoutubeId = lnk.Value;
                
                db.Entry(youtube).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.YouTube, youtube.TemplateId);
            FillAspectsSelectList(youtube.Aspect);
            FillQualitySelectList();
            FillRatesSelectList();


            return View(youtube);
        }

        // GET: /YouTube/Delete/5
        public ActionResult Delete(int id = 0)
        {
            Youtube youtube = db.Frames.Find(id) as Youtube;
            if (youtube == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(youtube);
        }

        // POST: /YouTube/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Frame frame = db.Frames.Find(id);
            db.Frames.Remove(frame);
            db.SaveChanges();

            return RedirectToAction("Index", "Frame");
        }

        private void FillAspectsSelectList(YTAspect? selected = null)
        {
            ViewBag.Aspects = selected.TranslatedSelectList();
        }

        private void FillQualitySelectList(YTQuality? selected = null)
        {
            ViewBag.Qualities = selected.TranslatedSelectList();
        }

        private void FillRatesSelectList(YTRate? selected = null)
        {
            ViewBag.Rates = selected.TranslatedSelectList();
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
