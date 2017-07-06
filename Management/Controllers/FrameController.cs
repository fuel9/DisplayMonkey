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

namespace DisplayMonkey.Controllers
{
    public class FrameController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        public const string SelectorFrameKey = "_selectorFrame";

        private void FillCanvasesSelectList(object selected = null)
        {
            var query = from c in db.Canvases
                        orderby c.Name
                        select c;
            ViewBag.CanvasId = new SelectList(query, "CanvasId", "Name", selected);
        }

        private class PanelSelectListItem
        {
            public int id { get; set; }
            public string name { get; set; }
            //public bool selected { get; set; }
        }

        private IQueryable<PanelSelectListItem> GetPanelsForCanvas(int canvasId = 0 /*, int panelId = 0*/)
        {
            IQueryable<Panel> list = db.Panels;

            if (canvasId > 0)
            {
                return db.Panels
                    .Where(p => p.CanvasId == canvasId)
                    .Select(p => new PanelSelectListItem { 
                        id = p.PanelId, 
                        name = p.Name, 
                        //selected = p.PanelId == panelId 
                    })
                    .OrderBy(p => p.name)
                    ;
            }
            else
            {
                return db.Panels
                    .Select(p => new PanelSelectListItem { 
                        id = p.PanelId,
                        name = canvasId == 0 ? p.Canvas.Name + " : " + p.Name : p.Name,
                        //selected = p.PanelId == panelId 
                    })
                    .OrderBy(p => p.name)
                    ;
            }
        }

        private void FillPanelsSelectList(object selected = null, int canvasId = 0)
        {
            ViewBag.PanelId = new SelectList(GetPanelsForCanvas(canvasId).AsEnumerable(), "id", "name", selected);
        }

        private void FillFrameTypeSelectList(FrameTypes? selected = null)
        {
            ViewBag.FrameType = selected.TranslatedSelectList(valueAsText: true);
        }

        private void FillTimingOptionsSelectList(Frame.TimingOptions? selected = null)
        {
            ViewBag.TimingOption = selected.TranslatedSelectList(valueAsText: false);
        }


        //
        // GET: /PanelsForCanvas/5

