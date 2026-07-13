using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recruitment_Portal.Models
{
    public class CompanyRegisterVM
    {
        public string CompanyName { get; set; }
        public string RegistrationNumber { get; set; }
        public string Industry { get; set; }
        public Guid DistrictId { get; set; }

        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string website { get; set; }
        public string physical_address { get; set; }

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }


    public class LoginVM
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}