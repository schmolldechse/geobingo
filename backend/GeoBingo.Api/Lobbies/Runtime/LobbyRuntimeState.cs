using System;
using System.Collections.Generic;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed record LobbyCreation
{
    public LobbyCreation(
        Guid hostUserId,
        string hostHandle,
        string hostDisplayName,
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
        Settings = CopySettings(settings!);
    }

    public Guid HostUserId { get; }

    public string HostHandle { get; }

    public string HostDisplayName { get; }

    public LobbySettings Settings { get; }

    private static LobbySettings CopySettings(LobbySettings settings) =>
        new()
        {
            MaxPlayers = settings.MaxPlayers
        };
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

    public required int JoinOrder { get; init; }

    public HashSet<string> ConnectionIds { get; } =
        new(StringComparer.Ordinal);
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
                JoinOrder = 1
            });
    }

    public Guid LobbyId { get; }

    public string Code { get; }

    public LobbyStatus Status { get; internal set; }

    public Guid HostUserId { get; internal set; }

    public LobbySettings Settings => CopySettings(settings);

    public string SelectedModeKey { get; internal set; }

    public Version SelectedModeVersion { get; internal set; }

    public IGameModeLobbyState ModeLobbyState { get; internal set; }

    public Dictionary<Guid, LobbyMemberState> Members { get; } = [];

    public HashSet<Guid> BannedUserIds { get; } = [];

    public long StateVersion { get; private set; }

    public int NextJoinOrder { get; private set; } = 2;

    public IGameModeRoundState? CurrentRound { get; internal set; }

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

    public void IncrementStateVersion() =>
        StateVersion = checked(StateVersion + 1);

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
}
