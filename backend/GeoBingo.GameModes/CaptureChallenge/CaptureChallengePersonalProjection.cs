using System;
using System.Collections.Generic;
using GeoBingo.Contracts.GameModes.CaptureChallenge;

namespace GeoBingo.GameModes.CaptureChallenge;

public sealed record CaptureChallengeCaptureSlotProjection(
    CaptureChallengeGoalProjection Goal,
    Guid? CaptureId,
    StreetViewPosition? Position,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CaptureChallengeCurrentAssignmentProjection(
    Guid AssignmentId,
    CaptureChallengeGoalProjection Goal,
    StreetViewPosition Position,
    int Sequence);

public sealed record CaptureChallengeVoteHistoryEntryProjection(
    Guid AssignmentId,
    VoteValue Value);

public sealed record CaptureChallengePersonalProjection(
    CaptureChallengeStatus? Status,
    IReadOnlyList<CaptureChallengeCaptureSlotProjection> CaptureSlots,
    CaptureChallengeCurrentAssignmentProjection? CurrentAssignment,
    int TotalAssignmentCount,
    int CompletedAssignmentCount,
    IReadOnlyList<CaptureChallengeVoteHistoryEntryProjection> VoteHistory);
