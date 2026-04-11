#if YANDEX_GAMES
using System;
using YandexGames;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.leaderboard;

namespace infrastructure.services.platform.yandex
{
    public class YandexLeaderboardProvider : ILeaderboardService
    {
        public async UniTask SubmitScore(string leaderboardId, long score)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            Leaderboard.SetScore(leaderboardId, (int)score,
                onSuccessCallback: () => tcs.TrySetResult(true),
                onErrorCallback: _ => tcs.TrySetResult(false));
            await tcs.Task;
        }

        public async UniTask<LeaderboardEntry[]> GetTopScores(string leaderboardId, int count)
        {
            var tcs = new UniTaskCompletionSource<LeaderboardGetEntriesResponse>();
            Leaderboard.GetEntries(leaderboardId,
                onSuccessCallback: r => tcs.TrySetResult(r),
                onErrorCallback: _ => tcs.TrySetResult(null),
                topPlayersCount: count);
            var r = await tcs.Task;
            if (r == null) return Array.Empty<LeaderboardEntry>();
            return MapEntries(r.entries);
        }

        private LeaderboardEntry[] MapEntries(LeaderboardEntryResponse[] entries)
        {
            var result = new LeaderboardEntry[entries.Length];
            for (int i = 0; i < entries.Length; i++)
                result[i] = new LeaderboardEntry
                {
                    Score = entries[i].score,
                    Rank = entries[i].rank,
                    PlayerName = entries[i].player.publicName
                };
            return result;
        }
    }
}
#endif
