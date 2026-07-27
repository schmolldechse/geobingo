using System;
using System.Collections.Generic;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.GameModes.CaptureChallenge;

public sealed record CaptureChallengePublicProjection(
    CaptureChallengeSettings Settings,
    IReadOnlyList<CaptureChallengeGoalProjection> Goals,
    int ActiveParticipantCount,
    int ProjectedCaptureSlotCount,
    IReadOnlyList<GameModeValidationIssue> RoundStartIssues,
    CaptureChallengeStatus? Status,
    DateTimeOffset? CaptureEndsAt,
    DateTimeOffset? VotingEndsAt,
    IReadOnlyList<CaptureChallengeParticipantCaptureProgress> ParticipantCaptureProgress,
    int SubmittedCaptureCount,
    int ReleasedCaptureCount,
    CaptureChallengeVotingProgress? VotingProgress);
