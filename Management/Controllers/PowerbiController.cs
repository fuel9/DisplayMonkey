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
using System.Net;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;
using System.Text.RegularExpressions;
using DisplayMonkey.Language;
using System.IO;
using System.Web.Script.Serialization;
using DisplayMonkey.AzureUtil;
using System.Threading.Tasks;

namespace DisplayMonkey.Controllers
{
    public class PowerbiController : BaseController
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        // GET: /Powerbi/Details/5
        public ActionResult Details(int id = 0)
        {
            Powerbi powerbi = db.Frames.Find(id) as Powerbi;
            if (powerbi == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(powerbi);
        }

        // GET: /Powerbi/Create
        public ActionResult Create()
        {
            Frame frame = TempData[FrameController.SelectorFrameKey] as Frame;

            if (frame == null || frame.PanelId == 0)
            {
                return RedirectToAction("Create", "Frame");
            }

            Powerbi powerbi = new Powerbi(frame, db);

            this.FillTemplatesSelectList(db, FrameTypes.Powerbi);
            FillTypesSelectList();
            FillAccountsSelectList();
            

            return View(powerbi);
        }

        // POST: /Powerbi/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Powerbi powerbi)
        {
            if (ModelState.IsValid)
            {
                db.Frames.Add(powerbi);
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Powerbi, powerbi.TemplateId);
            FillTypesSelectList(powerbi.Type);
            FillAccountsSelectList(powerbi.AccountId);
            

            return View(powerbi);
        }

        // GET: /Powerbi/Edit/5
        public ActionResult Edit(int id = 0)
        {
            Powerbi powerbi = db.Frames.Find(id) as Powerbi;
            if (powerbi == null)
            {
                return View("Missing", new MissingItem(id));
            }

            this.FillTemplatesSelectList(db, FrameTypes.Powerbi, powerbi.TemplateId);
            FillTypesSelectList(powerbi.Type);
            FillAccountsSelectList(powerbi.AccountId);
            

            return View(powerbi);
        }

        // POST: /Powerbi/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Powerbi powerbi)
        {
            if (ModelState.IsValid)
            {
                db.Entry(powerbi).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "Frame");
            }

            this.FillTemplatesSelectList(db, FrameTypes.Powerbi, powerbi.TemplateId);
            FillTypesSelectList(powerbi.Type);
            FillAccountsSelectList(powerbi.AccountId);
            
            
            return View(powerbi);
        }

        // GET: /Powerbi/Delete/5
        public ActionResult Delete(int id = 0)
        {
            Powerbi powerbi = db.Frames.Find(id) as Powerbi;
            if (powerbi == null)
            {
                return View("Missing", new MissingItem(id));
            }
            return View(powerbi);
        }

        // POST: /Powerbi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Frame frame = db.Frames.Find(id);
            db.Frames.Remove(frame);
            db.SaveChanges();

