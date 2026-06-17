using System;
using System.Linq;
using Recruitment_Portal.Models;


namespace RoyalPortalDb.Services
{
    public class ProfileCompletionService
    {
        private readonly RoyalPortalDbEntities db;


        public ProfileCompletionService()
        {
            db = new RoyalPortalDbEntities();
        }

        public int CalculateCompletion(Guid userId)
        {
            var profile = db.youth_profiles.FirstOrDefault(x => x.user_id == userId);
            if (profile == null) return 0;

            int score = 0;

            // 1. Basic profile fields (40%)
            if (!string.IsNullOrEmpty(profile.first_name)) score += 5;
            if (!string.IsNullOrEmpty(profile.last_name)) score += 5;
            if (profile.date_of_birth != null) score += 5;
            if (!string.IsNullOrEmpty(profile.gender)) score += 5;
            if (!string.IsNullOrEmpty(profile.address)) score += 5;
            if (!string.IsNullOrEmpty(profile.bio)) score += 10;
            if (!string.IsNullOrEmpty(profile.profile_photo_url)) score += 5;

            // 2. Work experience (30%)
            var workCount = db.work_experience.Count(x => x.user_id == userId);
            if (workCount > 0) score += 30;

            // 3. Education (30%)
            var eduCount = db.educations.Count(x => x.user_id == userId);
            if (eduCount > 0) score += 30;

            return score;
        }
    }
}