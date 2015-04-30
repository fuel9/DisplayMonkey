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
                //.Where(f => f.News == null)
                .Sum(t => Math.Round((double)t.Duration / 3600.0, 2))
                );

            ViewBag.Count_Levels = db.Levels.Count();
            ViewBag.Count_Locations = db.Locations.Count();
            ViewBag.Count_Canvases = db.Canvases.Count();
            ViewBag.Count_Panels = db.Panels.Count();
            ViewBag.Count_Displays = db.Displays.Count();
            
            ViewBag.Count_Html = db.Html.Count();
            ViewBag.Count_Html_7 = db.Html.Count(t => t.Frame.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Memos = db.Memos.Count();
            ViewBag.Count_Memos_7 = db.Memos.Count(t => t.Frame.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Outlook = db.Outlooks.Count();
            ViewBag.Count_Outlook_7 = db.Outlooks.Count(t => t.Frame.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Pictures = db.Pictures.Count();
            ViewBag.Count_Pictures_7 = db.Pictures.Count(t => t.Frame.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Reports = db.Reports.Count();
            ViewBag.Count_Reports_7 = db.Reports.Count(t => t.Frame.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Videos = db.Videos.Count();
            ViewBag.Count_Videos_7 = db.Videos.Count(t => t.Frame.DateCreated >= sevenDaysAgo);

            ViewBag.Count_Youtube = db.Youtube.Count();
            ViewBag.Count_Youtube_7 = db.Youtube.Count(t => t.Frame.DateCreated >= sevenDaysAgo);

            TopContent [] topFiveContent = db.Frames
                .Where(f => 
                    (f.BeginsOn == null || f.BeginsOn <= DateTime.Now) &&
                    (f.EndsOn == null | f.EndsOn >= DateTime.Now)
                )
                .Select(f => new
                {
                    Name =
                        f.Clock != null ? DisplayMonkey.Language.Resources.Clock :
                        f.Html != null ? f.Html.Name :
                        f.Memo != null ? f.Memo.Subject :
                        f.News != null ? DisplayMonkey.Language.Resources.News :
                        f.Outlook != null ? f.Outlook.Name :
                        f.Picture != null ? f.Picture.Content.Name :
                        f.Report != null ? f.Report.Name :
                        f.Video != null ? f.Video.Contents.FirstOrDefault().Name :
                        f.Weather != null ? DisplayMonkey.Language.Resources.Weather :
                        f.Youtube != null ? f.Youtube.Name :
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
