using System;
using System.Linq;
using JobBoardSystem.Models;

namespace JobBoardSystem.Infrastructure
{
    public static class JobAlertMatcher
    {
        public static void Match(ApplicationDbContext db, JobPosting job)
        {
            if (!JobRules.IsOpenForApplication(job, DateTime.Today))
            {
                return;
            }

            var skillNames = db.JobRequiredSkills
                .Where(item => item.JobPostingId == job.Id)
                .Select(item => item.Skill.Name)
                .ToList();

            var alerts = db.JobAlerts.Where(alert => alert.IsActive).ToList();
            foreach (var alert in alerts)
            {
                if (!ContainsKeyword(job, skillNames, alert.Keyword))
                {
                    continue;
                }

                var exists = db.JobAlertMatches.Any(match =>
                    match.JobAlertId == alert.Id && match.JobPostingId == job.Id);
                if (exists)
                {
                    continue;
                }

                db.JobAlertMatches.Add(new JobAlertMatch
                {
                    JobAlertId = alert.Id,
                    JobPostingId = job.Id,
                    MatchedAt = DateTime.Now,
                    IsRead = false
                });
            }
        }

        public static bool ContainsKeyword(JobPosting job, System.Collections.Generic.IEnumerable<string> skillNames, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return false;
            }

            var term = keyword.Trim();
            if (Has(job.Title, term) || Has(job.Description, term) || Has(job.CompanyName, term) || Has(job.Location, term))
            {
                return true;
            }

            return skillNames.Any(name => Has(name, term));
        }

        private static bool Has(string source, string term)
        {
            return !string.IsNullOrEmpty(source)
                && source.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
