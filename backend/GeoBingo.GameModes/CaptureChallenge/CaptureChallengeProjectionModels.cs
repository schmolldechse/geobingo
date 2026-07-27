using System;

namespace GeoBingo.GameModes.CaptureChallenge;

public sealed record CaptureChallengeGoalProjection(
    Guid GoalId,
    string Title,
    string? Description,
    int DisplayOrder,
    decimal ScoreFactor);

public sealed record CaptureChallengeParticipantCaptureProgress(
    Guid UserId,
    int SubmittedCaptureCount,
    int TotalGoalCount);

public sealed record CaptureChallengeVotingProgress(
    int CompletedAssignmentCount,
    int TotalAssignmentCount);
