using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("Submits the caller's first capture for one round goal.")]
public sealed record SubmitCaptureRequest
{
    [JsonPropertyName("roundGoalId")]
    [Description("The active round goal identifier.")]
    public required Guid RoundGoalId { get; init; }

    [JsonPropertyName("position")]
    [Required]
    [Description("The selected Street View position.")]
    public required StreetViewPosition Position { get; init; }
}

[TranspilationSource]
[Description("Replaces one capture owned by the caller.")]
public sealed record UpdateCaptureRequest
{
    [JsonPropertyName("captureId")]
    [Description("The existing capture identifier.")]
    public required Guid CaptureId { get; init; }

    [JsonPropertyName("position")]
    [Required]
    [Description("The replacement Street View position.")]
    public required StreetViewPosition Position { get; init; }
}

[TranspilationSource]
[Description("Removes one capture owned by the caller.")]
public sealed record RemoveCaptureRequest
{
    [JsonPropertyName("captureId")]
    [Description("The existing capture identifier.")]
    public required Guid CaptureId { get; init; }
}
