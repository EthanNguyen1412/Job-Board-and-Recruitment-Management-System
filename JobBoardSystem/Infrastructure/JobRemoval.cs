using System.Linq;
using JobBoardSystem.Models;

namespace JobBoardSystem.Infrastructure
{
    public static class JobRemoval
    {
        public static void Remove(ApplicationDbContext db, JobPosting job)
        {
            var matches = db.JobAlertMatches.Where(item => item.JobPostingId == job.Id).ToList();
            db.JobAlertMatches.RemoveRange(matches);
            db.JobPostings.Remove(job);
        }
    }
}