            return RedirectToAction("Index", "Frame");
        }

        #region Workspace helper

        //
        // POST: /Powerbi/groups/5
        [HttpPost, ActionName("Groups")]
        public async Task<JsonResult> GroupsAsync(int accountId)
        {
            try
            {
                var x = new List<SelectListItem>();

                AzureAccount acc = await AzureAccountRefreshTokenAsync(accountId);
                if (acc != null)
                {
                    WebRequest request = WebRequest.Create(String.Format(
                        "{0}/groups", 
                        _baseUrl
                        )) as System.Net.HttpWebRequest;

                    request.Method = "GET";
                    request.ContentLength = 0;
                    request.Headers.Add("Authorization", String.Format("Bearer {0}", acc.AccessToken));

                    using (var response = await request.GetResponseAsync() as HttpWebResponse)
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseContent = await reader.ReadToEndAsync();
                        PBIGroups groups = new JavaScriptSerializer().Deserialize<PBIGroups>(responseContent);
                        foreach (PBIGroup i in groups.value.OrderBy(j => j.name))
                        {
                            x.Add(new SelectListItem()
                            {
                                Value = i.id,
                                Text = i.name,
                            });
                        }
                    }
                }

                return Json(new { success = true, data = x.ToList() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private class PBIGroups
        {
            public PBIGroup[] value { get; set; }
        }
        private class PBIGroup
        {
            public string id { get; set; }

            public string name { get; set; }

            public bool isReadOnly { get; set; }
        }

        #endregion  // Workspace helper

        #region Reports helper

        //
        // POST: /Powerbi/reports/5
        [HttpPost, ActionName("Reports")]
        public async Task<JsonResult> ReportsAsync(int accountId, string group = null)
        {
            try 
            {
                JsonResult xx = await GroupsAsync(accountId);
                
                var x = new List<SelectListItemWithUrl>();

                AzureAccount acc = await AzureAccountRefreshTokenAsync(accountId);
                if (acc != null)
                {
                    WebRequest request = WebRequest.Create(String.Format(
                        "{0}{1}/reports", 
                        _baseUrl,
                        String.IsNullOrWhiteSpace(group) ? "" : String.Format("/groups/{0}", group)
                        )) as System.Net.HttpWebRequest;

                    request.Method = "GET";
                    request.ContentLength = 0;
                    request.Headers.Add("Authorization", String.Format("Bearer {0}", acc.AccessToken));

                    using (var response = await request.GetResponseAsync() as HttpWebResponse)
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseContent = await reader.ReadToEndAsync();
                        PBIReports reports = new JavaScriptSerializer().Deserialize<PBIReports>(responseContent);
                        foreach (PBIReport i in reports.value.Where(j => j.embedUrl != "").OrderBy(j => j.name))
                        {
                            x.Add(new SelectListItemWithUrl()
                            {
                                Value = i.id,
                                Text = i.name,
                                Url = i.embedUrl,
                            });
                        }
                    }
                }

                return Json(new { success = true, data = x.ToList() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private class PBIReports
        {
            public PBIReport[] value { get; set; }
        }
        private class PBIReport
        {
            public string id { get; set; }

            // the name of this property will change to 'displayName' when the API moves from Beta to V1 namespace
            public string name { get; set; }

            public string webUrl { get; set; }

            public string embedUrl { get; set; }
        }

        #endregion  // Reports helper

        #region Dashboards helper

        //
        // POST: /Powerbi/dashboards/5
        [HttpPost, ActionName("Dashboards")]
        public async Task<JsonResult> DashboardsAsync(int accountId, string group = null)
        {
            try
            {
                var x = new List<SelectListItem>();

                AzureAccount acc = await AzureAccountRefreshTokenAsync(accountId);
                if (acc != null)
                {
                    WebRequest request = WebRequest.Create(String.Format(
                        "{0}{1}/dashboards", 
                        _baseUrl,
                        String.IsNullOrWhiteSpace(group) ? "" : String.Format("/groups/{0}", group)
                        )) as System.Net.HttpWebRequest;

                    System.Diagnostics.Debug.Print(request.RequestUri.OriginalString);
                    request.Method = "GET";
                    request.ContentLength = 0;
                    request.Headers.Add("Authorization", String.Format("Bearer {0}", acc.AccessToken));

                    using (var response = await request.GetResponseAsync() as HttpWebResponse)
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseContent = await reader.ReadToEndAsync();
                        PBIDashboards reports = new JavaScriptSerializer().Deserialize<PBIDashboards>(responseContent);
                        foreach (PBIDashboard i in reports.value.OrderBy(j => j.displayName))
                        {
                            x.Add(new SelectListItem()
                            {
                                Value = i.id,
                                Text = i.displayName,
                            });
                        }
                    }
                }

                return Json(new { success = true, data = x.ToList() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private class PBIDashboards
        {
            public PBIDashboard[] value { get; set; }
        }
        
        private class PBIDashboard
        {
            public string id { get; set; }
            public string displayName { get; set; }
        }

        #endregion

        #region Tiles helper

        //
        // POST: /Powerbi/tiles/5
        [HttpPost, ActionName("Tiles")]
        public async Task<JsonResult> TilesAsync(int accountId, string dashboard, string group = null)
        {
            try
            {
                var x = new List<SelectListItemWithUrl>();

                AzureAccount acc = await AzureAccountRefreshTokenAsync(accountId);
                if (acc != null)
                {
                    WebRequest request = WebRequest.Create(String.Format(
                        "{0}{1}/dashboards/{2}/tiles", 
                        _baseUrl,
                        String.IsNullOrWhiteSpace(group) ? "" : String.Format("/groups/{0}", group), 
                        dashboard
                        )) as System.Net.HttpWebRequest;

                    request.Method = "GET";
                    request.ContentLength = 0;
                    request.Headers.Add("Authorization", String.Format("Bearer {0}", acc.AccessToken));

                    using (var response = await request.GetResponseAsync() as HttpWebResponse)
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseContent = await reader.ReadToEndAsync();
                        PBITiles reports = new JavaScriptSerializer().Deserialize<PBITiles>(responseContent);
                        foreach (PBITile i in reports.value.Where(j => j.embedUrl != "").OrderBy(j => j.title))
                        {
                            x.Add(new SelectListItemWithUrl()
                            {
                                Value = i.id,
                                Text = i.title,
                                Url = i.embedUrl,
                            });
                        }
                    }
                }

                return Json(new { success = true, data = x.ToList() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private class PBITiles
        {
            public PBITile[] value { get; set; }
        }

        private class PBITile
        {
            public string id { get; set; }
            public string title { get; set; }
            public string embedUrl { get; set; }
        }

        #endregion

        #region Misc members

        //
        // POST: /Powerbi/token/5
        [HttpPost, ActionName("Token")]
        public async Task<JsonResult> TokenAsync(int accountId)
        {
            try
            {
                AzureAccount acc = await AzureAccountRefreshTokenAsync(accountId);
                if (acc == null)
                {
                    throw new ApplicationException(Resources.FailedToObtainToken);
                }

                return Json(new { success = true, data = acc.AccessToken }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<AzureAccount> AzureAccountRefreshTokenAsync(int accountId)
        {
            AzureAccount az = db.AzureAccounts.Find(accountId);
            if (az != null && string.IsNullOrWhiteSpace(az.AccessToken) || !az.ExpiresOn.HasValue || az.ExpiresOn.Value < DateTime.UtcNow)
            {
                TokenInfo ti = await Token.GetGrantTypePasswordAsync(
                    az.Resource,
                    az.ClientId,
                    az.ClientSecret,
                    az.User,
                    Setting.GetEncryptor(db).Decrypt(az.Password),
                    az.TenantId
                    );

                az.RefreshToken = ti.RefreshToken;
                az.AccessToken = ti.AccessToken;
                az.ExpiresOn = ti.ExpiresOn;
                az.IdToken = ti.IdToken;

                db.Entry(az).State = EntityState.Modified;
                db.SaveChanges();
            }

            return az;
        }

        private const string _baseUrl = "https://api.powerbi.com/v1.0/myorg";

        private void FillTypesSelectList(PowerbiTypes? selected = null)
        {
            ViewBag.Types = selected.TranslatedSelectList();
        }

        private void FillAccountsSelectList(object selected = null)
        {
            ViewBag.Accounts = new SelectList(db.AzureAccounts, "AccountId", "Name", selected);
        }

        private class SelectListItemWithUrl : SelectListItem
        {
            public string Url { get; set; }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
