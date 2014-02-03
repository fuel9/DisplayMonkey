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
    public class PanelController : Controller
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        //
        // GET: /Panel/

        public ActionResult Index(int canvasId = 0, string name = "")
        {
            Navigation.SaveCurrent();

            var list = from l in db.Panels
                       select l;

            if (!String.IsNullOrEmpty(name) || canvasId > 0)
            {
                list = list.Where(s => (
                        (canvasId == 0 || s.CanvasId == canvasId) &&
                        (String.IsNullOrEmpty(name) || s.Name.Contains(name))
                    )
                );
            }

            FillCanvasesSelectList();

            return View(list.ToList());
        }

        //
        // GET: /Panel/Details/5

        public ActionResult Details(int id = 0)
        {
            Navigation.SaveCurrent();

            Panel panel = db.Panels.Find(id);
            if (panel == null)
            {
                return HttpNotFound();
            }

            return View(panel);
        }

        //
        // GET: /Panel/Create

        public ActionResult Create(int canvasId = 0)
        {
            ViewBag.CanvasId = new SelectList(db.Canvases, "CanvasId", "Name", canvasId);

            return View();
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

                Navigation.Restore();
                return RedirectToAction("Index");
            }

            ViewBag.CanvasId = new SelectList(db.Canvases, "CanvasId", "Name", panel.CanvasId);
            
            return View(panel);
        }

        //
        // GET: /Panel/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Panel panel = db.Panels.Find(id);
            if (panel == null)
            {
                return HttpNotFound();
            }

            ViewBag.CanvasId = new SelectList(db.Canvases, "CanvasId", "Name", panel.CanvasId);

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

                Navigation.Restore();
                return RedirectToAction("Index");
            }

            ViewBag.CanvasId = new SelectList(db.Canvases, "CanvasId", "Name", panel.CanvasId);
            return View(panel);
        }

        //
        // GET: /Panel/EditFS/5

        public ActionResult EditFS(int id = 0)
        {
            FullScreen panel = db.Panels.Find(id).FullScreens.First();
            if (panel == null)
            {
                return HttpNotFound();
            }

            return View(panel);
        }

        //
        // POST: /Panel/EditFS/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFS(FullScreen panel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(panel).State = EntityState.Modified;
                db.SaveChanges();

                Navigation.Restore();
                return RedirectToAction("Index");
            }
            
            return View(panel);
        }

        //
        // GET: /Panel/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Panel panel = db.Panels.Find(id);
            if (panel == null)
            {
                return HttpNotFound();
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
            db.Panels.Remove(panel);
            db.SaveChanges();

            Navigation.Restore();
            return RedirectToAction("Index");
        }

        private void FillCanvasesSelectList(object selected = null)
        {
            var query = from c in db.Canvases
                        orderby c.Name
                        select c;
            ViewBag.CanvasId = new SelectList(query, "CanvasId", "Name", selected);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}