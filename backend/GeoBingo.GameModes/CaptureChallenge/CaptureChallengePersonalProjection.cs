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

public sealed record CaptureChallengeCurrentCaptureProjection(
    Guid CaptureId,
    Guid OwnerUserId,
    CaptureChallengeGoalProjection Goal,
    StreetViewPosition Position,
    int Sequence,
    bool IsOwner,
    VoteValue? SelectedValue);

public sealed record CaptureChallengeVoteHistoryEntryProjection(
    Guid CaptureId,
    int Sequence,
    VoteValue Value);

public sealed record CaptureChallengePersonalProjection(
    CaptureChallengeStatus? Status,
    IReadOnlyList<CaptureChallengeCaptureSlotProjection> CaptureSlots,
    CaptureChallengeCurrentCaptureProjection? CurrentCapture,
    int TotalEligibleVoteCount,
    int SubmittedVoteCount,
    IReadOnlyList<CaptureChallengeVoteHistoryEntryProjection> VoteHistory);
