using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Recruitment_Portal.Models;


namespace RoyalPortalDb.Controllers
{
    public class WorkExperienceController : Controller
    {
        private RoyalPortalDbEntities _rpdb =
    new RoyalPortalDbEntities();


        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            Guid userId = AuthHelper.GetUserId(Session);
            var service = new ProfileCompletionService();
            
            var profile = _rpdb.youth_profiles.FirstOrDefault(x => x.user_id == userId);
            ViewBag.MissingFields = GetMissingFields(userId);
            ViewBag.Completion = service.Calculate(userId);

            userId = Guid.Parse(Session["UserId"].ToString());


            var education = _rpdb.educations
                .Where(x => x.user_id == userId)
                .ToList();

           

           

            var work = _rpdb.work_experience
                .Where(x => x.user_id == userId)
                .OrderByDescending(x => x.start_date)
                .ToList();

            ViewBag.Profile = profile;   
            ViewBag.Work = work;
            ViewBag.UserId = userId;

            return View();
        }

        // GET: /WorkExperience/GetByUser/5
        public JsonResult GetByUser(Guid userId)
        {
            var work = _rpdb.work_experience
                .Where(x => x.user_id == userId)
                .OrderByDescending(x => x.start_date)
                .ToList();

            return Json(work, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Add(work_experience model)
        {
            try
            {
                if (Session["UserId"] == null)
                    return Json(new { success = false, message = "Session expired" });

                model.id = Guid.NewGuid(); 
                model.user_id = Guid.Parse(Session["UserId"].ToString());

                _rpdb.work_experience.Add(model);
                _rpdb.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                var error = ex.InnerException?.InnerException?.Message
                            ?? ex.Message;

                return Json(new { success = false, message = error });
            }
        }

        // POST: /WorkExperience/Delete/5
        [HttpPost]
        public JsonResult Delete(Guid id)
        {
            var item = _rpdb.work_experience.FirstOrDefault(x => x.id == id);

            if (item == null)
                return Json(new { success = false });

            _rpdb.work_experience.Remove(item);
            _rpdb.SaveChanges();

            return Json(new { success = true });
        }


        public List<string> GetMissingFields(Guid userId)
        {
            var missing = new List<string>();

            using (var db = new RoyalPortalDbEntities())
            {
                var p = db.youth_profiles.FirstOrDefault(x => x.user_id == userId);

                if (string.IsNullOrEmpty(p.first_name)) missing.Add("First Name");
                if (string.IsNullOrEmpty(p.last_name)) missing.Add("Last Name");
                if (p.date_of_birth == null) missing.Add("Date of Birth");
                if (string.IsNullOrEmpty(p.gender)) missing.Add("Gender");
                if (string.IsNullOrEmpty(p.id_number)) missing.Add("ID Number");
                if (string.IsNullOrEmpty(p.address)) missing.Add("Address");
                if (string.IsNullOrEmpty(p.location)) missing.Add("Location");
                if (string.IsNullOrEmpty(p.bio)) missing.Add("Bio");
                if (string.IsNullOrEmpty(p.availability_status)) missing.Add("Availability Status");
                if (string.IsNullOrEmpty(p.drivers_license)) missing.Add("Driver’s License");
                if (string.IsNullOrEmpty(p.profile_photo_url)) missing.Add("Profile Photo");

                if (!db.work_experience.Any(x => x.user_id == userId))
                    missing.Add("Add Work Experience");

                if (!db.educations.Any(x => x.user_id == userId))
                    missing.Add("Add Education");
            }

            return missing;
        }


        public class ProfileCompletionService
        {
            public int Calculate(Guid userId)
            {
                using (var db = new RoyalPortalDbEntities())
                {
                    var profile = db.youth_profiles.FirstOrDefault(x => x.user_id == userId);
                    var work = db.work_experience.Where(x => x.user_id == userId).ToList();
                    var edu = db.educations.Where(x => x.user_id == userId).ToList();

                    int score = 0;

                    // ---------------- PROFILE (50%)
                    if (profile != null)
                    {
                        int profileFields = 10;
                        int filled = 0;

                        if (!string.IsNullOrEmpty(profile.first_name)) filled++;
                        if (!string.IsNullOrEmpty(profile.last_name)) filled++;
                        if (profile.date_of_birth != null) filled++;
                        if (!string.IsNullOrEmpty(profile.gender)) filled++;
                        if (!string.IsNullOrEmpty(profile.id_number)) filled++;
                        if (!string.IsNullOrEmpty(profile.address)) filled++;
                        if (!string.IsNullOrEmpty(profile.location)) filled++;
                        if (!string.IsNullOrEmpty(profile.availability_status)) filled++;
                        if (!string.IsNullOrEmpty(profile.drivers_license)) filled++;
                        if (!string.IsNullOrEmpty(profile.bio)) filled++;

                        score += (int)((filled / (double)profileFields) * 50);
                    }

                    // ---------------- WORK EXPERIENCE (25%)
                    if (work.Any())
                    {
                        score += 25;
                    }

                    // ---------------- EDUCATION (25%)
                    if (edu.Any())
                    {
                        score += 25;
                    }

                    return score;
                }
            }
        }
    }
}