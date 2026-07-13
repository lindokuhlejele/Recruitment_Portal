using Recruitment_Portal.Models;
using Recruitment_Portal.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Helpers;

namespace Recruitment_Portal.Controllers
{
    public class CompanyController : Controller
    {
        private RoyalPortalDbEntities _rpdb = new RoyalPortalDbEntities();

        // =====================================================
        // DASHBOARD
        // =====================================================
        public ActionResult CompanyDashboard()
        {
            if (Session["CompanyId"] == null)
                return RedirectToAction("Index", "Home");

            var companyId = (Guid)Session["CompanyId"];
            ViewBag.RecentOpportunities = _rpdb.opportunities
    .Where(x => x.company_id == companyId)
    .OrderByDescending(x => x.created_at)
    .Take(5)
    .ToList();
            ViewBag.TotalOpportunities = _rpdb.opportunities
                .Count(x => x.company_id == companyId);

            ViewBag.ActiveOpportunities = _rpdb.opportunities
                .Count(x => x.company_id == companyId && x.status == "Active");

            ViewBag.ApplicationsReceived = (from a in _rpdb.applications
                                            join o in _rpdb.opportunities
                                            on a.opportunity_id equals o.id
                                            where o.company_id == companyId
                                            select a).Count();

            ViewBag.InterviewsScheduled = (from i in _rpdb.interviews
                                           join a in _rpdb.applications
                                           on i.application_id equals a.id
                                           join o in _rpdb.opportunities
                                           on a.opportunity_id equals o.id
                                           where o.company_id == companyId
                                           select i).Count();

            return View();
        }

        // =====================================================
        // OPPORTUNITIES
        // =====================================================

        public ActionResult Opportunities()
        {
            var companyId = (Guid)Session["CompanyId"];

            var data = _rpdb.opportunities
                .Where(x => x.company_id == companyId)
                .OrderByDescending(x => x.created_at)
                .ToList();

            return View(data);
        }

