using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.leaderboard;
using UnityEngine;

namespace infrastructure.services.platform.stubs
{
    public class EditorLeaderboardProvider : ILeaderboardService
    {
        public async UniTask SubmitScore(string leaderboardId, long score)
        {
            Debug.Log($"[EditorLeaderboard] SubmitScore({leaderboardId}, {score})");
            await UniTask.CompletedTask;
        }

        public async UniTask<LeaderboardEntry[]> GetTopScores(string leaderboardId, int count)
        {
            Debug.Log($"[EditorLeaderboard] GetTopScores({leaderboardId}, {count}) — returning empty");
            await UniTask.CompletedTask;
            return System.Array.Empty<LeaderboardEntry>();
        }
    }
}
