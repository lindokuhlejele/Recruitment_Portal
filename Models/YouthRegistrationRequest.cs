using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Recruitment_Portal.Models
{
    public class YouthRegistrationRequest
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Password { get; set; }
    }

    public class EducationDto
    {
        public string InstitutionName { get; set; }
        public string Qualification { get; set; }
        public string FieldOfStudy { get; set; }
        public string Level { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Completed { get; set; }
    }



}
