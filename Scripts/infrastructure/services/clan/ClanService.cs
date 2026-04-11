using System;
using System.Collections.Generic;
using System.Linq;
using network;
using tools;
using UnityEngine;

namespace infrastructure.services.clan
{
    public class ClanService : IClanService
    {
        public enum FromClientMessage : byte
        {
            GetClansSettings = 0,
            GetClanInfo = 1,
            SearchClans = 2,
            CreateClan = 3,
            JoinClan = 4,
            LeaveClan = 5,
            SendInvite = 6,
            AcceptInvite = 7,
            DeclineInvite = 8,
            GetPendingInvites = 9,
            KickMember = 10,
            PromoteMember = 11,
            DemoteMember = 12,
            UpdatePermissions = 13,
            TransferOwnership = 14,
            UpdateClanSettings = 15,
            DisbandClan = 16,
            DonateGold = 17,
            DonateItems = 18,
            GetClanBuffs = 19,
            GetAvailableClanCrafts = 20,
            StartClanCraft = 21,
            CancelClanCraft = 22,
            ClaimClanCraft = 23,
            GetFactionInfo = 28,
            GetFactionLeaderboard = 29,
            GetClanHistory = 30,
            LevelUpClan = 31,
            GetMyClan = 32,
            GetClansList = 33,
        }

        public enum FromServerMessage : byte
        {
            ClansSettings,
            ClanInfo = 1,
            SearchResults = 2,
            CreationClanResult = 3,
            JoinResult = 4,
            LeaveResult = 5,
            InviteSent = 6,
            InviteReceived = 7,
            InviteResponse = 8,
            PendingInvites = 9,
            MemberKicked = 10,
            MemberPromoted = 11,
            MemberDemoted = 12,
            PermissionsUpdated = 13,
            OwnershipTransferred = 14,
            MemberJoined = 15,
            MemberLeft = 16,
            ClanDisbanded = 17,
            SettingsUpdated = 18,
            DonationResult = 19,
            CurrencyUpdated = 20,
            ClanBuffs = 21,
            BuffActivated = 22,
            BuffExpired = 23,
            AvailableClanCrafts = 24,
            ClanCraftStarted = 25,
            ClanCraftCancelled = 26,
            ClanCraftCompleted = 27,
            FactionInfo = 32,
            FactionLeaderboard = 33,
            FactionCompetitionResult = 34,
            ClanHistory = 35,
            ClanLeveledUp = 36,
            MyClan = 37,
            ClansList = 38,
            Error = 255,
        }

        private readonly INetworkManager _networkManager;

        public ClansSettings ClansSettings { get; private set; }
        public ClanData MyClan { get; private set; }
        public ClanMember MyMember { get; private set; }
        public bool IsInClan => MyClan != null;

        public List<ClanData> CachedClans { get; } = new List<ClanData>();
        public List<ClanInvite> PendingInvites { get; } = new List<ClanInvite>();

        // Events
        public event Action<ClanData> OnClanInfoReceived;
        public event Action<List<ClanData>> OnClansListReceived;
        public event Action<List<ClanData>> OnSearchResultsReceived;
        public event Action<bool, int, ClanData> OnClanCreated;
        public event Action<bool, int> OnClanJoined;
        public event Action<bool> OnClanLeft;
        public event Action<ClanError> OnClanError;
        public event Action<ClanData> OnMyClanUpdated;

        public event Action<int, string, int, ClanRole> OnMemberJoined;
        public event Action<int> OnMemberLeft;
        public event Action<int, bool> OnMemberKicked;
        public event Action<int, ClanRole, int> OnMemberPromoted;
        public event Action<int, ClanRole, int> OnMemberDemoted;
        public event Action<int, int> OnOwnershipTransferred;

        public event Action<string, bool> OnSettingsUpdated;
        public event Action<int> OnClanDisbanded;

        public event Action<bool, int> OnInviteSent;
        public event Action<ClanInvite> OnInviteReceived;
        public event Action<bool, int> OnInviteResponse;
        public event Action<List<ClanInvite>> OnPendingInvitesReceived;

        public event Action<bool, int, int> OnDonationResult;

        public event Action<List<ClanBuff>> OnClanBuffsReceived;
        public event Action<ClanCraft> OnCraftStarted;
        public event Action<int> OnCraftCancelled;
        public event Action<int, ClanBuff> OnCraftCompleted;

