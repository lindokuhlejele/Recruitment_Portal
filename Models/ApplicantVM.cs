using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recruitment_Portal.Models
{
    public class ApplicantVM
    {
        public Guid ApplicationId { get; set; }

        public Guid UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Status { get; set; }

        public DateTime? AppliedAt { get; set; }

        public string CvUrl { get; set; }

        public int ProfileCompletion { get; set; }
    }

    public class MyApplicationVM
    {
        public Guid ApplicationId { get; set; }

        public Guid OpportunityId { get; set; }

        public string JobTitle { get; set; }

        public string CompanyName { get; set; }

        public string Location { get; set; }

        public DateTime? AppliedAt { get; set; }

        public string Status { get; set; }

        public DateTime? ClosingDate { get; set; }
    }

    public class DocumentUploadVM
    {
        public string DocumentType { get; set; }

        public HttpPostedFileBase File { get; set; }
    }

    public static class DocumentTypes
    {
        public const string CV = "CV";
        public const string Qualification = "Qualification";
        public const string Certificate = "Certificate";
        public const string ID = "ID";
        public const string Other = "Other";
    }
}