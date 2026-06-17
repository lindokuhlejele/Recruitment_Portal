using Recruitment_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Recruitment_Portal.Controllers
{
    public class ApplicationsController : Controller
    {
        private RoyalPortalDbEntities _rpdb = new RoyalPortalDbEntities();
        // GET: Applications
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Apply(Guid opportunityId)
        {
            var userId =
                Guid.Parse(Session["UserId"].ToString());

            using (var db = new RoyalPortalDbEntities())
            {
                bool exists =
                    db.applications.Any(x =>
                        x.user_id == userId &&
                        x.opportunity_id == opportunityId);

                if (exists)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Already applied"
                    });
                }

                db.applications.Add(new application
                {
                    id = Guid.NewGuid(),
                    user_id = userId,
                    opportunity_id = opportunityId,
                    status = "Applied",
                    applied_at = DateTime.Now
                });

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Application submitted"
                });
            }
        }
    }
}