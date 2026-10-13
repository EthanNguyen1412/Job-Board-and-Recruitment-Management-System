using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JobBoardSystem.Infrastructure;
using JobBoardSystem.Models;

namespace JobBoardSystem.Controllers
{
    [AppAuthorize(Roles = RoleNames.Employer)]
    public class EmployerController : AppController
    {
        public ActionResult Dashboard()
        {
            return View(BuildDashboard(CurrentUserId));
        }

        public ActionResult Company()
        {
            var company = Db.CompanyProfiles.FirstOrDefault(item => item.EmployerId == CurrentUserId);
            var model = company == null
                ? new CompanyFormViewModel()
                : new CompanyFormViewModel
                {
                    Id = company.Id,
                    CompanyName = company.CompanyName,
                    Description = company.Description,
                    Industry = company.Industry,
                    Website = company.Website,
                    Address = company.Address,
                    Phone = company.Phone,
                    LogoPath = company.LogoPath
                };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Company(CompanyFormViewModel model, HttpPostedFileBase logoFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (logoFile != null && logoFile.ContentLength > 0)
            {
                var error = UploadRules.Validate(logoFile, "LogoExtensions");
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }

            var company = Db.CompanyProfiles.FirstOrDefault(item => item.EmployerId == CurrentUserId);
            if (company == null)
            {
                company = new CompanyProfile { EmployerId = CurrentUserId };
                Db.CompanyProfiles.Add(company);
            }

            company.CompanyName = model.CompanyName.Trim();
            company.Description = model.Description;
            company.Industry = model.Industry;
            company.Website = model.Website;
            company.Address = model.Address;
            company.Phone = model.Phone;

            if (logoFile != null && logoFile.ContentLength > 0)
            {
                var extension = Path.GetExtension(logoFile.FileName);
                var relativeDirectory = "Uploads/Logos";
                var absoluteDirectory = Server.MapPath("~/" + relativeDirectory);
                Directory.CreateDirectory(absoluteDirectory);
                var storedName = Guid.NewGuid().ToString("N") + extension;
                logoFile.SaveAs(Path.Combine(absoluteDirectory, storedName));
                company.LogoPath = relativeDirectory + "/" + storedName;
            }

            Db.SaveChanges();
            TempData["Message"] = "Company profile saved.";
            return RedirectToAction("Company");
        }

        public ActionResult Jobs()
        {
            var jobs = Db.JobPostings.Include(item => item.JobCategory).Include(item => item.Applications)
                .Where(item => item.EmployerId == CurrentUserId)
                .OrderByDescending(item => item.CreatedAt)
                .ToList()
                .Select(item => new EmployerJobItemViewModel
                {
                    Id = item.Id,
                    Title = item.Title,
                    CategoryName = item.JobCategory.Name,
                    Status = JobRules.EffectiveStatusName(item, DateTime.Today),
                    ApplicationDeadline = item.ApplicationDeadline,
                    ApplicantCount = item.Applications.Count,
                    CanClose = item.Status == JobPostingStatus.Active
                }).ToList();
            return View(jobs);
        }

        public ActionResult CreateJob()
        {
            var model = new JobFormViewModel
            {
                ApplicationDeadline = DateTime.Today.AddDays(30),
                CompanyName = CurrentCompanyName()
            };
            FillJobOptions(model);
            return View("JobForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateJob(JobFormViewModel model)
        {
            ValidateJob(model, true);
            if (!ModelState.IsValid)
            {
                FillJobOptions(model);
                return View("JobForm", model);
            }

            var job = new JobPosting
            {
                EmployerId = CurrentUserId,
                CompanyProfileId = CurrentCompanyId(),
                CreatedAt = DateTime.Now,
                Status = JobPostingStatus.Active
            };
            ApplyJob(job, model);
            Db.JobPostings.Add(job);
            Db.SaveChanges();
            ReplaceSkills(job, model.SelectedSkillIds);
            Db.SaveChanges();
            JobAlertMatcher.Match(Db, job);
            Db.SaveChanges();
            TempData["Message"] = "Job posting created.";
            return RedirectToAction("Jobs");
        }

        public ActionResult EditJob(int id)
        {
            var job = OwnJob(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            var model = new JobFormViewModel
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.CompanyName,
                JobCategoryId = job.JobCategoryId,
                Description = job.Description,
                WorkType = job.WorkType,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                Location = job.Location,
                ApplicationDeadline = job.ApplicationDeadline,
                IsFeatured = job.IsFeatured,
                SelectedSkillIds = job.RequiredSkills.Select(item => item.SkillId).ToArray()
            };
            FillJobOptions(model);
            return View("JobForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditJob(JobFormViewModel model)
        {
            var job = OwnJob(model.Id);
            if (job == null)
            {
                return HttpNotFound();
            }

            ValidateJob(model, job.Status == JobPostingStatus.Active);
            if (!ModelState.IsValid)
            {
                FillJobOptions(model);
                return View("JobForm", model);
            }

            ApplyJob(job, model);
            ReplaceSkills(job, model.SelectedSkillIds);
            Db.SaveChanges();
            JobAlertMatcher.Match(Db, job);
            Db.SaveChanges();
            TempData["Message"] = "Job posting updated.";
            return RedirectToAction("Jobs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CloseJob(int id)
        {
            var job = OwnJob(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            job.Status = JobPostingStatus.Closed;
            job.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            TempData["Message"] = "Job posting closed.";
            return RedirectToAction("Jobs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteJob(int id)
        {
            var job = OwnJob(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            JobRemoval.Remove(Db, job);
            Db.SaveChanges();
            TempData["Message"] = "Job posting deleted.";
            return RedirectToAction("Jobs");
        }

        public ActionResult Applications(int id)
        {
            var job = OwnJob(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            return View(new ApplicantPageViewModel
            {
                JobPostingId = job.Id,
                JobTitle = job.Title,
                Applicants = LoadApplicants(job.Id, null),
                Statuses = EnumExtensions.ToSelectList<ApplicationStatus>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, ApplicationStatus status)
        {
            var application = Db.JobApplications.Include(item => item.JobPosting)
                .FirstOrDefault(item => item.Id == id && item.JobPosting.EmployerId == CurrentUserId);
            if (application == null)
            {
                return HttpNotFound();
            }

            application.Status = status;
            application.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            TempData["Message"] = "Application status updated.";
            return RedirectToAction("Applications", new { id = application.JobPostingId });
        }

        public ActionResult DownloadResume(int id)
        {
            var application = Db.JobApplications.Include(item => item.CandidateProfile).Include(item => item.JobPosting)
                .FirstOrDefault(item => item.Id == id && item.JobPosting.EmployerId == CurrentUserId);
            if (application == null)
            {
                return HttpNotFound();
            }

            var profile = application.CandidateProfile;
            if (profile == null || string.IsNullOrEmpty(profile.ResumeFilePath))
            {
                TempData["Error"] = "This candidate has not uploaded a resume.";
                return RedirectToAction("Applications", new { id = application.JobPostingId });
            }

            var path = Server.MapPath("~/" + profile.ResumeFilePath);
            if (!System.IO.File.Exists(path))
            {
                TempData["Error"] = "The resume file is missing.";
                return RedirectToAction("Applications", new { id = application.JobPostingId });
            }

            return File(path, "application/octet-stream", profile.ResumeFileName ?? "resume");
        }

        private DashboardViewModel BuildDashboard(string employerId)
        {
            var today = DateTime.Today;
            var jobs = Db.JobPostings.Include(item => item.JobCategory).Include(item => item.Applications)
                .Where(item => item.EmployerId == employerId)
                .ToList();
            var applications = jobs.SelectMany(item => item.Applications).ToList();
            return new DashboardViewModel
            {
                ActiveJobCount = jobs.Count(item => item.Status == JobPostingStatus.Active && item.ApplicationDeadline >= today),
                TotalJobCount = jobs.Count,
                ApplicationCount = applications.Count,
                ApplicationsByStatus = Enum.GetValues(typeof(ApplicationStatus)).Cast<ApplicationStatus>()
                    .Select(status => new NameCountViewModel
                    {
                        Name = status.GetDisplayName(),
                        Count = applications.Count(item => item.Status == status)
                    }).ToList(),
                PopularCategories = jobs.GroupBy(item => item.JobCategory.Name)
                    .Select(group => new NameCountViewModel
                    {
                        Name = group.Key,
                        Count = group.Sum(item => item.Applications.Count)
                    })
                    .OrderByDescending(item => item.Count)
                    .ToList()
            };
        }

        private System.Collections.Generic.List<ApplicantItemViewModel> LoadApplicants(int jobId, string employerId)
        {
            var query = Db.JobApplications
                .Include(item => item.JobSeeker)
                .Include(item => item.CandidateProfile)
                .Include(item => item.JobPosting)
                .AsQueryable();
            query = query.Where(item => item.JobPostingId == jobId);
            return query.OrderByDescending(item => item.AppliedAt).ToList().Select(MapApplicant).ToList();
        }

        internal static ApplicantItemViewModel MapApplicant(JobApplication item)
        {
            return new ApplicantItemViewModel
            {
                ApplicationId = item.Id,
                CandidateName = item.JobSeeker.FullName,
                Email = item.JobSeeker.Email,
                Headline = item.CandidateProfile.Headline,
                CoverLetter = item.CoverLetter,
                Status = item.Status,
                StatusName = item.Status.GetDisplayName(),
                AppliedAt = item.AppliedAt,
                CandidateProfileId = item.CandidateProfileId,
                HasResume = !string.IsNullOrEmpty(item.CandidateProfile.ResumeFilePath),
                JobTitle = item.JobPosting.Title
            };
        }

        private JobPosting OwnJob(int id)
        {
            return Db.JobPostings.Include(item => item.RequiredSkills)
                .FirstOrDefault(item => item.Id == id && item.EmployerId == CurrentUserId);
        }

        private void ApplyJob(JobPosting job, JobFormViewModel model)
        {
            job.Title = model.Title.Trim();
            job.CompanyName = model.CompanyName.Trim();
            job.JobCategoryId = model.JobCategoryId;
            job.Description = model.Description;
            job.WorkType = model.WorkType;
            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;
            job.Location = model.Location.Trim();
            job.ApplicationDeadline = model.ApplicationDeadline;
            job.IsFeatured = model.IsFeatured;
            job.CompanyProfileId = CurrentCompanyId();
            job.UpdatedAt = DateTime.Now;
            if (job.Status == JobPostingStatus.Active && job.ApplicationDeadline.Date < DateTime.Today)
            {
                job.Status = JobPostingStatus.Expired;
            }
        }

        private void ReplaceSkills(JobPosting job, int[] skillIds)
        {
            var existing = Db.JobRequiredSkills.Where(item => item.JobPostingId == job.Id).ToList();
            Db.JobRequiredSkills.RemoveRange(existing);
            var ids = (skillIds ?? new int[0]).Distinct();
            foreach (var skillId in ids)
            {
                if (Db.Skills.Any(item => item.Id == skillId))
                {
                    Db.JobRequiredSkills.Add(new JobRequiredSkill { JobPostingId = job.Id, SkillId = skillId });
                }
            }
        }

        private void ValidateJob(JobFormViewModel model, bool requireFutureDeadline)
        {
            if (model.SalaryMax < model.SalaryMin)
            {
                ModelState.AddModelError("SalaryMax", "Maximum salary must be greater than or equal to minimum salary.");
            }

            if (requireFutureDeadline && model.ApplicationDeadline.Date < DateTime.Today)
            {
                ModelState.AddModelError("ApplicationDeadline", "The deadline cannot be in the past for an open posting.");
            }

            if (model.SelectedSkillIds == null || model.SelectedSkillIds.Length == 0)
            {
                ModelState.AddModelError("SelectedSkillIds", "Select at least one required skill.");
            }

            if (!Db.JobCategories.Any(item => item.Id == model.JobCategoryId && item.IsActive))
            {
                ModelState.AddModelError("JobCategoryId", "Select an active job category.");
            }
        }

        private void FillJobOptions(JobFormViewModel model)
        {
            model.Categories = Db.JobCategories.Where(item => item.IsActive).OrderBy(item => item.Name)
                .Select(item => new SelectListItem { Value = item.Id.ToString(), Text = item.Name }).ToList();
            var selectedSkillIds = new System.Collections.Generic.HashSet<int>(model.SelectedSkillIds ?? new int[0]);
            model.Skills = Db.Skills.OrderBy(item => item.Name).ToList()
                .Select(item => new SelectListItem
                {
                    Value = item.Id.ToString(),
                    Text = item.Name,
                    Selected = selectedSkillIds.Contains(item.Id)
                }).ToList();
            model.WorkTypes = EnumExtensions.ToSelectList<WorkType>(model.WorkType);
        }

        private string CurrentCompanyName()
        {
            var company = Db.CompanyProfiles.FirstOrDefault(item => item.EmployerId == CurrentUserId);
            return company == null ? string.Empty : company.CompanyName;
        }

        private int? CurrentCompanyId()
        {
            var company = Db.CompanyProfiles.FirstOrDefault(item => item.EmployerId == CurrentUserId);
            return company == null ? (int?)null : company.Id;
        }
    }
}
