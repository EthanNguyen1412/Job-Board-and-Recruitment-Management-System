using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using JobBoardSystem.Infrastructure;
using JobBoardSystem.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace JobBoardSystem.Data
{
    public class JobBoardDbInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        private const string DemoPassword = "Pass@123";

        protected override void Seed(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            foreach (var role in RoleNames.All)
            {
                if (!roleManager.RoleExists(role))
                {
                    roleManager.Create(new IdentityRole(role));
                }
            }

            var admin = CreateUser(userManager, "admin@jobboard.local", "System Administrator", RoleNames.Administrator);
            var employers = new[]
            {
                CreateUser(userManager, "employer1@jobboard.local", "Tran Minh Khoa", RoleNames.Employer),
                CreateUser(userManager, "employer2@jobboard.local", "Le Thu Ha", RoleNames.Employer),
                CreateUser(userManager, "employer3@jobboard.local", "Pham Quoc Bao", RoleNames.Employer),
                CreateUser(userManager, "employer4@jobboard.local", "Nguyen Hoang Lan", RoleNames.Employer),
                CreateUser(userManager, "employer5@jobboard.local", "Do Thanh Tam", RoleNames.Employer)
            };

            var seekerNames = new[]
            {
                "Nguyen Van An", "Tran Thi Bich", "Le Hoang Nam", "Pham Thu Trang", "Hoang Minh Duc",
                "Vo Ngoc Ha", "Dang Quoc Huy", "Bui Thanh Mai", "Ngo Gia Bao", "Ly Phuong Anh",
                "Truong Cam Tu", "Mai Duc Long", "Phan Kim Chi", "Cao Tuan Kiet", "Dinh Hong Nhung"
            };
            var seekers = new List<ApplicationUser>();
            for (var i = 0; i < seekerNames.Length; i++)
            {
                seekers.Add(CreateUser(userManager, "seeker" + (i + 1).ToString("00") + "@jobboard.local", seekerNames[i], RoleNames.JobSeeker));
            }

            var categories = new[]
            {
                new JobCategory { Name = "Information Technology", Description = "Software, infrastructure, and data roles.", IsActive = true },
                new JobCategory { Name = "Marketing", Description = "Brand, content, and growth roles.", IsActive = true },
                new JobCategory { Name = "Finance and Accounting", Description = "Accounting, analysis, and control roles.", IsActive = true },
                new JobCategory { Name = "Human Resources", Description = "Recruiting and people operations.", IsActive = true },
                new JobCategory { Name = "Design", Description = "Product, visual, and interaction design.", IsActive = true },
                new JobCategory { Name = "Sales", Description = "Business development and account roles.", IsActive = true }
            };
            context.JobCategories.AddRange(categories);

            var skillNames = new[]
            {
                "C#", "ASP.NET", "SQL", "JavaScript", "Communication", "Excel", "Photoshop",
                "SEO", "Recruitment", "Accounting", "Python", "UI Design", "Negotiation", "Linux", "Content Writing"
            };
            var skills = skillNames.Select(name => new Skill { Name = name }).ToList();
            context.Skills.AddRange(skills);
            context.SaveChanges();

            var categoryByName = context.JobCategories.ToDictionary(item => item.Name);
            var skillByName = context.Skills.ToDictionary(item => item.Name);
            var today = DateTime.Today;

            var companies = new[]
            {
                Company(employers[0], "FPT Software", "Information Technology", "https://www.fpt-software.com", "Hanoi", "02473003333", "FPT Software builds digital platforms and enterprise products for clients in Vietnam and overseas."),
                Company(employers[1], "Vingroup", "Conglomerate", "https://www.vingroup.net", "Hanoi", "02439749999", "Vingroup operates in technology, industry, and consumer services across Vietnam."),
                Company(employers[2], "Viettel Group", "Telecommunications", "https://viettel.com.vn", "Hanoi", "02462668888", "Viettel provides telecommunications, digital solutions, and technology services."),
                Company(employers[3], "Tiki Corporation", "E-commerce", "https://tiki.vn", "Ho Chi Minh City", "19006037", "Tiki runs an online retail platform connecting shoppers and sellers."),
                Company(employers[4], "Grab Vietnam", "Transportation", "https://www.grab.com", "Ho Chi Minh City", "02873098888", "Grab Vietnam provides ride-hailing and delivery services for cities nationwide.")
            };
            context.CompanyProfiles.AddRange(companies);
            context.SaveChanges();

            var jobs = new List<JobPosting>
            {
                Job(employers[0], companies[0], categoryByName["Information Technology"], "Senior C# Developer", WorkType.FullTime, 30000000, 45000000, "Hanoi", today.AddDays(30), JobPostingStatus.Active, true, "Build and review ASP.NET services. The team needs a Developer who can lead delivery."),
                Job(employers[0], companies[0], categoryByName["Information Technology"], "Junior ASP.NET Developer", WorkType.Internship, 8000000, 12000000, "Ho Chi Minh City", today.AddDays(20), JobPostingStatus.Active, false, "An internship for a junior Developer learning ASP.NET, SQL, and code review."),
                Job(employers[0], companies[0], categoryByName["Information Technology"], "QA Engineer", WorkType.FullTime, 18000000, 25000000, "Hanoi", today.AddDays(10), JobPostingStatus.Closed, false, "Closed requisition for a QA Engineer covering web regression and API checks."),
                Job(employers[1], companies[1], categoryByName["Marketing"], "Marketing Executive", WorkType.FullTime, 15000000, 22000000, "Ho Chi Minh City", today.AddDays(25), JobPostingStatus.Active, true, "Plan campaigns and report performance for the consumer Marketing team."),
                Job(employers[1], companies[1], categoryByName["Sales"], "Sales Representative", WorkType.FullTime, 12000000, 20000000, "Hanoi", today.AddDays(18), JobPostingStatus.Active, false, "Own a local Sales pipeline and keep weekly forecasts up to date."),
                Job(employers[1], companies[1], categoryByName["Finance and Accounting"], "Financial Analyst", WorkType.Contract, 20000000, 28000000, "Hanoi", today.AddDays(-12), JobPostingStatus.Expired, false, "This Analyst contract has passed its application deadline."),
                Job(employers[2], companies[2], categoryByName["Information Technology"], "Network Engineer", WorkType.FullTime, 22000000, 32000000, "Da Nang", today.AddDays(28), JobPostingStatus.Active, false, "Operate enterprise networks. The Engineer will join the Da Nang operations desk."),
                Job(employers[2], companies[2], categoryByName["Human Resources"], "HR Specialist", WorkType.FullTime, 14000000, 20000000, "Hanoi", today.AddDays(22), JobPostingStatus.Active, false, "Support recruiting operations and employee records for business units."),
                Job(employers[2], companies[2], categoryByName["Marketing"], "Content Marketing", WorkType.PartTime, 10000000, 15000000, "Da Nang", today.AddDays(15), JobPostingStatus.Active, false, "Write campaign stories for a part-time Marketing content calendar."),
                Job(employers[3], companies[3], categoryByName["Design"], "UI Designer", WorkType.Remote, 18000000, 30000000, "Ho Chi Minh City", today.AddDays(35), JobPostingStatus.Active, true, "Design product screens. The Designer works with product and engineering."),
                Job(employers[3], companies[3], categoryByName["Finance and Accounting"], "Accountant", WorkType.FullTime, 14000000, 19000000, "Ho Chi Minh City", today.AddDays(12), JobPostingStatus.Closed, false, "This accounting vacancy is closed and no longer accepts applications."),
                Job(employers[3], companies[3], categoryByName["Information Technology"], "Data Analyst", WorkType.FullTime, 20000000, 30000000, "Hanoi", today.AddDays(-5), JobPostingStatus.Expired, false, "The Data Analyst posting expired before the current hiring cycle."),
                Job(employers[4], companies[4], categoryByName["Sales"], "Operations Executive", WorkType.FullTime, 16000000, 24000000, "Ho Chi Minh City", today.AddDays(21), JobPostingStatus.Active, false, "Coordinate city operations and partner follow-up for the local team."),
                Job(employers[4], companies[4], categoryByName["Human Resources"], "Driver Partner Support", WorkType.Contract, 11000000, 16000000, "Hanoi", today.AddDays(8), JobPostingStatus.Closed, false, "Closed contract role supporting driver partners by phone and chat."),
                Job(employers[4], companies[4], categoryByName["Marketing"], "Mobile App Marketer", WorkType.PartTime, 12000000, 18000000, "Remote", today.AddDays(-8), JobPostingStatus.Expired, false, "Expired part-time marketing role for the mobile application.")
            };

            var jobSkills = new[]
            {
                new[] { "C#", "ASP.NET", "SQL" },
                new[] { "ASP.NET", "SQL", "JavaScript" },
                new[] { "SQL", "Communication", "Linux" },
                new[] { "SEO", "Content Writing", "Communication" },
                new[] { "Negotiation", "Communication", "Excel" },
                new[] { "Excel", "Accounting", "Communication" },
                new[] { "Linux", "Communication", "Python" },
                new[] { "Recruitment", "Communication", "Excel" },
                new[] { "Content Writing", "SEO", "Photoshop" },
                new[] { "UI Design", "Photoshop", "JavaScript" },
                new[] { "Accounting", "Excel", "Communication" },
                new[] { "Python", "SQL", "Excel" },
                new[] { "Negotiation", "Excel", "Communication" },
                new[] { "Communication", "Recruitment", "Excel" },
                new[] { "SEO", "Content Writing", "JavaScript" }
            };

            for (var i = 0; i < jobs.Count; i++)
            {
                jobs[i].RequiredSkills = jobSkills[i]
                    .Select(name => new JobRequiredSkill { Skill = skillByName[name] })
                    .ToList();
                context.JobPostings.Add(jobs[i]);
            }

            var profiles = BuildProfiles(seekers, skillByName);
            context.CandidateProfiles.AddRange(profiles);
            context.SaveChanges();

            var applications = new[]
            {
                App(jobs[0], profiles[0], ApplicationStatus.Submitted, -2),
                App(jobs[0], profiles[1], ApplicationStatus.UnderReview, -6),
                App(jobs[0], profiles[2], ApplicationStatus.Shortlisted, -8),
                App(jobs[2], profiles[3], ApplicationStatus.InterviewScheduled, -10),
                App(jobs[2], profiles[4], ApplicationStatus.Offered, -12),
                App(jobs[3], profiles[5], ApplicationStatus.Rejected, -4),
                App(jobs[3], profiles[6], ApplicationStatus.Submitted, -1),
                App(jobs[3], profiles[7], ApplicationStatus.Withdrawn, -9),
                App(jobs[5], profiles[0], ApplicationStatus.UnderReview, -15),
                App(jobs[5], profiles[1], ApplicationStatus.Shortlisted, -14),
                App(jobs[6], profiles[2], ApplicationStatus.InterviewScheduled, -3),
                App(jobs[6], profiles[3], ApplicationStatus.Offered, -7),
                App(jobs[7], profiles[4], ApplicationStatus.Rejected, -5),
                App(jobs[7], profiles[5], ApplicationStatus.Submitted, -2),
                App(jobs[9], profiles[6], ApplicationStatus.UnderReview, -6),
                App(jobs[9], profiles[7], ApplicationStatus.Shortlisted, -4),
                App(jobs[12], profiles[0], ApplicationStatus.InterviewScheduled, -1),
                App(jobs[12], profiles[1], ApplicationStatus.Rejected, -3),
                App(jobs[14], profiles[2], ApplicationStatus.Withdrawn, -11),
                App(jobs[14], profiles[3], ApplicationStatus.Submitted, -13)
            };
            context.JobApplications.AddRange(applications);

            context.SavedJobs.Add(new SavedJob { JobSeekerId = seekers[8].Id, JobPosting = jobs[0], SavedAt = today.AddDays(-1) });
            context.SavedJobs.Add(new SavedJob { JobSeekerId = seekers[8].Id, JobPosting = jobs[4], SavedAt = today.AddDays(-2) });
            context.SavedJobs.Add(new SavedJob { JobSeekerId = seekers[9].Id, JobPosting = jobs[1], SavedAt = today.AddDays(-3) });
            context.SavedJobs.Add(new SavedJob { JobSeekerId = seekers[0].Id, JobPosting = jobs[3], SavedAt = today });

            context.JobAlerts.Add(new JobAlert { JobSeekerId = seekers[8].Id, Keyword = "Developer", IsActive = true, CreatedAt = today.AddDays(-4) });
            context.JobAlerts.Add(new JobAlert { JobSeekerId = seekers[9].Id, Keyword = "Marketing", IsActive = true, CreatedAt = today.AddDays(-3) });
            context.JobAlerts.Add(new JobAlert { JobSeekerId = seekers[0].Id, Keyword = "Engineer", IsActive = true, CreatedAt = today.AddDays(-2) });
            context.JobAlerts.Add(new JobAlert { JobSeekerId = seekers[4].Id, Keyword = "Designer", IsActive = true, CreatedAt = today.AddDays(-1) });
            context.JobAlerts.Add(new JobAlert { JobSeekerId = seekers[8].Id, Keyword = "Internship", IsActive = false, CreatedAt = today });
            context.SaveChanges();

            AttachResume(profiles[0], seekers[0]);
            AttachResume(profiles[3], seekers[3]);
            AttachResume(profiles[5], seekers[5]);

            var openJobs = context.JobPostings.Where(job => job.Status == JobPostingStatus.Active).ToList();
            foreach (var job in openJobs)
            {
                JobAlertMatcher.Match(context, job);
            }

            context.SaveChanges();
        }

        private static void AttachResume(CandidateProfile profile, ApplicationUser user)
        {
            var relativeDirectory = "App_Data/Resumes/" + user.Id;
            var absoluteDirectory = HostingEnvironment.MapPath("~/" + relativeDirectory);
            Directory.CreateDirectory(absoluteDirectory);
            var bytes = PdfResumeBuilder.Build(profile, user.FullName, user.Email);
            File.WriteAllBytes(Path.Combine(absoluteDirectory, "resume.pdf"), bytes);
            profile.ResumeFileName = "resume.pdf";
            profile.ResumeFilePath = relativeDirectory + "/resume.pdf";
        }

        private static List<CandidateProfile> BuildProfiles(IList<ApplicationUser> seekers, Dictionary<string, Skill> skills)
        {
            var profiles = new List<CandidateProfile>();
            profiles.Add(Profile(seekers[0], "Senior .NET developer", "Eight years delivering business web applications.", "0901000001", "Ba Dinh, Hanoi", new DateTime(1992, 4, 12),
                new[] { Edu("Hanoi University of Science and Technology", "Bachelor", "Information Technology", 2010, 2014), Edu("FPT University", "Certificate", "Software Architecture", 2018, 2018) },
                new[] { Exp("FPT Software", "Developer", 2014, 2018, false), Exp("Viettel", "Senior Developer", 2018, null, true) },
                new[] { Sk(skills, "C#", SkillLevel.Expert), Sk(skills, "ASP.NET", SkillLevel.Advanced), Sk(skills, "SQL", SkillLevel.Advanced), Sk(skills, "JavaScript", SkillLevel.Intermediate) },
                new[] { Lang("English", LanguageProficiency.Fluent), Lang("Japanese", LanguageProficiency.Basic) }));

            profiles.Add(Profile(seekers[1], "Marketing associate", "Campaign planning for retail brands.", "0901000002", "District 1, Ho Chi Minh City", new DateTime(1996, 8, 2),
                new[] { Edu("University of Economics Ho Chi Minh City", "Bachelor", "Marketing", 2014, 2018) },
                new[] { Exp("Tiki", "Marketing Executive", 2018, null, true) },
                new[] { Sk(skills, "SEO", SkillLevel.Intermediate), Sk(skills, "Content Writing", SkillLevel.Advanced), Sk(skills, "Communication", SkillLevel.Advanced) },
                new[] { Lang("English", LanguageProficiency.Fluent) }));

            profiles.Add(Profile(seekers[2], "Network support", null, "0901000003", null, null,
                new EducationRecord[0],
                new[] { Exp("Viettel", "Network Technician", 2019, null, true) },
                new[] { Sk(skills, "Linux", SkillLevel.Intermediate), Sk(skills, "Communication", SkillLevel.Intermediate), Sk(skills, "Python", SkillLevel.Beginner) },
                new CandidateLanguage[0]));

            profiles.Add(Profile(seekers[3], "HR generalist", "Recruiting for technology teams.", "0901000004", "Cau Giay, Hanoi", new DateTime(1994, 1, 20),
                new[] { Edu("National Economics University", "Bachelor", "Human Resources", 2012, 2016), Edu("RMIT", "Short course", "People Analytics", 2020, 2020) },
                new[] { Exp("FPT Software", "HR Specialist", 2016, null, true) },
                new[] { Sk(skills, "Recruitment", SkillLevel.Advanced), Sk(skills, "Communication", SkillLevel.Advanced), Sk(skills, "Excel", SkillLevel.Intermediate), Sk(skills, "Negotiation", SkillLevel.Intermediate) },
                new[] { Lang("English", LanguageProficiency.Conversational), Lang("Korean", LanguageProficiency.Basic) }));

            profiles.Add(Profile(seekers[4], null, "Designer moving from agency work into product teams.", null, "Da Nang", null,
                new[] { Edu("University of Arts", "Bachelor", "Graphic Design", 2013, 2017) },
                new[] { Exp("Tiki", "UI Designer", 2017, 2021, false), Exp("Freelance", "Product Designer", 2021, null, true) },
                new[] { Sk(skills, "UI Design", SkillLevel.Advanced), Sk(skills, "Photoshop", SkillLevel.Expert), Sk(skills, "JavaScript", SkillLevel.Beginner) },
                new[] { Lang("English", LanguageProficiency.Conversational) }));

            profiles.Add(Profile(seekers[5], "Accountant", "Month-end close and management reports.", "0901000006", "Hai Ba Trung, Hanoi", new DateTime(1993, 11, 5),
                new EducationRecord[0],
                new[] { Exp("Vingroup", "Accountant", 2016, null, true) },
                new[] { Sk(skills, "Accounting", SkillLevel.Advanced), Sk(skills, "Excel", SkillLevel.Expert), Sk(skills, "Communication", SkillLevel.Intermediate), Sk(skills, "SQL", SkillLevel.Beginner), Sk(skills, "Negotiation", SkillLevel.Beginner) },
                new CandidateLanguage[0]));

            profiles.Add(Profile(seekers[6], null, null, null, null, null,
                new EducationRecord[0],
                new[] { Exp("Grab Vietnam", "Operations Coordinator", 2020, null, true) },
                new[] { Sk(skills, "Communication", SkillLevel.Intermediate), Sk(skills, "Excel", SkillLevel.Intermediate), Sk(skills, "Negotiation", SkillLevel.Beginner) },
                new CandidateLanguage[0]));

            profiles.Add(Profile(seekers[7], "Content writer", "Writes product and recruitment stories.", "0901000008", "District 3, Ho Chi Minh City", new DateTime(1998, 6, 18),
                new[] { Edu("University of Social Sciences", "Bachelor", "Journalism", 2016, 2020) },
                new[] { Exp("Local Media", "Content Writer", 2020, null, true) },
                new[] { Sk(skills, "Content Writing", SkillLevel.Advanced), Sk(skills, "SEO", SkillLevel.Intermediate), Sk(skills, "Communication", SkillLevel.Advanced) },
                new CandidateLanguage[0]));

            profiles.Add(Profile(seekers[8], "ASP.NET intern candidate", "Recent graduate looking for a developer role.", "0901000009", "Thu Duc, Ho Chi Minh City", new DateTime(2001, 2, 9),
                new[] { Edu("HUTECH", "Bachelor", "Software Engineering", 2019, 2023) },
                new[] { Exp("Campus Lab", "Intern Developer", 2023, 2024, false) },
                new[] { Sk(skills, "C#", SkillLevel.Intermediate), Sk(skills, "ASP.NET", SkillLevel.Intermediate), Sk(skills, "SQL", SkillLevel.Beginner) },
                new[] { Lang("English", LanguageProficiency.Conversational) }));

            profiles.Add(Profile(seekers[9], "Sales graduate", null, "0901000010", null, null,
                new[] { Edu("Foreign Trade University", "Bachelor", "International Business", 2017, 2021) },
                new[] { Exp("Retail Group", "Sales Executive", 2021, null, true) },
                new[] { Sk(skills, "Negotiation", SkillLevel.Intermediate), Sk(skills, "Communication", SkillLevel.Advanced), Sk(skills, "Excel", SkillLevel.Beginner) },
                new CandidateLanguage[0]));

            return profiles;
        }

        private static ApplicationUser CreateUser(UserManager<ApplicationUser> userManager, string email, string fullName, string role)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-30)
            };
            var result = userManager.Create(user, DemoPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors));
            }

            userManager.AddToRole(user.Id, role);
            return user;
        }

        private static CompanyProfile Company(ApplicationUser employer, string name, string industry, string website, string address, string phone, string description)
        {
            return new CompanyProfile
            {
                EmployerId = employer.Id,
                CompanyName = name,
                Industry = industry,
                Website = website,
                Address = address,
                Phone = phone,
                Description = description
            };
        }

        private static JobPosting Job(ApplicationUser employer, CompanyProfile company, JobCategory category, string title, WorkType workType, decimal salaryMin, decimal salaryMax, string location, DateTime deadline, JobPostingStatus status, bool featured, string description)
        {
            var created = DateTime.Now.AddDays(-20);
            return new JobPosting
            {
                EmployerId = employer.Id,
                CompanyProfile = company,
                JobCategory = category,
                Title = title,
                CompanyName = company.CompanyName,
                Description = description,
                WorkType = workType,
                SalaryMin = salaryMin,
                SalaryMax = salaryMax,
                Location = location,
                ApplicationDeadline = deadline,
                Status = status,
                IsFeatured = featured,
                CreatedAt = created,
                UpdatedAt = created
            };
        }

        private static JobApplication App(JobPosting job, CandidateProfile profile, ApplicationStatus status, int daysAgo)
        {
            var applied = DateTime.Now.AddDays(daysAgo);
            return new JobApplication
            {
                JobPosting = job,
                CandidateProfile = profile,
                JobSeekerId = profile.JobSeekerId,
                CoverLetter = "I would like to apply for " + job.Title + " at " + job.CompanyName + ". My experience matches the required skills for this role.",
                Status = status,
                AppliedAt = applied,
                UpdatedAt = applied
            };
        }

        private static CandidateProfile Profile(ApplicationUser seeker, string headline, string summary, string phone, string address, DateTime? dob, EducationRecord[] educations, WorkExperience[] experiences, CandidateSkill[] profileSkills, CandidateLanguage[] languages)
        {
            return new CandidateProfile
            {
                JobSeekerId = seeker.Id,
                Headline = headline,
                Summary = summary,
                Phone = phone,
                Address = address,
                DateOfBirth = dob,
                UpdatedAt = DateTime.Now,
                Educations = educations.ToList(),
                WorkExperiences = experiences.ToList(),
                Skills = profileSkills.ToList(),
                Languages = languages.ToList()
            };
        }

        private static EducationRecord Edu(string school, string degree, string field, int startYear, int endYear)
        {
            return new EducationRecord
            {
                School = school,
                Degree = degree,
                FieldOfStudy = field,
                StartDate = new DateTime(startYear, 9, 1),
                EndDate = new DateTime(endYear, 6, 30)
            };
        }

        private static WorkExperience Exp(string company, string title, int startYear, int? endYear, bool current)
        {
            return new WorkExperience
            {
                CompanyName = company,
                JobTitle = title,
                StartDate = new DateTime(startYear, 3, 1),
                EndDate = endYear.HasValue ? new DateTime(endYear.Value, 12, 31) : (DateTime?)null,
                IsCurrent = current,
                Description = title + " at " + company + "."
            };
        }

        private static CandidateSkill Sk(Dictionary<string, Skill> skills, string name, SkillLevel level)
        {
            return new CandidateSkill { Skill = skills[name], Level = level };
        }

        private static CandidateLanguage Lang(string name, LanguageProficiency proficiency)
        {
            return new CandidateLanguage { LanguageName = name, Proficiency = proficiency };
        }
    }
}
