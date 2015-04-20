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

        private void FillPanelsSelectList(object selected = null, int canvasId = 0)
        {
            if (canvasId > 0)
            {
                var query = db.Panels
                    .Where(p => p.CanvasId == canvasId)
                    .Select(p => new
                    {
                        PanelId = p.PanelId,
                        Name = p.Name
                    })
                    .OrderBy(p => p.Name)
                    .ToList()
                    ;

                ViewBag.PanelId = new SelectList(query, "PanelId", "Name", selected);
            }
            else
            {
                var query = db.Panels
                    .Include(p => p.Canvas)
                    .Select(p => new
                    {
                        PanelId = p.PanelId,
                        Name = p.Canvas.Name + " : " + p.Name
                    })
                    .OrderBy(p => p.Name)
                    .ToList()
                    ;

                ViewBag.PanelId = new SelectList(query, "PanelId", "Name", selected);
            }
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
        // GET: /Frame/

        public ActionResult Index(int canvasId = 0, int panelId = 0, FrameTypes? frameType = null, int? timingOption = null /*, int page = 1*/)
        {
            this.SaveReferrer();

            //if (page <= 0) page = 1;

            IQueryable<Frame> list = db.Frames
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                .Include(f => f.News)
                .Include(f => f.Clock)
                .Include(f => f.Weather)
                .Include(f => f.Memo)
                .Include(f => f.Report)
                .Include(f => f.Picture)
                .Include(f => f.Video)
                .Include(f => f.Html)
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
            if (panelId == 0)
            {
                if (canvasId == 0)
                    return RedirectToAction("ForCanvas");
                else
                    return RedirectToAction("ForPanel", new { canvasId = canvasId });
            }
            
            else if (TempData[SelectorFrameKey] == null)
            {
                Panel panel = db.Panels
                    .Include(p => p.Canvas)
                    .First(p => p.PanelId == panelId)
                    ;

                FrameSelector selector = new FrameSelector()
                {
                    Panel = panel,
                    PanelId = panel.PanelId,
                };

                TempData[SelectorFrameKey] = selector;
            }

            if (frameType == null)
            {
                return RedirectToAction("ForFrameType", new { panelId = panelId });
            }

            return RedirectToAction("Create", frameType.ToString());
        }

        public ActionResult ForCanvas()
        {
            FillCanvasesSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForCanvas(Canvas canvas)
        {
            if (canvas.CanvasId > 0)
            {
                return RedirectToAction("ForPanel", new { canvasId = canvas.CanvasId });
            }

            return RedirectToAction("ForCanvas");
        }

        public ActionResult ForPanel(int canvasId)
        {
            Panel panel = db.Panels
                .Include(p => p.Canvas)
                .FirstOrDefault(p => p.CanvasId == canvasId)
                ;

            if (panel.PanelId == 0)
            {
                return RedirectToAction("ForCanvas");
            }

            FillPanelsSelectList(canvasId: canvasId);
            return View(panel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForPanel(Panel panel)
        {
            if (panel.PanelId > 0)
            {
                return RedirectToAction("ForFrameType", new { panelId = panel.PanelId });
            }

            return RedirectToAction("ForPanel", new { canvasId = panel.Canvas.CanvasId });
        }

        public ActionResult ForFrameType(int panelId)
        {
            Panel panel = db.Panels
                .Include(p => p.Canvas)
                .FirstOrDefault(p => p.PanelId == panelId)
                ;

            if (panel.PanelId == 0)
            {
                return RedirectToAction("ForCanvas");
            }

            FrameSelector selector = new FrameSelector() 
            { 
                Panel = panel,
                PanelId = panel.PanelId,
            };

            FillFrameTypeSelectList();
            return View(selector);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForFrameType(FrameSelector selector)
        {
            if (selector.FrameType != null)
            {
                Panel panel = db.Panels
                    .Include(p => p.Canvas)
                    .FirstOrDefault(p => p.PanelId == selector.PanelId)
                    ;

                selector.Panel = panel;
                selector.CacheMode = CacheModes.CacheMode_None;
                selector.CacheInterval = 0;

                TempData[SelectorFrameKey] = selector;
                return RedirectToAction("Create", selector.FrameType.ToString());
            }

            return RedirectToAction("ForFrameType", new { panelId = selector.PanelId });
        }

        //
        // GET: /Frame/Details/5

        public ActionResult Details(int id = 0)
        {
            Frame frame = db.Frames
                .Where( f => f.FrameId == id)
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                .Include(f => f.News)
                .Include(f => f.Clock)
                .Include(f => f.Weather)
                .Include(f => f.Memo)
                .Include(f => f.Report)
                .Include(f => f.Picture)
                .Include(f => f.Video)
                .Include(f => f.Html)
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
                .Include(f => f.News)
                .Include(f => f.Clock)
                .Include(f => f.Weather)
                .Include(f => f.Memo)
                .Include(f => f.Report)
                .Include(f => f.Picture)
                .Include(f => f.Video)
                .Include(f => f.Html)
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

                return this.RestoreReferrer() ?? RedirectToAction("Index");
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

            return this.RestoreReferrer() ?? RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}