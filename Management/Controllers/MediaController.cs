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
using System.Drawing;
using System.Drawing.Imaging;
using System.Data.Entity.Validation;
using System.Text;
using System.Threading.Tasks;
using DisplayMonkey.Language;

namespace DisplayMonkey.Controllers
{
    public class MediaController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        private void FillMediaTypesSelectList(ContentTypes? selected = null)
        {
            ViewBag.MediaType = selected.TranslatedSelectList();
        }

        //
        // GET: /Media/

        public ActionResult Index(ContentTypes? mediaType = null)
        {
            var list = db.Contents
                .Select(m => new ContentWithSize
                { 
                    ContentId = m.ContentId,
                    Name = m.Name,
                    Type = (ContentTypes)m.Type,
                    Size = System.Data.Entity.SqlServer.SqlFunctions.DataLength(m.Data) / 1024
                })
                .OrderBy(m => m.Name)
                .AsQueryable()
                ;

            if (mediaType != null)
            {
                list = list.Where(c => c.Type == mediaType);
            }

            FillMediaTypesSelectList();
            
            return View(list);
        }

        //
        // GET: /Media/Details/5

        public ActionResult Details(int id = 0)
        {
            // TODO: video player in the view
            Content content = db.Contents.Find(id);
            if (content == null)
            {
                return View("Missing", new MissingItem(id));
            }

            if (content.Type == ContentTypes.ContentType_Video)
            {
                ViewBag.FrameList = content.Videos
                    .OrderBy(f => f.FrameId)
                    .AsEnumerable()
                    ;
            }

            else
            {
                ViewBag.FrameList = content.Pictures
                    .OrderBy(f => f.FrameId)
                    .AsEnumerable()
                    ;
            }

            return View(content);
        }

        //
        // GET: /Media/Create

        public ActionResult Upload()
        {
            ViewBag.MaxImageSize = Setting.GetSetting(db, Setting.Keys.MaxImageSize).IntValuePositive;
            ViewBag.MaxVideoSize = Setting.GetSetting(db, Setting.Keys.MaxVideoSize).IntValuePositive;
            return View();
        }

        //
        // POST: /Media/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
        {
            // TODO: EditorFor

            bool hasFiles = false, addedFiles = false;
            foreach (HttpPostedFileBase file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string ext = Path.GetExtension(file.FileName).Replace(".", "").ToUpper();
                    bool isPicture = Picture.SupportedFormats.Contains(ext);
                    bool isVideo = Video.SupportedFormats.Contains(ext);
                        
                    if (isPicture || isVideo)
                    {
                        byte[] buffer = null;
                        using (BinaryReader reader = new BinaryReader(file.InputStream))
                        {
                            buffer = reader.ReadBytes(file.ContentLength);
                        }

                        Content content = new Content {
                            Type = isPicture ? ContentTypes.ContentType_Picture : ContentTypes.ContentType_Video,
                            Name = Path.GetFileName(file.FileName),
                            Data = buffer,
                        };

                        db.Contents.Add(content);

                        addedFiles = true;
                    }

                    hasFiles = true;
                }
            }

            if (addedFiles)
            {
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            else if (hasFiles)
            {
                // TODO: validator for wrong file types
            }

            return View();
        }

        //
        // GET: /Media/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Content content = db.Contents.Find(id);
            if (content == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(content);
        }

