using System;
using System.Threading;
using System.Threading.Tasks;

namespace Engine.MonoGame.Achivements
{
    public interface IAchievementAdapter : IDisposable
    {
        Task UnlockAchievementAsync(string achievementID, CancellationToken cancellation);

        Task SetAchievementProgressAsync(string acheivementID, double progress, double target, CancellationToken cancellation);

        Task ResetAchievementAsync(string achievementID, CancellationToken cancellation);
    }
}
