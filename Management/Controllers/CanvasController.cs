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

        private void fillSelectBackgroundImage(object selected = null)
        {
            var contents = db.Contents
                .Where(c => c.Type == ContentTypes.ContentType_Picture)
                .OrderBy(c => c.Name)
                .ToList()
                ;
            ViewBag.BackgroundImage = new SelectList(contents, "ContentId", "Name", selected);
        }

        //
        // GET: /Canvas/

        public ActionResult Index()
        {
            this.SaveReferrer();

            var canvas = db.Canvases
                .OrderBy(c => c.Name)
                ;

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
            this.SaveReferrer(true);
            
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
            fillSelectBackgroundImage();
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

                return this.RestoreReferrer() ?? RedirectToAction("Index");
            }

            fillSelectBackgroundImage(canvas.BackgroundImage);
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
            fillSelectBackgroundImage(canvas.BackgroundImage);
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
                db.SaveChanges();

                return this.RestoreReferrer() ?? RedirectToAction("Index");
            }
            fillSelectBackgroundImage(canvas.BackgroundImage);
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

            return this.RestoreReferrer(true) ?? RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}