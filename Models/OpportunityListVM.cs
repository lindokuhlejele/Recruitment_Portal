using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recruitment_Portal.Models
{
    public class OpportunityListVM
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string CompanyName { get; set; }

        public string Category { get; set; }

        public string Location { get; set; }

        public string District { get; set; }

        public decimal? Stipend { get; set; }

        public DateTime? ClosingDate { get; set; }

        public string Status { get; set; }
    }

    public class OpportunityDetailsVM
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Requirements { get; set; }

        public string Location { get; set; }

        public string District { get; set; }

        public string Category { get; set; }

        public decimal? Stipend { get; set; }

        public DateTime? ClosingDate { get; set; }

        public string Status { get; set; }

        public string CompanyName { get; set; }

        public string Industry { get; set; }
    }
}