#if RU_STORE
using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.leaderboard;

namespace infrastructure.services.platform.rustore
{
    /// <summary>
    /// Заглушка таблицы лидеров для RuStore (отдельного API нет).
    /// </summary>
    public class RuStoreLeaderboardProvider : ILeaderboardService
    {
        public UniTask SubmitScore(string leaderboardId, long score) => UniTask.CompletedTask;

        public UniTask<LeaderboardEntry[]> GetTopScores(string leaderboardId, int count)
            => UniTask.FromResult(Array.Empty<LeaderboardEntry>());
    }
}
#endif
