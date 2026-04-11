namespace infrastructure.services.platform.core.leaderboard
{
    public class LeaderboardEntry
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public long Score { get; set; }
        public int Rank { get; set; }
    }
}
