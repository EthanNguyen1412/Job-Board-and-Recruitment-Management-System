using System;
using JobBoardSystem.Models;

namespace JobBoardSystem.Infrastructure
{
    public static class JobRules
    {
        public static bool IsOpenForApplication(JobPosting job, DateTime today)
        {
            return job != null
                && job.Status == JobPostingStatus.Active
                && job.ApplicationDeadline.Date >= today.Date;
        }

        public static string EffectiveStatusName(JobPosting job, DateTime today)
        {
            if (job.Status == JobPostingStatus.Active && job.ApplicationDeadline.Date < today.Date)
            {
                return JobPostingStatus.Expired.GetDisplayName();
            }

            return job.Status.GetDisplayName();
        }

        public static string SalaryText(decimal min, decimal max)
        {
            return min.ToString("N0") + " - " + max.ToString("N0");
        }
    }
}
