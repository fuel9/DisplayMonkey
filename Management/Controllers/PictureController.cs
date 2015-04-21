using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;

using System.IO;

namespace DisplayMonkey.Controllers
{
    public class PictureController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        //
        // GET: /Picture/Details/5

        public ActionResult Details(int id = 0)
        {
            this.SaveReferrer(true);

            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(picture);
        }

        //
        // GET: /Picture/Create

        public ActionResult Create()
        {
            Frame frame = TempData[FrameController.SelectorFrameKey] as Frame;

            if (frame == null || frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            Picture picture = new Picture()
            {
                Frame = frame,
            };

            picture.init();

            this.FillTemplatesSelectList(db, FrameTypes.Picture);
            FillPicturesSelectList();
            FillModesSelectList();

            return View(picture);
        }

        //
        // POST: /Picture/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Picture picture, Frame frame)
        {
            picture.Frame = frame;

            if (ModelState.IsValid)
            {
                if (picture.SavedContentId.HasValue)
                {
                    picture.ContentId = picture.SavedContentId.Value;
                    db.Pictures.Add(picture);
                    db.SaveChanges();

                    return this.RestoreReferrer() ?? RedirectToAction("Index", "Frame");
                }
                else
                {
                    TempData["_newPicture"] = picture;
                    return RedirectToAction("Upload", "Picture");
                }
            }

            this.FillTemplatesSelectList(db, FrameTypes.Picture, picture.Frame.TemplateId);
            FillPicturesSelectList();
            FillModesSelectList();

            return View(picture);
        }

        //
        // GET: /Picture/Upload

        public ActionResult Upload()
        {
            Picture picture = TempData["_newPicture"] as Picture;

            if (picture == null || picture.Frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Picture, picture.Frame.TemplateId);
            FillPicturesSelectList();
            FillModesSelectList();
            ViewBag.MaxImageSize = db.Settings.FirstOrDefault(s => s.Key == DisplayMonkey.Models.Setting.Key_MaxImageSize).IntValuePositive;

            return View(picture);
        }

        //
        // POST: /Picture/Upload

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(Picture picture, Frame frame, HttpPostedFileBase file)
        {
            picture.Frame = frame;
            
            if (ModelState.IsValid && 
                file != null && 
                file.ContentLength > 0)
            {
                // TODO: EditorFor, validate file type

                string ext = Path.GetExtension(file.FileName).Replace(".", "").ToUpper();
                if (Picture.SupportedFormats.Contains(ext))
                {
                    byte[] buffer = null;
                    using (BinaryReader reader = new BinaryReader(file.InputStream))
                    {
                        buffer = reader.ReadBytes(file.ContentLength);
                    }

                    picture.Content = new Models.Content
                    {
                        Type = ContentTypes.ContentType_Picture,
                        Name = Path.GetFileName(file.FileName),
                        Data = buffer,
                    };

                    db.Pictures.Add(picture);
                    db.SaveChanges();

                    return this.RestoreReferrer() ?? RedirectToAction("Index", "Frame");
                }
            }

            this.FillTemplatesSelectList(db, FrameTypes.Picture, picture.Frame.TemplateId);
            FillPicturesSelectList();
            FillModesSelectList();
            ViewBag.MaxImageSize = db.Settings.FirstOrDefault(s => s.Key == DisplayMonkey.Models.Setting.Key_MaxImageSize).IntValuePositive;

            return View(picture);
        }

        //
        // GET: /Picture/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return View("Missing", new MissingItem(id));
            }

            this.FillTemplatesSelectList(db, FrameTypes.Picture, picture.Frame.TemplateId);
            FillPicturesSelectList(picture.ContentId);
            FillModesSelectList(picture.Mode);

            return View(picture);
        }

        //
        // POST: /Picture/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Picture picture, Frame frame)
        {
            if (ModelState.IsValid)
            {
                db.Entry(frame).State = EntityState.Modified;
                db.Entry(picture).State = EntityState.Modified;
                db.SaveChanges();

                return this.RestoreReferrer() ?? RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Picture, picture.Frame.TemplateId);
            FillPicturesSelectList(picture.ContentId);
            FillModesSelectList(picture.Mode);

            picture.Frame = frame;

            return View(picture);
        }

        //
        // GET: /Picture/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(picture);
        }

        //
        // POST: /Picture/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Frame frame = db.Frames.Find(id);
            db.Frames.Remove(frame);
            db.SaveChanges();

            return this.RestoreReferrer(true) ?? RedirectToAction("Index", "Frame");
        }

        private void FillPicturesSelectList(object selected = null)
        {
            var savedPictures = from p in db.Contents
                                where p.Type == ContentTypes.ContentType_Picture
                                orderby p.Name
                                select p;

            ViewBag.Pictures = new SelectList(savedPictures, "ContentId", "Name", selected);
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