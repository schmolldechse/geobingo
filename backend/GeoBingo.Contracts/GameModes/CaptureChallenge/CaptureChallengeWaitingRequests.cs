using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("Replaces the Capture Challenge settings of the current waiting lobby.")]
public sealed record UpdateCaptureChallengeSettingsRequest
{
    [JsonPropertyName("settings")]
    [Required]
    [Description("The complete replacement settings.")]
    public required CaptureChallengeSettings Settings { get; init; }
}

[TranspilationSource]
[Description("Adds one goal to the current waiting Capture Challenge configuration.")]
public sealed record AddCaptureGoalRequest
{
    [JsonPropertyName("goal")]
    [Required]
    [Description("The goal to append at the end of the display order.")]
    public required CaptureGoalInput Goal { get; init; }
}

[TranspilationSource]
[Description("Replaces one configured Capture Challenge goal.")]
public sealed record UpdateCaptureGoalRequest
{
    [JsonPropertyName("goalId")]
    [Description("The goal identifier.")]
    public required Guid GoalId { get; init; }

    [JsonPropertyName("goal")]
    [Required]
    [Description("The complete replacement goal data. Its display order remains unchanged.")]
    public required CaptureGoalInput Goal { get; init; }
}

[TranspilationSource]
[Description("Removes one configured Capture Challenge goal.")]
public sealed record RemoveCaptureGoalRequest
{
    [JsonPropertyName("goalId")]
    [Description("The goal identifier.")]
    public required Guid GoalId { get; init; }
}

[TranspilationSource]
[Description("Replaces the visual display order of all configured Capture Challenge goals without affecting gameplay.")]
public sealed record ReorderCaptureGoalsRequest
{
    [JsonPropertyName("goalIdsInDisplayOrder")]
    [Required]
    [MinLength(1)]
    [MaxLength(25)]
    [Description("Every current goal identifier exactly once in the desired visual display order.")]
    public required IReadOnlyList<Guid> GoalIdsInDisplayOrder { get; init; }
}
