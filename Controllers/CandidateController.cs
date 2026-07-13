using Recruitment_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoyalPortalDb.Services;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Recruitment_Portal.Controllers
{

    public class CandidateController : Controller
    {
        private RoyalPortalDbEntities _rpdb =
       new RoyalPortalDbEntities();
        // GET: Candidate
        public ActionResult Dashboard()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            Guid userId = AuthHelper.GetUserId(Session);
            var service = new ProfileCompletionService();
           
            var profile = _rpdb.youth_profiles.FirstOrDefault(x => x.user_id == userId);
            ViewBag.MissingFields = GetMissingFields(userId);
            ViewBag.Completion = service.Calculate(userId);


            var work = _rpdb.work_experience
                .Where(x => x.user_id == userId)
                .ToList();

            var education = _rpdb.educations
                .Where(x => x.user_id == userId)
                .ToList();


            var documents = _rpdb.documents
                .Where(x => x.user_id == userId)
                .ToList();

            var userSkills = (from us in _rpdb.user_skills
                              join s in _rpdb.skills on us.skill_id equals s.id
                              where us.user_id == userId
                              select new
                              {
                                  id = s.id,
                                  name = s.name
                              }).ToList();

            ViewBag.UserSkills = userSkills;

            ViewBag.Documents = documents;

            _rpdb.SaveChanges();

            ViewBag.Profile = profile;
            ViewBag.Work = work;
            ViewBag.Education = education;

            return View();
        }

        public ActionResult UserProfile()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            Guid userId = AuthHelper.GetUserId(Session);

            var profile = GetUserProfile(userId); 

            ViewBag.MissingFields = GetMissingFields(userId);

            
            var service = new ProfileCompletionService();
            profile.profile_completion = service.Calculate(userId);

            
            using (var db = new RoyalPortalDbEntities())
            {
                var existing = db.youth_profiles.FirstOrDefault(x => x.user_id == userId);

                if (existing != null)
                {
                    existing.profile_completion = profile.profile_completion;
                    db.SaveChanges();
                }
            }

            ViewBag.IsEditMode = false;

            return View(profile);
        }

        // GET: /YouthProfile/Get/5
        public youth_profiles GetUserProfile(Guid userId)
        {
            using (var db = new RoyalPortalDbEntities())
            {
                var profile = db.youth_profiles
                    .FirstOrDefault(x => x.user_id == userId);
                var service = new ProfileCompletionService();
                int completion = service.Calculate(userId);

                ViewBag.Completion = completion;

                if (profile == null)
                {
                    profile = new youth_profiles
                    {
                        user_id = userId,
                        created_at = DateTime.Now,
                        profile_completion = 0
                    };

                    db.youth_profiles.Add(profile);
                    db.SaveChanges();
                }

                return profile;
            }
        }

        // POST: /YouthProfile/Update
        [HttpPost]
        public JsonResult UpdateProfile(youth_profiles model)
        {
            try
            {
                Guid userId = AuthHelper.GetUserId(Session);

                using (var db = new RoyalPortalDbEntities())
                {
                    var user = db.youth_profiles
                        .FirstOrDefault(x => x.user_id == userId);


                    if (user == null)
                    {
                        return Json(new { success = false, message = "Profile not found (DB mismatch)" });
                    }

                    user.first_name = model.first_name;
                    user.last_name = model.last_name;
                    user.date_of_birth = model.date_of_birth;
                    user.gender = model.gender;
                    user.id_number = model.id_number;
                    user.address = model.address;
                    user.availability_status = model.availability_status;
                    user.drivers_license = model.drivers_license;
                    user.profile_photo_url = model.profile_photo_url;
                    user.bio = model.bio;
                    user.location = model.location;
                    var service = new ProfileCompletionService();

                    user.profile_completion = service.Calculate(user.user_id);
                   

                    db.SaveChanges();

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateWork(work_experience model)
        {
            var work = _rpdb.work_experience.FirstOrDefault(x => x.id == model.id);

            if (work == null)
                return Json(new { success = false });

            work.company_name = model.company_name;
            work.position = model.position;
            work.start_date = model.start_date;
            work.end_date = model.end_date;
            work.description = model.description;

            _rpdb.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult UpdateEducation(education model)
        {
            var edu = _rpdb.educations.FirstOrDefault(x => x.id == model.id);

            if (edu == null)
                return Json(new { success = false });

            edu.institution_name = model.institution_name;
            edu.qualification = model.qualification;
            edu.field_of_study = model.field_of_study;
            edu.level = model.level;
            edu.start_date = model.start_date;
            edu.end_date = model.end_date;
            edu.completed = model.completed;

            _rpdb.SaveChanges();

            return Json(new { success = true });
        }

        public ActionResult MyDocuments()
        {
            var userId = Guid.Parse(Session["UserId"].ToString());

            using (var db = new RoyalPortalDbEntities())
            {
                var docs = db.documents
                    .Where(x => x.user_id == userId)
                    .OrderByDescending(x => x.uploaded_at)
                    .ToList();

                return View(docs);
            }
        }

        [HttpPost]
        public JsonResult UploadDocument(DocumentUploadVM model)
        {
            try
            {
                var userId = Guid.Parse(Session["UserId"].ToString());

                if (model.File == null || model.File.ContentLength == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No file selected"
                    });
                }

                // validate size (5MB)
                if (model.File.ContentLength > 5 * 1024 * 1024)
                {
                    return Json(new
                    {
                        success = false,
                        message = "File too large (max 5MB)"
                    });
                }

                string fileName = Guid.NewGuid() +
                                  System.IO.Path.GetExtension(model.File.FileName);

                string folder = Server.MapPath("~/Uploads/Documents/");

                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                string path = System.IO.Path.Combine(folder, fileName);

                model.File.SaveAs(path);

                string fileUrl = "/Uploads/Documents/" + fileName;

                using (var db = new RoyalPortalDbEntities())
                {
                    db.documents.Add(new document
                    {
                        id = Guid.NewGuid(),
                        user_id = userId,
                        document_type = model.DocumentType,
                        file_url = fileUrl,
                        is_verified = false,
                        uploaded_at = DateTime.Now
                    });

                    db.SaveChanges();
                }

                return Json(new
                {
                    success = true,
                    message = "Document uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        public ActionResult MyApplications()
        {
            var userId = Guid.Parse(Session["UserId"].ToString());

            using (var db = new RoyalPortalDbEntities())
            {
                var data =
                    (from a in db.applications

                     join o in db.opportunities
                        on a.opportunity_id equals o.id

                     join c in db.companies
                        on o.company_id equals c.id

                     where a.user_id == userId

                     orderby a.applied_at descending

                     select new MyApplicationVM
                     {
                         ApplicationId = a.id,
                         OpportunityId = o.id,
                         JobTitle = o.title,
                         CompanyName = c.company_name,
                         Location = o.location,
                         AppliedAt = a.applied_at,
                         Status = a.status,
                         ClosingDate = o.closing_date
                     }).ToList();

                return View(data);
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

    }


}
