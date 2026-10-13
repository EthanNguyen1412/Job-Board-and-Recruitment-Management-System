using System.Linq;
using JobBoardSystem.Models;

namespace JobBoardSystem.Infrastructure
{
    public static class ProfileCompletion
    {
        public static int Percent(CandidateProfile profile)
        {
            if (profile == null)
            {
                return 0;
            }

            var score = 0;
            const int total = 9;
            if (!string.IsNullOrWhiteSpace(profile.Headline)) score++;
            if (!string.IsNullOrWhiteSpace(profile.Summary)) score++;
            if (!string.IsNullOrWhiteSpace(profile.Phone)) score++;
            if (!string.IsNullOrWhiteSpace(profile.Address)) score++;
            if (profile.Educations != null && profile.Educations.Any()) score++;
            if (profile.WorkExperiences != null && profile.WorkExperiences.Any()) score++;
            if (profile.Skills != null && profile.Skills.Any()) score++;
            if (profile.Languages != null && profile.Languages.Any()) score++;
            if (!string.IsNullOrWhiteSpace(profile.ResumeFilePath)) score++;
            return (score * 100) / total;
        }
    }
}
