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
    public class ClockController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        private void FillClockTypeSelectList(Clock.ClockTypes? selected = null)
        {
            ViewBag.Types = selected.TranslatedSelectList();
        }

        //
        // GET: /Clock/Details/5

        public ActionResult Details(int id = 0)
        {
            Navigation.SaveCurrent();

            Clock clock = db.Clocks.Find(id);
            if (clock == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(clock);
        }

        //
        // GET: /Clock/Create

        public ActionResult Create()
        {
            Frame frame = TempData[FrameController.SelectorFrameKey] as Frame;

            if (frame == null || frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            Clock clock = new Clock()
            {
                Frame = frame,
            };

            clock.SetDefaultDuration();

            FillClockTypeSelectList();

            return View(clock);
        }

        //
        // POST: /Clock/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Clock clock, Frame frame)
        {
            if (ModelState.IsValid)
            {
                clock.Frame = frame;
                db.Clocks.Add(clock);
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index", "Frame");
            }

            FillClockTypeSelectList();

            clock.Frame = frame;

            return View(clock);
        }

        //
        // GET: /Clock/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Clock clock = db.Clocks.Find(id);
            if (clock == null)
            {
                return View("Missing", new MissingItem(id));
            }

            FillClockTypeSelectList();
            
            return View(clock);
        }

        //
        // POST: /Clock/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Clock clock, Frame frame)
        {
            if (ModelState.IsValid)
            {
                db.Entry(frame).State = EntityState.Modified;
                db.Entry(clock).State = EntityState.Modified;
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index");
            }

            FillClockTypeSelectList();

            clock.Frame = frame;

            return View(clock);
        }

        //
        // GET: /Clock/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Clock clock = db.Clocks.Find(id);
            if (clock == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(clock);
        }

        //
        // POST: /Clock/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Frame frame = db.Frames.Find(id);
            db.Frames.Remove(frame);
            db.SaveChanges();

            return Navigation.Restore() ?? RedirectToAction("Index", "Frame");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}