        public JsonResult PanelsForCanvas(int /*canvas*/ id = 0)
        {
            return Json(GetPanelsForCanvas(id).ToList(), JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Frame/

        public ActionResult Index(int canvasId = 0, int panelId = 0, FrameTypes? frameType = null, int? timingOption = null /*, int page = 1*/)
        {
            //if (page <= 0) page = 1;

            IQueryable<Frame> list = db.Frames
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                ;

            if (canvasId > 0)
            {
                list = list.Where(f => f.Panel.CanvasId == canvasId);
            }

            if (panelId > 0)
            {
                list = list.Where(f => f.PanelId == panelId);
            }

            if (frameType != null)
            {
                list = list.Where(Frame.FilterByFrameType(frameType));
            }

            DateTime dt = DateTime.Now;
            if (timingOption != null)
            {
                switch ((Frame.TimingOptions)timingOption)
                {
                    case Frame.TimingOptions.TimingOption_Pending:
                        list = list.Where(f => dt < f.BeginsOn);
                        break;

                    case Frame.TimingOptions.TimingOption_Current:
                        list = list.Where(f => (f.BeginsOn == null || f.BeginsOn <= dt) && (f.EndsOn == null || dt < f.EndsOn));
                        break;

                    case Frame.TimingOptions.TimingOption_Expired:
                        list = list.Where(f => f.EndsOn <= dt);
                        break;
                }
            }

            //ViewBag.TotalPages = (int)Math.Ceiling((float)list.Count() / 20.0);
            //ViewBag.CurrentPage = page;

            list = list
                //.Skip((page - 1) * 20)
                //.Take(20)
                .OrderBy(f => f.Panel.Canvas.Name)
                .ThenBy(f => f.Panel.Name)
                .ThenBy(f => f.Sort == null ? (float)f.FrameId : (float)f.Sort)
                .ThenBy(f => f.FrameId)
                ;

            FillCanvasesSelectList(canvasId);
            FillPanelsSelectList(panelId, canvasId);
            FillFrameTypeSelectList(frameType);
            FillTimingOptionsSelectList((Frame.TimingOptions?)timingOption);
             
            return View(list.ToList());
        }

        //
        // GET: /Frame/Create

        public ActionResult Create(int canvasId = 0, int panelId = 0, FrameTypes? frameType = null)
        {
            Panel panel = null;
            if (panelId != 0)
            {
                panel = db.Panels
                    .Include(p => p.Canvas)
                    .FirstOrDefault(p => p.PanelId == panelId)
                    ;

                if (panel == null)
                    panelId = 0;
            }
            
            if (frameType == null || panel == null)
            {
                if (canvasId == 0 && panel != null)
                    canvasId = panel.CanvasId;
                
                return RedirectToAction("ForFrameType", new { 
                    canvasId = canvasId, 
                    panelId = panelId, 
                    frameType = frameType
                });
            }

            TempData[SelectorFrameKey] = new Frame()
            {
                Panel = panel,
                PanelId = panelId,
                CacheInterval = 0,
            };

            return RedirectToAction("Create", frameType.ToString());
        }

        public ActionResult ForFrameType(int canvasId = 0, int panelId = 0, FrameTypes? frameType = null)
        {
            if (canvasId == 0)
            {
                Canvas canvas = db.Canvases
                    .OrderBy(c => c.Name)
                    .FirstOrDefault()
                    ;

                if (canvas != null)
                    canvasId = canvas.CanvasId;
            }
            
            FrameSelector selector = new FrameSelector() 
            { 
                CanvasId = canvasId,
                PanelId = panelId,
                FrameType = frameType.HasValue ? frameType.Value : 0,
            };

            FillCanvasesSelectList(canvasId);
            FillPanelsSelectList(panelId, canvasId);
            FillFrameTypeSelectList(frameType);
            return View(selector);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForFrameType([Bind(Include = "CanvasId,PanelId,FrameType")] FrameSelector selector)
        {
            PushReferrer();

            if (selector.PanelId != 0 && selector.FrameType != null)
            {
                Panel panel = db.Panels
                    .Include(p => p.Canvas)
                    .FirstOrDefault(p => p.PanelId == selector.PanelId)
                    ;

                selector.Panel = panel;
                //selector.CacheInterval = 0;

                TempData[SelectorFrameKey] = selector;
                return RedirectToAction("Create", selector.FrameType.ToString());
            }

            return RedirectToAction("ForFrameType", new { 
                canvasId = selector.CanvasId, 
                panelId = selector.PanelId, 
                frameType = selector.FrameType 
            });
        }

        //
        // GET: /Frame/Details/5

        public ActionResult Details(int id = 0)
        {
            Frame frame = db.Frames
                .Where( f => f.FrameId == id)
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                .FirstOrDefault()
                ;

            if (frame == null)
            {
                return View("Missing", new MissingItem(id));
            }

            return RedirectToAction("Details", frame.FrameType.ToString(), new { id = id });
        }

        //
        // GET: /Frame/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Frame frame = db.Frames
                .Where(f => f.FrameId == id)
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                .FirstOrDefault()
                ;

            if (frame == null)
            {
                return View("Missing", new MissingItem(id));
            }

            return RedirectToAction("Edit", frame.FrameType.ToString(), new { id = id });
        }

        //
        // GET: /Frame/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Frame frame = db.Frames
                .Where(f => f.FrameId == id)
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                .FirstOrDefault()
                ;

            if (frame == null)
            {
                return View("Missing", new MissingItem(id));
            }

            return RedirectToAction("Delete", frame.FrameType.ToString(), new { id = id });
        }

        //
        // GET: /Frame/Attach/5

        public ActionResult Attach(int id = 0)
        {
            Frame frame = db.Frames.Find(id);
            if (frame == null)
            {
                return View("Missing", new MissingItem(id));
            }

            LocationSelector selector = new LocationSelector
            {
                FrameId = id,
            };

            var locations = db.Locations
                .Where(l => !db.Frames
                    .FirstOrDefault(f => f.FrameId == selector.FrameId)
                    .Locations.Any(fl => fl.LocationId == l.LocationId))
                    .Include(l => l.Level)
                    .Select(l => new
                    {
                        LocationId = l.LocationId,
                        Name = l.Level.Name + " : " + l.Name
                    })
                    .OrderBy(l => l.Name)
                    .ToList()
                ;
            ViewBag.Locations = new SelectList(locations, "LocationId", "Name");

            return View(selector);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Attach(LocationSelector selector)
        {
            Frame frame = db.Frames.Find(selector.FrameId);
            if (frame == null)
            {
                return View("Missing", new MissingItem(selector.FrameId));
            }

            if (selector.LocationId > 0)
            {
                Location location = db.Locations.Find(selector.LocationId);
                if (location == null)
                {
                    return View("Missing", new MissingItem(selector.LocationId, "Location"));
                }
                frame.Locations.Add(location);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            IEnumerable<Location> locations = db.Locations
                .Where(l => !db.Frames
                    .FirstOrDefault(f => f.FrameId == selector.FrameId)
                    .Locations.Any(fl => fl.LocationId == l.LocationId))
                ;
            ViewBag.Locations = new SelectList(db.Locations, "LocationId", "Name");

            return View(selector);
        }

        //
        // GET: /Frame/Detach/5

        public ActionResult Detach(int id = 0, int locationId = 0)
        {
            Frame frame = db.Frames.Find(id);
            if (frame == null)
            {
                return View("Missing", new MissingItem(id));
            }

            LocationSelector selector = new LocationSelector
            {
                FrameId = id,
                LocationId = locationId,
                LocationName = db.Locations.Find(locationId).Name,
            };

            return View(selector);
        }

        [HttpPost, ActionName("Detach")]
        [ValidateAntiForgeryToken]
        public ActionResult DetachConfirmed(int id, int locationId)
        {
            Frame frame = db.Frames.Find(id);
            Location location = db.Locations.Find(locationId);
            frame.Locations.Remove(location);
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