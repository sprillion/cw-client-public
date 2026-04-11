using System;
using System.Collections.Generic;
using network;

namespace infrastructure.services.clan
{
    public interface IClanService : IReceiver
    {
        // Current player's clan
        ClansSettings ClansSettings { get; }
        ClanData MyClan { get; }
        ClanMember MyMember { get; }
        bool IsInClan { get; }

        // Cached data
        List<ClanData> CachedClans { get; }
        List<ClanInvite> PendingInvites { get; }

        // Events
        event Action<ClanData> OnClanInfoReceived;
        event Action<List<ClanData>> OnClansListReceived;
        event Action<List<ClanData>> OnSearchResultsReceived;
        event Action<bool, int, ClanData> OnClanCreated;
        event Action<bool, int> OnClanJoined;
        event Action<bool> OnClanLeft;
        event Action<ClanError> OnClanError;
        event Action<ClanData> OnMyClanUpdated;

        // Member events
        event Action<int, string, int, ClanRole> OnMemberJoined;
        event Action<int> OnMemberLeft;
        event Action<int, bool> OnMemberKicked;
        event Action<int, ClanRole, int> OnMemberPromoted;
        event Action<int, ClanRole, int> OnMemberDemoted;
        event Action<int, int> OnOwnershipTransferred;

        // Settings events
        event Action<string, bool> OnSettingsUpdated;
        event Action<int> OnClanDisbanded;

        // Invite events
        event Action<bool, int> OnInviteSent;
        event Action<ClanInvite> OnInviteReceived;
        event Action<bool, int> OnInviteResponse;
        event Action<List<ClanInvite>> OnPendingInvitesReceived;

        // Donation events
        event Action<bool, int, int> OnDonationResult;

        // Buff and craft events
        event Action<List<ClanBuff>> OnClanBuffsReceived;
        event Action<ClanCraft> OnCraftStarted;
        event Action<int> OnCraftCancelled;
        event Action<int, ClanBuff> OnCraftCompleted;

        // History events
        event Action<List<ClanHistoryEntry>> OnClanHistoryReceived;

        // Level up events
        event Action<int, int, int> OnClanLeveledUp;

        // Faction events
        event Action<List<FactionData>> OnFactionInfoReceived;
        event Action<int, List<FactionLeaderboardEntry>> OnFactionLeaderboardReceived;

        // Clan list and info
        void RequestClansSettings();
        void RequestClansList();
        void RequestClanInfo(int clanId);
        void RequestMyClan();
        void SearchClans(string query, int limit = 20, int offset = 0);

        // Create, join, leave
        void CreateClan(string clanName, string shortName, FactionType factionType, int iconId);
        void JoinClan(int clanId);
        void LeaveClan();

        // Invites
        void SendInvite(int targetCharacterId);
        void AcceptInvite(int inviteId);
        void DeclineInvite(int inviteId);
        void RequestPendingInvites();

        // Member management
        void KickMember(int targetCharacterId);
        void PromoteMember(int targetCharacterId);
        void DemoteMember(int targetCharacterId);
        void TransferOwnership(int targetCharacterId);

        // Clan settings
        void UpdateClanSettings(string description, bool isOpen);
        void DisbandClan();

        // Donations
        void DonateGold(int amount);

        // Buffs and crafts
        void RequestClanBuffs();
        void StartClanCraft(int craftId);
        void CancelClanCraft(int craftDbId);
        void ClaimClanCraft(int craftDbId);

        // History
        void RequestClanHistory(int limit = 50, int offset = 0);

        // Level up
        void LevelUpClan();

        // Faction
        void RequestFactionInfo();
        void RequestFactionLeaderboard();
    }
}
