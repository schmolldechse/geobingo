using System;
using System.Collections.Generic;
using System.Linq;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.Api.Lobbies.Runtime;

public sealed record LobbyCreation
{
    public LobbyCreation(
        Guid hostUserId,
        string hostHandle,
        string hostDisplayName,
        string? hostAvatarUrl,
        LobbySettings settings)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (hostUserId == Guid.Empty)
        {
            errors["hostUserId"] = ["The host user identifier cannot be empty."];
        }

        if (string.IsNullOrWhiteSpace(hostHandle)
            || hostHandle.Length > 64
            || !string.Equals(hostHandle, hostHandle.Trim(), StringComparison.Ordinal))
        {
            errors["hostHandle"] = ["The host handle must be trimmed and contain between 1 and 64 characters."];
        }

        if (string.IsNullOrWhiteSpace(hostDisplayName)
            || hostDisplayName.Length > 64
            || !string.Equals(
                hostDisplayName,
                hostDisplayName.Trim(),
                StringComparison.Ordinal))
        {
            errors["hostDisplayName"] = ["The host display name must be trimmed and contain between 1 and 64 characters."];
        }

        if (settings is null)
        {
            errors["settings"] = ["Lobby settings are required."];
        }
        else if (settings.MaxPlayers is < 2 or > 32)
        {
            errors["settings.maxPlayers"] = ["The maximum player count must be between 2 and 32."];
        }

        if (errors.Count > 0)
        {
            throw new LobbyRuntimeException(
                ErrorCode.VALIDATION_FAILED,
                "The lobby creation request is invalid.",
                errors);
        }

        HostUserId = hostUserId;
        HostHandle = hostHandle;
        HostDisplayName = hostDisplayName;
        HostAvatarUrl = NormalizeAvatarUrl(hostAvatarUrl);
        Settings = CopySettings(settings!);
    }

    public Guid HostUserId { get; }

    public string HostHandle { get; }

    public string HostDisplayName { get; }

    public string? HostAvatarUrl { get; }

    public LobbySettings Settings { get; }

    private static LobbySettings CopySettings(LobbySettings settings) => new()
    {
        MaxPlayers = settings.MaxPlayers
    };

    private static string? NormalizeAvatarUrl(string? avatarUrl) =>
        string.IsNullOrWhiteSpace(avatarUrl)
            ? null
            : avatarUrl.Trim();
}

internal sealed record LobbyActor(
    Guid? UserId,
    string? ConnectionId,
    bool IsSystem)
{
    public static LobbyActor System { get; } = new(null, null, true);
}

internal enum LobbyDeadlineKind
{
    PREPARATION,
    MODE,
    DISCONNECT_GRACE
}

internal readonly record struct LobbyDeadlineKey(
    LobbyDeadlineKind Kind,
    Guid? RoundId,
    Guid? UserId);

internal sealed class LobbyRuntimeException : Exception
{
    public LobbyRuntimeException(
        ErrorCode code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Code = code;
        Errors = errors;
    }

    public ErrorCode Code { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}

internal sealed class LobbyMemberState
{
    public required Guid UserId { get; init; }

    public required string Handle { get; init; }

    public required string DisplayName { get; init; }

    public string? AvatarUrl { get; init; }

    public required int JoinOrder { get; init; }

    public HashSet<string> ConnectionIds { get; } =
        new(StringComparer.Ordinal);

    public DateTimeOffset? DisconnectGraceEndsAt { get; private set; }

    public bool IsConnected => ConnectionIds.Count > 0;

    public void AttachConnection(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ConnectionIds.Add(connectionId);
        DisconnectGraceEndsAt = null;
    }

    public bool DetachConnection(
        string connectionId,
        DateTimeOffset now,
        TimeSpan gracePeriod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        if (gracePeriod <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gracePeriod),
                "The disconnect grace period must be positive.");
        }

        if (!ConnectionIds.Remove(connectionId))
        {
            return false;
        }

        if (ConnectionIds.Count == 0)
        {
            DisconnectGraceEndsAt = now.Add(gracePeriod);
        }

        return true;
    }

    public bool HasExpiredDisconnectGrace(DateTimeOffset now) =>
        ConnectionIds.Count == 0
        && DisconnectGraceEndsAt is DateTimeOffset deadline
        && now >= deadline;

    public bool MatchesDisconnectDeadline(DateTimeOffset deadline) =>
        ConnectionIds.Count == 0
        && DisconnectGraceEndsAt == deadline;

    public void ClearDisconnectGrace() =>
        DisconnectGraceEndsAt = null;
}

internal sealed record LobbyRuntimeSummary(
    Guid LobbyId,
    string Code,
    LobbyStatus Status,
    Guid HostUserId,
    int PlayerCount,
    int MaxPlayers,
    string SelectedModeKey,
    Version SelectedModeVersion,
    long StateVersion,
    bool IsClosing,
    bool IsClosed,
    LobbyEndedReason? CloseReason);

internal sealed class LobbyRuntimeState
{
    private LobbySettings settings;

    internal static TimeSpan PreparationDuration { get; } =
        TimeSpan.FromSeconds(5);

