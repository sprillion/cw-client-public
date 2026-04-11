using System;

namespace infrastructure.services.clan
{
    [Flags]
    public enum ClanPermission
    {
        None = 0,
        InviteMembers = 1 << 0,
        KickMembers = 1 << 1,
        PromoteMembers = 1 << 2,
        DemoteMembers = 1 << 3,
        EditDescription = 1 << 4,
        EditOpenStatus = 1 << 5,
        StartCraft = 1 << 6,
        CancelCraft = 1 << 7,
        WithdrawEmeralds = 1 << 8,
        AcceptQuests = 1 << 9,
        TransferOwnership = 1 << 10,
        DisbandClan = 1 << 11,
    }

    public enum ClanRole
    {
        Member = 0,
        Officer = 1,
        CoLeader = 2,
        Leader = 3,
    }

    public enum ClanError : byte
    {
        None = 0,
        InsufficientLevel = 1,
        InsufficientGold = 2,
        InsufficientPermissions = 3,
        AlreadyInClan = 4,
        ClanFull = 5,
        ClanNotFound = 6,
        MemberNotFound = 7,
        InvalidName = 8,
        NameTaken = 9,
        ClanClosed = 10,
        OnCooldown = 11,
        InsufficientEmeralds = 12,
        CraftAlreadyInProgress = 13,
        QuestNotAvailable = 14,
        QuestAlreadyActive = 15,
        NotInClan = 16,
        CannotKickSelf = 17,
        CannotKickHigherRank = 18,
        CannotPromoteHigherOrEqual = 19,
        CannotDemoteHigherOrEqual = 20,
        AlreadyLowestRank = 21,
        AlreadyHighestRank = 22,
        InviteNotFound = 23,
        AlreadyInvited = 24,
    }

    public enum ClanHistoryType
    {
        ClanCreated = 0,
        MemberJoined = 10,
        MemberLeft = 11,
        MemberKicked = 12,
        MemberPromoted = 13,
        MemberDemoted = 14,
        GoldDonated = 20,
        ItemDonated = 21,
        EmeraldsDonated = 22,
        CraftStarted = 30,
        CraftCompleted = 31,
        CraftCancelled = 32,
        BuffActivated = 40,
        BuffExpired = 41,
        QuestAccepted = 50,
        QuestCompleted = 51,
        FactionPointsEarned = 61,
        FactionCompetitionWon = 62,
        FactionCompetitionLost = 63,
        SettingsChanged = 70,
        OwnershipTransferred = 71,
        ClanLeveledUp = 72,
    }
}
