/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using DisplayMonkey.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace DisplayMonkey.Controllers
{
    public class HomeController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        public ActionResult Index()
        {
            
            
            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);

            ViewBag.Count_Frames = db.Frames.Count();
            ViewBag.Count_Active_Frames = db.Frames.Count(t =>
                (t.BeginsOn == null || t.BeginsOn <= DateTime.Now) &&
                (t.EndsOn == null || t.EndsOn > DateTime.Now)
            );
            ViewBag.Count_Expired_Frames = db.Frames.Count(t => t.EndsOn <= DateTime.Now);
            ViewBag.Count_Pending_Frames = db.Frames.Count(t => DateTime.Now < t.BeginsOn);
            ViewBag.Duration_Hours = string.Format("{0:N2}", db.Frames
                .Select(t => Math.Round((double)t.Duration / 3600.0, 2))
                .DefaultIfEmpty(0)
                .Sum()
                );

            ViewBag.Count_Levels = db.Levels.Count();
            ViewBag.Count_Locations = db.Locations.Count();
            ViewBag.Count_Canvases = db.Canvases.Count();
            ViewBag.Count_Panels = db.Panels.Count();
            ViewBag.Count_Displays = db.Displays.Count();

            ViewBag.Count_Html = db.Frames.OfType<Html>().Count();
            ViewBag.Count_Html_7 = db.Frames.OfType<Html>().Count(t => t.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Memos = db.Frames.OfType<Memo>().Count();
            ViewBag.Count_Memos_7 = db.Frames.OfType<Memo>().Count(t => t.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Outlook = db.Frames.OfType<Outlook>().Count();
            ViewBag.Count_Outlook_7 = db.Frames.OfType<Outlook>().Count(t => t.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Pictures = db.Frames.OfType<Picture>().Count();
            ViewBag.Count_Pictures_7 = db.Frames.OfType<Picture>().Count(t => t.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Powerbi = db.Frames.OfType<Powerbi>().Count();
            ViewBag.Count_Powerbi_7 = db.Frames.OfType<Powerbi>().Count(t => t.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Reports = db.Frames.OfType<Report>().Count();
            ViewBag.Count_Reports_7 = db.Frames.OfType<Report>().Count(t => t.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Videos = db.Frames.OfType<Video>().Count();
            ViewBag.Count_Videos_7 = db.Frames.OfType<Video>().Count(t => t.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Youtube = db.Frames.OfType<Youtube>().Count();
            ViewBag.Count_Youtube_7 = db.Frames.OfType<Youtube>().Count(t => t.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Clock = db.Frames.OfType<Clock>().Count();
            ViewBag.Count_Clock_7 = db.Frames.OfType<Clock>().Count(t => t.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Weather = db.Frames.OfType<Weather>().Count();
            ViewBag.Count_Weather_7 = db.Frames.OfType<Weather>().Count(t => t.DateCreated >= sevenDaysAgo);

            TopContent [] topFiveContent = db.Frames
                .Where(f => 
                    (f.BeginsOn == null || f.BeginsOn <= DateTime.Now) &&
                    (f.EndsOn == null | f.EndsOn >= DateTime.Now)
                )
                .Select(f => new
                {
                    Name =
                        f is Clock ? DisplayMonkey.Language.Resources.Clock :
                        f is Html ? ((f as Html).Name ?? DisplayMonkey.Language.Resources.Html) :
                        f is Memo ? ((f as Memo).Subject ?? DisplayMonkey.Language.Resources.Memo) :
                        //f is News ? DisplayMonkey.Language.Resources.News :
                        f is Outlook ? ((f as Outlook).Name ?? DisplayMonkey.Language.Resources.Outlook) :
                        f is Picture ? ((f as Picture).Content.Name ?? DisplayMonkey.Language.Resources.Picture) :
                        f is Powerbi ? ((f as Powerbi).Name ?? DisplayMonkey.Language.Resources.Powerbi) :
                        f is Report ? ((f as Report).Name ?? DisplayMonkey.Language.Resources.Report) :
                        f is Video ? ((f as Video).Contents.FirstOrDefault().Name ?? DisplayMonkey.Language.Resources.Video) :
                        f is Weather ? DisplayMonkey.Language.Resources.Weather :
                        f is Youtube ? ((f as Youtube).Name ?? DisplayMonkey.Language.Resources.YouTube) :
                        DisplayMonkey.Language.Resources.Unknown
                })
                .GroupBy(f => f.Name)
                .OrderByDescending(f => f.Count())
                .Take(5)
                .Select(f => new TopContent { Name = f.Key, Count = f.Count() })
                .OrderByDescending(f => f.Count)
                .ToArray()
                ;

            ViewBag.TopFiveContent = topFiveContent;

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
