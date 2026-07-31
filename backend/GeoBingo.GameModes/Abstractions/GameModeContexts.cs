using System;
using System.Collections.Generic;

namespace GeoBingo.GameModes.Abstractions;

public sealed record GameModeValidationContext(
    int ActiveParticipantCount);

public sealed record GameModeParticipant(
    Guid UserId,
    string Handle,
    string DisplayName,
    int JoinOrder);

public sealed record GameModeRoundCreationContext(
    Guid RoundId,
    int RoundNumber,
    IReadOnlyList<GameModeParticipant> Participants,
    DateTimeOffset CreatedAt);

public sealed record GameModeProjectionContext(
    IGameModeLobbyState LobbyState,
    IGameModeRoundState? RoundState,
    int ActiveParticipantCount,
    Guid? RecipientUserId,
    DateTimeOffset Now);

public sealed record GameModeAdvanceContext(
    IGameModeLobbyState LobbyState,
    IGameModeRoundState RoundState,
    IReadOnlySet<Guid> CurrentMemberUserIds,
    DateTimeOffset Now);
