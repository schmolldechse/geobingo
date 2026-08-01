using System;

namespace GeoBingo.GameModes.CaptureChallenge;

public sealed record CaptureChallengeGoalProjection(
    Guid GoalId,
    string Title,
    int DisplayOrder,
    decimal ScoreFactor);

public sealed record CaptureChallengeParticipantCaptureProgress(
    Guid UserId,
    int SubmittedCaptureCount,
    int TotalGoalCount);

public sealed record CaptureChallengeVotingProgress(
    int SubmittedVoteCount,
    int EligibleVoteCount);
