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
    [AppAuthorize(Roles = RoleNames.JobSeeker)]
    public class CandidateController : AppController
    {
        public new ActionResult Profile()
        {
            var profile = LoadProfile();
            if (profile == null)
            {
                profile = new CandidateProfile
                {
                    JobSeekerId = CurrentUserId,
                    UpdatedAt = DateTime.Now,
                    Educations = new System.Collections.Generic.List<EducationRecord>(),
                    WorkExperiences = new System.Collections.Generic.List<WorkExperience>(),
                    Skills = new System.Collections.Generic.List<CandidateSkill>(),
                    Languages = new System.Collections.Generic.List<CandidateLanguage>()
                };
                Db.CandidateProfiles.Add(profile);
                Db.SaveChanges();
                profile = LoadProfile();
            }

            return View(BuildPage(profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveProfile(ProfilePersonalInput input)
        {
            var profile = RequireProfile();
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Check the personal information and try again.";
                return RedirectToAction("Profile");
            }

            profile.Headline = input.Headline;
            profile.Summary = input.Summary;
            profile.Phone = input.Phone;
            profile.Address = input.Address;
            profile.DateOfBirth = input.DateOfBirth;
            profile.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            TempData["Message"] = "Personal information has been saved.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEducation(EducationInput input)
        {
            var profile = RequireProfile();
            if (!ModelState.IsValid || (input.EndDate.HasValue && input.EndDate < input.StartDate))
            {
                TempData["Error"] = "Education dates are invalid.";
                return RedirectToAction("Profile");
            }

            Db.EducationRecords.Add(new EducationRecord
            {
                CandidateProfileId = profile.Id,
                School = input.School.Trim(),
                Degree = input.Degree.Trim(),
                FieldOfStudy = input.FieldOfStudy,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Description = input.Description
            });
            profile.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEducation(int id)
        {
            var profile = RequireProfile();
            var item = Db.EducationRecords.FirstOrDefault(record => record.Id == id && record.CandidateProfileId == profile.Id);
            if (item != null)
            {
                Db.EducationRecords.Remove(item);
                Db.SaveChanges();
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddExperience(ExperienceInput input)
        {
            var profile = RequireProfile();
            if (!ModelState.IsValid || (!input.IsCurrent && input.EndDate.HasValue && input.EndDate < input.StartDate))
            {
                TempData["Error"] = "Work experience dates are invalid.";
                return RedirectToAction("Profile");
            }

            Db.WorkExperiences.Add(new WorkExperience
            {
                CandidateProfileId = profile.Id,
                CompanyName = input.CompanyName.Trim(),
                JobTitle = input.JobTitle.Trim(),
                StartDate = input.StartDate,
                EndDate = input.IsCurrent ? null : input.EndDate,
                IsCurrent = input.IsCurrent,
                Description = input.Description
            });
            profile.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteExperience(int id)
        {
            var profile = RequireProfile();
            var item = Db.WorkExperiences.FirstOrDefault(record => record.Id == id && record.CandidateProfileId == profile.Id);
            if (item != null)
            {
                Db.WorkExperiences.Remove(item);
                Db.SaveChanges();
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSkill(SkillInput input)
        {
            var profile = RequireProfile();
            if (!ModelState.IsValid || !Db.Skills.Any(item => item.Id == input.SkillId))
            {
                TempData["Error"] = "Select a skill from the list.";
                return RedirectToAction("Profile");
            }

            if (Db.CandidateSkills.Any(item => item.CandidateProfileId == profile.Id && item.SkillId == input.SkillId))
            {
                TempData["Error"] = "That skill is already on your profile.";
                return RedirectToAction("Profile");
            }

            Db.CandidateSkills.Add(new CandidateSkill
            {
                CandidateProfileId = profile.Id,
                SkillId = input.SkillId,
                Level = input.Level
            });
            profile.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSkill(int id)
        {
            var profile = RequireProfile();
            var item = Db.CandidateSkills.FirstOrDefault(record => record.Id == id && record.CandidateProfileId == profile.Id);
            if (item != null)
            {
                Db.CandidateSkills.Remove(item);
                Db.SaveChanges();
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLanguage(LanguageInput input)
        {
            var profile = RequireProfile();
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Enter a language name.";
                return RedirectToAction("Profile");
            }

            Db.CandidateLanguages.Add(new CandidateLanguage
            {
                CandidateProfileId = profile.Id,
                LanguageName = input.LanguageName.Trim(),
                Proficiency = input.Proficiency
            });
            profile.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteLanguage(int id)
        {
            var profile = RequireProfile();
            var item = Db.CandidateLanguages.FirstOrDefault(record => record.Id == id && record.CandidateProfileId == profile.Id);
            if (item != null)
            {
                Db.CandidateLanguages.Remove(item);
                Db.SaveChanges();
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadResume(HttpPostedFileBase resumeFile)
        {
            var profile = RequireProfile();
            var error = UploadRules.Validate(resumeFile, "ResumeExtensions");
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Profile");
            }

            var extension = Path.GetExtension(resumeFile.FileName);
            var relativeDirectory = "App_Data/Resumes/" + CurrentUserId;
            var absoluteDirectory = Server.MapPath("~/" + relativeDirectory);
            Directory.CreateDirectory(absoluteDirectory);
            var storedName = Guid.NewGuid().ToString("N") + extension;
            resumeFile.SaveAs(Path.Combine(absoluteDirectory, storedName));
            profile.ResumeFileName = Path.GetFileName(resumeFile.FileName);
            profile.ResumeFilePath = relativeDirectory + "/" + storedName;
            profile.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            TempData["Message"] = "Resume uploaded.";
            return RedirectToAction("Profile");
        }

        public ActionResult DownloadResume()
        {
            var profile = RequireProfile();
            return ResumeFile(profile);
        }

        public ActionResult ExportPdf()
        {
            var profile = LoadProfile();
            if (profile == null)
            {
                return RedirectToAction("Profile");
            }

            var user = Db.Users.Find(CurrentUserId);
            var bytes = PdfResumeBuilder.Build(profile, user.FullName, user.Email);
            var fileName = "resume-" + user.FullName.Replace(" ", "-") + ".pdf";
            return File(bytes, "application/pdf", fileName);
        }

        public ActionResult Applications()
        {
            var items = Db.JobApplications
                .Include(item => item.JobPosting)
                .Where(item => item.JobSeekerId == CurrentUserId)
                .OrderByDescending(item => item.AppliedAt)
                .ToList()
                .Select(item => new SeekerApplicationItemViewModel
                {
                    Id = item.Id,
                    JobPostingId = item.JobPostingId,
                    JobTitle = item.JobPosting.Title,
                    CompanyName = item.JobPosting.CompanyName,
                    Status = item.Status.GetDisplayName(),
                    AppliedAt = item.AppliedAt,
                    CanWithdraw = item.Status != ApplicationStatus.Withdrawn && item.Status != ApplicationStatus.Rejected && item.Status != ApplicationStatus.Offered
                }).ToList();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Withdraw(int id)
        {
            var application = Db.JobApplications.FirstOrDefault(item => item.Id == id && item.JobSeekerId == CurrentUserId);
            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Status == ApplicationStatus.Withdrawn || application.Status == ApplicationStatus.Rejected || application.Status == ApplicationStatus.Offered)
            {
                TempData["Error"] = "This application can no longer be withdrawn.";
                return RedirectToAction("Applications");
            }

            application.Status = ApplicationStatus.Withdrawn;
            application.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            TempData["Message"] = "The application has been withdrawn.";
            return RedirectToAction("Applications");
        }

        public ActionResult SavedJobs()
        {
            var jobs = Db.SavedJobs
                .Include(item => item.JobPosting.JobCategory)
                .Where(item => item.JobSeekerId == CurrentUserId)
                .OrderByDescending(item => item.SavedAt)
                .ToList()
                .Select(item => new JobListItemViewModel
                {
                    Id = item.JobPosting.Id,
                    Title = item.JobPosting.Title,
                    CompanyName = item.JobPosting.CompanyName,
                    CompanyProfileId = item.JobPosting.CompanyProfileId,
                    CategoryName = item.JobPosting.JobCategory.Name,
                    Location = item.JobPosting.Location,
                    WorkType = item.JobPosting.WorkType.GetDisplayName(),
                    Salary = JobRules.SalaryText(item.JobPosting.SalaryMin, item.JobPosting.SalaryMax),
                    Status = JobRules.EffectiveStatusName(item.JobPosting, DateTime.Today),
                    ApplicationDeadline = item.JobPosting.ApplicationDeadline,
                    IsSaved = true
                }).ToList();
            return View(jobs);
        }

        public ActionResult Alerts()
        {
            var alerts = Db.JobAlerts
                .Include(item => item.Matches.Select(match => match.JobPosting))
                .Where(item => item.JobSeekerId == CurrentUserId)
                .OrderByDescending(item => item.CreatedAt)
                .ToList();
            return View(new AlertPageViewModel { Alerts = alerts });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAlert(AlertInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Enter a keyword for the alert.";
                return RedirectToAction("Alerts");
            }

            var alert = new JobAlert
            {
                JobSeekerId = CurrentUserId,
                Keyword = input.Keyword.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            Db.JobAlerts.Add(alert);
            Db.SaveChanges();

            var jobs = Db.JobPostings.Where(item => item.Status == JobPostingStatus.Active).ToList();
            foreach (var job in jobs)
            {
                JobAlertMatcher.Match(Db, job);
            }

            Db.SaveChanges();
            TempData["Message"] = "Job alert saved. Matching open postings are listed below.";
            return RedirectToAction("Alerts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleAlert(int id)
        {
            var alert = Db.JobAlerts.FirstOrDefault(item => item.Id == id && item.JobSeekerId == CurrentUserId);
            if (alert != null)
            {
                alert.IsActive = !alert.IsActive;
                Db.SaveChanges();
            }

            return RedirectToAction("Alerts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAlert(int id)
        {
            var alert = Db.JobAlerts.FirstOrDefault(item => item.Id == id && item.JobSeekerId == CurrentUserId);
            if (alert != null)
            {
                var matches = Db.JobAlertMatches.Where(item => item.JobAlertId == alert.Id);
                Db.JobAlertMatches.RemoveRange(matches);
                Db.JobAlerts.Remove(alert);
                Db.SaveChanges();
            }

            return RedirectToAction("Alerts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAlertRead(int id)
        {
            var match = Db.JobAlertMatches.Include(item => item.JobAlert)
                .FirstOrDefault(item => item.Id == id && item.JobAlert.JobSeekerId == CurrentUserId);
            if (match != null)
            {
                match.IsRead = true;
                Db.SaveChanges();
            }

            return RedirectToAction("Alerts");
        }

        private CandidateProfile RequireProfile()
        {
            return Db.CandidateProfiles.First(item => item.JobSeekerId == CurrentUserId);
        }

        private CandidateProfile LoadProfile()
        {
            return Db.CandidateProfiles
                .Include(item => item.Educations)
                .Include(item => item.WorkExperiences)
                .Include(item => item.Skills.Select(skill => skill.Skill))
                .Include(item => item.Languages)
                .FirstOrDefault(item => item.JobSeekerId == CurrentUserId);
        }

        private CandidateProfilePageViewModel BuildPage(CandidateProfile profile)
        {
            return new CandidateProfilePageViewModel
            {
                Profile = profile,
                CompletionPercent = ProfileCompletion.Percent(profile),
                Skills = Db.Skills.OrderBy(item => item.Name).Select(item => new SelectListItem
                {
                    Value = item.Id.ToString(),
                    Text = item.Name
                }).ToList(),
                SkillLevels = EnumExtensions.ToSelectList<SkillLevel>(),
                Proficiencies = EnumExtensions.ToSelectList<LanguageProficiency>()
            };
        }

        private ActionResult ResumeFile(CandidateProfile profile)
        {
            if (profile == null || string.IsNullOrEmpty(profile.ResumeFilePath))
            {
                TempData["Error"] = "No resume has been uploaded.";
                return RedirectToAction("Profile");
            }

            var path = Server.MapPath("~/" + profile.ResumeFilePath);
            if (!System.IO.File.Exists(path))
            {
                TempData["Error"] = "The resume file is missing.";
                return RedirectToAction("Profile");
            }

            return File(path, Mime(profile.ResumeFileName), profile.ResumeFileName);
        }

        private static string Mime(string fileName)
        {
            var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
            if (extension == ".pdf") return "application/pdf";
            if (extension == ".doc") return "application/msword";
            if (extension == ".docx") return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            return "application/octet-stream";
        }
    }
}
