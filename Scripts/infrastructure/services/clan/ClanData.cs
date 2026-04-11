using System;
using System.Collections.Generic;

namespace infrastructure.services.clan
{
    public class ClanData
    {
        public int Id { get; set; }
        public string ClanName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public int MemberCount { get; set; }
        public int MaxMembers { get; set; }
        public int IconId { get; set; }
        public bool IsOpen { get; set; }
        public FactionType FactionType { get; set; }
        public int OwnerId { get; set; }
        public int Emeralds { get; set; }
        public DateTime CreationDate { get; set; }
        public List<ClanMember> Members { get; set; } = new List<ClanMember>();
    }

    public class ClanMember
    {
        public int CharacterId { get; set; }
        public string CharacterName { get; set; }
        public int CharacterLevel { get; set; }
        public ClanRole Role { get; set; }
        public int Permissions { get; set; }
        public bool IsOnline { get; set; }
        public DateTime JoinDate { get; set; }

        public bool HasPermission(ClanPermission permission)
        {
            return (Permissions & (int)permission) != 0;
        }
    }

    public class ClanInvite
    {
        public int Id { get; set; }
        public int ClanId { get; set; }
        public string ClanName { get; set; }
        public string ShortName { get; set; }
        public int InviterCharacterId { get; set; }
        public string InviterName { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public class ClanBuff
    {
        public int Id { get; set; }
        public int BuffId { get; set; }
        public float Duration { get; set; }
        public DateTime StartTime { get; set; }

        public bool IsActive => Duration <= 0 || StartTime.AddSeconds(Duration) > DateTime.UtcNow;

        public float RemainingTime
        {
            get
            {
                if (Duration <= 0) return float.MaxValue;
                var remaining = (float)(StartTime.AddSeconds(Duration) - DateTime.UtcNow).TotalSeconds;
                return Math.Max(0, remaining);
            }
        }
    }

    public class ClanCraft
    {
        public int Id { get; set; }
        public int CraftId { get; set; }
        public float Duration { get; set; }
        public DateTime StartTime { get; set; }

        public bool IsCompleted => StartTime.AddSeconds(Duration) <= DateTime.UtcNow;

        public float Progress
        {
            get
            {
                if (Duration <= 0) return 0;
                var elapsed = (float)(DateTime.UtcNow - StartTime).TotalSeconds;
                return Math.Min(1f, elapsed / Duration);
            }
        }

        public float RemainingTime
        {
            get
            {
                var remaining = (float)(StartTime.AddSeconds(Duration) - DateTime.UtcNow).TotalSeconds;
                return Math.Max(0, remaining);
            }
        }
    }
    
    public class ClanHistoryEntry
    {
        public int Id { get; set; }
        public ClanHistoryType HistoryType { get; set; }
        public int Value { get; set; }
        public int CharacterPerformer { get; set; }
        public int CharacterTarget { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class FactionData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class FactionLeaderboardEntry
    {
        public int FactionId { get; set; }
        public long TotalPoints { get; set; }
        public int ParticipatingClans { get; set; }
    }

    public class ClansSettings
    {
        public int MinLevelToCreate { get; set; }
        public int CreationCostGold { get; set; }
        public int MinNameLength { get; set; }
        public int MaxNameLength { get; set; }
        public int MinShortNameLength { get; set; }
        public int MaxShortNameLength { get; set; }
    }
}