        [HttpPost]
        public JsonResult CreateOpportunity(OpportunityVM model)
        {
            try
            {
                var companyId = (Guid)Session["CompanyId"];

                var opp = new opportunity
                {
                    id = Guid.NewGuid(),
                    company_id = companyId,
                    title = model.Title,
                    description = model.Description,
                    location = model.Location,
                    requirements = model.Requirements,
                    stipend = model.Stipend,
                    district_id = model.DistrictId,
                    category_id = model.CategoryId,
                    status = "Active",
                    created_at = DateTime.Now
                };

                _rpdb.opportunities.Add(opp);
                _rpdb.SaveChanges();

                return Json(new { success = true, message = "Opportunity created" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EditOpportunity(OpportunityVM model)
        {
            try
            {
                var opp = _rpdb.opportunities.Find(model.Id);

                if (opp == null)
                    return Json(new { success = false, message = "Not found" });

                opp.title = model.Title;
                opp.description = model.Description;
                opp.location = model.Location;
                opp.requirements = model.Requirements;
                opp.stipend = model.Stipend;
                opp.district_id = model.DistrictId;
                opp.category_id = model.CategoryId;

                _rpdb.SaveChanges();

                return Json(new { success = true, message = "Updated" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CloseOpportunity(Guid id)
        {
            var opp = _rpdb.opportunities.Find(id);

            if (opp == null)
                return Json(new { success = false, message = "Not found" });

            opp.status = "Closed";
            _rpdb.SaveChanges();

            return Json(new { success = true });
        }

        // =====================================================
        // APPLICATIONS
        // =====================================================

        public ActionResult Applications(Guid opportunityId)
        {
            var data = (from a in _rpdb.applications
                        join u in _rpdb.users on a.user_id equals u.id
                        where a.opportunity_id == opportunityId
                        select new ApplicationVM
                        {
                            Id = a.id,
                            Email = u.email,
                            Status = a.status,
                            AppliedAt = a.applied_at
                        }).ToList();

            return View(data);
        }

        [HttpPost]
        public JsonResult UpdateApplicationStatus(Guid applicationId, string status)
        {
            using (var db = new RoyalPortalDbEntities())
            {
                var app = db.applications.FirstOrDefault(x => x.id == applicationId);

                if (app == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Application not found"
                    });
                }

                app.status = status;
                app.updated_at = DateTime.Now;

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Status updated"
                });
            }
        }

        // =====================================================
        // INTERVIEWS
        // =====================================================

        [HttpPost]
        public JsonResult ScheduleInterview(InterviewVM model)
        {
            try
            {
                var interview = new interview
                {
                    id = Guid.NewGuid(),
                    application_id = model.ApplicationId,
                    interview_date = model.InterviewDate,
                    interview_mode = model.Mode,
                    notes = model.Notes,
                    status = "Scheduled"
                };

                _rpdb.interviews.Add(interview);
                _rpdb.SaveChanges();

                return Json(new { success = true, message = "Interview scheduled" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =====================================================
        // COMPANY PROFILE
        // =====================================================

        public ActionResult Profile()
        {
            var companyId = (Guid)Session["CompanyId"];

            var company = _rpdb.companies
                .FirstOrDefault(x => x.id == companyId);

            return View(company);
        }

        // =====================================================
        // COMPANY USERS
        // =====================================================

        public ActionResult Users()
        {
            var companyId = (Guid)Session["CompanyId"];

            var users = (from cu in _rpdb.company_users
                         join u in _rpdb.users on cu.user_id equals u.id
                         where cu.company_id == companyId
                         select new CompanyUserVM
                         {
                             UserId = u.id,
                             Email = u.email,
                             Role = cu.role
                         }).ToList();

            return View(users);
        }

        [HttpPost]
        public JsonResult AddCompanyUser(AddCompanyUserVM model)
        {
            try
            {
                var companyId = (Guid)Session["CompanyId"];

                var user = new user
                {
                    id = Guid.NewGuid(),
                    email = model.Email,
                    phone = model.Phone,
                    password_hash = Crypto.HashPassword(model.Password),
                    role = "User",
                    is_active = true,
                    created_at = DateTime.Now
                };

                _rpdb.users.Add(user);
                _rpdb.SaveChanges();

                _rpdb.company_users.Add(new company_users
                {
                    company_id = companyId,
                    user_id = user.id,
                    role = model.Role
                });

                _rpdb.SaveChanges();

                return Json(new { success = true, message = "User added" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Pipeline(Guid opportunityId)
        {
            var applicants = (from a in _rpdb.applications
                              join u in _rpdb.users on a.user_id equals u.id
                              join yp in _rpdb.youth_profiles on u.id equals yp.user_id
                              where a.opportunity_id == opportunityId
                              select new PipelineApplicantVM
                              {
                                  ApplicationId = a.id,
                                  UserId = u.id,
                                  FullName = yp.first_name + " " + yp.last_name,
                                  Email = u.email,
                                  Status = a.status
                              }).ToList();

            ViewBag.OpportunityId = opportunityId;

            return View(applicants);
        }

        public ActionResult Applicants(Guid opportunityId)
        {
            using (var db = new RoyalPortalDbEntities())
            {
                var companyId =
                    Guid.Parse(Session["CompanyId"].ToString());

                // security check: ensure opportunity belongs to company
                var opportunityExists =
                    db.opportunities.Any(x =>
                        x.id == opportunityId &&
                        x.company_id == companyId);

                if (!opportunityExists)
                    return HttpNotFound();

                var applicants =
                    (from a in db.applications

                     join u in db.users
                        on a.user_id equals u.id

                     join y in db.youth_profiles
                        on u.id equals y.user_id into yp
                     from profile in yp.DefaultIfEmpty()

                     where a.opportunity_id == opportunityId

                     select new Models.ApplicantVM
                     {
                         ApplicationId = a.id,
                         UserId = u.id,
                         FullName = profile.first_name + " " + profile.last_name,
                         Email = u.email,
                         Phone = u.phone,
                         Status = a.status,
                         AppliedAt = a.applied_at,
                         CvUrl = db.generated_cvs
                                    .Where(c => c.user_id == u.id)
                                    .Select(c => c.file_url)
                                    .FirstOrDefault(),

                         ProfileCompletion = profile.profile_completion ?? 0
                     }).ToList();

                ViewBag.OpportunityId = opportunityId;

                return View(applicants);
            }
        }



        // =====================================================
        // DISPOSE
        // =====================================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rpdb.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}