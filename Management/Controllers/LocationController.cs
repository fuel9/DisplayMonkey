using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;

using System.Data.Entity.Infrastructure;
using System.Reflection;


namespace DisplayMonkey.Controllers
{
    public class LocationController : Controller
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        //
        // GET: /Location/

        public ActionResult Index(int levelId = 0, int areaId = 0, string name = null)
        {
            Navigation.SaveCurrent();
            
            IQueryable<Location> list = db.Locations;

            if (levelId > 0)
            {
                list = list
                    .Where(s => s.LevelId == levelId)
                    ;
            }

            if (areaId > 0)
            {
                list = list
                    .Where(s => s.AreaId == areaId)
                    ;
            }

            if (!String.IsNullOrEmpty(name))
            {
                list = list
                    .Where(s => s.Name.Contains(name))
                    ;
            }

            FillLevelsSelectList();
            FillTemperatureUnitSelectList();
            FillAreaSelectList(0);

            return View(list.ToList());
            //return View(db.Locations.ToList());
        }

        //
        // GET: /Location/Details/5

        public ActionResult Details(int id = 0)
        {
            Navigation.SaveCurrent();

            Location location = db.Locations.Find(id);
            if (location == null)
            {
                return HttpNotFound();
            }

            var locationIds = db.Locations
                .Find(id)
                .SelfAndChildren
                .ToList()
                .Select(l => l.LocationId)
                ;

            ViewBag.Displays = db.Displays
                .Where(d => locationIds.Any(l => l == d.LocationId))
                .OrderBy(d => d.Name)
                .ToList()
                ;

            ViewBag.Frames = db.Frames
                .Where(f => f.Locations.Any(l => locationIds.Contains(l.LocationId)))
                .ToList()
                ;

            return View(location);
        }

        //
        // GET: /Location/Create

        public ActionResult Create()
        {
            FillLevelsSelectList();
            FillTemperatureUnitSelectList();
            FillAreaSelectList(0);

            return View();
        }

        //
        // POST: /Location/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Location location)
        {
            if (ModelState.IsValid)
            {
                db.Locations.Add(location);
                db.SaveChanges();

                Navigation.Restore();

                return RedirectToAction("Index");
            }

            FillLevelsSelectList(location.LevelId);
            FillTemperatureUnitSelectList(location.TemperatureUnit);
            FillAreaSelectList(location.LocationId, location.AreaId);
            return View(location);
        }

        //
        // GET: /Location/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Location location = db.Locations.Find(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            FillLevelsSelectList(location.LevelId);
            FillTemperatureUnitSelectList(location.TemperatureUnit);
            FillAreaSelectList(location.LocationId, location.AreaId);

            return View(location);
        }

        //
        // POST: /Location/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Location location)
        {
            if (ModelState.IsValid)
            {
                db.Entry(location).State = EntityState.Modified;
                db.Entry(location).Property(l => l.LevelId).IsModified = false;
                db.Entry(location).Property(l => l.AreaId).IsModified = false;
                db.SaveChanges();

                Navigation.Restore();

                return RedirectToAction("Index");
            }
            FillLevelsSelectList(location.LevelId);
            FillTemperatureUnitSelectList(location.TemperatureUnit);
            FillAreaSelectList(location.LocationId, location.AreaId);
            return View(location);
        }

        //
        // GET: /Location/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Location location = db.Locations.Find(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            
            return View(location);
        }

        //
        // POST: /Location/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Location location = db.Locations.Find(id);
            db.Locations.Remove(location);
            db.SaveChanges();

            Navigation.Restore();

            return RedirectToAction("Index");
        }

        //private class HH { public int H1 = 0; public int H2 = 0; }

        public ActionResult Test()
        {
            //return JavaScript("<script type='text/javascript'>alert('Hello World!');</script>");
            //return File("e:\\permobil_displaymonkey_background.png", "image/png");
            //return Json(new () { H1 = 300, H2 = 400 }, JsonRequestBehavior.AllowGet);
            return Content("Test!");
        }

        private void FillLevelsSelectList(object selected = null)
        {
            var query = from d in db.Levels
                              orderby d.LevelId
                              select d;
            ViewBag.LevelId = new SelectList(query, "LevelId", "Name", selected);
        }

        private void FillAreaSelectList(int self, object selected = null)
        {
            var query = from d in db.Locations
                        where d.LocationId != self
                              orderby d.Name
                              select d;
            ViewBag.AreaId = new SelectList(query, "LocationId", "Name", selected);
        }

        private void FillTemperatureUnitSelectList(object selected = null)
        {
            ViewBag.TemperatureUnit = new SelectList(
                new []
                {
                    new {TemperatureUnit = "C"},
                    new {TemperatureUnit = "F"},
                },
                "TemperatureUnit", 
                "TemperatureUnit", 
                selected
            );
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}