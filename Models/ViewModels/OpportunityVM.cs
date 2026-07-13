using System;
using System.ComponentModel.DataAnnotations;

namespace Recruitment_Portal.ViewModels
{
    public class OpportunityVM
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public string Requirements { get; set; }

        public decimal? Stipend { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? CategoryId { get; set; }
    }

    public class ApplicationVM
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Status { get; set; }

        public DateTime? AppliedAt { get; set; }
    }

    public class InterviewVM
    {
        public Guid ApplicationId { get; set; }

        public DateTime InterviewDate { get; set; }

        public string Mode { get; set; } // Online / Physical

        public string Notes { get; set; }
    }

    public class CompanyUserVM
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public string Role { get; set; } // HR / Recruiter / Manager
    }

    public class AddCompanyUserVM
    {
        [Required]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // HR / Recruiter / Manager
    }

    public class CompanyProfileVM
    {
        public Guid Id { get; set; }

        public string CompanyName { get; set; }

        public string RegistrationNumber { get; set; }

        public string Industry { get; set; }

        public string Website { get; set; }

        public bool? Verified { get; set; }
    }

    public class PipelineApplicantVM
    {
        public Guid ApplicationId { get; set; }

        public Guid UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Status { get; set; }
    }

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
}