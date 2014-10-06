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
using System.Net;
using System.Text;
using System.Xml;
using DisplayMonkey.Language;


namespace DisplayMonkey.Controllers
{
    public class LocationController : BaseController
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
                return View("Missing", new MissingItem(id));
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
                // compute Woeid & GMT offset
                location.Woeid = GetWoeid(location.Latitude, location.Longitude);
                if (location.OffsetGMT == null)
                    location.OffsetGMT = GetTimeOffset(location.Latitude, location.Longitude);
                
                db.Locations.Add(location);
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index");
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
                return View("Missing", new MissingItem(id));
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
                // compute Woeid & GMT offset
                location.Woeid = GetWoeid(location.Latitude, location.Longitude);
                if (location.OffsetGMT == null)
                    location.OffsetGMT = GetTimeOffset(location.Latitude, location.Longitude);

                db.Entry(location).State = EntityState.Modified;
                db.Entry(location).Property(l => l.LevelId).IsModified = false;
                db.Entry(location).Property(l => l.AreaId).IsModified = false;
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index");
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
                return View("Missing", new MissingItem(id));
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

            return Navigation.Restore() ?? RedirectToAction("Index");
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

        private int? GetWoeid(double? latitude, double? longitude)
        {
            // translate LAT/LNG to WOEID
            // get local time
            string url, xml = "";

            if ((latitude ?? 0) == 0 || (longitude ?? 0) == 0)
                return null;

            // get GEO data
            url = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                @"http://query.yahooapis.com/v1/public/yql?q=select+*+from+geo.placefinder+where+text%3D%22{0}%2C{1}%22+and+gflags%3D%22R%22",
                latitude.Value,
                longitude.Value
                );

            try
            {
                using (WebClient client = new WebClient())
                {
                    xml = Encoding.ASCII.GetString(client.DownloadData(url));
                }
            }

            catch (WebException ex)
            {
                throw new Exception(Resources.GeoTranslationHasFailed, ex);
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode nWoeid = doc.SelectSingleNode("//woeid");
            if (nWoeid != null)
            {
                return Convert.ToInt32(nWoeid.InnerText);
            }

            return null;
        }

        private int? GetTimeOffset(double? latitude, double? longitude)
        {
            // translate LAT/LNG to WOEID
            // get local time
            string url, xml = "";

            if ((latitude ?? 0) == 0 || (longitude ?? 0) == 0)
                return null;

            // get GEO data
            url = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                @"https://maps.googleapis.com/maps/api/timezone/xml?location={0},{1}&timestamp=0",
                latitude.Value,
                longitude.Value
                );

            try
            {
                using (WebClient client = new WebClient())
                {
                    xml = Encoding.ASCII.GetString(client.DownloadData(url));
                }
            }

            catch (WebException ex)
            {
                throw new Exception(Resources.GeoTranslationHasFailed, ex);
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode nWoeid = doc.SelectSingleNode("//raw_offset");
            if (nWoeid != null)
            {
                return (int)Convert.ToDouble(nWoeid.InnerText) / 3600;
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}