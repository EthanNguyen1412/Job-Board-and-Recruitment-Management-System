using System;
using System.Linq;
using System.Web.Mvc;
using JobBoardSystem.Infrastructure;
using JobBoardSystem.Models;

namespace JobBoardSystem.Controllers
{
    public class CompaniesController : AppController
    {
        public ActionResult Index()
        {
            var today = DateTime.Today;
            var companies = Db.CompanyProfiles.OrderBy(item => item.CompanyName).ToList();
            var model = companies.Select(company => new CompanyListItemViewModel
            {
                Id = company.Id,
                CompanyName = company.CompanyName,
                Industry = company.Industry,
                LogoPath = company.LogoPath,
                ActiveJobCount = Db.JobPostings.Count(job =>
                    job.CompanyProfileId == company.Id &&
                    job.Status == JobPostingStatus.Active &&
                    job.ApplicationDeadline >= today)
            }).ToList();
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var company = Db.CompanyProfiles.FirstOrDefault(item => item.Id == id);
            if (company == null)
            {
                return HttpNotFound();
            }

            var today = DateTime.Today;
            var jobs = Db.JobPostings
                .Include("JobCategory")
                .Where(job => job.CompanyProfileId == company.Id && job.Status == JobPostingStatus.Active && job.ApplicationDeadline >= today)
                .OrderByDescending(job => job.CreatedAt)
                .ToList()
                .Select(job => new JobListItemViewModel
                {
                    Id = job.Id,
                    Title = job.Title,
                    CompanyName = job.CompanyName,
                    CompanyProfileId = job.CompanyProfileId,
                    CategoryName = job.JobCategory == null ? string.Empty : job.JobCategory.Name,
                    Location = job.Location,
                    WorkType = job.WorkType.GetDisplayName(),
                    Salary = JobRules.SalaryText(job.SalaryMin, job.SalaryMax),
                    Status = JobRules.EffectiveStatusName(job, today),
                    ApplicationDeadline = job.ApplicationDeadline
                }).ToList();

            return View(new CompanyDetailsViewModel
            {
                Company = company,
                ActiveJobs = jobs
            });
        }
    }
}
