using Cysharp.Threading.Tasks;

namespace infrastructure.services.platform.core.leaderboard
{
    public interface ILeaderboardService
    {
        UniTask SubmitScore(string leaderboardId, long score);
        UniTask<LeaderboardEntry[]> GetTopScores(string leaderboardId, int count);
    }
}
