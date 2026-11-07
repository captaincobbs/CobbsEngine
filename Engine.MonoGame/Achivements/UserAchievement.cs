using System;

namespace Engine.MonoGame.Achivements
{
    /// <summary>Represents a user's progress towards an achievement</summary>
    public class UserAchievement
    {
        public string AchievementID { get; set; } = string.Empty;
        
        public bool IsUnlocked { get; set; }
        public DateTimeOffset? UnlockedAt { get; set; }

        // Progress towards unlocking
        public double? Progress { get; set; }
        public double? ProgressTarget { get; set; }
        public double? ProgressPercentage => ProgressTarget is > 0 && Progress.HasValue ? Math.Clamp(Progress.Value / ProgressTarget.Value * 100, 0, 100) : null;

        // Optional provider-specific info
        public double? GlobalUnlockPercentage { get; set; }
    }
}