        public event Action<List<ClanHistoryEntry>> OnClanHistoryReceived;

        public event Action<int, int, int> OnClanLeveledUp;

        public event Action<List<FactionData>> OnFactionInfoReceived;
        public event Action<int, List<FactionLeaderboardEntry>> OnFactionLeaderboardReceived;

        public ClanService(INetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void ReceiveMessage(Message message)
        {
            var type = message.GetByteEnum<FromServerMessage>();

            switch (type)
            {
                case FromServerMessage.ClansSettings:
                    HandleClansSettings(message);
                    break;
                case FromServerMessage.ClanInfo:
                    HandleClanInfo(message);
                    break;
                case FromServerMessage.SearchResults:
                    HandleSearchResults(message);
                    break;
                case FromServerMessage.CreationClanResult:
                    HandleCreationResult(message);
                    break;
                case FromServerMessage.JoinResult:
                    HandleJoinResult(message);
                    break;
                case FromServerMessage.LeaveResult:
                    HandleLeaveResult(message);
                    break;
                case FromServerMessage.InviteSent:
                    HandleInviteSent(message);
                    break;
                case FromServerMessage.InviteReceived:
                    HandleInviteReceived(message);
                    break;
                case FromServerMessage.InviteResponse:
                    HandleInviteResponse(message);
                    break;
                case FromServerMessage.PendingInvites:
                    HandlePendingInvites(message);
                    break;
                case FromServerMessage.MemberKicked:
                    HandleMemberKicked(message);
                    break;
                case FromServerMessage.MemberPromoted:
                    HandleMemberPromoted(message);
                    break;
                case FromServerMessage.MemberDemoted:
                    HandleMemberDemoted(message);
                    break;
                case FromServerMessage.OwnershipTransferred:
                    HandleOwnershipTransferred(message);
                    break;
                case FromServerMessage.MemberJoined:
                    HandleMemberJoined(message);
                    break;
                case FromServerMessage.MemberLeft:
                    HandleMemberLeft(message);
                    break;
                case FromServerMessage.ClanDisbanded:
                    HandleClanDisbanded(message);
                    break;
                case FromServerMessage.SettingsUpdated:
                    HandleSettingsUpdated(message);
                    break;
                case FromServerMessage.DonationResult:
                    HandleDonationResult(message);
                    break;
                case FromServerMessage.ClanBuffs:
                    HandleClanBuffs(message);
                    break;
                case FromServerMessage.ClanCraftStarted:
                    HandleCraftStarted(message);
                    break;
                case FromServerMessage.ClanCraftCancelled:
                    HandleCraftCancelled(message);
                    break;
                case FromServerMessage.ClanCraftCompleted:
                    HandleCraftCompleted(message);
                    break;
                case FromServerMessage.ClanHistory:
                    HandleClanHistory(message);
                    break;
                case FromServerMessage.ClanLeveledUp:
                    HandleClanLeveledUp(message);
                    break;
                case FromServerMessage.MyClan:
                    HandleMyClan(message);
                    break;
                case FromServerMessage.FactionInfo:
                    HandleFactionInfo(message);
                    break;
                case FromServerMessage.FactionLeaderboard:
                    HandleFactionLeaderboard(message);
                    break;
                case FromServerMessage.ClansList:
                    HandleClansList(message);
                    break;
                case FromServerMessage.Error:
                    HandleError(message);
                    break;
            }
        }

        #region Request Methods

        public void RequestClansSettings()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.GetClansSettings.ToByte());
            _networkManager.SendMessage(message);
        }

