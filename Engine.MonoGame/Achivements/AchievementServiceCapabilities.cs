using System;

namespace Engine.MonoGame.Achivements
{
    [Flags]
    public enum AchievementServiceCapabilities
    {
        None = 0,

        Authentication =       1 << 0,
        UserProfile =          1 << 1,
        GameInformation =      1 << 2,

        Achievements =         1 << 3,
        UserProgress =         1 << 4,
        AchievementHistory =   1 << 5,

        GlobalStatistics =     1 << 6,
        GlobalRarity =         1 << 7,

        Friends =              1 << 8,
        FriendStatistics =     1 << 9,

        Leaderboards =         1 << 10,

        AchievementUnlocking = 1 << 11,
        ProgressReporting =    1 << 12,

        OfflineSupport =       1 << 13,
    }
}
