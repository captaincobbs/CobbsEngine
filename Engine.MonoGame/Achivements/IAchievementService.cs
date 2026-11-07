using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Engine.Core.Services;

namespace Engine.MonoGame.Achivements
{
    interface IAchievementService : IService
    {
        AchievementServiceCapabilities Capabilities { get; }

        event EventHandler AuthenticationChanged;
        event EventHandler<AchievementUnlockedEventArgs> AchievementUnlocked;
        event EventHandler<AchievementProgressEventArgs> AchievementProgressChanged;
        event EventHandler DataRefreshed;

        #region Authentication
        Task<bool> AuthenticateAsync(CancellationToken cancellation);

        Task DisconnectAsync(CancellationToken cancellation);

        Task<AchievementUser> GetCurrentUserAsync(CancellationToken cancellation);
        #endregion

        #region Game Information
        Task<AchievementGame> GetGameAsync(CancellationToken cancellation);
        #endregion

        #region Achievement Definitions
        Task<IReadOnlyList<Achievement>> GetAchivementsAsync(CancellationToken cancellation);
        Task<Achievement> GetAchievementAsync(string achievementID, CancellationToken cancellation);
        #endregion

        #region Player Achievement State
        Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(CancellationToken cancellation);

        Task<UserAchievement> GetUserAchievementAsync(string achievementID, CancellationToken cancellation);

        Task<IReadOnlyList<UserAchievement>> GetUnlockedAchievementsAsync(CancellationToken cancellation);

        Task<IReadOnlyList<UserAchievement>> GetLockedAchievementsAsyncs(CancellationToken cancellation);

        Task<IReadOnlyList<UserAchievement>> GetAchievementsUnlockedBetween(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellation);
        #endregion

        #region Statistics
        Task<AchievementStatistics> GetStatisticsAsync(CancellationToken cancellation);

        Task<IReadOnlyList<AchievementRarity>> GetGlobalAchievementStatisticsAsync(CancellationToken cancellation);
        #endregion

        #region Synchronization
        /// <summary>Refresh cached provider data</summary>
        Task RefreshAsync();
        #endregion
    }

    public class AchievementUnlockedEventArgs : EventArgs
    {
        public string AchievementID { get; set; }
    }

    public class AchievementProgressEventArgs : EventArgs
    {
        public string AchievementID { get; set; }
        public double Progress { get; set; }
        public double ProgressTarget { get; set; }
    }
}
