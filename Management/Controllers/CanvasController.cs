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
using DisplayMonkey.Language;

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


        private void makeFullscreenPanel(Canvas canvas)
        {
            canvas.Panels.Add(new FullScreen(db, canvas));
        }
        
        //
        // POST: /Canvas/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Canvas canvas)
        {
            if (ModelState.IsValid)
            {
                this.makeFullscreenPanel(canvas);
                db.Canvases.Add(canvas);
                db.SaveChanges();

                return RedirectToAction("Index");
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

                return RedirectToAction("Index");
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

            return RedirectToAction("Index");
        }


        //
        // GET: /Canvas/Copy

        public ActionResult Copy(int id = 0)
        {
            Canvas canvas = db.Canvases.Find(id);
            if (canvas == null)
            {
                return View("Missing", new MissingItem(id));
            }

            ViewBag.PanelCount = canvas.Panels.Count;
            ViewBag.FrameCount = canvas.Panels.Sum(p => p.Frames.Count);
            ViewBag.LocationCount = canvas.Panels.Sum(p => p.Frames.Sum(f => f.Locations.Count));
            CanvasCopy canvasCopy = new CanvasCopy()
            {
                CanvasId = canvas.CanvasId,
                Name = string.Format("{0} - {1}", canvas.Name, Resources.Copy),
                CopyPanels = ViewBag.PanelCount > 0,
                CopyFrames = ViewBag.FrameCount > 0,
                CopyFrameLocations = ViewBag.LocationCount > 0,
            };
            ViewBag.Canvas = canvas;
            return View(canvasCopy);
        }

        //
        // POST: /Canvas/Copy

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Copy(CanvasCopy canvasCopy)
        {
            int id = canvasCopy.CanvasId;
            IQueryable<Canvas> canvasQ = db.Canvases
                .AsQueryable()
                ;

            if (canvasCopy.CopyPanels)
            {
                //canvasQ = canvasQ
                //    .Include(c => c.Panels.Select(p => p.FullScreens))
                //    ;

                if (canvasCopy.CopyFrames)
                {
                    canvasQ = canvasQ
                        .Include(c => c.Panels.Select(p => p.Frames))
                        //.Include(c => c.Panels.Select(p => p.Frames.OfType<Video>().Select(v => v.Contents)))
                        ;

                    if (canvasCopy.CopyFrameLocations)
                        canvasQ = canvasQ.Include(c => c.Panels.Select(p => p.Frames.Select(f => f.Locations)));
                }
            }

            Canvas canvas = canvasQ
                .AsNoTracking()
                .FirstOrDefault(c => c.CanvasId == id)
                ;

            if (canvas == null)
            {
                return View("Missing", new MissingItem(id));
            }

            db.Entry(canvas).State = EntityState.Detached;

            canvas.Name = canvasCopy.Name;
            if (!canvasCopy.CopyPanels)
            {
                canvas.Panels.Clear();
                this.makeFullscreenPanel(canvas);
            }

            if (ModelState.IsValid)
            {
                db.Canvases.Add(canvas);
                db.SaveChanges();

                return RedirectToAction("Details", new { id = canvas.CanvasId });
            }

            ViewBag.PanelCount = canvas.Panels.Count;
            ViewBag.FrameCount = canvas.Panels.Sum(p => p.Frames.Count);
            ViewBag.LocationCount = canvas.Panels.Sum(p => p.Frames.Sum(f => f.Locations.Count));
            canvasCopy.CopyPanels = ViewBag.PanelCount > 0;
            canvasCopy.CopyFrames = ViewBag.FrameCount > 0;
            canvasCopy.CopyFrameLocations = ViewBag.LocationCount > 0;
            ViewBag.Canvas = canvas;
            return View(canvasCopy);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}