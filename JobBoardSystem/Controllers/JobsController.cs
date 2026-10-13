using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using JobBoardSystem.Infrastructure;
using JobBoardSystem.Models;
using Microsoft.AspNet.Identity;

namespace JobBoardSystem.Controllers
{
    public class JobsController : AppController
    {
        public ActionResult Index(string keyword, int? categoryId, string location, WorkType? workType, decimal? salaryMin, decimal? salaryMax, JobPostingStatus? status)
        {
            var query = Db.JobPostings.Include(item => item.JobCategory).Include(item => item.RequiredSkills.Select(skill => skill.Skill)).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(item =>
                    item.Title.Contains(keyword) ||
                    item.Description.Contains(keyword) ||
                    item.CompanyName.Contains(keyword) ||
                    item.Location.Contains(keyword) ||
                    item.RequiredSkills.Any(skill => skill.Skill.Name.Contains(keyword)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(item => item.JobCategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(item => item.Location == location);
            }

            if (workType.HasValue)
            {
                query = query.Where(item => item.WorkType == workType.Value);
            }

            if (salaryMin.HasValue)
            {
                query = query.Where(item => item.SalaryMax >= salaryMin.Value);
            }

            if (salaryMax.HasValue)
            {
                query = query.Where(item => item.SalaryMin <= salaryMax.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(item => item.Status == status.Value);
            }

            var savedIds = SavedIds();
            var jobs = query.OrderByDescending(item => item.CreatedAt).ToList()
                .Select(item => ToCard(item, savedIds))
                .ToList();

            return View(new JobSearchViewModel
            {
                Keyword = keyword,
                CategoryId = categoryId,
                Location = location,
                WorkType = workType,
                SalaryMin = salaryMin,
                SalaryMax = salaryMax,
                Status = status,
                Categories = CategoryOptions(categoryId),
                Locations = LocationOptions(location),
                WorkTypes = WithAny(EnumExtensions.ToSelectList<WorkType>(workType)),
                Statuses = WithAny(EnumExtensions.ToSelectList<JobPostingStatus>(status)),
                Jobs = jobs
            });
        }

        public ActionResult Details(int id)
        {
            var job = Db.JobPostings
                .Include(item => item.JobCategory)
                .Include(item => item.CompanyProfile)
                .Include(item => item.RequiredSkills.Select(skill => skill.Skill))
                .FirstOrDefault(item => item.Id == id);
            if (job == null)
            {
                return HttpNotFound();
            }

            var model = new JobDetailsViewModel
            {
                Job = job,
                RequiredSkills = job.RequiredSkills.Select(item => item.Skill.Name).OrderBy(name => name).ToList(),
                StatusName = JobRules.EffectiveStatusName(job, DateTime.Today),
                IsSaved = false,
                HasApplied = false
            };

            if (User.IsInRole(RoleNames.JobSeeker))
            {
                model.IsSaved = Db.SavedJobs.Any(item => item.JobSeekerId == CurrentUserId && item.JobPostingId == job.Id);
                model.HasApplied = Db.JobApplications.Any(item => item.JobSeekerId == CurrentUserId && item.JobPostingId == job.Id);
                var profile = Db.CandidateProfiles.FirstOrDefault(item => item.JobSeekerId == CurrentUserId);
                if (!JobRules.IsOpenForApplication(job, DateTime.Today))
                {
                    model.ApplyBlockReason = "This posting is closed or the application deadline has passed.";
                }
                else if (profile == null)
                {
                    model.ApplyBlockReason = "Create a candidate profile before applying.";
                }
                else if (model.HasApplied)
                {
                    model.ApplyBlockReason = "You have already applied for this posting.";
                }
                else
                {
                    model.CanApply = true;
                }
            }

            return View(model);
        }

        [AppAuthorize(Roles = RoleNames.JobSeeker)]
        public ActionResult Apply(int id)
        {
            var job = Db.JobPostings.Find(id);
            var blocked = BlockReason(job);
            if (blocked != null)
            {
                TempData["Error"] = blocked;
                return RedirectToAction("Details", new { id });
            }

            return View(new ApplyViewModel
            {
                JobPostingId = job.Id,
                JobTitle = job.Title,
                CompanyName = job.CompanyName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = RoleNames.JobSeeker)]
        public ActionResult Apply(int id, ApplyViewModel model)
        {
            model.JobPostingId = id;
            var job = Db.JobPostings.Find(id);
            var blocked = BlockReason(job);
            if (blocked != null)
            {
                TempData["Error"] = blocked;
                return RedirectToAction("Details", new { id = model.JobPostingId });
            }

            if (!ModelState.IsValid)
            {
                model.JobTitle = job.Title;
                model.CompanyName = job.CompanyName;
                return View(model);
            }

            var profile = Db.CandidateProfiles.First(item => item.JobSeekerId == CurrentUserId);
            Db.JobApplications.Add(new JobApplication
            {
                JobPostingId = job.Id,
                CandidateProfileId = profile.Id,
                JobSeekerId = CurrentUserId,
                CoverLetter = model.CoverLetter.Trim(),
                Status = ApplicationStatus.Submitted,
                AppliedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
            Db.SaveChanges();
            TempData["Message"] = "Your application has been submitted.";
            return RedirectToAction("Details", new { id = job.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = RoleNames.JobSeeker)]
        public ActionResult Save(int id)
        {
            var job = Db.JobPostings.Find(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            var exists = Db.SavedJobs.Any(item => item.JobSeekerId == CurrentUserId && item.JobPostingId == id);
            if (!exists)
            {
                Db.SavedJobs.Add(new SavedJob
                {
                    JobSeekerId = CurrentUserId,
                    JobPostingId = id,
                    SavedAt = DateTime.Now
                });
                Db.SaveChanges();
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = RoleNames.JobSeeker)]
        public ActionResult Unsave(int id)
        {
            var saved = Db.SavedJobs.FirstOrDefault(item => item.JobSeekerId == CurrentUserId && item.JobPostingId == id);
            if (saved != null)
            {
                Db.SavedJobs.Remove(saved);
                Db.SaveChanges();
            }

            var returnUrl = Request["returnUrl"];
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", new { id });
        }

        private string BlockReason(JobPosting job)
        {
            if (job == null)
            {
                return "The job posting was not found.";
            }

            if (!JobRules.IsOpenForApplication(job, DateTime.Today))
            {
                return "Applications are not accepted for closed or expired postings.";
            }

            var profile = Db.CandidateProfiles.FirstOrDefault(item => item.JobSeekerId == CurrentUserId);
            if (profile == null)
            {
                return "Create a candidate profile before applying.";
            }

            if (Db.JobApplications.Any(item => item.JobPostingId == job.Id && item.JobSeekerId == CurrentUserId))
            {
                return "You have already applied for this posting.";
            }

            return null;
        }

        private HashSet<int> SavedIds()
        {
            if (!User.IsInRole(RoleNames.JobSeeker))
            {
                return new HashSet<int>();
            }

            return new HashSet<int>(Db.SavedJobs.Where(item => item.JobSeekerId == CurrentUserId).Select(item => item.JobPostingId));
        }

        private IList<SelectListItem> CategoryOptions(int? selected)
        {
            var items = Db.JobCategories.Where(item => item.IsActive).OrderBy(item => item.Name)
                .Select(item => new SelectListItem
                {
                    Value = item.Id.ToString(),
                    Text = item.Name,
                    Selected = selected.HasValue && selected.Value == item.Id
                }).ToList();
            items.Insert(0, new SelectListItem { Text = "All categories", Value = "" });
            return items;
        }

        private IList<SelectListItem> LocationOptions(string selected)
        {
            var items = Db.JobPostings.Select(item => item.Location).Distinct().OrderBy(item => item)
                .ToList()
                .Select(item => new SelectListItem { Value = item, Text = item, Selected = item == selected })
                .ToList();
            items.Insert(0, new SelectListItem { Text = "All locations", Value = "" });
            return items;
        }

        private static IList<SelectListItem> WithAny(IList<SelectListItem> items)
        {
            items.Insert(0, new SelectListItem { Text = "All", Value = "" });
            return items;
        }

        private static JobListItemViewModel ToCard(JobPosting job, HashSet<int> savedIds)
        {
            return new JobListItemViewModel
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.CompanyName,
                CompanyProfileId = job.CompanyProfileId,
                CategoryName = job.JobCategory == null ? string.Empty : job.JobCategory.Name,
                Location = job.Location,
                WorkType = job.WorkType.GetDisplayName(),
                Salary = JobRules.SalaryText(job.SalaryMin, job.SalaryMax),
                Status = JobRules.EffectiveStatusName(job, DateTime.Today),
                ApplicationDeadline = job.ApplicationDeadline,
                IsFeatured = job.IsFeatured,
                IsSaved = savedIds.Contains(job.Id)
            };
        }
    }
}
