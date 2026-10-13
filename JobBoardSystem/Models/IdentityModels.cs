using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using JobBoardSystem.Models;

namespace JobBoardSystem.Models
{
    // You can add User data for the user by adding more properties to your User class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(150)]
        public string FullName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<JobPosting> JobPostings { get; set; }

        public ClaimsIdentity GenerateUserIdentity(ApplicationUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = manager.CreateIdentity(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            return Task.FromResult(GenerateUserIdentity(manager));
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<JobCategory> JobCategories { get; set; }
        public DbSet<CompanyProfile> CompanyProfiles { get; set; }
        public DbSet<CandidateProfile> CandidateProfiles { get; set; }
        public DbSet<EducationRecord> EducationRecords { get; set; }
        public DbSet<WorkExperience> WorkExperiences { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<CandidateSkill> CandidateSkills { get; set; }
        public DbSet<CandidateLanguage> CandidateLanguages { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<JobRequiredSkill> JobRequiredSkills { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }
        public DbSet<JobAlert> JobAlerts { get; set; }
        public DbSet<JobAlertMatch> JobAlertMatches { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CompanyProfile>()
                .HasRequired(item => item.Employer)
                .WithMany()
                .HasForeignKey(item => item.EmployerId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<CompanyProfile>()
                .HasIndex(item => item.EmployerId)
                .IsUnique();

            modelBuilder.Entity<CandidateProfile>()
                .HasRequired(item => item.JobSeeker)
                .WithMany()
                .HasForeignKey(item => item.JobSeekerId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<CandidateProfile>()
                .HasIndex(item => item.JobSeekerId)
                .IsUnique();

            modelBuilder.Entity<EducationRecord>()
                .HasRequired(item => item.CandidateProfile)
                .WithMany(profile => profile.Educations)
                .HasForeignKey(item => item.CandidateProfileId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<WorkExperience>()
                .HasRequired(item => item.CandidateProfile)
                .WithMany(profile => profile.WorkExperiences)
                .HasForeignKey(item => item.CandidateProfileId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CandidateLanguage>()
                .HasRequired(item => item.CandidateProfile)
                .WithMany(profile => profile.Languages)
                .HasForeignKey(item => item.CandidateProfileId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CandidateSkill>()
                .HasRequired(item => item.CandidateProfile)
                .WithMany(profile => profile.Skills)
                .HasForeignKey(item => item.CandidateProfileId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<CandidateSkill>()
                .HasRequired(item => item.Skill)
                .WithMany(skill => skill.CandidateSkills)
                .HasForeignKey(item => item.SkillId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<CandidateSkill>()
                .HasIndex(item => new { item.CandidateProfileId, item.SkillId })
                .IsUnique();

            modelBuilder.Entity<JobPosting>()
                .HasRequired(item => item.Employer)
                .WithMany(user => user.JobPostings)
                .HasForeignKey(item => item.EmployerId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<JobPosting>()
                .HasRequired(item => item.JobCategory)
                .WithMany(category => category.JobPostings)
                .HasForeignKey(item => item.JobCategoryId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<JobPosting>()
                .HasOptional(item => item.CompanyProfile)
                .WithMany(company => company.JobPostings)
                .HasForeignKey(item => item.CompanyProfileId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<JobRequiredSkill>()
                .HasRequired(item => item.JobPosting)
                .WithMany(job => job.RequiredSkills)
                .HasForeignKey(item => item.JobPostingId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<JobRequiredSkill>()
                .HasRequired(item => item.Skill)
                .WithMany(skill => skill.JobRequiredSkills)
                .HasForeignKey(item => item.SkillId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<JobRequiredSkill>()
                .HasIndex(item => new { item.JobPostingId, item.SkillId })
                .IsUnique();

            modelBuilder.Entity<JobApplication>()
                .HasRequired(item => item.JobPosting)
                .WithMany(job => job.Applications)
                .HasForeignKey(item => item.JobPostingId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<JobApplication>()
                .HasRequired(item => item.CandidateProfile)
                .WithMany(profile => profile.Applications)
                .HasForeignKey(item => item.CandidateProfileId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<JobApplication>()
                .HasRequired(item => item.JobSeeker)
                .WithMany()
                .HasForeignKey(item => item.JobSeekerId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<JobApplication>()
                .HasIndex(item => new { item.JobPostingId, item.JobSeekerId })
                .IsUnique();

            modelBuilder.Entity<SavedJob>()
                .HasRequired(item => item.JobSeeker)
                .WithMany()
                .HasForeignKey(item => item.JobSeekerId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<SavedJob>()
                .HasRequired(item => item.JobPosting)
                .WithMany(job => job.SavedBy)
                .HasForeignKey(item => item.JobPostingId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<SavedJob>()
                .HasIndex(item => new { item.JobSeekerId, item.JobPostingId })
                .IsUnique();

            modelBuilder.Entity<JobAlert>()
                .HasRequired(item => item.JobSeeker)
                .WithMany()
                .HasForeignKey(item => item.JobSeekerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<JobAlertMatch>()
                .HasRequired(item => item.JobAlert)
                .WithMany(alert => alert.Matches)
                .HasForeignKey(item => item.JobAlertId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<JobAlertMatch>()
                .HasRequired(item => item.JobPosting)
                .WithMany(job => job.AlertMatches)
                .HasForeignKey(item => item.JobPostingId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<JobAlertMatch>()
                .HasIndex(item => new { item.JobAlertId, item.JobPostingId })
                .IsUnique();
        }
    }
}

#region Helpers
namespace JobBoardSystem
{
    public static class IdentityHelper
    {
        // Used for XSRF when linking external logins
        public const string XsrfKey = "XsrfId";

        public const string ProviderNameKey = "providerName";
        public static string GetProviderNameFromRequest(HttpRequest request)
        {
            return request.QueryString[ProviderNameKey];
        }

        public const string CodeKey = "code";
        public static string GetCodeFromRequest(HttpRequest request)
        {
            return request.QueryString[CodeKey];
        }

        public const string UserIdKey = "userId";
        public static string GetUserIdFromRequest(HttpRequest request)
        {
            return HttpUtility.UrlDecode(request.QueryString[UserIdKey]);
        }

        public static string GetResetPasswordRedirectUrl(string code, HttpRequest request)
        {
            var absoluteUri = "/Account/ResetPassword?" + CodeKey + "=" + HttpUtility.UrlEncode(code);
            return new Uri(request.Url, absoluteUri).AbsoluteUri.ToString();
        }

        public static string GetUserConfirmationRedirectUrl(string code, string userId, HttpRequest request)
        {
            var absoluteUri = "/Account/Confirm?" + CodeKey + "=" + HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + HttpUtility.UrlEncode(userId);
            return new Uri(request.Url, absoluteUri).AbsoluteUri.ToString();
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

        public static void RedirectToReturnUrl(string returnUrl, HttpResponse response)
        {
            if (!String.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
            {
                response.Redirect(returnUrl);
            }
            else
            {
                response.Redirect("~/");
            }
        }
    }
}
#endregion
