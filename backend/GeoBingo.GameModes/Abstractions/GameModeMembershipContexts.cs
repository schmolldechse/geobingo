using System;

namespace GeoBingo.GameModes.Abstractions;

public sealed record GameModeParticipantWithdrawalContext(
    IGameModeLobbyState LobbyState,
    IGameModeRoundState RoundState,
    Guid ParticipantUserId,
    DateTimeOffset Now);
