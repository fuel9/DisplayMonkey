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

using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace DisplayMonkey.Controllers
{
    public class ReportController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        //
        // GET: /Report/Details/5

        public ActionResult Details(int id = 0)
        {
            Report report = db.Frames.Find(id) as Report;
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

            Report report = new Report(frame, db);


            this.FillTemplatesSelectList(db, FrameTypes.Report);
            FillServersSelectList();
            FillModesSelectList();

            return View(report);
        }

        //
        // POST: /Report/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Report report)
        {
            if (ModelState.IsValid)
            {
                db.Frames.Add(report);
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Report, report.TemplateId);
            FillServersSelectList();
            FillModesSelectList();
            
            
            return View(report);
        }

        //
        // GET: /Report/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Report report = db.Frames.Find(id) as Report;
            if (report == null)
            {
                return View("Missing", new MissingItem(id));
            }

            this.FillTemplatesSelectList(db, FrameTypes.Report, report.TemplateId);
            FillServersSelectList(report.ServerId);
            FillModesSelectList(report.Mode);
            
            return View(report);
        }

        //
        // POST: /Report/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Report report)
        {
            if (ModelState.IsValid)
            {
                db.Entry(report).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Report, report.TemplateId);
            FillServersSelectList(report.ServerId);
            FillModesSelectList(report.Mode);
            
            
            return View(report);
        }

        //
        // GET: /Report/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Report report = db.Frames.Find(id) as Report;
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

            return RedirectToAction("Index", "Frame");
        }

        //
        // GET: /Content/Thumb/5

        //[Authorize]
        [HttpGet, ActionName("Thumb")]
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> ThumbAsync(int id, int width = 0, int height = 0, RenderModes mode = RenderModes.RenderMode_Fit, int trace = 0)
        {
            StringBuilder message = new StringBuilder();

            try
            {
                Report report = db.Frames.OfType<Report>()
                    .Include(r => r.ReportServer)
                    .FirstOrDefault(r => r.FrameId == id)
                    ;

                if (width <= 120 && height <= 120)
                {
                    byte[] cache = await HttpRuntime.Cache.GetOrAddSlidingAsync(
                        string.Format("thumb_report_{0}_{1}x{2}_{3}", report.FullPath, width, height, (int)mode),
                        async (expire) => 
                        {
                            expire.After = TimeSpan.FromHours(1);
                            byte[] img = await GetReportBytesAsync(report);
                            using (MemoryStream trg = new MemoryStream())
                            using (MemoryStream src = new MemoryStream(img))
                            {
                                MediaController.WriteImage(src, trg, width, height, mode);
                                return trg.GetBuffer();
                            }
                        });

                    return new FileStreamResult(new MemoryStream(cache), "image/png");
                }

                else
                {
                    byte[] img = await GetReportBytesAsync(report);
                    using (MemoryStream src = new MemoryStream(img))
                    {
                        MemoryStream trg = new MemoryStream();
                        MediaController.WriteImage(src, trg, width, height, mode);
                        return new FileStreamResult(trg, "image/png");
                    }
                }
            }

            catch (Exception ex)
            {
                if (trace > 0)
                    message.Append(ex.ToString());

                if (trace > 1)
                {
                    Report report = db.Frames.OfType<Report>()
                        .Include(r => r.ReportServer)
                        .FirstOrDefault(r => r.FrameId == id)
                        ;
                    
                    message.Append("\n");
                    if (report == null)
                    {
                        message.AppendFormat("\nReport id {0} not found", id);
                    }
                    else
                    {
                        message.AppendFormat("\nReport name: {0}", report.Name);
                        message.AppendFormat("\nReport path: {0}", report.Path);
                        message.AppendFormat("\nReport server URL: {0}", report.ReportServer.BaseUrl);
                        message.AppendFormat("\nReport server account: {0}\\{1}", report.ReportServer.Domain, report.ReportServer.User);
                        //message.AppendFormat("\nReport server passwrd: {0}", HttpUtility.HtmlEncode(RsaUtil.Decrypt(report.ReportServer.Password)));
                    }
                }
            }

            if (trace == 0)
                return RedirectToAction("BadImg", "Media");
            else
                return Content(message.Length == 0 ? "OK" : message.ToString());
        }

        /*private byte[] GetReportBytes(int id)
        {
            Report report = db.Frames.OfType<Report>()
                .Include(r => r.ReportServer)
                .FirstOrDefault(r => r.FrameId == id)
                ;

            WebClient client = new WebClient();
            string
                user = (report.ReportServer.User ?? "").Trim(),
                domain = (report.ReportServer.Domain ?? "").Trim()
                ;
            
            if (!string.IsNullOrWhiteSpace(user))
            {
                client.Credentials = new NetworkCredential(
                    user,
                    RsaUtil.Decrypt(report.ReportServer.Password),
                    domain
                    );
            }

            return client.DownloadData(report.FullPath);
        }*/

        private async Task<byte[]> GetReportBytesAsync(Report report)
        {
            var client = new WebClient();
            string
                user = (report.ReportServer.User ?? "").Trim(),
                domain = (report.ReportServer.Domain ?? "").Trim()
                ;

            if (!string.IsNullOrWhiteSpace(user))
            {
                client.Credentials = new NetworkCredential(
                    user,
                    Setting.GetEncryptor(db).Decrypt(report.ReportServer.Password),
                    domain
                    );
            }

            return await client.DownloadDataTaskAsync(report.FullPath);
        }

        private void FillServersSelectList(object selected = null)
        {
            ViewBag.Servers = new SelectList(db.ReportServers, "ServerId", "Name", selected);
        }

        private void FillModesSelectList(RenderModes? selected = null)
        {
            ViewBag.Modes = selected.TranslatedSelectList();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}