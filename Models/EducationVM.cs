using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recruitment_Portal.Models
{
    public class EducationVM
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