using Recruitment_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Recruitment_Portal.Controllers
{
    public class OpportunitiesController : Controller
    {
        // GET: Opportunities
        public ActionResult Index(
    string search,
    Guid? districtId,
    Guid? categoryId)
        {
            using (var db = new RoyalPortalDbEntities())
            {
                var query =
                    from o in db.opportunities

                    join c in db.companies
                        on o.company_id equals c.id

                    join d in db.districts
                        on o.district_id equals d.id into districts
                    from district in districts.DefaultIfEmpty()

                    join cat in db.opportunity_categories
                        on o.category_id equals cat.id into cats
                    from category in cats.DefaultIfEmpty()

                    where o.status == "Active"

                    select new OpportunityListVM
                    {
                        Id = o.id,
                        Title = o.title,
                        CompanyName = c.company_name,
                        Category = category.name,
                        Location = o.location,
                        District = district.name,
                        Stipend = o.stipend,
                        ClosingDate = o.closing_date,
                        Status = o.status
                    };

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.Title.Contains(search) ||
                        x.CompanyName.Contains(search));
                }

                if (districtId.HasValue)
                {
                    query = query.Where(x =>
                        db.opportunities.Any(o =>
                            o.id == x.Id &&
                            o.district_id == districtId));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(x =>
                        db.opportunities.Any(o =>
                            o.id == x.Id &&
                            o.category_id == categoryId));
                }

                ViewBag.Districts = db.districts.ToList();
                ViewBag.Categories = db.opportunity_categories.ToList();

                return View(query.ToList());
            }
        }


        public ActionResult Details(Guid id)
        {
            using (var db = new RoyalPortalDbEntities())
            {
                var opportunity =
                    (from o in db.opportunities
                     join c in db.companies
                        on o.company_id equals c.id

                     join d in db.districts
                        on o.district_id equals d.id into districts
                     from district in districts.DefaultIfEmpty()

                     join cat in db.opportunity_categories
                        on o.category_id equals cat.id into categories
                     from category in categories.DefaultIfEmpty()

                     where o.id == id

                     select new OpportunityDetailsVM
                     {
                         Id = o.id,
                         Title = o.title,
                         Description = o.description,
                         Requirements = o.requirements,
                         Location = o.location,
                         Stipend = o.stipend,
                         ClosingDate = o.closing_date,
                         Status = o.status,

                         CompanyName = c.company_name,
                         Industry = c.industry,

                         District = district.name,
                         Category = category.name
                     })
                     .FirstOrDefault();

                if (opportunity == null)
                    return HttpNotFound();

                if (Session["UserId"] != null)
                {
                    Guid userId =
                        Guid.Parse(Session["UserId"].ToString());

                    ViewBag.AlreadyApplied =
                        db.applications.Any(x =>
                            x.user_id == userId &&
                            x.opportunity_id == id);
                }

                return View(opportunity);
            }
        }
    }
}