        public void RequestClansList()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.GetClansList.ToByte());
            _networkManager.SendMessage(message);
        }

        public void RequestClanInfo(int clanId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.GetClanInfo.ToByte());
            message.AddInt(clanId);
            _networkManager.SendMessage(message);
        }

        public void RequestMyClan()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.GetMyClan.ToByte());
            _networkManager.SendMessage(message);
        }

        public void SearchClans(string query, int limit = 20, int offset = 0)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.SearchClans.ToByte());
            message.AddString(query);
            message.AddInt(limit);
            message.AddInt(offset);
            _networkManager.SendMessage(message);
        }

        public void CreateClan(string clanName, string shortName, FactionType factionType, int iconId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.CreateClan.ToByte());
            message.AddString(clanName);
            message.AddString(shortName);
            message.AddByte(factionType.ToByte());
            message.AddInt(iconId);

            _networkManager.SendMessage(message);
        }

        public void JoinClan(int clanId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.JoinClan.ToByte());
            message.AddInt(clanId);
            _networkManager.SendMessage(message);
        }

        public void LeaveClan()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.LeaveClan.ToByte());
            _networkManager.SendMessage(message);
        }

        public void SendInvite(int targetCharacterId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.SendInvite.ToByte());
            message.AddInt(targetCharacterId);
            _networkManager.SendMessage(message);
        }

        public void AcceptInvite(int inviteId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.AcceptInvite.ToByte());
            message.AddInt(inviteId);
            _networkManager.SendMessage(message);
        }

        public void DeclineInvite(int inviteId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.DeclineInvite.ToByte());
            message.AddInt(inviteId);
            _networkManager.SendMessage(message);
        }

        public void RequestPendingInvites()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.GetPendingInvites.ToByte());
            _networkManager.SendMessage(message);
        }

        public void KickMember(int targetCharacterId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.KickMember.ToByte());
            message.AddInt(targetCharacterId);
            _networkManager.SendMessage(message);
        }

        public void PromoteMember(int targetCharacterId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.PromoteMember.ToByte());
            message.AddInt(targetCharacterId);
            _networkManager.SendMessage(message);
        }

        public void DemoteMember(int targetCharacterId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.DemoteMember.ToByte());
            message.AddInt(targetCharacterId);
            _networkManager.SendMessage(message);
        }

        public void TransferOwnership(int targetCharacterId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.TransferOwnership.ToByte());
            message.AddInt(targetCharacterId);
            _networkManager.SendMessage(message);
        }

        public void UpdateClanSettings(string description, bool isOpen)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.UpdateClanSettings.ToByte());
            message.AddString(description);
            message.AddBool(isOpen);
            _networkManager.SendMessage(message);
        }

        public void DisbandClan()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.DisbandClan.ToByte());
            _networkManager.SendMessage(message);
        }

        public void DonateGold(int amount)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.DonateGold.ToByte());
            message.AddInt(amount);
            _networkManager.SendMessage(message);
        }

        public void RequestClanBuffs()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.GetClanBuffs.ToByte());
            _networkManager.SendMessage(message);
        }

        public void StartClanCraft(int craftId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.StartClanCraft.ToByte());
            message.AddInt(craftId);
            _networkManager.SendMessage(message);
        }

        public void CancelClanCraft(int craftDbId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.CancelClanCraft.ToByte());
            message.AddInt(craftDbId);
            _networkManager.SendMessage(message);
        }

        public void ClaimClanCraft(int craftDbId)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.ClaimClanCraft.ToByte());
            message.AddInt(craftDbId);
            _networkManager.SendMessage(message);
        }

        public void RequestClanHistory(int limit = 50, int offset = 0)
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.GetClanHistory.ToByte());
            message.AddInt(limit);
            message.AddInt(offset);
            _networkManager.SendMessage(message);
        }

        public void LevelUpClan()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.LevelUpClan.ToByte());
            _networkManager.SendMessage(message);
        }

        public void RequestFactionInfo()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.GetFactionInfo.ToByte());
            _networkManager.SendMessage(message);
        }

        public void RequestFactionLeaderboard()
        {
            var message = new Message(MessageType.Clan);
            message.AddByte(FromClientMessage.GetFactionLeaderboard.ToByte());
            _networkManager.SendMessage(message);
        }

        #endregion

        #region Message Handlers

        private void HandleClansSettings(Message message)
        {
            ClansSettings = new ClansSettings()
            {
                MinLevelToCreate = message.GetInt(),
                CreationCostGold = message.GetInt(),
                MinNameLength = message.GetInt(),
                MaxNameLength = message.GetInt(),
                MinShortNameLength = message.GetInt(),
                MaxShortNameLength = message.GetInt(),
            };
        }

        private void HandleClansList(Message message)
        {
            var count = message.GetInt();
            var clans = new List<ClanData>();

            for (int i = 0; i < count; i++)
            {
                clans.Add(ReadClanPreview(message));
            }

            CachedClans.Clear();
            CachedClans.AddRange(clans);
            OnClansListReceived?.Invoke(clans);
        }

        private void HandleClanInfo(Message message)
        {
            var success = message.GetBool();
            if (!success)
            {
                OnClanInfoReceived?.Invoke(null);
                return;
            }

            var clan = ReadClanFull(message);
            OnClanInfoReceived?.Invoke(clan);
        }

        private void HandleSearchResults(Message message)
        {
            var count = message.GetInt();
            var clans = new List<ClanData>();

            for (int i = 0; i < count; i++)
            {
                clans.Add(ReadClanPreview(message));
            }

            OnSearchResultsReceived?.Invoke(clans);
        }

        private void HandleCreationResult(Message message)
        {
            var success = message.GetBool();
            var clanId = message.GetInt();
            ClanData clan = null;

            if (success)
            {
                clan = ReadClanFull(message);
                MyClan = clan;

                MyMember = clan.Members.FirstOrDefault();
            }

            OnClanCreated?.Invoke(success, clanId, clan);
        }

        private void HandleJoinResult(Message message)
        {
            var success = message.GetBool();
            var clanId = message.GetInt();

            if (success)
            {
                // Request full clan info after joining
                RequestMyClan();
            }

            OnClanJoined?.Invoke(success, clanId);
        }

        private void HandleLeaveResult(Message message)
        {
            var success = message.GetBool();

            if (success)
            {
                MyClan = null;
                MyMember = null;
            }

            OnClanLeft?.Invoke(success);
        }

        private void HandleInviteSent(Message message)
        {
            var success = message.GetBool();
            var targetId = message.GetInt();
            OnInviteSent?.Invoke(success, targetId);
        }

        private void HandleInviteReceived(Message message)
        {
            var invite = new ClanInvite
            {
                Id = message.GetInt(),
                ClanId = message.GetInt(),
                ClanName = message.GetString(),
                ShortName = message.GetString(),
                InviterCharacterId = message.GetInt(),
                InviterName = message.GetString()
            };

            PendingInvites.Add(invite);
            OnInviteReceived?.Invoke(invite);
        }

        private void HandleInviteResponse(Message message)
        {
            var accepted = message.GetBool();
            var clanId = message.GetInt();

            if (accepted)
            {
                RequestMyClan();
            }

            OnInviteResponse?.Invoke(accepted, clanId);
        }

        private void HandlePendingInvites(Message message)
        {
            var count = message.GetInt();
            var invites = new List<ClanInvite>();

            for (int i = 0; i < count; i++)
            {
                var invite = new ClanInvite
                {
                    Id = message.GetInt(),
                    ClanId = message.GetInt(),
                    ClanName = message.GetString(),
                    InviterCharacterId = message.GetInt(),
                    ExpirationDate = DateTimeOffset.FromUnixTimeSeconds(message.GetLong()).DateTime
                };
                invites.Add(invite);
            }

            PendingInvites.Clear();
            PendingInvites.AddRange(invites);
            OnPendingInvitesReceived?.Invoke(invites);
        }

        private void HandleMemberKicked(Message message)
        {
            var characterId = message.GetInt();
            var isTarget = message.GetBool();

            if (isTarget)
            {
                // We were kicked
                MyClan = null;
                MyMember = null;
            }
            else if (MyClan != null)
            {
                // Remove from members list
                MyClan.Members.RemoveAll(m => m.CharacterId == characterId);
                MyClan.MemberCount--;
            }

            OnMemberKicked?.Invoke(characterId, isTarget);
        }

        private void HandleMemberPromoted(Message message)
        {
            var characterId = message.GetInt();
            var newRole = (ClanRole)message.GetInt();
            var newPermissions = message.GetInt();

            if (MyClan != null)
            {
                var member = MyClan.Members.Find(m => m.CharacterId == characterId);
                if (member != null)
                {
                    member.Role = newRole;
                    member.Permissions = newPermissions;

                    if (MyMember?.CharacterId == characterId)
                    {
                        MyMember = member;
                    }
                }
            }

            OnMemberPromoted?.Invoke(characterId, newRole, newPermissions);
        }

        private void HandleMemberDemoted(Message message)
        {
            var characterId = message.GetInt();
            var newRole = (ClanRole)message.GetInt();
            var newPermissions = message.GetInt();

            if (MyClan != null)
            {
                var member = MyClan.Members.Find(m => m.CharacterId == characterId);
                if (member != null)
                {
                    member.Role = newRole;
                    member.Permissions = newPermissions;

                    if (MyMember?.CharacterId == characterId)
                    {
                        MyMember = member;
                    }
                }
            }

            OnMemberDemoted?.Invoke(characterId, newRole, newPermissions);
        }

        private void HandleOwnershipTransferred(Message message)
        {
            var oldOwnerId = message.GetInt();
            var newOwnerId = message.GetInt();

            if (MyClan != null)
            {
                MyClan.OwnerId = newOwnerId;

                // Update roles
                var oldOwner = MyClan.Members.Find(m => m.CharacterId == oldOwnerId);
                var newOwner = MyClan.Members.Find(m => m.CharacterId == newOwnerId);

                if (oldOwner != null)
                {
                    oldOwner.Role = ClanRole.CoLeader;
                }

                if (newOwner != null)
                {
                    newOwner.Role = ClanRole.Leader;
                }

                if (MyMember?.CharacterId == oldOwnerId)
                {
                    MyMember = oldOwner;
                }
                else if (MyMember?.CharacterId == newOwnerId)
                {
                    MyMember = newOwner;
                }
            }

            OnOwnershipTransferred?.Invoke(oldOwnerId, newOwnerId);
        }

        private void HandleMemberJoined(Message message)
        {
            var characterId = message.GetInt();
            var characterName = message.GetString();
            var characterLevel = message.GetInt();
            var role = (ClanRole)message.GetInt();

            if (MyClan != null)
            {
                var newMember = new ClanMember
                {
                    CharacterId = characterId,
                    CharacterName = characterName,
                    CharacterLevel = characterLevel,
                    Role = role,
                    IsOnline = true,
                    JoinDate = DateTime.UtcNow
                };

                MyClan.Members.Add(newMember);
                MyClan.MemberCount++;
            }

            OnMemberJoined?.Invoke(characterId, characterName, characterLevel, role);
        }

        private void HandleMemberLeft(Message message)
        {
            var characterId = message.GetInt();

            if (MyClan != null)
            {
                MyClan.Members.RemoveAll(m => m.CharacterId == characterId);
                MyClan.MemberCount--;
            }

            OnMemberLeft?.Invoke(characterId);
        }

        private void HandleClanDisbanded(Message message)
        {
            var clanId = message.GetInt();

            if (MyClan?.Id == clanId)
            {
                MyClan = null;
                MyMember = null;
            }

            OnClanDisbanded?.Invoke(clanId);
        }

        private void HandleSettingsUpdated(Message message)
        {
            var description = message.GetString();
            var isOpen = message.GetBool();

            if (MyClan != null)
            {
                MyClan.Description = description;
                MyClan.IsOpen = isOpen;
            }

            OnSettingsUpdated?.Invoke(description, isOpen);
        }

        private void HandleDonationResult(Message message)
        {
            var success = message.GetBool();
            var amount = message.GetInt();
            var totalDonated = message.GetInt();

            if (success && MyMember != null)
            {
                // MyMember.TotalDonatedGold = totalDonated;
            }

            OnDonationResult?.Invoke(success, amount, totalDonated);
        }

        private void HandleClanBuffs(Message message)
        {
            var count = message.GetInt();
            var buffs = new List<ClanBuff>();

            for (int i = 0; i < count; i++)
            {
                var buff = new ClanBuff
                {
                    Id = message.GetInt(),
                    BuffId = message.GetInt(),
                    Duration = message.GetFloat(),
                    StartTime = DateTimeOffset.FromUnixTimeSeconds(message.GetLong()).DateTime
                };
                buffs.Add(buff);
            }

            OnClanBuffsReceived?.Invoke(buffs);
        }

        private void HandleCraftStarted(Message message)
        {
            var craft = new ClanCraft
            {
                Id = message.GetInt(),
                CraftId = message.GetInt(),
                Duration = message.GetFloat(),
                StartTime = DateTimeOffset.FromUnixTimeSeconds(message.GetLong()).DateTime
            };

            OnCraftStarted?.Invoke(craft);
        }

        private void HandleCraftCancelled(Message message)
        {
            var craftDbId = message.GetInt();
            OnCraftCancelled?.Invoke(craftDbId);
        }

        private void HandleCraftCompleted(Message message)
        {
            var craftDbId = message.GetInt();
            var buff = new ClanBuff
            {
                Id = message.GetInt(),
                BuffId = message.GetInt(),
                Duration = message.GetFloat(),
                StartTime = DateTimeOffset.FromUnixTimeSeconds(message.GetLong()).DateTime
            };

            OnCraftCompleted?.Invoke(craftDbId, buff);
        }

        private void HandleClanHistory(Message message)
        {
            var count = message.GetInt();
            var history = new List<ClanHistoryEntry>();

            for (int i = 0; i < count; i++)
            {
                var entry = new ClanHistoryEntry
                {
                    Id = message.GetInt(),
                    HistoryType = (ClanHistoryType)message.GetInt(),
                    Value = message.GetInt(),
                    CharacterPerformer = message.GetInt(),
                    CharacterTarget = message.GetInt(),
                    CreationDate = DateTimeOffset.FromUnixTimeSeconds(message.GetLong()).DateTime
                };
                history.Add(entry);
            }

            OnClanHistoryReceived?.Invoke(history);
        }

        private void HandleClanLeveledUp(Message message)
        {
            var newLevel = message.GetInt();
            var newMaxMembers = message.GetInt();
            var emeralds = message.GetInt();

            if (MyClan != null)
            {
                MyClan.Level = newLevel;
                MyClan.MaxMembers = newMaxMembers;
                MyClan.Emeralds = emeralds;
            }

            OnClanLeveledUp?.Invoke(newLevel, newMaxMembers, emeralds);
        }

        private void HandleMyClan(Message message)
        {
            var hasClan = message.GetBool();

            if (!hasClan)
            {
                MyClan = null;
                MyMember = null;
                OnMyClanUpdated?.Invoke(null);
                return;
            }

            MyClan = ReadClanFull(message);

            var myRole = message.GetInt();
            var myPermissions = message.GetInt();

            // Find self in members list
            MyMember = MyClan.Members.Find(m => m.Role == (ClanRole)myRole);
            if (MyMember != null)
            {
                MyMember.Permissions = myPermissions;
            }

            OnMyClanUpdated?.Invoke(MyClan);
        }

        private void HandleFactionInfo(Message message)
        {
            var count = message.GetInt();
            var factions = new List<FactionData>();

            for (int i = 0; i < count; i++)
            {
                var faction = new FactionData
                {
                    Id = message.GetInt(),
                    Name = message.GetString(),
                    Description = message.GetString()
                };
                factions.Add(faction);
            }

            OnFactionInfoReceived?.Invoke(factions);
        }

        private void HandleFactionLeaderboard(Message message)
        {
            var weekNumber = message.GetInt();
            var count = message.GetInt();
            var entries = new List<FactionLeaderboardEntry>();

            for (int i = 0; i < count; i++)
            {
                var entry = new FactionLeaderboardEntry
                {
                    FactionId = message.GetInt(),
                    TotalPoints = message.GetLong(),
                    ParticipatingClans = message.GetInt()
                };
                entries.Add(entry);
            }

            OnFactionLeaderboardReceived?.Invoke(weekNumber, entries);
        }

        private void HandleError(Message message)
        {
            var error = (ClanError)message.GetByte();
            OnClanError?.Invoke(error);
        }

        #endregion

        #region Data Reading Helpers

        private ClanData ReadClanPreview(Message message)
        {
            return new ClanData
            {
                Id = message.GetInt(),
                ClanName = message.GetString(),
                ShortName = message.GetString(),
                Level = message.GetInt(),
                MemberCount = message.GetInt(),
                MaxMembers = message.GetInt(),
                IconId = message.GetInt(),
                IsOpen = message.GetBool(),
                FactionType = message.GetByteEnum<FactionType>()
            };
        }

        private ClanData ReadClanFull(Message message)
        {
            var clan = ReadClanPreview(message);

            clan.Description = message.GetString();
            clan.OwnerId = message.GetInt();
            clan.Emeralds = message.GetInt();
            clan.CreationDate = DateTimeOffset.FromUnixTimeSeconds(message.GetLong()).DateTime;

            var memberCount = message.GetInt();
            for (int i = 0; i < memberCount; i++)
            {
                var member = new ClanMember
                {
                    CharacterId = message.GetInt(),
                    CharacterName = message.GetString(),
                    CharacterLevel = message.GetInt(),
                    Role = (ClanRole)message.GetInt(),
                    Permissions = message.GetInt(),
                    IsOnline = message.GetBool(),
                    JoinDate = DateTimeOffset.FromUnixTimeSeconds(message.GetLong()).DateTime
                };
                clan.Members.Add(member);
            }

            return clan;
        }

        #endregion
    }
}