using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using JobBoardSystem.Models;

namespace JobBoardSystem.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [Required]
        public string Role { get; set; }

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        public IList<SelectListItem> Roles { get; set; }
    }

    public class EditAccountViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }

    public class JobListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public int? CompanyProfileId { get; set; }
        public string CategoryName { get; set; }
        public string Location { get; set; }
        public string WorkType { get; set; }
        public string Salary { get; set; }
        public string Status { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsSaved { get; set; }
    }

    public class JobSearchViewModel
    {
        public string Keyword { get; set; }
        public int? CategoryId { get; set; }
        public string Location { get; set; }
        public WorkType? WorkType { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public JobPostingStatus? Status { get; set; }
        public IList<SelectListItem> Categories { get; set; }
        public IList<SelectListItem> Locations { get; set; }
        public IList<SelectListItem> WorkTypes { get; set; }
        public IList<SelectListItem> Statuses { get; set; }
        public IList<JobListItemViewModel> Jobs { get; set; }
    }

    public class JobDetailsViewModel
    {
        public JobPosting Job { get; set; }
        public IList<string> RequiredSkills { get; set; }
        public string StatusName { get; set; }
        public bool CanApply { get; set; }
        public string ApplyBlockReason { get; set; }
        public bool IsSaved { get; set; }
        public bool HasApplied { get; set; }
    }

    public class ApplyViewModel
    {
        public int JobPostingId { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Cover letter")]
        public string CoverLetter { get; set; }
    }

    public class HomeViewModel
    {
        public IList<JobCategory> Categories { get; set; }
        public IList<JobListItemViewModel> FeaturedJobs { get; set; }
        public int ActiveJobCount { get; set; }
        public int CompanyCount { get; set; }
        public int CategoryCount { get; set; }
    }

    public class CompanyListItemViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public string LogoPath { get; set; }
        public int ActiveJobCount { get; set; }
    }

    public class CompanyDetailsViewModel
    {
        public CompanyProfile Company { get; set; }
        public IList<JobListItemViewModel> ActiveJobs { get; set; }
    }

    public class ProfilePersonalInput
    {
        [StringLength(200)]
        public string Headline { get; set; }

        public string Summary { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }

    public class EducationInput
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string School { get; set; }

        [Required, StringLength(150)]
        public string Degree { get; set; }

        [StringLength(150)]
        public string FieldOfStudy { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public string Description { get; set; }
    }

    public class ExperienceInput
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string CompanyName { get; set; }

        [Required, StringLength(150)]
        public string JobTitle { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        public string Description { get; set; }
    }

    public class SkillInput
    {
        [Required]
        public int SkillId { get; set; }

        [Required]
        public SkillLevel Level { get; set; }
    }

    public class LanguageInput
    {
        [Required, StringLength(80)]
        public string LanguageName { get; set; }

        [Required]
        public LanguageProficiency Proficiency { get; set; }
    }

    public class AlertInput
    {
        [Required, StringLength(150)]
        public string Keyword { get; set; }
    }

    public class CandidateProfilePageViewModel
    {
        public CandidateProfile Profile { get; set; }
        public int CompletionPercent { get; set; }
        public IList<SelectListItem> Skills { get; set; }
        public IList<SelectListItem> SkillLevels { get; set; }
        public IList<SelectListItem> Proficiencies { get; set; }
    }

    public class SeekerApplicationItemViewModel
    {
        public int Id { get; set; }
        public int JobPostingId { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public bool CanWithdraw { get; set; }
    }

    public class AlertPageViewModel
    {
        public IList<JobAlert> Alerts { get; set; }
    }

    public class NameCountViewModel
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class EmployerActivityViewModel
    {
        public string EmployerName { get; set; }
        public string CompanyName { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalJobs { get; set; }
        public int Applications { get; set; }
    }

    public class DashboardViewModel
    {
        public int ActiveJobCount { get; set; }
        public int TotalJobCount { get; set; }
        public int ApplicationCount { get; set; }
        public int EmployerCount { get; set; }
        public int SeekerCount { get; set; }
        public IList<NameCountViewModel> ApplicationsByStatus { get; set; }
        public IList<NameCountViewModel> PopularCategories { get; set; }
        public IList<EmployerActivityViewModel> EmployerActivity { get; set; }
    }

    public class CompanyFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Required]
        public string Description { get; set; }

        [StringLength(150)]
        public string Industry { get; set; }

        [StringLength(200)]
        public string Website { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        public string LogoPath { get; set; }
    }

    public class JobFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Job category")]
        public int JobCategoryId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Work type")]
        public WorkType WorkType { get; set; }

        [Required]
        [Display(Name = "Minimum salary")]
        public decimal SalaryMin { get; set; }

        [Required]
        [Display(Name = "Maximum salary")]
        public decimal SalaryMax { get; set; }

        [Required, StringLength(150)]
        public string Location { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Application deadline")]
        public DateTime ApplicationDeadline { get; set; }

        [Display(Name = "Featured on homepage")]
        public bool IsFeatured { get; set; }

        public int[] SelectedSkillIds { get; set; }
        public IList<SelectListItem> Categories { get; set; }
        public IList<SelectListItem> Skills { get; set; }
        public IList<SelectListItem> WorkTypes { get; set; }
    }

    public class EmployerJobItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public string Status { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public int ApplicantCount { get; set; }
        public bool CanClose { get; set; }
    }

    public class ApplicantItemViewModel
    {
        public int ApplicationId { get; set; }
        public string CandidateName { get; set; }
        public string Email { get; set; }
        public string Headline { get; set; }
        public string CoverLetter { get; set; }
        public ApplicationStatus Status { get; set; }
        public string StatusName { get; set; }
        public DateTime AppliedAt { get; set; }
        public int CandidateProfileId { get; set; }
        public bool HasResume { get; set; }
        public string JobTitle { get; set; }
    }

    public class ApplicantPageViewModel
    {
        public int? JobPostingId { get; set; }
        public string JobTitle { get; set; }
        public IList<ApplicantItemViewModel> Applicants { get; set; }
        public IList<SelectListItem> Statuses { get; set; }
    }

    public class CategoryInput
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }
    }

    public class CategoryPageViewModel
    {
        public IList<JobCategory> Categories { get; set; }
        public CategoryInput Input { get; set; }
    }

    public class AdminUserItemViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminUserEditViewModel
    {
        public string Id { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; }

        public string Email { get; set; }
        public string Role { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        public bool IsActive { get; set; }
    }

    public class AdminJobItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string EmployerName { get; set; }
        public string CategoryName { get; set; }
        public string Status { get; set; }
        public int ApplicantCount { get; set; }
        public DateTime ApplicationDeadline { get; set; }
    }
}
