#if VK_PLAY
using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.leaderboard;
using UnityEngine;

namespace infrastructure.services.platform.vk
{
    public class VKLeaderboardProvider : ILeaderboardService
    {
        public UniTask SubmitScore(string leaderboardId, long score)
        {
            Debug.LogWarning("[VKLeaderboard] Нативные лидерборды не поддерживаются в VK Play.");
            return UniTask.CompletedTask;
        }

        public UniTask<LeaderboardEntry[]> GetTopScores(string leaderboardId, int count)
        {
            Debug.LogWarning("[VKLeaderboard] Нативные лидерборды не поддерживаются в VK Play.");
            return UniTask.FromResult(Array.Empty<LeaderboardEntry>());
        }
    }
}
#endif