    public LobbyRuntimeState(
        Guid lobbyId,
        string code,
        LobbyCreation creation,
        string selectedModeKey,
        Version selectedModeVersion,
        IGameModeLobbyState modeLobbyState)
    {
        if (lobbyId == Guid.Empty)
        {
            throw new ArgumentException(
                "The lobby identifier cannot be empty.",
                nameof(lobbyId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(creation);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedModeKey);
        ArgumentNullException.ThrowIfNull(selectedModeVersion);
        ArgumentNullException.ThrowIfNull(modeLobbyState);

        LobbyId = lobbyId;
        Code = code;
        Status = LobbyStatus.WAITING;
        HostUserId = creation.HostUserId;
        settings = CopySettings(creation.Settings);
        SelectedModeKey = selectedModeKey;
        SelectedModeVersion = selectedModeVersion;
        ModeLobbyState = modeLobbyState;
        StateVersion = 1;

        Members.Add(
            creation.HostUserId,
            new LobbyMemberState
            {
                UserId = creation.HostUserId,
                Handle = creation.HostHandle,
                DisplayName = creation.HostDisplayName,
                AvatarUrl = creation.HostAvatarUrl,
                JoinOrder = 1
            });
    }

    public Guid LobbyId { get; }

    public string Code { get; }

    public LobbyStatus Status { get; private set; }

    public Guid HostUserId { get; internal set; }

    public LobbySettings Settings => CopySettings(settings);

    public string SelectedModeKey { get; internal set; }

    public Version SelectedModeVersion { get; internal set; }

    public IGameModeLobbyState ModeLobbyState { get; internal set; }

    public Dictionary<Guid, LobbyMemberState> Members { get; } = [];

    public HashSet<Guid> BannedUserIds { get; } = [];

    public long StateVersion { get; private set; }

    public int NextJoinOrder { get; private set; } = 2;

    public int? CurrentRoundNumber { get; internal set; }

    public IGameModeRoundState? CurrentRound { get; internal set; }

    public List<CompletedRoundSummary>
        CompletedRoundSummaries
    { get; } = [];

    public List<CompletedRoundResults>
        CompletedRoundResults
    { get; } = [];

    public List<CumulativePlayerResult>
        CumulativeResults
    { get; } = [];

    public bool HasNonDefaultModeConfiguration
    {
        get;
        internal set;
    }

    public DateTimeOffset? PreparationDeadline { get; internal set; }

    public DateTimeOffset? ModeDeadline { get; internal set; }

    public Guid? ModeDeadlineRoundId { get; internal set; }

    public bool IsClosing { get; private set; }

    public bool IsClosed { get; private set; }

    public LobbyEndedReason? CloseReason { get; private set; }

    public bool CanCreateMembership(Guid userId) =>
        userId != Guid.Empty
        && !IsClosing
        && !IsClosed
        && Status == LobbyStatus.WAITING
        && !Members.ContainsKey(userId)
        && !BannedUserIds.Contains(userId)
        && Members.Count < settings.MaxPlayers;

    public bool CanReconnect(Guid userId) =>
        userId != Guid.Empty
        && !IsClosing
        && !IsClosed
        && Members.ContainsKey(userId);

    public bool TryGetMember(
        Guid userId,
        out LobbyMemberState? member)
    {
        if (userId == Guid.Empty)
        {
            member = null;
            return false;
        }

        return Members.TryGetValue(userId, out member);
    }

    public LobbyMemberState AddMember(
        Guid userId,
        string handle,
        string displayName,
        string? avatarUrl)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "The user identifier cannot be empty.",
                nameof(userId));
        }

        ValidateMemberName(handle, nameof(handle));
        ValidateMemberName(displayName, nameof(displayName));

        if (Members.ContainsKey(userId))
        {
            throw new InvalidOperationException(
                "The user is already a lobby member.");
        }

        var member = new LobbyMemberState
        {
            UserId = userId,
            Handle = handle,
            DisplayName = displayName,
            AvatarUrl = avatarUrl,
            JoinOrder = AllocateJoinOrder()
        };

