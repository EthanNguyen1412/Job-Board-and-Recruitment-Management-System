using System;
using System.Linq;
using System.Web.Mvc;
using JobBoardSystem.Infrastructure;
using JobBoardSystem.Models;

namespace JobBoardSystem.Controllers
{
    public class HomeController : AppController
    {
        public ActionResult Index()
        {
            var today = DateTime.Today;
            var categories = Db.JobCategories.Where(item => item.IsActive).OrderBy(item => item.Name).ToList();
            var featured = Db.JobPostings
                .Where(item => item.IsFeatured && item.Status == JobPostingStatus.Active && item.ApplicationDeadline >= today)
                .OrderByDescending(item => item.CreatedAt)
                .ToList()
                .Select(item => ToCard(item))
                .ToList();

            return View(new HomeViewModel
            {
                Categories = categories,
                FeaturedJobs = featured,
                ActiveJobCount = Db.JobPostings.Count(item => item.Status == JobPostingStatus.Active && item.ApplicationDeadline >= today),
                CompanyCount = Db.CompanyProfiles.Count(),
                CategoryCount = categories.Count
            });
        }

        private JobListItemViewModel ToCard(JobPosting job)
        {
            var category = Db.JobCategories.Find(job.JobCategoryId);
            return new JobListItemViewModel
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.CompanyName,
                CompanyProfileId = job.CompanyProfileId,
                CategoryName = category == null ? string.Empty : category.Name,
                Location = job.Location,
                WorkType = job.WorkType.GetDisplayName(),
                Salary = JobRules.SalaryText(job.SalaryMin, job.SalaryMax),
                Status = JobRules.EffectiveStatusName(job, DateTime.Today),
                ApplicationDeadline = job.ApplicationDeadline,
                IsFeatured = job.IsFeatured
            };
        }
    }
}
