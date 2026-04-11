#if GOOGLE_PLAY
using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.leaderboard;

namespace infrastructure.services.platform.google
{
    /// <summary>
    /// Заглушка таблицы лидеров для Google Play. Google Play Games Plugin — отдельный этап.
    /// </summary>
    public class GooglePlayLeaderboardProvider : ILeaderboardService
    {
        public UniTask SubmitScore(string leaderboardId, long score) => UniTask.CompletedTask;

        public UniTask<LeaderboardEntry[]> GetTopScores(string leaderboardId, int count)
            => UniTask.FromResult(Array.Empty<LeaderboardEntry>());
    }
}
#endif
