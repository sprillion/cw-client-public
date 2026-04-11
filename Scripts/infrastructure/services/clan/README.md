# ClanService Implementation

## Overview

This is a complete client-side implementation of the Clan system that matches the server's protocol exactly.

## Files Created

1. **ClanEnums.cs** - All clan-related enumerations
   - `ClanPermission` - Permission flags (bitwise)
   - `ClanRole` - Member roles (Member, Officer, CoLeader, Leader)
   - `ClanError` - Error codes matching server
   - `ClanHistoryType` - History event types

2. **ClanData.cs** - Data classes
   - `ClanData` - Main clan information
   - `ClanMember` - Member information with permission checking
   - `ClanInvite` - Invite data
   - `ClanBuff` - Active buff with time calculations
   - `ClanCraft` - Ongoing craft with progress tracking
   - `ClanQuest` - Quest data with progress dictionary
   - `ClanHistoryEntry` - History entry
   - `FactionData` - Faction information
   - `FactionLeaderboardEntry` - Leaderboard entry

3. **IClanService.cs** - Service interface
   - All public methods for clan operations
   - Events for all server responses
   - Properties for current player's clan state

4. **ClanService.cs** - Full implementation
   - Complete message protocol implementation
   - Big-endian serialization matching server
   - Event-driven architecture for UI updates
   - Local state management for player's clan

## Integration with Zenject

To register the service, add to your installer:

```csharp
Container.BindInterfacesTo<ClanService>().AsSingle();
```

## Network Integration

The service requires `INetworkManager` to be injected. The network manager should route `MessageType.Clan` messages to this service via the `IReceiver` interface.

## Usage Examples

### Basic Operations

```csharp
public class ClanUI
{
    private readonly IClanService _clanService;

    public ClanUI(IClanService clanService)
    {
        _clanService = clanService;

        // Subscribe to events
        _clanService.OnMyClanUpdated += HandleMyClanUpdated;
        _clanService.OnClanError += HandleClanError;
    }

    public void Initialize()
    {
        // Request player's clan on startup
        _clanService.RequestMyClan();
    }

    private void HandleMyClanUpdated(ClanData clan)
    {
        if (clan == null)
        {
            // Player is not in a clan
            ShowNoClanUI();
        }
        else
        {
            // Display clan information
            DisplayClan(clan);

            // Check permissions
            if (_clanService.MyMember.HasPermission(ClanPermission.InviteMembers))
            {
                EnableInviteButton();
            }
        }
    }

    private void HandleClanError(ClanError error)
    {
        // Display error to user
        ShowError(error);
    }
}
```

### Creating a Clan

```csharp
public void OnCreateClanClicked()
{
    _clanService.OnClanCreated += HandleClanCreated;
    _clanService.CreateClan("MyClan", "MC", factionId: 1);
}

private void HandleClanCreated(bool success, int clanId, ClanData clan)
{
    _clanService.OnClanCreated -= HandleClanCreated;

    if (success)
    {
        // Clan created successfully
        ShowClanUI(clan);
    }
}
```

### Inviting Players

```csharp
public void InvitePlayer(int targetCharacterId)
{
    if (!_clanService.IsInClan)
    {
        ShowError("You must be in a clan to invite players");
        return;
    }

    if (!_clanService.MyMember.HasPermission(ClanPermission.InviteMembers))
    {
        ShowError("You don't have permission to invite members");
        return;
    }

    _clanService.OnInviteSent += HandleInviteSent;
    _clanService.SendInvite(targetCharacterId);
}

private void HandleInviteSent(bool success, int targetId)
{
    _clanService.OnInviteSent -= HandleInviteSent;

    if (success)
    {
        ShowMessage("Invite sent successfully");
    }
}
```

### Monitoring Clan Changes

```csharp
private void SubscribeToEvents()
{
    _clanService.OnMemberJoined += (id, name, level, role) =>
    {
        ShowNotification($"{name} joined the clan!");
        RefreshMemberList();
    };

    _clanService.OnMemberLeft += (id) =>
    {
        ShowNotification("A member left the clan");
        RefreshMemberList();
    };

    _clanService.OnCraftCompleted += (craftId, buff) =>
    {
        ShowNotification("Clan craft completed!");
        ShowNewBuff(buff);
    };

    _clanService.OnClanLeveledUp += (level, maxMembers, emeralds) =>
    {
        ShowNotification($"Clan leveled up to {level}!");
        UpdateClanInfo();
    };
}
```

## Message Protocol

The service uses two-level message routing:
1. First byte after MessageType is the submessage type (FromClientMessage/FromServerMessage)
2. All integers, longs, floats use big-endian byte order (network byte order)
3. Strings use UTF-8 encoding with length prefix (short for <32KB, int for larger)
4. Timestamps are Unix timestamps (seconds since epoch) as long values

## Error Handling

All operations that can fail will trigger the `OnClanError` event with a specific error code. UI should subscribe to this event and display appropriate messages to the user.

Common error codes:
- `InsufficientPermissions` - User lacks required permission
- `AlreadyInClan` - Cannot join/create while in a clan
- `NotInClan` - Operation requires clan membership
- `ClanFull` - Clan at maximum capacity
- `InsufficientGold/Emeralds` - Not enough currency

## State Management

The service maintains:
- `MyClan` - Current player's clan (null if not in clan)
- `MyMember` - Player's member data (null if not in clan)
- `CachedClans` - Recently fetched clan list (e.g., from search)
- `PendingInvites` - List of pending invites for the player

## Server Alignment

All message structures, field orders, and data types match the server implementation exactly:
- Server: `C:\UnityProjects\CubeWorldServer\Assets\Scripts\infrastructure\services\clans\ClanService.cs`
- Enums match server definitions
- Message serialization order is identical
- All 33 client message types implemented
- All 38 server message types handled

## Future Considerations

Not yet implemented (server-side incomplete):
- UpdatePermissions (message type 13)
- DonateItems (message type 18)
- GetAvailableClanCrafts (message type 20)
- Quest progress updates (automatic)
- Quest reward claiming

These can be added when server implementations are completed.
