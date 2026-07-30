using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using GeoBingo.Contracts.Common;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("A safe reason that the current game-mode configuration cannot start.")]
public sealed record GameModeValidationIssueView
{
    [JsonPropertyName("code")]
    [Description("The machine-readable validation code.")]
    public required ErrorCode Code { get; init; }

    [JsonPropertyName("message")]
    [Description("The safe validation summary.")]
    public required string Message { get; init; }

    [JsonPropertyName("memberName")]
    [Description("The affected contract member when one exists.")]
    public string? MemberName { get; init; }
}

[TranspilationSource]
[Description("A configured Capture Challenge goal.")]
public sealed record CaptureChallengeGoalProjection
{
    [JsonPropertyName("goalId")]
    public required Guid GoalId { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("displayOrder")]
    public required int DisplayOrder { get; init; }

    [JsonPropertyName("scoreFactor")]
    public required decimal ScoreFactor { get; init; }
}

[TranspilationSource]
public sealed record CaptureChallengeParticipantCaptureProgress
{
    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }

    [JsonPropertyName("submittedCaptureCount")]
    public required int SubmittedCaptureCount { get; init; }

    [JsonPropertyName("totalGoalCount")]
    public required int TotalGoalCount { get; init; }
}

[TranspilationSource]
public sealed record CaptureChallengeVotingProgress
{
    [JsonPropertyName("completedAssignmentCount")]
    public required int CompletedAssignmentCount { get; init; }

    [JsonPropertyName("totalAssignmentCount")]
    public required int TotalAssignmentCount { get; init; }
}

[TranspilationSource]
[Description("The public Capture Challenge projection.")]
public sealed record CaptureChallengePublicProjection
{
    [JsonPropertyName("settings")]
    public required CaptureChallengeSettings Settings { get; init; }

    [JsonPropertyName("goals")]
    public required IReadOnlyList<CaptureChallengeGoalProjection> Goals { get; init; }

    [JsonPropertyName("activeParticipantCount")]
    public required int ActiveParticipantCount { get; init; }

    [JsonPropertyName("projectedCaptureSlotCount")]
    public required int ProjectedCaptureSlotCount { get; init; }

    [JsonPropertyName("roundStartIssues")]
    public required IReadOnlyList<GameModeValidationIssueView> RoundStartIssues { get; init; }

    [JsonPropertyName("status")]
    public CaptureChallengeStatus? Status { get; init; }

    [JsonPropertyName("captureEndsAt")]
    public DateTimeOffset? CaptureEndsAt { get; init; }

    [JsonPropertyName("votingEndsAt")]
    public DateTimeOffset? VotingEndsAt { get; init; }

    [JsonPropertyName("participantCaptureProgress")]
    public required IReadOnlyList<CaptureChallengeParticipantCaptureProgress> ParticipantCaptureProgress { get; init; }

    [JsonPropertyName("submittedCaptureCount")]
    public required int SubmittedCaptureCount { get; init; }

    [JsonPropertyName("releasedCaptureCount")]
    public required int ReleasedCaptureCount { get; init; }

    [JsonPropertyName("votingProgress")]
    public CaptureChallengeVotingProgress? VotingProgress { get; init; }
}

[TranspilationSource]
public sealed record CaptureChallengeCaptureSlotProjection
{
    [JsonPropertyName("goal")]
    public required CaptureChallengeGoalProjection Goal { get; init; }

    [JsonPropertyName("captureId")]
    public Guid? CaptureId { get; init; }

    [JsonPropertyName("position")]
    public StreetViewPosition? Position { get; init; }

    [JsonPropertyName("submittedAt")]
    public DateTimeOffset? SubmittedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; init; }
}

[TranspilationSource]
public sealed record CaptureChallengeCurrentAssignmentProjection
{
    [JsonPropertyName("assignmentId")]
    public required Guid AssignmentId { get; init; }

    [JsonPropertyName("goal")]
    public required CaptureChallengeGoalProjection Goal { get; init; }

    [JsonPropertyName("position")]
    public required StreetViewPosition Position { get; init; }

    [JsonPropertyName("sequence")]
    public required int Sequence { get; init; }
}

[TranspilationSource]
public sealed record CaptureChallengeVoteHistoryEntryProjection
{
    [JsonPropertyName("assignmentId")]
    public required Guid AssignmentId { get; init; }

    [JsonPropertyName("value")]
    public required VoteValue Value { get; init; }
}

[TranspilationSource]
[Description("The current member's private Capture Challenge projection.")]
public sealed record CaptureChallengePersonalProjection
{
    [JsonPropertyName("status")]
    public CaptureChallengeStatus? Status { get; init; }

    [JsonPropertyName("captureSlots")]
    public required IReadOnlyList<CaptureChallengeCaptureSlotProjection> CaptureSlots { get; init; }

    [JsonPropertyName("currentAssignment")]
    public CaptureChallengeCurrentAssignmentProjection? CurrentAssignment { get; init; }

    [JsonPropertyName("totalAssignmentCount")]
    public required int TotalAssignmentCount { get; init; }

    [JsonPropertyName("completedAssignmentCount")]
    public required int CompletedAssignmentCount { get; init; }

    [JsonPropertyName("voteHistory")]
    public required IReadOnlyList<CaptureChallengeVoteHistoryEntryProjection> VoteHistory { get; init; }
}
