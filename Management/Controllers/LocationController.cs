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
            IQueryable<Location> list = db.Locations
                .OrderBy(l => l.Level.Name)
                .ThenBy(l => l.Area.Name)
                .ThenBy(l => l.Name)
                ;

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
            FillAreaSelectList(0);

            return View(list.ToList());
            //return View(db.Locations.ToList());
        }

        //
        // GET: /Location/Details/5

        public ActionResult Details(int id = 0)
        {
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
            FillCulturesSelectList();
            this.FillSystemTimeZoneSelectList();

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
                location.Woeid = GetDefaultWoeid(location.Latitude, location.Longitude);
                if (location.TimeZone == null)
                    location.TimeZone = TimeZoneInfo.Local.Id; // GetDefaultTimeZone(location.Latitude, location.Longitude);
                
                db.Locations.Add(location);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            FillLevelsSelectList(location.LevelId);
            FillTemperatureUnitSelectList(location.TemperatureUnit);
            FillAreaSelectList(location.LocationId, location.AreaId);
            FillCulturesSelectList(location.Culture);
            this.FillSystemTimeZoneSelectList(location.TimeZone);

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
            //FillLevelsSelectList(location.LevelId);
            FillTemperatureUnitSelectList(location.TemperatureUnit);
            //FillAreaSelectList(location.LocationId, location.AreaId);
            FillCulturesSelectList(location.Culture);
            this.FillSystemTimeZoneSelectList(location.TimeZone);

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
                if (!location.Woeid.HasValue)
                    location.Woeid = GetDefaultWoeid(location.Latitude, location.Longitude);
                if (location.TimeZone == null)
                    location.TimeZone = TimeZoneInfo.Local.Id; // GetDefaultTimeZone(location.Latitude, location.Longitude);

                db.Entry(location).State = EntityState.Modified;
                db.Entry(location).Property(l => l.LevelId).IsModified = false;
                db.Entry(location).Property(l => l.AreaId).IsModified = false;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            //FillLevelsSelectList(location.LevelId);
            FillTemperatureUnitSelectList(location.TemperatureUnit);
            //FillAreaSelectList(location.LocationId, location.AreaId);
            FillCulturesSelectList(location.Culture);
            this.FillSystemTimeZoneSelectList(location.TimeZone);

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

            return RedirectToAction("Index");
        }

        private void FillLevelsSelectList(object selected = null)
        {
            var query = from d in db.Levels
                        orderby d.Name
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
            ViewBag.TempUnits = new SelectList(
                new []
                {
                    new SelectListItem() {Value = "C", Text = Resources.C},
                    new SelectListItem() {Value = "F", Text = Resources.F},
                },
                "Value", 
                "Text", 
                selected
            );
        }

        private void FillCulturesSelectList(object selected = null)
        {
            var query = Info.AllCultures
                .Select(c => new {Code = c.Name, Name = c.DisplayName})
                .OrderBy(c => c.Name)
                ;
            ViewBag.Cultures = new SelectList(query, "Code", "Name", selected);
        }

        private int? GetDefaultWoeid(double? latitude, double? longitude)
        {
            // translate LAT/LNG to WOEID
            if ((latitude ?? 0) == 0 || (longitude ?? 0) == 0)
                return null;

            // get GEO data
            // https://github.com/fuel9/DisplayMonkey/issues/9
            string url = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                @"https://query.yahooapis.com/v1/public/yql?q=select+*+from+geo.places+where+text%3D%22({0}%2C{1})%22",
                latitude.Value,
                longitude.Value
                );

            try
            {
                string xml = "";
                using (WebClient client = new WebClient())
                {
                    xml = Encoding.ASCII.GetString(client.DownloadData(url));
                }

                XmlDocument doc = new XmlDocument();
                using (System.IO.StringReader sreader = new System.IO.StringReader(xml))
                using (XmlReader xmlreader = new XmlTextReader(sreader) { Namespaces = false })
                {
                    doc.Load(xmlreader);
                }

                XmlNode nWoeid = doc.SelectSingleNode("//woeid");
                if (nWoeid != null)
                {
                    return Convert.ToInt32(nWoeid.InnerText);
                }
            }

            catch (WebException ex)
            {
                throw new Exception(Resources.GeoTranslationHasFailed, ex);
            }

            return null;
        }

        private string GetDefaultTimeZone(double? latitude, double? longitude)
        {
            // translate LAT/LNG to time zone
            string url, xml = "";
            int offsetGmt = 0;

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
            XmlNode nOffset = doc.SelectSingleNode("//raw_offset"); // seconds
            if (nOffset != null)
            {
                offsetGmt = (int)Convert.ToDouble(nOffset.InnerText) / 60;
            }

            return null;    // TODO: lookup appropriate time zone id based on GMT offset
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}