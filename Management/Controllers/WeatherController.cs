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
    public class WeatherController : Controller
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        //
        // GET: /Weather/Details/5

        public ActionResult Details(int id = 0)
        {
            Navigation.SaveCurrent();

            Weather weather = db.Weathers.Find(id);
            if (weather == null)
            {
                return HttpNotFound();
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

            Weather weather = new Weather()
            {
                Frame = frame,
            };

            weather.SetDefaultDuration();

            ViewBag.Types = new SelectList(DisplayMonkey.Models.Weather.WeatherTypes, "Type", "Name");

            return View(weather);
        }

        //
        // POST: /Weather/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Weather weather, Frame frame)
        {
            if (ModelState.IsValid)
            {
                weather.Frame = frame;
                db.Weathers.Add(weather);
                db.SaveChanges();
                Navigation.Restore();
                return RedirectToAction("Index");
            }

            ViewBag.Types = new SelectList(DisplayMonkey.Models.Weather.WeatherTypes, "Type", "Name");

            weather.Frame = frame;

            return View(weather);
        }

        //
        // GET: /Weather/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Weather weather = db.Weathers.Find(id);
            if (weather == null)
            {
                return HttpNotFound();
            }

            ViewBag.Types = new SelectList(DisplayMonkey.Models.Weather.WeatherTypes, "Type", "Name");
            
            return View(weather);
        }

        //
        // POST: /Weather/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Weather weather, Frame frame)
        {
            if (ModelState.IsValid)
            {
                db.Entry(frame).State = EntityState.Modified;
                db.Entry(weather).State = EntityState.Modified;
                db.SaveChanges();
                Navigation.Restore();
                return RedirectToAction("Index");
            }

            ViewBag.Types = new SelectList(DisplayMonkey.Models.Weather.WeatherTypes, "Type", "Name");

            weather.Frame = frame;

            return View(weather);
        }

        //
        // GET: /Weather/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Weather weather = db.Weathers.Find(id);
            if (weather == null)
            {
                return HttpNotFound();
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
            Navigation.Restore();
            return RedirectToAction("Index", "Frame");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}