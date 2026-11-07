using System;

namespace Engine.MonoGame.Achivements
{
    public class AchievementRarity
    {
        public string AchievementID { get; set; } = string.Empty;

        public double? UnlockedPercentage => TotalPlayers is > 0 && TotalPlayers.HasValue ? Math.Clamp(PlayersUnlocked.Value / TotalPlayers.Value * 100, 0, 100) : null;
        public long? TotalPlayers { get; set; }
        public long? PlayersUnlocked { get; set; }
    }
}
