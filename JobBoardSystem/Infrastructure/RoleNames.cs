namespace JobBoardSystem.Infrastructure
{
    public static class RoleNames
    {
        public const string JobSeeker = "JobSeeker";
        public const string Employer = "Employer";
        public const string Administrator = "Administrator";

        public static readonly string[] All = { JobSeeker, Employer, Administrator };
        public static readonly string[] Registerable = { JobSeeker, Employer };

        public static string Display(string role)
        {
            if (role == JobSeeker) return "Job Seeker";
            if (role == Employer) return "Employer";
            if (role == Administrator) return "Administrator";
            return role;
        }
    }
}
