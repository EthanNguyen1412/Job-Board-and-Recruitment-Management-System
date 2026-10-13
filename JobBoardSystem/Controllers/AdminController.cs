using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using JobBoardSystem.Infrastructure;
using JobBoardSystem.Models;
using Microsoft.AspNet.Identity;

namespace JobBoardSystem.Controllers
{
    [AppAuthorize(Roles = RoleNames.Administrator)]
    public class AdminController : AppController
    {
        public ActionResult Dashboard()
        {
            var today = DateTime.Today;
            var jobs = Db.JobPostings.Include(item => item.JobCategory).Include(item => item.Applications).Include(item => item.Employer).ToList();
            var applications = Db.JobApplications.ToList();
            var employerRole = Db.Roles.First(item => item.Name == RoleNames.Employer);
            var seekerRole = Db.Roles.First(item => item.Name == RoleNames.JobSeeker);
            var employers = Db.Users.Where(item => item.Roles.Any(role => role.RoleId == employerRole.Id)).ToList();
            var companies = Db.CompanyProfiles.ToList();

            return View(new DashboardViewModel
            {
                ActiveJobCount = jobs.Count(item => item.Status == JobPostingStatus.Active && item.ApplicationDeadline >= today),
                TotalJobCount = jobs.Count,
                ApplicationCount = applications.Count,
                EmployerCount = employers.Count,
                SeekerCount = Db.Users.Count(item => item.Roles.Any(role => role.RoleId == seekerRole.Id)),
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
                    .ThenBy(item => item.Name)
                    .ToList(),
                EmployerActivity = employers.Select(employer =>
                {
                    var employerJobs = jobs.Where(item => item.EmployerId == employer.Id).ToList();
                    var company = companies.FirstOrDefault(item => item.EmployerId == employer.Id);
                    return new EmployerActivityViewModel
                    {
                        EmployerName = employer.FullName,
                        CompanyName = company == null ? "-" : company.CompanyName,
                        ActiveJobs = employerJobs.Count(item => item.Status == JobPostingStatus.Active && item.ApplicationDeadline >= today),
                        TotalJobs = employerJobs.Count,
                        Applications = employerJobs.Sum(item => item.Applications.Count)
                    };
                }).OrderByDescending(item => item.Applications).ToList()
            });
        }

        public ActionResult Users()
        {
            var users = Db.Users.OrderBy(item => item.FullName).ToList();
            var model = users.Select(user => new AdminUserItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = RoleNames.Display(UserManager.GetRoles(user.Id).FirstOrDefault()),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }).ToList();
            return View(model);
        }