        Members.Add(userId, member);
        return member;
    }

    public bool RemoveMember(Guid userId) =>
        userId != Guid.Empty
        && Members.Remove(userId);

    public Guid? FindHostSuccessorUserId() =>
        Members.Values
            .OrderBy(member => member.JoinOrder)
            .Select(member => (Guid?)member.UserId)
            .FirstOrDefault();

    public IReadOnlyList<Guid> FindExpiredDisconnectedMemberIds(
        DateTimeOffset now) =>
        Members.Values
            .Where(member => member.HasExpiredDisconnectGrace(now))
            .OrderBy(member => member.JoinOrder)
            .Select(member => member.UserId)
            .ToArray();

    public bool CanReplaceSettings(LobbySettings replacement) =>
        replacement is not null
        && !IsClosing
        && !IsClosed
        && Status == LobbyStatus.WAITING
        && replacement.MaxPlayers is >= 2 and <= 32
        && replacement.MaxPlayers >= Members.Count;

    public void ReplaceSettings(LobbySettings replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        if (!CanReplaceSettings(replacement))
        {
            throw new LobbyRuntimeException(
                ErrorCode.VALIDATION_FAILED,
                "The lobby settings cannot be applied in the current state.");
        }

        settings = CopySettings(replacement);
    }

    public int AllocateJoinOrder()
    {
        var joinOrder = NextJoinOrder;
        NextJoinOrder = checked(NextJoinOrder + 1);
        return joinOrder;
    }

    public int GetNextRoundNumber()
    {
        if (Status != LobbyStatus.WAITING
            || CurrentRound is not null
            || CurrentRoundNumber is not null)
        {
            throw new InvalidOperationException(
                "A round number can only be allocated for an idle waiting lobby.");
        }

        var latestCompletedRoundNumber = CompletedRoundSummaries
            .Select(summary => summary.RoundNumber)
            .DefaultIfEmpty(0)
            .Max();
        return checked(latestCompletedRoundNumber + 1);
    }

    public void BeginPreparation(
        IGameModeRoundState round,
        int roundNumber,
        DateTimeOffset deadline)
    {
        ArgumentNullException.ThrowIfNull(round);

        if (round.RoundId == Guid.Empty)
        {
            throw new ArgumentException(
                "The round identifier cannot be empty.",
                nameof(round));
        }

        if (roundNumber <= 0 || round.RoundNumber != roundNumber)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roundNumber),
                "The round number must be positive and match the round snapshot.");
        }

        if (!string.Equals(
                round.ModeKey,
                SelectedModeKey,
                StringComparison.Ordinal)
            || round.ModeVersion != SelectedModeVersion)
        {
            throw new InvalidOperationException(
                "The round snapshot must match the selected game mode and version.");
        }

        if (deadline == default
            || deadline
            != round.CreatedAt.Add(
                PreparationDuration))
        {
            throw new ArgumentException(
                "The preparation deadline must be exactly five seconds after preparation starts.",
                nameof(deadline));
        }

        if (Status != LobbyStatus.WAITING
            || CurrentRound is not null
            || CurrentRoundNumber is not null)
        {
            throw new InvalidOperationException(
                "Only an idle waiting lobby may begin preparation.");
        }

        CurrentRound = round;
        CurrentRoundNumber = roundNumber;
        PreparationDeadline = deadline;
        ModeDeadline = null;
        ModeDeadlineRoundId = null;
        TransitionTo(LobbyStatus.PREPARING);
    }

    public Guid DiscardPreparingRound()
    {
        if (Status != LobbyStatus.PREPARING
            || CurrentRound is not IGameModeRoundState round
            || CurrentRoundNumber is not int roundNumber
            || round.RoundNumber != roundNumber)
        {
            throw new InvalidOperationException(
                "Only a coherent preparing round may be discarded.");
        }

        var roundId = round.RoundId;
        CurrentRound = null;
        CurrentRoundNumber = null;
        PreparationDeadline = null;
        ModeDeadline = null;
        ModeDeadlineRoundId = null;
        TransitionTo(LobbyStatus.WAITING);
        return roundId;
    }

    public void IncrementStateVersion() =>
        StateVersion = checked(StateVersion + 1);

    public void TransitionTo(LobbyStatus nextStatus)
    {
        LobbyStateMachine.EnsureTransitionAllowed(Status, nextStatus);
        Status = nextStatus;
    }

    public void BeginClosing(LobbyEndedReason reason)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("A closed lobby cannot begin closing again.");
        }

        if (IsClosing)
        {
            return;
        }

        IsClosing = true;
        CloseReason = reason;
    }

    public void MarkClosed()
    {
        if (!IsClosing || CloseReason is null)
        {
            throw new InvalidOperationException("A lobby must begin closing before it can be marked closed.");
        }

        IsClosed = true;
    }

    public void ClearSensitiveState()
    {
        Members.Clear();
        BannedUserIds.Clear();
        CompletedRoundSummaries.Clear();
        CompletedRoundResults.Clear();
        CumulativeResults.Clear();
        CurrentRound = null;
        CurrentRoundNumber = null;
        PreparationDeadline = null;
        ModeDeadline = null;
        ModeDeadlineRoundId = null;
        ModeLobbyState = ClearedGameModeLobbyState.Instance;
        HasNonDefaultModeConfiguration = false;
    }

    public LobbyRuntimeSummary CreateSummary() =>
        new(
            LobbyId,
            Code,
            Status,
            HostUserId,
            Members.Count,
            settings.MaxPlayers,
            SelectedModeKey,
            SelectedModeVersion,
            StateVersion,
            IsClosing,
            IsClosed,
            CloseReason);

    private static LobbySettings CopySettings(LobbySettings source) =>
        new()
        {
            MaxPlayers = source.MaxPlayers
        };

    private static void ValidateMemberName(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > 64
            || !string.Equals(
                value,
                value.Trim(),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The member name must be trimmed and contain between 1 and 64 characters.",
                parameterName);
        }
    }

    private sealed class ClearedGameModeLobbyState
        : IGameModeLobbyState
    {
        public static ClearedGameModeLobbyState Instance { get; } =
            new();

        private ClearedGameModeLobbyState()
        {
        }
    }
}
