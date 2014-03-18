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
    public class CanvasController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        //
        // GET: /Canvas/

        public ActionResult Index()
        {
            Navigation.SaveCurrent();

            var canvas = db.Canvases;
            return View(canvas.ToList());
        }

        //
        // POST: /Canvas/Data/5

        [HttpPost]
        public ActionResult Data(int id = 0)
        {
            object data = db.Canvases
                .Include(c => c.Panels)
                .Where(c => c.CanvasId == id)
                .AsEnumerable()
                .Select(c => new {
                    BackgroundImage = c.BackgroundImage,
                    BackgroundColor = c.BackgroundColor,
                    Width = c.Width,
                    Height = c.Height,
                    Panels = c.Panels.Where(p => !p.IsFullscreen).Select(p => new { 
                        PanelId = p.PanelId,
                        Name = p.Name,
                        Left = p.Left,
                        Top = p.Top,
                        Width = p.Width,
                        Height = p.Height,
                    }).ToArray(),
                })
                .FirstOrDefault()
                ;

            return Json(data);
        }

        //
        // GET: /Canvas/Details/5

        public ActionResult Details(int id = 0)
        {
            Navigation.SaveCurrent();
            
            Canvas canvas = db.Canvases.Find(id);
            if (canvas == null)
            {
                return View("Missing", new MissingItem(id));
            }

            canvas.Panels = db.Panels
                               .Where(l => l.CanvasId == id)
                               .ToList();

            return View(canvas);
        }

        //
        // GET: /Canvas/Create

        public ActionResult Create()
        {
            var contents = db.Contents.Where(c => c.Type == 0).ToList();
            ViewBag.BackgroundImage = new SelectList(contents, "ContentId", "Name");
            return View();
        }

        //
        // POST: /Canvas/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Canvas canvas)
        {
            if (ModelState.IsValid)
            {
                db.Canvases.Add(canvas);
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index");
            }

            ViewBag.BackgroundImage = new SelectList(db.Contents, "ContentId", "Name", canvas.BackgroundImage);
            return View(canvas);
        }

        //
        // GET: /Canvas/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Canvas canvas = db.Canvases.Find(id);
            if (canvas == null)
            {
                return View("Missing", new MissingItem(id));
            }
            var contents = db.Contents.Where(c => c.Type == 0).ToList();
            ViewBag.BackgroundImage = new SelectList(contents, "ContentId", "Name", canvas.BackgroundImage);
            return View(canvas);
        }

        //
        // POST: /Canvas/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Canvas canvas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(canvas).State = EntityState.Modified;
                //db.Canvases.Attach(canvas).Version++;
                

                //db.Entry(canvas).Entity.Version++;
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index");
            }
            ViewBag.BackgroundImage = new SelectList(db.Contents, "ContentId", "Name", canvas.BackgroundImage);
            return View(canvas);
        }

        //
        // GET: /Canvas/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Canvas canvas = db.Canvases.Find(id);
            if (canvas == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(canvas);
        }

        //
        // POST: /Canvas/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Canvas canvas = db.Canvases.Find(id);
            db.Canvases.Remove(canvas);
            db.SaveChanges();

            return Navigation.Restore() ?? RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}