        public ActionResult EditUser(string id)
        {
            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(new AdminUserEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Role = RoleNames.Display(UserManager.GetRoles(user.Id).FirstOrDefault()),
                IsActive = user.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(AdminUserEditViewModel model)
        {
            var user = Db.Users.Find(model.Id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Email = user.Email;
                model.Role = RoleNames.Display(UserManager.GetRoles(user.Id).FirstOrDefault());
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.Phone;
            user.IsActive = model.IsActive;
            Db.SaveChanges();
            TempData["Message"] = "User account updated.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleUser(string id)
        {
            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (user.Id == CurrentUserId)
            {
                TempData["Error"] = "You cannot disable your own account.";
                return RedirectToAction("Users");
            }

            user.IsActive = !user.IsActive;
            Db.SaveChanges();
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(string id)
        {
            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (user.Id == CurrentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            var alerts = Db.JobAlerts.Where(item => item.JobSeekerId == user.Id).ToList();
            foreach (var alert in alerts)
            {
                Db.JobAlertMatches.RemoveRange(Db.JobAlertMatches.Where(item => item.JobAlertId == alert.Id));
            }
            Db.JobAlerts.RemoveRange(alerts);
            Db.SavedJobs.RemoveRange(Db.SavedJobs.Where(item => item.JobSeekerId == user.Id));
            Db.JobApplications.RemoveRange(Db.JobApplications.Where(item => item.JobSeekerId == user.Id));

            var profile = Db.CandidateProfiles.FirstOrDefault(item => item.JobSeekerId == user.Id);
            if (profile != null)
            {
                Db.CandidateProfiles.Remove(profile);
            }

            var jobs = Db.JobPostings.Where(item => item.EmployerId == user.Id).ToList();
            foreach (var job in jobs)
            {
                JobRemoval.Remove(Db, job);
            }

            var company = Db.CompanyProfiles.FirstOrDefault(item => item.EmployerId == user.Id);
            if (company != null)
            {
                Db.CompanyProfiles.Remove(company);
            }

            Db.SaveChanges();
            UserManager.Delete(user);
            TempData["Message"] = "User account deleted.";
            return RedirectToAction("Users");
        }

        public ActionResult Categories()
        {
            return View(new CategoryPageViewModel
            {
                Categories = Db.JobCategories.OrderBy(item => item.Name).ToList(),
                Input = new CategoryInput { IsActive = true }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCategory(CategoryInput input)
        {
            if (!ModelState.IsValid)
            {
                return View("Categories", new CategoryPageViewModel
                {
                    Categories = Db.JobCategories.OrderBy(item => item.Name).ToList(),
                    Input = input
                });
            }

            var duplicate = Db.JobCategories.Any(item => item.Name == input.Name.Trim() && item.Id != input.Id);
            if (duplicate)
            {
                TempData["Error"] = "A category with that name already exists.";
                return RedirectToAction("Categories");
            }

            if (input.Id == 0)
            {
                Db.JobCategories.Add(new JobCategory
                {
                    Name = input.Name.Trim(),
                    Description = input.Description,
                    IsActive = input.IsActive
                });
            }
            else
            {
                var category = Db.JobCategories.Find(input.Id);
                if (category == null)
                {
                    return HttpNotFound();
                }

                category.Name = input.Name.Trim();
                category.Description = input.Description;
                category.IsActive = input.IsActive;
            }

            Db.SaveChanges();
            TempData["Message"] = "Job category saved.";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategory(int id)
        {
            var category = Db.JobCategories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            if (Db.JobPostings.Any(item => item.JobCategoryId == id))
            {
                TempData["Error"] = "This category is used by job postings. Deactivate it instead of deleting it.";
                return RedirectToAction("Categories");
            }

            Db.JobCategories.Remove(category);
            Db.SaveChanges();
            TempData["Message"] = "Job category deleted.";
            return RedirectToAction("Categories");
        }

        public ActionResult EditCategory(int id)
        {
            var category = Db.JobCategories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View("Categories", new CategoryPageViewModel
            {
                Categories = Db.JobCategories.OrderBy(item => item.Name).ToList(),
                Input = new CategoryInput
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive
                }
            });
        }

        public ActionResult Jobs()
        {
            var jobs = Db.JobPostings.Include(item => item.JobCategory).Include(item => item.Employer).Include(item => item.Applications)
                .OrderByDescending(item => item.CreatedAt)
                .ToList()
                .Select(item => new AdminJobItemViewModel
                {
                    Id = item.Id,
                    Title = item.Title,
                    CompanyName = item.CompanyName,
                    EmployerName = item.Employer.FullName,
                    CategoryName = item.JobCategory.Name,
                    Status = JobRules.EffectiveStatusName(item, DateTime.Today),
                    ApplicantCount = item.Applications.Count,
                    ApplicationDeadline = item.ApplicationDeadline
                }).ToList();
            return View(jobs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CloseJob(int id)
        {
            var job = Db.JobPostings.Find(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            job.Status = JobPostingStatus.Closed;
            job.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            return RedirectToAction("Jobs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteJob(int id)
        {
            var job = Db.JobPostings.Find(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            JobRemoval.Remove(Db, job);
            Db.SaveChanges();
            TempData["Message"] = "Job posting deleted.";
            return RedirectToAction("Jobs");
        }

        public ActionResult Applications(ApplicationStatus? status)
        {
            var query = Db.JobApplications
                .Include(item => item.JobSeeker)
                .Include(item => item.CandidateProfile)
                .Include(item => item.JobPosting)
                .AsQueryable();
            if (status.HasValue)
            {
                query = query.Where(item => item.Status == status.Value);
            }

            ViewBag.Status = status;
            ViewBag.Statuses = EnumExtensions.ToSelectList<ApplicationStatus>(status);
            return View(query.OrderByDescending(item => item.AppliedAt).ToList().Select(EmployerController.MapApplicant).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, ApplicationStatus status)
        {
            var application = Db.JobApplications.Find(id);
            if (application == null)
            {
                return HttpNotFound();
            }

            application.Status = status;
            application.UpdatedAt = DateTime.Now;
            Db.SaveChanges();
            TempData["Message"] = "Application status updated.";
            return RedirectToAction("Applications");
        }
    }
}
