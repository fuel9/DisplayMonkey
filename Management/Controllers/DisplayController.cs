using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using DisplayMonkey.Models;

namespace DisplayMonkey.Controllers
{
    public class DisplayController : Controller
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        //
        // GET: /Display/

        public ActionResult Index(int canvasId = 0, int locationId = 0, string name = null, string host = null)
        {
            Navigation.SaveCurrent();

            IQueryable<Display> displays = db.Displays
                .Include(d => d.Canvas)
                .Include(d => d.Location)
                ;

            if (canvasId > 0)
            {
                displays = displays
                    .Where(d => d.CanvasId == canvasId)
                    ;
            }

            if (locationId > 0)
            {
                IEnumerable<int> tree = db.Locations
                    .Find(locationId)
                    .SelfAndChildren
                    .Select(l => l.LocationId)
                    ;
                
                displays = displays
                    .Where(d => tree.Contains(d.LocationId))
                    ;
            }

            if (!string.IsNullOrEmpty(name))
            {
                displays = displays
                    .Where(d => d.Name.Contains(name))
                    ;
            }

            if (!string.IsNullOrEmpty(host))
            {
                displays = displays
                    .Where(d => d.Host.Contains(host))
                    ;
            }

            ViewBag.CanvasId = new SelectList(db.Canvases, "CanvasId", "Name", canvasId);
            ViewBag.LocationId = new SelectList(db.Locations, "LocationId", "Name", locationId);            
            
            return View(displays.ToList());
        }

        //
        // GET: /Display/Browse/5

        public ActionResult Browse(int id = 0)
        {
            string site = ConfigurationManager.AppSettings["presentationSite"];
            if (string.IsNullOrEmpty(site)) site = Request.Url.DnsSafeHost;
            
            string root = ConfigurationManager.AppSettings["presentationRoot"];
            if (string.IsNullOrEmpty(root)) root = "";
            if (root != "" && !site.EndsWith("/"))
                root += "/";
            
            string url = string.Format(
                "http://{0}/{1}getCanvas.aspx?display={2}",
                site,
                root,
                id
                );
            return Redirect(url);
        }

        //
        // GET: /Display/Create

        public ActionResult Create(int canvasId = 0, int locationId = 0)
        {
            ViewBag.CanvasId = new SelectList(db.Canvases, "CanvasId", "Name", canvasId);
            ViewBag.LocationId = new SelectList(db.Locations, "LocationId", "Name", locationId);
            return View();
        }

        //
        // POST: /Display/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Display display)
        {
            if (ModelState.IsValid)
            {
                db.Displays.Add(display);
                db.SaveChanges();
                Navigation.Restore();
                return RedirectToAction("Index");
            }

            ViewBag.CanvasId = new SelectList(db.Canvases, "CanvasId", "Name", display.CanvasId);
            ViewBag.LocationId = new SelectList(db.Locations, "LocationId", "Name", display.LocationId);
            return View(display);
        }

        //
        // GET: /Display/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Display display = db.Displays.Find(id);
            if (display == null)
            {
                return HttpNotFound();
            }
            ViewBag.CanvasId = new SelectList(db.Canvases, "CanvasId", "Name", display.CanvasId);
            ViewBag.LocationId = new SelectList(db.Locations, "LocationId", "Name", display.LocationId);
            return View(display);
        }

        //
        // POST: /Display/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Display display)
        {
            if (ModelState.IsValid)
            {
                db.Entry(display).State = EntityState.Modified;
                db.SaveChanges();
                Navigation.Restore();
                return RedirectToAction("Index");
            }
            ViewBag.CanvasId = new SelectList(db.Canvases, "CanvasId", "Name", display.CanvasId);
            ViewBag.LocationId = new SelectList(db.Locations, "LocationId", "Name", display.LocationId);
            return View(display);
        }

        //
        // GET: /Display/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Display display = db.Displays.Find(id);
            if (display == null)
            {
                return HttpNotFound();
            }
            return View(display);
        }

        //
        // POST: /Display/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Display display = db.Displays.Find(id);
            db.Displays.Remove(display);
            Navigation.Restore();
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}