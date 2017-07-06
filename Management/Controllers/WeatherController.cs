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
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;

namespace DisplayMonkey.Controllers
{
    public class WeatherController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        private void FillWeatherTypeSelectList(WeatherTypes? selected = null)
        {
            ViewBag.Types = selected.TranslatedSelectList();
        }

        //
        // GET: /Weather/Details/5

        public ActionResult Details(int id = 0)
        {
            Weather weather = db.Frames.Find(id) as Weather;
            if (weather == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(weather);
        }

        //
        // GET: /Weather/Create

        public ActionResult Create()
        {
            Frame frame = TempData[FrameController.SelectorFrameKey] as Frame;

            if (frame == null || frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            Weather weather = new Weather(frame, db);


            this.FillTemplatesSelectList(db, FrameTypes.Weather);
            FillWeatherTypeSelectList();

            return View(weather);
        }

        //
        // POST: /Weather/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Weather weather)
        {
            if (ModelState.IsValid)
            {
                db.Frames.Add(weather);
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Weather, weather.TemplateId);
            FillWeatherTypeSelectList();


            return View(weather);
        }

        //
        // GET: /Weather/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Weather weather = db.Frames.Find(id) as Weather;
            if (weather == null)
            {
                return View("Missing", new MissingItem(id));
            }

            this.FillTemplatesSelectList(db, FrameTypes.Weather, weather.TemplateId);
            FillWeatherTypeSelectList();
            
            return View(weather);
        }

        //
        // POST: /Weather/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Weather weather)
        {
            if (ModelState.IsValid)
            {
                db.Entry(weather).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Weather, weather.TemplateId);
            FillWeatherTypeSelectList();


            return View(weather);
        }

        //
        // GET: /Weather/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Weather weather = db.Frames.Find(id) as Weather;
            if (weather == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(weather);
        }

        //
        // POST: /Weather/Delete/5

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
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}