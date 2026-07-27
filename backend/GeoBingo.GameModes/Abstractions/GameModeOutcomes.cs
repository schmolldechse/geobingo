using System;
using System.Collections.Generic;
using GeoBingo.Contracts.Common;

namespace GeoBingo.GameModes.Abstractions;

public sealed record GameModeValidationIssue(
    ErrorCode Code,
    string Message,
    string? MemberName);

public sealed record GameModeValidationResult(
    bool IsValid,
    IReadOnlyList<GameModeValidationIssue> Issues);

public sealed record GameModeRoundCreation(
    IGameModeRoundState RoundState);

public enum GameModeAdvanceKind
{
    NO_CHANGE,
    STATE_CHANGED,
    COMPLETED
}

public sealed record GameModeAdvanceOutcome(
    GameModeAdvanceKind Kind,
    DateTimeOffset? NextDeadline);
