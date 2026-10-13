using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JobBoardSystem.Models
{
    public class JobCategory
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<JobPosting> JobPostings { get; set; }
    }

    public class CompanyProfile
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string EmployerId { get; set; }

        [Required, StringLength(200)]
        public string CompanyName { get; set; }

        public string Description { get; set; }

        [StringLength(300)]
        public string LogoPath { get; set; }

        [StringLength(150)]
        public string Industry { get; set; }

        [StringLength(200)]
        public string Website { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        public virtual ApplicationUser Employer { get; set; }
        public virtual ICollection<JobPosting> JobPostings { get; set; }
    }

    public class CandidateProfile
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string JobSeekerId { get; set; }

        [StringLength(200)]
        public string Headline { get; set; }

        public string Summary { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(300)]
        public string ResumeFilePath { get; set; }

        [StringLength(200)]
        public string ResumeFileName { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ApplicationUser JobSeeker { get; set; }
        public virtual ICollection<EducationRecord> Educations { get; set; }
        public virtual ICollection<WorkExperience> WorkExperiences { get; set; }
        public virtual ICollection<CandidateSkill> Skills { get; set; }
        public virtual ICollection<CandidateLanguage> Languages { get; set; }
        public virtual ICollection<JobApplication> Applications { get; set; }
    }

    public class EducationRecord
    {
        public int Id { get; set; }

        public int CandidateProfileId { get; set; }

        [Required, StringLength(200)]
        public string School { get; set; }

        [Required, StringLength(150)]
        public string Degree { get; set; }

        [StringLength(150)]
        public string FieldOfStudy { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Description { get; set; }

        public virtual CandidateProfile CandidateProfile { get; set; }
    }

    public class WorkExperience
    {
        public int Id { get; set; }

        public int CandidateProfileId { get; set; }

        [Required, StringLength(200)]
        public string CompanyName { get; set; }

        [Required, StringLength(150)]
        public string JobTitle { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        public string Description { get; set; }

        public virtual CandidateProfile CandidateProfile { get; set; }
    }

    public class Skill
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<CandidateSkill> CandidateSkills { get; set; }
        public virtual ICollection<JobRequiredSkill> JobRequiredSkills { get; set; }
    }

    public class CandidateSkill
    {
        public int Id { get; set; }

        public int CandidateProfileId { get; set; }

        public int SkillId { get; set; }

        public SkillLevel Level { get; set; }

        public virtual CandidateProfile CandidateProfile { get; set; }
        public virtual Skill Skill { get; set; }
    }

    public class CandidateLanguage
    {
        public int Id { get; set; }

        public int CandidateProfileId { get; set; }

        [Required, StringLength(80)]
        public string LanguageName { get; set; }

        public LanguageProficiency Proficiency { get; set; }

        public virtual CandidateProfile CandidateProfile { get; set; }
    }

    public class JobPosting
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string EmployerId { get; set; }

        public int? CompanyProfileId { get; set; }

        public int JobCategoryId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(200)]
        public string CompanyName { get; set; }

        [Required]
        public string Description { get; set; }

        public WorkType WorkType { get; set; }

        public decimal SalaryMin { get; set; }

        public decimal SalaryMax { get; set; }

        [Required, StringLength(150)]
        public string Location { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        public JobPostingStatus Status { get; set; }

        public bool IsFeatured { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ApplicationUser Employer { get; set; }
        public virtual CompanyProfile CompanyProfile { get; set; }
        public virtual JobCategory JobCategory { get; set; }
        public virtual ICollection<JobRequiredSkill> RequiredSkills { get; set; }
        public virtual ICollection<JobApplication> Applications { get; set; }
        public virtual ICollection<SavedJob> SavedBy { get; set; }
        public virtual ICollection<JobAlertMatch> AlertMatches { get; set; }
    }

    public class JobRequiredSkill
    {
        public int Id { get; set; }

        public int JobPostingId { get; set; }

        public int SkillId { get; set; }

        public virtual JobPosting JobPosting { get; set; }
        public virtual Skill Skill { get; set; }
    }

    public class JobApplication
    {
        public int Id { get; set; }

        public int JobPostingId { get; set; }

        public int CandidateProfileId { get; set; }

        [Required, StringLength(128)]
        public string JobSeekerId { get; set; }

        [Required]
        public string CoverLetter { get; set; }

        public ApplicationStatus Status { get; set; }

        public DateTime AppliedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual JobPosting JobPosting { get; set; }
        public virtual CandidateProfile CandidateProfile { get; set; }
        public virtual ApplicationUser JobSeeker { get; set; }
    }

    public class SavedJob
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string JobSeekerId { get; set; }

        public int JobPostingId { get; set; }

        public DateTime SavedAt { get; set; }

        public virtual ApplicationUser JobSeeker { get; set; }
        public virtual JobPosting JobPosting { get; set; }
    }

    public class JobAlert
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string JobSeekerId { get; set; }

        [Required, StringLength(150)]
        public string Keyword { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ApplicationUser JobSeeker { get; set; }
        public virtual ICollection<JobAlertMatch> Matches { get; set; }
    }

    public class JobAlertMatch
    {
        public int Id { get; set; }

        public int JobAlertId { get; set; }

        public int JobPostingId { get; set; }

        public DateTime MatchedAt { get; set; }

        public bool IsRead { get; set; }

        public virtual JobAlert JobAlert { get; set; }
        public virtual JobPosting JobPosting { get; set; }
    }
}
