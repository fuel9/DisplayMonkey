using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;

using System.IO;
using System.Net;

namespace DisplayMonkey.Controllers
{
    public class ReportController : Controller
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        //
        // GET: /Report/Details/5

        public ActionResult Details(int id = 0)
        {
            Navigation.SaveCurrent();

            Report report = db.Reports.Find(id);
            if (report == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(report);
        }

        //
        // GET: /Report/Create

        public ActionResult Create()
        {
            Frame frame = TempData[FrameController.SelectorFrameKey] as Frame;

            if (frame == null || frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            Report report = new Report()
            {
                Frame = frame,
            };

            FillServersSelectList();
            FillModesSelectList();

            return View(report);
        }

        //
        // POST: /Report/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Report report, Frame frame)
        {
            if (ModelState.IsValid)
            {
                report.Frame = frame;
                db.Reports.Add(report);
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index", "Frame");
            }

            FillServersSelectList();
            FillModesSelectList();
            
            report.Frame = frame;
            
            return View(report);
        }

        //
        // GET: /Report/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Report report = db.Reports.Find(id);
            if (report == null)
            {
                return View("Missing", new MissingItem(id));
            }

            FillServersSelectList(report.ServerId);
            FillModesSelectList(report.Mode);
            
            return View(report);
        }

        //
        // POST: /Report/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Report report, Frame frame)
        {
            if (ModelState.IsValid)
            {
                db.Entry(frame).State = EntityState.Modified;
                db.Entry(report).State = EntityState.Modified;
                db.SaveChanges();

                return Navigation.Restore() ?? RedirectToAction("Index");
            }

            FillServersSelectList(report.ServerId);
            FillModesSelectList(report.Mode);
            
            report.Frame = frame;
            
            return View(report);
        }

        //
        // GET: /Report/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Report report = db.Reports.Find(id);
            if (report == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(report);
        }

        //
        // POST: /Report/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Frame frame = db.Frames.Find(id);
            db.Frames.Remove(frame);
            db.SaveChanges();

            return Navigation.Restore() ?? RedirectToAction("Index", "Frame");
        }

        //
        // GET: /Content/Thumb/5

        [Authorize]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Thumb(int id, int width = 0, int height = 0, int mode = DisplayMonkey.Models.Content.RenderMode_Fit)
        {
            Report report = db.Reports
                .Include(r => r.ReportServer)
                .FirstOrDefault(r => r.FrameId == id)
                ;

            if (report.Path != null && report.ReportServer != null)
            {
                string baseUrl = report.ReportServer.BaseUrl
                    , url = report.Path;
                
                if (baseUrl.EndsWith("/"))
                    baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

                if (!url.StartsWith("/"))
                    url = "/" + url;

                url = string.Format(
                    "{0}?{1}&rs:format=IMAGE",
                    baseUrl,
                    url
                    );

                WebClient client = new WebClient();
                if (!string.IsNullOrEmpty(report.ReportServer.User))
                {
                    client.Credentials = new NetworkCredential(
                        report.ReportServer.User,
                        RsaUtil.Decrypt(report.ReportServer.Password),
                        report.ReportServer.Domain
                        );
                }

                try
                {
                    byte[] img = client.DownloadData(url);

                    using (MemoryStream src = new MemoryStream(img))
                    {
                        Response.ContentType = "image/png";
                        MediaController.WriteImage(src, Response.OutputStream, width, height, mode);
                        Response.OutputStream.Flush();
                        return Content("");

                        /*MemoryStream trg = new MemoryStream();
                        MediaController.WriteImage(src, trg, width, height, mode);
                        trg.Seek(0, SeekOrigin.Begin);
                        return new FileStreamResult(trg, "image/png");*/
                    }
                }

                catch
                {
                }
            }

            return RedirectToAction("BadImg", "Media");
        }

        private void FillServersSelectList(object selected = null)
        {
            ViewBag.Servers = new SelectList(db.ReportServers, "ServerId", "Name", selected);
        }

        private void FillModesSelectList(object selected = null)
        {
            ViewBag.Modes = new SelectList(DisplayMonkey.Models.Content.RenderModes, "Mode", "Name", selected);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}