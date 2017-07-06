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

using System.Web.Script.Serialization;

namespace DisplayMonkey.Controllers
{
    public class PanelController : BaseController
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


        //
        // GET: /Panel/

        public ActionResult Index(int canvasId = 0, string name = "")
        {
            IQueryable<Panel> list = db.Panels
                .OrderBy(p => p.Canvas.Name)
                .ThenBy(p => p.Name)
                ;

            if (!String.IsNullOrEmpty(name) || canvasId > 0)
            {
                list = list
                    .Where(s => (
                        (canvasId == 0 || s.CanvasId == canvasId) &&
                        (String.IsNullOrEmpty(name) || s.Name.Contains(name))
                    )
                );
            }

            FillCanvasSelectList(canvasId);

            return View(list.ToList());
        }

        //
        // GET: /Panel/Details/5

        public ActionResult Details(int id = 0)
        {
            Panel panel = db.Panels.Find(id);
            if (panel == null)
            {
                return View("Missing", new MissingItem(id));
            }

            panel.Frames = db.Frames
                .Where(f => f.PanelId == panel.PanelId)
                .OrderBy(f => f.Sort == null ? (float)f.FrameId : (float)f.Sort)
                .ThenBy(f => f.FrameId)
                .ToList()
                ;

            return View(panel);
        }

        //
        // GET: /Panel/Create

        public ActionResult Create(int canvasId = 0)
        {
            Panel panel = new Panel(db);

            FillCanvasSelectList(canvasId);

            return View(panel);
        }

        //
        // POST: /Panel/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Panel panel)
        {
            if (ModelState.IsValid)
            {
                db.Panels.Add(panel);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            FillCanvasSelectList(panel.CanvasId);
            
            return View(panel);
        }

        //
        // GET: /Panel/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Panel panel = db.Panels.Find(id);
            if (panel == null)
            {
                return View("Missing", new MissingItem(id));
            }

            return View(panel);
        }

        //
        // POST: /Panel/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Panel panel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(panel).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(panel);
        }

        //
        // GET: /Panel/EditFS/5

        public ActionResult EditFS(int id = 0)
        {
            FullScreen fs = db.Panels
                .OfType<FullScreen>()
                //.Find(id)
                //.FullScreens
                .FirstOrDefault(f => f.PanelId == id)
                ;

            if (fs == null)
            {
                return View("Missing", new MissingItem(id));
            }

            //fs.FadeLength = fs.Panel.FadeLength;

            return View(fs);
        }

        //
        // POST: /Panel/EditFS/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFS(FullScreen fs)
        {
            if (ModelState.IsValid)
            {
                Panel panel = db.Panels.Find(fs.PanelId);
                panel.FadeLength = fs.FadeLength;
                db.Entry(panel).State = EntityState.Modified;
                //db.Entry(fs).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            
            return View(fs);
        }

        //
        // GET: /Panel/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Panel panel = db.Panels.Find(id);
            if (panel == null)
            {
                return View("Missing", new MissingItem(id));
            }

            return View(panel);
        }

        //
        // POST: /Panel/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Panel panel = db.Panels.Find(id);

            if (!panel.IsFullscreen)
            {
                db.Panels.Remove(panel);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /Panel/SortFrames/5

        public ActionResult SortFrames(int id = 0)
        {
            var list = db.Frames
                .Where(f => f.PanelId == id)
                .Include(f => f.Panel.Canvas)
                .AsEnumerable()             // sort in controller
                .OrderBy(f => f.Sort == null ? (float)f.FrameId : (float)f.Sort)
                .ThenBy(f => f.FrameId)
                ;

            return View(list.ToList());
        }

        //
        // POST: /Panel/Delete/5

        private class SortedId
        {
            public int newSort { get; set; }
            public int oldSort { get; set; }
            public int frameId { get; set; }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SortFrames(int panelId, string json)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var sortedIds = ser.Deserialize<List<SortedId>>(json);
            int prevFrameId = 0; sortedIds.ForEach(s =>
            {
                Frame frame = db.Frames.Find(s.frameId);
                frame.Sort = s.newSort;
                db.Entry(frame).Property(f => f.Sort).IsModified = true;
                prevFrameId = s.frameId;
            });
            db.SaveChanges();
            return RedirectToAction("Details", new { id = panelId });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}