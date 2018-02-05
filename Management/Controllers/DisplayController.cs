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
using System.Configuration;
using DisplayMonkey.Models;

namespace DisplayMonkey.Controllers
{
    public class DisplayController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        private void FillCanvasSelectList(object selected = null)
        {
            var list = db.Canvases
                .OrderBy(c => c.Name)
                .ToList()
                ;

            ViewBag.Canvases = new SelectList(list, "CanvasId", "Name", selected);
        }

        private void FillLocationSelectList(object selected = null)
        {
            var list = db.Locations
                .AsEnumerable()
                .Select(l => new
                {
                    LocationId = l.LocationId,
                    Name = l.Area == null ? 
                        string.Format("{0} : {1}", l.Level.Name, l.Name) :
                        string.Format("{0} : {1} : {2}", l.Level.Name, l.Area.Name, l.Name),
                })
                .OrderBy(l => l.Name)
                .ToList()
                ;

            ViewBag.Locations = new SelectList(list, "LocationId", "Name", selected);
        }

        //
        // GET: /Display/

        public ActionResult Index(int canvasId = 0, int locationId = 0, string name = null, string host = null)
        {
            IQueryable<Display> displays = db.Displays
                .Include(d => d.Canvas)
                .Include(d => d.Location)
                .OrderBy(d => d.Name)
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

            FillCanvasSelectList(canvasId);
            FillLocationSelectList(locationId);
            
            return View(displays.ToList());
        }

        //
        // GET: /Display/Browse/5

        public ActionResult Browse(int id = 0)
        {
            string path = Setting.GetSetting(db, Setting.Keys.PresentationSite).StringValue;

            if (string.IsNullOrWhiteSpace(path))
            {
                string site = ConfigurationManager.AppSettings["presentationSite"];
                if (string.IsNullOrEmpty(site)) site = Request.Url.DnsSafeHost;

                string root = ConfigurationManager.AppSettings["presentationRoot"];
                if (string.IsNullOrEmpty(root)) root = "";

                path = string.Format(
                    "http://{0}/{1}",
                    site,
                    root
                    );
            }

            if (!path.EndsWith("/")) path += "/";
            
            string url = string.Format(
                "{0}getCanvas.aspx?display={1}",
                path,
                id
                );
            return Redirect(url);
        }

        //
        // GET: /Display/Create

        public ActionResult Create(int canvasId = 0, int locationId = 0)
        {
            Display display = new Display(db, canvasId, locationId);

            FillCanvasSelectList(display.CanvasId);
            FillLocationSelectList(display.LocationId);

            return View(display);
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

                return RedirectToAction("Index");
            }

            FillCanvasSelectList(display.CanvasId);
            FillLocationSelectList(display.LocationId);

            return View(display);
        }

        //
        // GET: /Display/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Display display = db.Displays.Find(id);
            if (display == null)
            {
                return View("Missing", new MissingItem(id));
            }

            FillCanvasSelectList(display.CanvasId);
            FillLocationSelectList(display.LocationId);

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

                return RedirectToAction("Index");
            }

            FillCanvasSelectList(display.CanvasId);
            FillLocationSelectList(display.LocationId);

            return View(display);
        }

        //
        // GET: /Display/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Display display = db.Displays.Find(id);
            if (display == null)
            {
                return View("Missing", new MissingItem(id));
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