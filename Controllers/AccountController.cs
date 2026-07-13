using Recruitment_Portal.Models;
using Recruitment_Portal.Utils;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Helpers;
using System.Transactions;
using System.Web.Mvc;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Org.BouncyCastle.Crypto.Generators;
using DocumentFormat.OpenXml.EMMA;
using System.Web.Security;

public class AccountController : Controller
{
    private RoyalPortalDbEntities _rpdb =
        new RoyalPortalDbEntities();

    public ActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public JsonResult Register(YouthRegistrationRequest request)
    {
        try
        {
            if (request == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request."
                });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Phone) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Json(new
                {
                    success = false,
                    message = "Please complete all required fields."
                });
            }

            string email = request.Email.Trim().ToLower();

            bool emailExists = _rpdb.users.Any(x => x.email == email);

            if (emailExists)
            {
                return Json(new
                {
                    success = false,
                    message = "Email address already exists."
                });
            }

            bool phoneExists = _rpdb.users.Any(x => x.phone == request.Phone);

            if (phoneExists)
            {
                return Json(new
                {
                    success = false,
                    message = "Phone number already exists."
                });
            }

            var user = new user
            {
                id = Guid.NewGuid(),
                email = email,
                phone = request.Phone,
                password_hash = HashPassword(request.Password),
                role = "youth",
                is_verified = false,
                is_active = true,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _rpdb.users.Add(user);

            var profile = new youth_profiles
            {
                user_id = user.id,
                first_name = request.FirstName.Trim(),
                last_name = request.LastName.Trim(),
                profile_completion = 20,
                created_at = DateTime.Now
            };

            _rpdb.youth_profiles.Add(profile);

            _rpdb.SaveChanges();

            FormsAuthentication.SetAuthCookie(
                user.email,
                false
            );

            Session["UserId"] = user.id;
            Session["Role"] = user.role;

            return Json(new
            {
                success = true,
                message = "Registration successful.",
                redirectUrl = Url.Action(
                    "Dashboard",
                    "Candidate"
                )
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

    [HttpPost]
    public JsonResult Login(string email, string password)
    {
        try
        {
            string passwordHash = HashPassword(password);

            var user = _rpdb.users.FirstOrDefault(x =>
                x.email == email &&
                x.password_hash == passwordHash &&
                x.is_active == true);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid email or password."
                });
            }

            FormsAuthentication.SetAuthCookie(user.email, false);

            
            Session["UserId"] = user.id.ToString(); 
            Session["Role"] = user.role;

            return Json(new
            {
                success = true,
                redirectUrl = "/Candidate/Dashboard"
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

    //=======================================================
    //==============Company Register=========================
    //=======================================================

    public ActionResult CompanyRegister()
    {
        using (var db = new RoyalPortalDbEntities())
        {
            ViewBag.Districts = db.districts.ToList();
        }

        return View();
    }

    [HttpPost]
    public JsonResult CompanyRegister(CompanyRegisterVM model)
    {
        try
        {
            using (var db = new RoyalPortalDbEntities())
            {
                if (db.users.Any(x => x.email == model.Email))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Email already exists"
                    });
                }

                Guid userId = Guid.NewGuid();
                Guid companyId = Guid.NewGuid();

                user user = new user
                {
                    id = userId,
                    email = model.Email,
                    phone = model.Phone,
                    password_hash = Crypto.HashPassword(model.Password),
                    role = "employer",
                    is_active = true,
                    is_verified = false,
                    created_at = DateTime.Now
                };
                if (model.Password != model.ConfirmPassword)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Passwords do not match"
                    });
                }
                db.users.Add(user);

                company company = new company
                {
                    id = companyId,
                    company_name = model.CompanyName,
                    registration_number = model.RegistrationNumber,
                    email = model.Email,
                    phone = model.Phone,
                    ContactPerson = model.ContactPerson,
                    industry = model.Industry,
                    website = model.website,
                    district_id = model.DistrictId,
                    physical_address = model.physical_address,
                    verified = false,
                    created_at = DateTime.Now
                };

                db.companies.Add(company);

                company_users companyUser = new company_users
                {
                    company_id = companyId,
                    user_id = userId,
                    role = "Manager"
                };

                db.company_users.Add(companyUser);

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Company registered successfully"
                });
            }
        }
        catch (Exception ex)
        {
            var error = ex.ToString(); 

            return Json(new
            {
                success = false,
                message = error
            });
        }
    }

    [HttpPost]
    public JsonResult CompanyLogin(LoginVM model)
    {
        using (var db = new RoyalPortalDbEntities())
        {
            var email = model.Email?.Trim().ToLower();

            var user = db.users
                .FirstOrDefault(x => x.email.ToLower() == email);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid credentials"
                });
            }

            bool validPassword = Crypto.VerifyHashedPassword(
                user.password_hash,
                model.Password);

            if (!validPassword)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid credentials"
                });
            }

            var company = db.company_users
                .FirstOrDefault(x => x.user_id == user.id);

            if (company == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No company linked to this account"
                });
            }

            Session["UserId"] = user.id;
            Session["Role"] = user.role;
            Session["CompanyId"] = company.company_id;

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("CompanyDashboard", "Company")
            });
        }
    
}

    //=======================================================
    //================End of Company Register===============
    //=======================================================



public ActionResult Logout()
    {
        FormsAuthentication.SignOut();

        Session.Clear();
        Session.Abandon();

        return RedirectToAction(
            "Index",
            "Home"
        );
    }

    private string HashPassword(string password)
    {
        return Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(password)
        );
    }
}