using System;

namespace Engine.MonoGame.Achivements
{
    public class AchievementStatistics
    {
        public int TotalAchievements { get; set; }

        public int UnlockedAchievements { get; set; }
        public int LockedAchievements { get; set; }

        public int TotalPoints { get; set; }
        public int EarnedPoints { get; set; }

        public double CompletionPercentage { get; set; }

        public DateTimeOffset? LastUpdated { get; set; }
    }
}
