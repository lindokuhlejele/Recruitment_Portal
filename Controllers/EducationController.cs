using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Spreadsheet;
using Recruitment_Portal.Models;
using Recruitment_Portal.Utils;

namespace Recruitment_Portal.Controllers
{
    public class EducationController : Controller
    {
        private RoyalPortalDbEntities _db = new RoyalPortalDbEntities();

        // =========================
        // LIST EDUCATION
        // =========================
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            Guid userId = AuthHelper.GetUserId(Session);

            var service = new ProfileCompletionService();
            
            var profile = _db.youth_profiles.FirstOrDefault(x => x.user_id == userId);
            ViewBag.MissingFields = GetMissingFields(userId);
            ViewBag.Completion = service.Calculate(userId);

            userId = Guid.Parse(Session["UserId"].ToString());

            ViewBag.Completion = profile.profile_completion;

            var education = _db.educations
                .Where(x => x.user_id == userId)
                .OrderByDescending(x => x.start_date)
                .ToList();

          

            ViewBag.Profile = profile;   

            ViewBag.UserId = userId;




            return View(education);
        }

        // =========================
        // ADD EDUCATION (AJAX)
        // =========================
        [HttpPost]
        public JsonResult Add(education model)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return Json(new { success = false, message = "Session expired" });
                }

                Guid userId = AuthHelper.GetUserId(Session);

                var edu = new education
                {
                    user_id = userId,
                    institution_name = model.institution_name,
                    qualification = model.qualification,
                    field_of_study = model.field_of_study,
                    level = model.level,
                    start_date = model.start_date,
                    end_date = model.end_date,
                    completed = model.completed
                };

                _db.educations.Add(edu);
                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =========================
        // DELETE EDUCATION
        // =========================
        [HttpPost]
        public JsonResult Delete(Guid id)
        {
            try
            {
                var edu = _db.educations.FirstOrDefault(x => x.id == id);

                if (edu == null)
                    return Json(new { success = false, message = "Not found" });

                _db.educations.Remove(edu);
                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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


        public JsonResult SearchSkills(string term)
        {
            var results = _db.skills
                .Where(x => x.name.Contains(term))
                .Select(x => new
                {
                    id = x.id,
                    name = x.name
                })
                .Take(10)
                .ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateSkill(string name)
        {
            var exists = _db.skills.FirstOrDefault(x => x.name == name);

            if (exists != null)
            {
                return Json(new { success = true, id = exists.id, name = exists.name });
            }

            var skill = new skill
            {
                id = Guid.NewGuid(),
                name = name
            };

            _db.skills.Add(skill);
            _db.SaveChanges();

            return Json(new { success = true, id = skill.id, name = skill.name });
        }

        [HttpPost]
        public JsonResult SaveUserSkills(List<Guid> skillIds)
        {
            Guid userId = AuthHelper.GetUserId(Session);

            var existing = _db.user_skills.Where(x => x.user_id == userId);
            _db.user_skills.RemoveRange(existing);

            foreach (var id in skillIds)
            {
                _db.user_skills.Add(new user_skills
                {
                    user_id = userId,
                    skill_id = id,
                    proficiency_level = "Basic"
                });
            }

            _db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