        //
        // POST: /Media/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Content content)
        {
            if (ModelState.IsValid)
            {
                content.Data = new byte[] { 0 };    // trick EF, this is a required field
                db.Contents.Attach(content);
                db.Entry(content).Property(m => m.Name).IsModified = true;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(content);
        }

        //
        // GET: /Media/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Content content = db.Contents.Find(id);
            if (content == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(content);
        }

        //
        // POST: /Media/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Content content = db.Contents
                .Include(c => c.Pictures)
                .Include(c => c.Videos)
                .SingleOrDefault(c => c.ContentId == id)
                ;

            if (content != null)
            {
                db.Frames.RemoveRange(content.Pictures.AsEnumerable<Frame>());
                db.Frames.RemoveRange(content.Videos.AsEnumerable<Frame>());

                db.Contents.Remove(content);

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /Media/BadImg

        public ActionResult BadImg()
        {
            return File(Request.MapPath("~/Images/question.png"), "image/png");
        }

        //
        // GET: /Media/Playback/5

        //[Authorize]
        [HttpGet, ActionName("Playback")]
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> PlaybackAsync(int id)
        {
            Content content = await db.Contents.FindAsync(id);
            if (content.Data != null)
            {
                string contentType = string.Format(
                    "video/{0}",
                    System.IO.Path.GetExtension(content.Name).Replace(".", "").ToLower()
                    );
                return File(content.Data, contentType);
            }

            return Content(Resources.NotSupported);
        }

        //
        // GET: /Media/Thumb/5
        //[Authorize]
        [HttpGet, ActionName("Thumb")]
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> ThumbAsync(int id, int width = 0, int height = 0, RenderModes mode = RenderModes.RenderMode_Fit, int trace = 0)
        {
            StringBuilder message = new StringBuilder();

            try
            {
                if (width <= 120 && height <= 120)
                {
                    byte[] cache = await HttpRuntime.Cache.GetOrAddSlidingAsync(
                        string.Format("thumb_image_{0}_{1}x{2}_{3}", id, width, height, (int)mode),
                        async (expire) => 
                        {
                            expire.After = TimeSpan.FromHours(1);
                            var data = await db.Contents.FindAsync(id);
                            using (MemoryStream trg = new MemoryStream())
                            using (MemoryStream src = new MemoryStream(data.Data))
                            {
                                MediaController.WriteImage(src, trg, width, height, mode);
                                return trg.GetBuffer();
                            }
                        });

                    return new FileStreamResult(new MemoryStream(cache), "image/png");
                }

                else
                {
                    var data = await db.Contents.FindAsync(id);
                    using (MemoryStream src = new MemoryStream(data.Data))
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
            }

            if (trace == 0)
                return RedirectToAction("BadImg");
            else
                return Content(message.Length == 0 ? "OK" : message.ToString());
        }

        // TODO: move to separate file
        public static void WriteImage(Stream inStream, Stream outStream, int panelWidth, int panelHeight, RenderModes mode)
        {
            Bitmap bmpSrc = null, bmpTrg = null;

            try
            {
                inStream.Position = 0;
                bmpSrc = new Bitmap(inStream);

                // convert TIFF to BMP, use only the first page
                FrameDimension fd = new FrameDimension(bmpSrc.FrameDimensionsList[0]);
                bmpSrc.SelectActiveFrame(fd, 0);

                // crop/fit/stretch
                int imageHeight = bmpSrc.Height, imageWidth = bmpSrc.Width, targetWidth, targetHeight;

                if (panelWidth <= 0) panelWidth = imageWidth;
                if (panelHeight <= 0) panelHeight = imageHeight;

                if (panelWidth != imageWidth || panelHeight != imageHeight)
                {
                    switch (mode)
                    {
                        case RenderModes.RenderMode_Stretch:
                            if (panelWidth <= 120 && panelHeight <= 120)
                                bmpTrg = new Bitmap(bmpSrc.GetThumbnailImage(panelWidth, panelHeight, () => false, IntPtr.Zero)); 
                            else
                                bmpTrg = new Bitmap(bmpSrc, new Size(panelWidth, panelHeight));
                            break;

                        case RenderModes.RenderMode_Fit:
                            targetWidth = imageWidth;
                            targetHeight = imageHeight;
                            float scale = 1F;
                            // a. panel is greater than image: grow
                            if (panelHeight > imageHeight && panelWidth > imageWidth)
                            {
                                scale = Math.Min((float)panelWidth / imageWidth, (float)panelHeight / imageHeight);
                                targetHeight = Math.Min((int)((float)imageHeight * scale), panelHeight);
                                targetWidth = Math.Min((int)((float)imageWidth * scale), panelWidth);
                            }
                            // b. image is greater than panel: shrink
                            else
                            {
                                scale = Math.Max((float)imageWidth / panelWidth, (float)imageHeight / panelHeight);
                                targetWidth = Math.Min((int)((float)imageWidth / scale), panelWidth);
                                targetHeight = Math.Min((int)((float)imageHeight / scale), panelHeight);
                            }
                            if (targetWidth <= 120 && targetHeight <= 120)
                                bmpTrg = new Bitmap(bmpSrc.GetThumbnailImage(targetWidth, targetHeight, () => false, IntPtr.Zero)); 
                            else
                                bmpTrg = new Bitmap(bmpSrc, new Size(targetWidth, targetHeight));
                            break;

                        case RenderModes.RenderMode_Crop:
                            targetWidth = Math.Min(panelWidth, imageWidth);
                            targetHeight = Math.Min(panelHeight, imageHeight);
                            bmpTrg = bmpSrc.Clone(
                                new Rectangle(0, 0, targetWidth, targetHeight),
                                bmpSrc.PixelFormat
                                );
                            break;
                    }

                    bmpTrg.Save(outStream, ImageFormat.Png);    
                }
                else
                    bmpSrc.Save(outStream, ImageFormat.Png);

                outStream.Seek(0, SeekOrigin.Begin);
            }

            finally
            {
                if (bmpTrg != null) bmpTrg.Dispose();
                if (bmpSrc != null) bmpSrc.Dispose();
                GC.Collect();
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}