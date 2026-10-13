using System.ComponentModel.DataAnnotations;

namespace JobBoardSystem.Models
{
    public enum WorkType
    {
        [Display(Name = "Full-time")]
        FullTime = 0,

        [Display(Name = "Part-time")]
        PartTime = 1,

        [Display(Name = "Contract")]
        Contract = 2,

        [Display(Name = "Internship")]
        Internship = 3,

        [Display(Name = "Remote")]
        Remote = 4
    }

    public enum JobPostingStatus
    {
        [Display(Name = "Active")]
        Active = 0,

        [Display(Name = "Closed")]
        Closed = 1,

        [Display(Name = "Expired")]
        Expired = 2
    }

    public enum ApplicationStatus
    {
        [Display(Name = "Submitted")]
        Submitted = 0,

        [Display(Name = "Under Review")]
        UnderReview = 1,

        [Display(Name = "Shortlisted")]
        Shortlisted = 2,

        [Display(Name = "Interview Scheduled")]
        InterviewScheduled = 3,

        [Display(Name = "Offered")]
        Offered = 4,

        [Display(Name = "Rejected")]
        Rejected = 5,

        [Display(Name = "Withdrawn")]
        Withdrawn = 6
    }

    public enum SkillLevel
    {
        [Display(Name = "Beginner")]
        Beginner = 0,

        [Display(Name = "Intermediate")]
        Intermediate = 1,

        [Display(Name = "Advanced")]
        Advanced = 2,

        [Display(Name = "Expert")]
        Expert = 3
    }

    public enum LanguageProficiency
    {
        [Display(Name = "Basic")]
        Basic = 0,

        [Display(Name = "Conversational")]
        Conversational = 1,

        [Display(Name = "Fluent")]
        Fluent = 2,

        [Display(Name = "Native")]
        Native = 3
    }
}
