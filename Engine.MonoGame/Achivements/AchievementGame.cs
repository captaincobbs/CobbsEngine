using System;

namespace Engine.MonoGame.Achivements
{
    public class AchievementGame
    {
        public string ID { get; set; } = string.Empty;

        public string Name { get; set; }

        public Uri IconURL { get; set; }
        public Uri CoverURL { get; set; }

        public int? TotalAchievements { get; set; }
    }
}
